using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HandyQuery.Language.Lexing.Gramma.Structure;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing.Gramma
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
                .GetManifestResourceStream("HandyQuery.Language.Lexing.Gramma.Language.gramma"))
            using (var textStream = new StreamReader(stream))
            {
                var gramma = textStream.ReadToEnd();
                var reader = new LexerStringReader(gramma, 0);
                var parser = new ParserImpl(reader, tokenizersSource);
                var root = parser.Parse();

                return Lexer.Build(root);
            }
        }

        /// <summary>
        /// Language gramma definition parser implementation.
        /// </summary>
        private class ParserImpl
        {
            private readonly LexerStringReader _reader;
            private readonly Dictionary<string, GrammaPart> _parts = new Dictionary<string, GrammaPart>();
            private readonly TokenizersSource _tokenizersSource;

            private const string Comment = "//";
            private const string Return = "return ";
            private const string GrammaPart = "$";
            private const string Optional = "?";

            public ParserImpl(LexerStringReader reader, TokenizersSource tokenizersSource)
            {
                _reader = reader;
                _tokenizersSource = tokenizersSource;
            }

            public GrammaPart Parse()
            {
                GrammaPart final = null;

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

                    if (_reader.StartsWith(GrammaPart))
                    {
                        // ____________________________________
                        // $Value = Literal|$FunctionInvokation

                        var partName = _reader.ReadTillEndOfWord(); // $Value
                        var part = GetGrammaPartByName(partName);

                        if (part.FullyParsed)
                        {
                            throw new GrammaLexerGeneratorException($"Cannot declare '{partName}' more than once.");
                        }

                        // skip '=' char
                        _reader.MoveBy(3); 

                        part.Body = ParseGrammaPartBody();
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
                    throw new GrammaLexerGeneratorException("Unable to parse language gramma.");
                }

                var notExistingPart = _parts.Select(x => x.Value).FirstOrDefault(x => x.FullyParsed == false);
                if (notExistingPart != null)
                {
                    throw new GrammaLexerGeneratorException($"Part '{notExistingPart.Name}' is not declared. Are you sure your gramma is fine?");
                }

                return final;
            }

            private GrammaPart GetGrammaPartByName(string partName)
            {
                if (_parts.ContainsKey(partName) == false)
                {
                    _parts.Add(partName, new GrammaPart(partName));
                }

                return _parts[partName];
            }

            /// <remarks>
            ///          ___________________________
            /// $Value = Literal|$FunctionInvokation
            /// </remarks>
            private GrammaPartBody ParseGrammaPartBody()
            {
                var bodyString = _reader.ReadTillNewLine();
                var blockItems = bodyString.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                var body = new GrammaPartBody();

                foreach (var blockItem in blockItems)
                {
                    var orConditions = blockItem.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
                    if (orConditions.Length > 1)
                    {
                        var operands = new List<IGrammaElement>();

                        foreach (var operand in orConditions)
                        {
                            if (operand.StartsWith(Optional))
                            {
                                throw new GrammaLexerGeneratorException($"Error occurred while parsing '{blockItem}'. " +
                                                                         "Cannot use optional character '?' in OR conditions.");
                            }

                            operands.Add(ParsePartBodyItem(operand));
                        }

                        body.Add(new GrammaOrCondition( operands));
                        continue;
                    }

                    body.Add(ParsePartBodyItem(blockItem));
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
            private IGrammaBodyItem ParsePartBodyItem(string blockItem)
            {
                var isOptional = blockItem.StartsWith(Optional);
                var name = blockItem;
                if (isOptional) name = name.Substring(Optional.Length);

                IGrammaBodyItem result;

                if (name.StartsWith(GrammaPart))
                {
                    result = new GrammaPartUsage(name, isOptional, GetGrammaPartByName(name));
                }
                else
                {
                    ITokenizer tokenizer;
                    if (_tokenizersSource.TryGetTokenizer(name, out tokenizer) == false)
                    {
                        throw new GrammaLexerGeneratorException($"Tokenizer '{name}' does not exist.");
                    }

                    result = new GrammaTokenizerUsage(name, isOptional, tokenizer);
                }

                return result;
            }

            /// <remarks>
            /// _____________
            /// return $Value
            /// </remarks>
            private GrammaPart ParseReturn()
            {
                _reader.MoveBy(Return.Length);
                var partName = _reader.ReadTillEndOfWord();
                GrammaPart grammaElement;
                if (_parts.TryGetValue(partName, out grammaElement) == false)
                {
                    throw new GrammaLexerGeneratorException($"Part '{partName}' does not exist.");
                }

                return grammaElement;
            }
        }
    }
}