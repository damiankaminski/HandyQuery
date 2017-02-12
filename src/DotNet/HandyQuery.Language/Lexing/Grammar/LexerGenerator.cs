using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HandyQuery.Language.Lexing.Grammar.Structure;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing.Grammar
{
    internal sealed class LexerGenerator
    {
        /// <summary>
        /// Generates a new parser which can be then reused to parse user queries.
        /// </summary>
        /// <remarks>Lexer is generated only once, so there is no need to avoid allocations.</remarks>
        public Lexer GenerateLexer()
        {
            var tokenizersSource = new TokenizersSource();

            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("HandyQuery.Language.Lexing.Grammar.Language.grammar"))
            using (var textStream = new StreamReader(stream))
            {
                var grammar = textStream.ReadToEnd();
                var reader = new LexerStringReader(grammar, 0);
                var parser = new ParserImpl(reader, tokenizersSource);
                var root = parser.Parse();

                return Lexer.Build(root);
            }
        }

        /// <summary>
        /// Language grammar definition parser implementation.
        /// </summary>
        internal sealed class ParserImpl
        {
            private readonly LexerStringReader _reader;
            private readonly Dictionary<string, GrammarPart> _parts = new Dictionary<string, GrammarPart>();
            private readonly TokenizersSource _tokenizersSource;

            private const string Comment = "//";
            private const string Return = "return ";
            private const string GrammarPart = "$";
            private const string Optional = "?";

            public ParserImpl(LexerStringReader reader, TokenizersSource tokenizersSource)
            {
                _reader = reader;
                _tokenizersSource = tokenizersSource;
            }

            public GrammarPart Parse()
            {
                GrammarPart final = null;

                while (_reader.IsInRange())
                {
                    if (_reader.IsNewLine())
                    {
                        _reader.MoveNext();
                        continue;
                    }

                    if (_reader.StartsWith(Comment))
                    {
                        _reader.MoveToNextLine();
                        continue;
                    }

                    _reader.ReadTillEndOfWhitespace();

                    if (_reader.StartsWith(GrammarPart))
                    {
                        // ____________________________________
                        // $Value = Literal|$FunctionInvokation

                        var partName = _reader.ReadTillEndOfWord(); // $Value
                        var part = GetPartByName(partName);

                        if (part.FullyParsed)
                        {
                            throw new GrammarLexerGeneratorException($"Cannot declare '{partName}' more than once.");
                        }

                        // skip '=' char
                        _reader.MoveBy(3); 

                        part.Body = ParsePartBody();
                        continue;
                    }

                    if (_reader.StartsWith(Return))
                    {
                        final = ParseReturn();
                        break;
                    }
                }

                if (final == null)
                {
                    throw new GrammarLexerGeneratorException("Unable to parse language gramma.");
                }

                var notExistingPart = _parts.Select(x => x.Value).FirstOrDefault(x => x.FullyParsed == false);
                if (notExistingPart != null)
                {
                    throw new GrammarLexerGeneratorException($"Part '{notExistingPart.Name}' is not declared. Are you sure your gramma is fine?");
                }

                return final;
            }

            private GrammarPart GetPartByName(string partName)
            {
                if (_parts.ContainsKey(partName) == false)
                {
                    _parts.Add(partName, new GrammarPart(partName));
                }

                return _parts[partName];
            }

            /// <remarks>
            ///          ___________________________
            /// $Value = Literal|$FunctionInvokation
            /// </remarks>
            private GrammarPartBody ParsePartBody()
            {
                var bodyString = _reader.ReadTillNewLine();
                var blockItems = bodyString.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                var body = new GrammarPartBody();

                foreach (var blockItem in blockItems)
                {
                    var orConditions = blockItem.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
                    if (orConditions.Length > 1)
                    {
                        var operands = new List<IGrammarBodyItem>();

                        foreach (var operand in orConditions)
                        {
                            if (operand.StartsWith(Optional))
                            {
                                throw new GrammarLexerGeneratorException($"Error occurred while parsing '{blockItem}'. " +
                                                                         "Cannot use optional character '?' in OR conditions.");
                            }

                            operands.Add(ParsePartBodyItem(operand));
                        }

                        body.Add(new GrammarOrCondition(operands));
                        continue;
                    }

                    body.Add(ParsePartBodyItem(blockItem));
                }

                if (body.All(x => x.IsOptional))
                {
                    throw new GrammarLexerGeneratorException($"Body cannot contain only optional items ('{bodyString}')");
                }

                return body;
            }

            /// <remarks>
            ///          _______
            /// $Value = Literal|$FunctionInvokation
            /// 
            /// OR
            ///                  ___________________
            /// $Value = Literal|$FunctionInvokation
            /// </remarks>
            private IGrammarBodyItem ParsePartBodyItem(string blockItem)
            {
                var isOptional = blockItem.StartsWith(Optional);
                var name = blockItem;
                if (isOptional) name = name.Substring(Optional.Length);

                IGrammarBodyItem result;

                if (name.StartsWith(GrammarPart))
                {
                    result = new GrammarPartUsage(name, isOptional, GetPartByName(name));
                }
                else
                {
                    ITokenizer tokenizer;
                    if (_tokenizersSource.TryGetTokenizer(name, out tokenizer) == false)
                    {
                        throw new GrammarLexerGeneratorException($"Tokenizer '{name}' does not exist.");
                    }

                    result = new GrammarTokenizerUsage(name, isOptional, tokenizer);
                }

                return result;
            }

            /// <remarks>
            /// _____________
            /// return $Value
            /// </remarks>
            private GrammarPart ParseReturn()
            {
                _reader.MoveBy(Return.Length);
                var partName = _reader.ReadTillEndOfWord();
                GrammarPart grammarElement;
                if (_parts.TryGetValue(partName, out grammarElement) == false)
                {
                    throw new GrammarLexerGeneratorException($"Part '{partName}' does not exist.");
                }

                return grammarElement;
            }
        }
    }
}