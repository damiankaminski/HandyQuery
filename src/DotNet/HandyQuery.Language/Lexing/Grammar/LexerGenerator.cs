using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Grammar
{
    internal sealed class LexerGenerator
    {
        /// <summary>
        /// Generates a new lexer which can be then reused to tokenize user queries.
        /// </summary>
        /// <remarks>Lexer is generated only once, so there is no need to avoid allocations.</remarks>
        public Lexer GenerateLexer()
        {
            var tokenizersSource = new TokenizersSource();
            var notFoundException = new GrammarLexerGeneratorException("Grammar not found.");
            
            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("HandyQuery.Language.Lexing.Grammar.Language.grammar"))
            using (var textStream = new StreamReader(stream ?? throw notFoundException))
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
            private readonly Dictionary<string, GrammarNonTerminal> _nonTerminals = new Dictionary<string, GrammarNonTerminal>();
            private readonly TokenizersSource _tokenizersSource;

            private const string Comment = "//";
            private const string Return = "return ";
            private const string GrammarNonTerminalStart = "<";
            private const string GrammarNonTerminalEnd = ">";

            public ParserImpl(LexerStringReader reader, TokenizersSource tokenizersSource)
            {
                _reader = reader;
                _tokenizersSource = tokenizersSource;
            }

            public GrammarReturn Parse()
            {
                GrammarReturn final = null;

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

                    if (_reader.StartsWith(GrammarNonTerminalStart))
                    {
                        // _______________________________________
                        // <value> ::= Literal|<function-invokation>

                        var nonTerminalName = _reader.ReadTillEndOfWord(); // <value>
                        
                        if (nonTerminalName.EndsWith(GrammarNonTerminalEnd) == false)
                        {
                            throw new GrammarLexerGeneratorException($"Non-terminal {nonTerminalName} name " +
                                                                     $"should finish with {GrammarNonTerminalEnd}");
                        }
                        
                        var nonTerminal = GetNonTerminalByName(nonTerminalName);

                        if (nonTerminal.FullyParsed)
                        {
                            throw new GrammarLexerGeneratorException($"Cannot declare {nonTerminalName} more than once.");
                        }

                        // skip '::='
                        var equals = _reader.ReadTill(x => x == '=') + _reader.CurrentChar;
                        if (equals.EndsWith("::=") == false)
                        {
                            throw new GrammarLexerGeneratorException($"Invalid syntax for {nonTerminalName}.");
                        }
                        _reader.MoveNext();
                        _reader.ReadTillEndOfWhitespace();

                        nonTerminal.Body = ParseNonTerminalBody();
                        continue;
                    }

                    if (_reader.StartsWith(Return))
                    {
                        final = ParseReturn();
                        break;
                    }
                    
                    throw new GrammarLexerGeneratorException($"Invalid grammar syntax. Line: {_reader.ReadTillNewLine()}");
                }

                if (final == null)
                {
                    throw new GrammarLexerGeneratorException("Return statement not found.");
                }

                var notExistingNonTerminal = _nonTerminals.Select(x => x.Value).FirstOrDefault(x => x.FullyParsed == false);
                if (notExistingNonTerminal != null)
                {
                    throw new GrammarLexerGeneratorException($"Non-terminal '{notExistingNonTerminal.Name}' " +
                                                              "is not declared. Are you sure your gramma is fine?");
                }

                return final;
            }

            private GrammarNonTerminal GetNonTerminalByName(string nonTerminalName)
            {
                if (_nonTerminals.ContainsKey(nonTerminalName) == false)
                {
                    _nonTerminals.Add(nonTerminalName, new GrammarNonTerminal(nonTerminalName));
                }

                return _nonTerminals[nonTerminalName];
            }

            //           _____________________________
            // <value> ::= Literal|<function-invokation>
            private GrammarNonTerminalBody ParseNonTerminalBody()
            {
                var body = new GrammarNonTerminalBody();
                
                var bodyString = _reader.ReadTillNewLine();
                var orConditions = bodyString.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var operandString in orConditions)
                {
                    var operand = new GrammarNonTerminalBody.Operand();
                    var blockItems = operandString.Trim().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var blockItem in blockItems)
                    {
                        operand.Add(ParseNonTerminalBodyItem(blockItem.Trim()));
                    }                    
                    
                    body.Operands.Add(operand);
                }

                return body;
            }

            //           _______
            // <value> ::= Literal|<function-invokation>
            // 
            // OR
            //                   _____________________
            // <value> ::= Literal|<function-invokation>
            private IGrammarBodyItem ParseNonTerminalBodyItem(string blockItem)
            {
                var name = blockItem;

                IGrammarBodyItem result;

                if (name.StartsWith(GrammarNonTerminalStart))
                {
                    result = new GrammarNonTerminalUsage(name, GetNonTerminalByName(name));
                }
                else
                {
                    if (_tokenizersSource.TryGetTokenizer(name, out var tokenizer) == false)
                    {
                        throw new GrammarLexerGeneratorException($"Terminal '{name}' does not exist.");
                    }
                    
                    result = new GrammarTerminalUsage(name, tokenizer);
                }

                return result;
            }

            // ______________
            // return <value>
            private GrammarReturn ParseReturn()
            {
                _reader.MoveBy(Return.Length);
                var nonTerminalName = _reader.ReadTillEndOfWord();
                if (_nonTerminals.TryGetValue(nonTerminalName, out var grammarElement) == false)
                {
                    throw new GrammarLexerGeneratorException($"Non-terminal {nonTerminalName} does not exist.");
                }

                return new GrammarReturn(new GrammarNonTerminalUsage(grammarElement.Name, grammarElement));
            }
        }
    }
}