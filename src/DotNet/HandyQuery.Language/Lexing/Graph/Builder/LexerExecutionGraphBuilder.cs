using System.Collections.Generic;
using System.Collections.ObjectModel;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class LexerExecutionGraphBuilder
    {
        public RootNode BuildGraph(GrammarReturn grammarRoot)
        {
            var root = new RootNode();

            Process(root, grammarRoot.NonTerminalUsage.Impl);
            
            return root;
        }

        private NodesCollection Process(NodesCollection tail, GrammarNonTerminal nonTerminal)
        {
            var finalTail = new NodesCollection();

            if (nonTerminal.Body.Operands.Count > 1)
            {
                var branch = new BranchNode();
                tail.AddChild(branch);
                tail = new NodesCollection(branch);
            }
            
            foreach (var body in nonTerminal.Body.Operands)
            {
                var currentTail = tail;
                var hasTail = true;
                
                foreach (var bodyItem in body)
                {
                    switch (bodyItem)
                    {
                        case GrammarTerminalUsage terminalUsage:
                            var terminalNode = new TerminalNode(terminalUsage);
                            currentTail.AddChild(terminalNode);
                            currentTail = terminalNode;
                            break;
                            
                        case GrammarNonTerminalUsage nonTerminalUsage:
                            if (ReferenceEquals(nonTerminalUsage.Impl, nonTerminal))
                            {
                                // cycle
                                
                                if (nonTerminal.Body.Operands.Count < 2)
                                {
                                    throw new LexerExecutionGraphException("Infinite cycle detected. " +
                                                                           "Use '|' to create escape path.");
                                }
                                
                                currentTail.AddChildren(tail);
                                hasTail = false; // cyclic operands do not have tails
                                break;
                            }
                            
                            currentTail = Process(currentTail, nonTerminalUsage.Impl);
                            break;
                            
                        default:
                            throw new LexerExecutionGraphException(
                                $"Unknown non-terminal body item type: {bodyItem.Type}");
                    }
                }

                if (hasTail)
                {
                    finalTail.AddRange(currentTail);                    
                }
            }

            return finalTail;
        }

        private sealed class NodesCollection : Collection<Node>
        {
            public NodesCollection()
            {
            }

            /// <summary>
            /// Creates a new Nodes list with single value added.
            /// </summary>
            public NodesCollection(Node node)
            {
                Add(node);
            }

            /// <summary>
            /// Creates a new Nodes list with multiple values added.
            /// </summary>
            public NodesCollection(IEnumerable<Node> nodes)
            {
                foreach (var node in nodes)
                {
                    Add(node);
                }
            }

            public void AddChild(Node node)
            {
                foreach (var item in this)
                {
                    switch (item)
                    {    
                        case BranchNode n:
                            n.AddChild(node);
                            break;
                        case RootNode n:
                            n.WithChild(node);
                            break;
                        case TerminalNode n:
                            n.WithChild(node);
                            break;
                        default:
                            throw new LexerExecutionGraphException($"Unkown type: {item.GetType()}");
                    }
                }
            }

            public void AddChildren(NodesCollection nodes)
            {
                foreach (var node in nodes)
                {
                    AddChild(node);
                }
            }

            public static implicit operator NodesCollection(Node node)
            {
                return new NodesCollection(node);
            }

            public override string ToString()
            {
                return $"[{string.Join(", ", this)}]";
            }

            public void AddRange(NodesCollection nodes)
            {
                foreach (var node in nodes)
                {
                    Add(node);
                }
            }
        }
    }
}