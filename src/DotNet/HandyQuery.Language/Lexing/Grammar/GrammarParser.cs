using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Grammar
{
    internal sealed class GrammarParser
    {
        private readonly LexerStringReader _reader;

        private readonly Dictionary<string, GrammarNonTerminal> _nonTerminals =
            new Dictionary<string, GrammarNonTerminal>();

        private readonly TokenizersSource _tokenizersSource;

        private const string Comment = "//";
        private const string Return = "return ";
        private const string GrammarNonTerminalStart = "<";
        private const string GrammarNonTerminalEnd = ">";

        public GrammarParser(LexerStringReader reader, TokenizersSource tokenizersSource)
        {
            _reader = reader;
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
                    // _________________________________________
                    // <value> ::= Literal|<function-invokation>

                    var nonTerminalName = _reader.ReadTillEndOfWord(); // <value>

                    if (nonTerminalName.EndsWith(GrammarNonTerminalEnd) == false)
                    {
                        throw new GrammarParserException($"Non-terminal {nonTerminalName} name " +
                                                                 $"should finish with {GrammarNonTerminalEnd}");
                    }

                    var nonTerminal = GetNonTerminalByName(nonTerminalName);

                    if (nonTerminal.FullyParsed)
                    {
                        throw new GrammarParserException($"Cannot declare {nonTerminalName} more than once.");
                    }

                    // skip '::='
                    var equals = _reader.ReadTill(x => x == '=') + _reader.CurrentChar;
                    if (@equals.EndsWith("::=") == false)
                    {
                        throw new GrammarParserException($"Invalid syntax for {nonTerminalName}.");
                    }
                    _reader.MoveNext();
                    _reader.ReadTillEndOfWhitespace();

                    nonTerminal.Body = ParseNonTerminalBody(nonTerminal);
                    continue;
                }

                if (_reader.StartsWith(Return))
                {
                    final = ParseReturn();
                    break;
                }

                throw new GrammarParserException($"Invalid grammar syntax. Line: {_reader.ReadTillNewLine()}");
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
        private GrammarNonTerminalBody ParseNonTerminalBody(GrammarNonTerminal nonTerminal)
        {
            var body = new GrammarNonTerminalBody();

            var bodyString = _reader.ReadTillNewLine();
            var orConditions = bodyString.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var operandString in orConditions)
            {
                var operand = new GrammarNonTerminalBody.OrConditionOperand();
                var blockItems = operandString.Trim().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var blockItem in blockItems)
                {
                    var bodyItem = ParseNonTerminalBodyItem(blockItem.Trim());
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
                    throw new GrammarParserException($"Terminal '{name}' does not exist.");
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
                throw new GrammarParserException($"Non-terminal {nonTerminalName} does not exist.");
            }

            return new GrammarReturn(new GrammarNonTerminalUsage(grammarElement.Name, grammarElement));
        }
    }
}