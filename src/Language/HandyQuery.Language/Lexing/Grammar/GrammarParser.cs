using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Extensions;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Grammar
{
    internal sealed class GrammarParser
    {
        private readonly Dictionary<string, GrammarNonTerminal> _nonTerminals =
            new Dictionary<string, GrammarNonTerminal>();

        private readonly string _grammar;
        private readonly TokenizersSource _tokenizersSource;

        private const string Comment = "//";
        private const string Return = "return ";
        private const char GrammarNonTerminalStart = '<';
        private const char GrammarNonTerminalEnd = '>';

        public GrammarParser(string grammar, TokenizersSource tokenizersSource)
        {
            _grammar = grammar;
            _tokenizersSource = tokenizersSource;
        }

        public Grammar Parse()
        {
            var final = ParseImpl();
            var nonTerminals = _nonTerminals.Select(x => x.Value).ToArray();
            return new Grammar(final, nonTerminals);
        }

        private GrammarReturn ParseImpl()
        {
            GrammarReturn final = null;
            var reader = new LexerStringReader(_grammar, 0);

            while (reader.IsInRange())
            {
                if (reader.IsEndOfLine())
                {
                    reader.MoveNext();
                    continue;
                }

                if (reader.StartsWith(Comment.AsSpan()))
                {
                    reader.MoveToNextLine();
                    continue;
                }

                reader.ReadTillEndOfWhitespace();

                if (reader.StartsWith(GrammarNonTerminalStart.ToString().AsSpan()))
                {
                    // _________________________________________
                    // <value> ::= Literal|<function-invokation>

                    var nonTerminalName = reader.ReadTillEndOfWord(); // <value>

                    if (nonTerminalName.EndsWith(GrammarNonTerminalEnd) == false)
                    {
                        throw new GrammarParserException($"Non-terminal {nonTerminalName.SlowlyCreateString()} name " +
                                                                 $"should finish with {GrammarNonTerminalEnd}");
                    }

                    var nonTerminal = GetNonTerminalByName(nonTerminalName.SlowlyCreateString());

                    if (nonTerminal.FullyParsed)
                    {
                        throw new GrammarParserException($"Cannot declare {nonTerminalName.SlowlyCreateString()} more than once.");
                    }

                    // skip '::='
                    var equals = reader.ReadWhile(x => x != '=').SlowlyCreateString() + reader.CurrentChar;
                    if (equals.EndsWith("::=") == false)
                    {
                        throw new GrammarParserException($"Invalid syntax for {nonTerminalName.SlowlyCreateString()}.");
                    }
                    reader.MoveNext();
                    reader.ReadTillEndOfWhitespace();

                    nonTerminal.Body = ParseNonTerminalBody(ref reader, nonTerminal);
                    continue;
                }

                if (reader.StartsWith(Return.AsSpan()))
                {
                    final = ParseReturn(ref reader);
                    break;
                }

                throw new GrammarParserException($"Invalid grammar syntax. Line: {reader.ReadTillNewLine().SlowlyCreateString()}");
            }

            if (final == null)
            {
                throw new GrammarParserException("Return statement not found.");
            }

            var notExistingNonTerminal = _nonTerminals.Select(x => x.Value).FirstOrDefault(x => x.FullyParsed == false);
            if (notExistingNonTerminal != null)
            {
                throw new GrammarParserException($"Non-terminal '{notExistingNonTerminal.Name}' " +
                                                         "is not declared. Are you sure your gramma is fine?");
            }

            foreach (var nonTerminal in _nonTerminals.Select(x => x.Value))
            {
                if (nonTerminal.Body.OrConditionOperands.All(x => x.IsRecursive))
                {
                    var msg = $"Infinite recursion detected in {nonTerminal.Name} non-terminal. " +
                              "Use '|' to create escape path.";
                    throw new GrammarParserException(msg);
                }
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

        //             _____________________________
        // <value> ::= Literal|<function-invokation>
        private GrammarNonTerminalBody ParseNonTerminalBody(ref LexerStringReader reader, GrammarNonTerminal nonTerminal)
        {
            var body = new GrammarNonTerminalBody();

            var bodySpan = reader.ReadTillNewLine();
            var orConditions = bodySpan.Split('|', StringSplitOptions.RemoveEmptyEntries);
            foreach (var operandSplitItem in orConditions)
            {
                var operand = new GrammarNonTerminalBody.OrConditionOperand();
                var operandSpan = operandSplitItem.SliceFrom(ref bodySpan).Trim();
                var blockItems = operandSpan
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var blockItem in blockItems)
                {
                    var bodyItem = ParseNonTerminalBodyItem(blockItem.SliceFrom(ref operandSpan).Trim());
                    if (bodyItem is GrammarNonTerminalUsage nonTerminalUsage
                        && ReferenceEquals(nonTerminal, nonTerminalUsage.Impl))
                    {
                        operand.IsRecursive = true;
                    }
                    operand.Add(bodyItem);
                }

                body.OrConditionOperands.Add(operand);
            }

            return body;
        }

        //             _______
        // <value> ::= Literal|<function-invokation>
        // 
        // OR
        //                     _____________________
        // <value> ::= Literal|<function-invokation>
        private IGrammarBodyItem ParseNonTerminalBodyItem(ReadOnlySpan<char> blockItem)
        {
            var nameSpan = blockItem;
            var nameString = nameSpan.SlowlyCreateString();

            IGrammarBodyItem result;

            if (nameSpan.StartsWith(GrammarNonTerminalStart.ToString().AsSpan()))
            {
                result = new GrammarNonTerminalUsage(nameString, GetNonTerminalByName(nameString));
            }
            else
            {
                if (_tokenizersSource.TryGetTokenizer(nameString, out var tokenizer) == false)
                {
                    throw new GrammarParserException($"Terminal '{nameString}' does not exist.");
                }

                result = new GrammarTerminalUsage(nameString, tokenizer);
            }

            return result;
        }

        // ______________
        // return <value>
        private GrammarReturn ParseReturn(ref LexerStringReader reader)
        {
            reader.MoveBy(Return.Length);
            var nonTerminalName = reader.ReadTillEndOfWord().SlowlyCreateString();
            
            if (_nonTerminals.TryGetValue(nonTerminalName, out var grammarElement) == false)
            {
                throw new GrammarParserException($"Non-terminal {nonTerminalName} does not exist.");
            }

            return new GrammarReturn(new GrammarNonTerminalUsage(grammarElement.Name, grammarElement));
        }
    }
}