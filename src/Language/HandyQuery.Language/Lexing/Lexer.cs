using System;
using System.Collections.Generic;
using System.Globalization;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Graph;
using HandyQuery.Language.Lexing.Graph.Builder;
using HandyQuery.Language.Lexing.Tokenizers;

namespace HandyQuery.Language.Lexing
{
    internal sealed class Lexer
    {
        internal readonly LexerExecutionGraph ExecutionGraph;

        private readonly WhitespaceTokenizer _whitespaceTokenizer = new WhitespaceTokenizer();
        
        private Lexer(LexerExecutionGraph executionGraph)
        {
            ExecutionGraph = executionGraph;
        }

        public static Lexer Build(Grammar.Grammar grammar)
        {
            return new Lexer(LexerExecutionGraph.Build(grammar));
        }

        [HotPath]
        public LexerResult Tokenize(string query, LanguageConfig languageConfig, LexerConfig config = null)
        {
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException();

            config = config ?? LexerConfig.Default;
            var finalResult = new LexerResult();
            var reader = new LexerStringReader(query, 0);
            var restorableReader = new LexerStringReader.Restorable();
            var runtimeInfo = new LexerRuntimeInfo(reader, languageConfig); // TODO: ref reader?

            var stateStack = new Stack<BranchState>(); // TODO: pool?

            var node = ExecutionGraph.Root.Child;
            while (node != null)
            {
                // TODO: I think this will mess things up on long branch miss scenario...
                restorableReader.RestorePosition(ref reader);
                
                switch (node)
                {
                    case TerminalNode terminalNode:
                    {
                        var tokenizationResult = terminalNode.Tokenizer.Tokenize(ref runtimeInfo);
                        if (tokenizationResult.Success == false)
                        {
                            // try to go with other branch if any available
                            // TODO: remove tokens from finalResult.Tokens (or make them "commitable" somehow and just not commit them in this scenario)
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

                        // search for whitespaces
                        var whitespaceTokenizationResult = _whitespaceTokenizer.Tokenize(ref runtimeInfo);
                        if (whitespaceTokenizationResult.Success && whitespaceTokenizationResult.Token.Length > 0)
                        {
                            finalResult.Tokens.Add(whitespaceTokenizationResult.Token);
                        }
                        
                        node = terminalNode.Child;
                        restorableReader.CaptureCurrentPosition(ref reader);
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