using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Parsing.Grammar.Structure;

namespace HandyQuery.Language.Parsing.Grammar
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
        private const char ArgStart = '(';
        private const char ArgEnd = ')';
        private const char ArgIdentifier = '"';
        private const char Assign = ':';
        private const char OrSeparator = '|';

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
                    // _______________________________________
                    // <value> : Literal|<function-invokation>

                    var nonTerminalName = new string(reader.ReadTillEndOfWord()); // <value>

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

                    // skip ':'
                    var assign = new string(reader.ReadWhile(x => x != Assign)) + reader.CurrentChar;
                    if (assign.EndsWith(Assign) == false)
                    {
                        throw new GrammarParserException($"Invalid syntax for {nonTerminalName}.");
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

                throw new GrammarParserException($"Invalid grammar syntax. Line: {new string(reader.ReadTillNewLine())}");
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

        //           _____________________________
        // <value> : Literal|<function-invokation>
        private GrammarNonTerminalBody ParseNonTerminalBody(ref LexerStringReader reader, GrammarNonTerminal nonTerminal)
        {
            var body = new GrammarNonTerminalBody();

            var bodyString = new string(reader.ReadTillNewLine());
            while (!reader.IsEndOfQuery())
            {
                reader.ReadTillEndOfWhitespace();
                
                if (reader.CurrentChar != OrSeparator && reader.CurrentChar != Assign)
                {
                    break;
                }

                bodyString += new string(reader.ReadTillNewLine());
            }
            
            var orConditions = bodyString.Split(OrSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var operandSplitItem in orConditions)
            {
                var operand = new GrammarNonTerminalBody.OrConditionOperand();
                var blockItems = operandSplitItem.Split(' ', StringSplitOptions.RemoveEmptyEntries);

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

        //           _______
        // <value> : Literal|<function-invokation>|Keyword("is")
        // 
        // OR
        //                   _____________________
        // <value> : Literal|<function-invokation>|Keyword("is")
        //
        // OR
        //                                         _____________                   
        // <value> : Literal|<function-invokation>|Keyword("is")
        private IGrammarBodyItem ParseNonTerminalBodyItem(ReadOnlySpan<char> blockItem)
        {
            var name = new string(blockItem);

            IGrammarBodyItem result;

            if (name.StartsWith(GrammarNonTerminalStart))
            {
                result = new GrammarNonTerminalUsage(name, GetNonTerminalByName(name));
            }
            else if (name.Contains(ArgStart))
            {
                if (name.EndsWith(ArgEnd) == false) throw new GrammarParserException("Argument not closed");

                var argStartIndex = name.IndexOf(ArgStart);

                var tokenizerName = name.Substring(0, argStartIndex);
                
                var arg = name.Substring(argStartIndex);
                arg = arg.Substring(1); // remove (
                arg = arg.Substring(0, arg.Length - 1); // remove )
                
                if (arg.StartsWith(ArgIdentifier) == false) throw new GrammarParserException("Argument not closed");
                if (arg.EndsWith(ArgIdentifier) == false) throw new GrammarParserException("Argument not closed");
                arg = arg.Substring(1); // remove "
                arg = arg.Substring(0, arg.Length - 1); // remove "
                
                if (_tokenizersSource.TryGetTokenizer(tokenizerName, out var tokenizer) == false)
                {
                    throw new GrammarParserException($"Terminal '{tokenizerName}' does not exist.");
                }

                result = new GrammarTerminalUsage(tokenizerName, arg, tokenizer);
            }
            else
            {
                if (_tokenizersSource.TryGetTokenizer(name, out var tokenizer) == false)
                {
                    throw new GrammarParserException($"Terminal '{name}' does not exist.");
                }

                result = new GrammarTerminalUsage(name, null, tokenizer);
            }

            return result;
        }

        // ______________
        // return <value>
        private GrammarReturn ParseReturn(ref LexerStringReader reader)
        {
            reader.MoveBy(Return.Length);
            var nonTerminalName = new string(reader.ReadTillEndOfWord());
            
            if (_nonTerminals.TryGetValue(nonTerminalName, out var grammarElement) == false)
            {
                throw new GrammarParserException($"Non-terminal {nonTerminalName} does not exist.");
            }

            return new GrammarReturn(new GrammarNonTerminalUsage(grammarElement.Name, grammarElement));
        }
    }
}