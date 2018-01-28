using System;
using System.Collections.Generic;
using System.Globalization;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Graph;
using HandyQuery.Language.Lexing.Graph.Builder;

namespace HandyQuery.Language.Lexing
{
    internal sealed class Lexer
    {
        internal readonly LexerExecutionGraph ExecutionGraph;

        private Lexer(LexerExecutionGraph executionGraph)
        {
            ExecutionGraph = executionGraph;
        }

        public static Lexer Build(Grammar.Grammar grammar)
        {
            return new Lexer(LexerExecutionGraph.Build(grammar));
        }

        [PerformanceCritical]
        public LexerResult Tokenize(string query, ILanguageInternalConfig languageConfig, CultureInfo cultureInfo,
            LexerConfig config = null)
        {
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException();

            config = config ?? LexerConfig.Default;
            var finalResult = new LexerResult();
            var reader = new LexerStringReader(query, 0);
            var runtimeInfo = new LexerRuntimeInfo(reader, languageConfig, cultureInfo); // TODO: ref reader?

            var stateStack = new Stack<BranchState>(); // TODO: pool?

            var node = ExecutionGraph.Root.Child;
            while (node != null)
            {
                switch (node)
                {
                    case TerminalNode terminalNode:
                    {
                        var tokenizationResult = terminalNode.Tokenizer.Tokenize(runtimeInfo); // TODO: ref runtimeInfo?
                        if (tokenizationResult.Success == false)
                        {
                            // try to go with other branch if any available
                            var nextFound = false;
                            while (true)
                            {
                                if (stateStack.Count == 0)
                                {
                                    break;
                                }

                                var state = stateStack.Peek();
                                if (state.TryMoveToNextNode(out node) == false)
                                {
                                    stateStack.Pop();
                                    continue;
                                }

                                nextFound = true;
                                break;
                            }

                            if (nextFound == false)
                            {
                                node = null;
                                finalResult.Errors.Add(tokenizationResult.Error);
                            }
                            
                            continue;
                        }

                        finalResult.Tokens.Add(tokenizationResult.Token);
                        
                        // TODO: after each successful tokenization look for whitespaces and save them as WhitespaceTokens
                        
                        node = terminalNode.Child;
                        break;
                    }
                        
                    case BranchNode branchNode:
                    {
                        var nodesEnumerator = branchNode.Heads.GetEnumerator();
                        nodesEnumerator.MoveNext();
                        node = nodesEnumerator.Current;

                        // TODO: save current reader positon and restore it in case of branch failure
                        stateStack.Push(new BranchState(nodesEnumerator));
                        break;
                    }
                        
                    case NonTerminalUsageNode nonTerminalUsageNode:
                    {
                        node = nonTerminalUsageNode.Head;
                        break;
                    }
                        
                    default:
                        throw new QueryLanguageException($"Invalid node type: {node.GetType()}");
                }
            }

            return finalResult;
        }

        private class BranchState
        {
            private readonly IEnumerator<Node> _nodesEnumerator;

            public BranchState(IEnumerator<Node> nodesEnumerator)
            {
                _nodesEnumerator = nodesEnumerator;
            }

            public bool TryMoveToNextNode(out Node nextNode)
            {
                if (_nodesEnumerator.MoveNext() == false)
                {
                    nextNode = null;
                    return false;
                }

                nextNode = _nodesEnumerator.Current;
                return true;
            }
        }
    }
}