using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class LexerExecutionGraphBuilder
    {
        public static RootNode BuildGraph(GrammarReturn grammarRoot)
        {
            var root = new RootNode();

            var currentNodes = new Nodes(root);
            ProcessPart(grammarRoot.PartUsage, currentNodes, out var _);

            return root;
        }

        private static void ProcessPart(GrammarPartUsage part, Nodes currentNodes, out Nodes leaveNodes)
        {
            leaveNodes = new Nodes();

            var listeners = new ProcessListeners();

            var contextStack = new Stack<PartContext>();

            var context = new PartContext(part)
            {
                IsInOptionalScope = false,
                LastNonOptionalNodes = null
            };

            while (true)
            {
                context.WasInOptionalScope = context.IsInOptionalScope;
                context.LastNonOptionalNodes = context.IsInOptionalScope ? context.LastNonOptionalNodes : currentNodes;

                if (context.MoveNext() == false)
                {
                    if (contextStack.Any() == false)
                    {
                        //MakeOptionalEdgeIfNeeded();
                        break;
                    }

                    if (context.IsInOptionalScope)
                    {
                        // ended with optional tokenizer

                        var c = context;
                        listeners.OnFirstNonOptionalNode += nodes =>
                        {
                            MakeOptionalEdge(c.LastNonOptionalNodes, nodes);
                        };
                    }

                    context = contextStack.Pop();
                    continue;
                }

                switch (context.CurrentBodyItem)
                {
                    case GrammarTokenizerUsage tokenizerUsage:
                    {
                        var node = new TokenizerNode(tokenizerUsage);
                        currentNodes.AddChild(node);
                        currentNodes = node;

                        context.IsInOptionalScope = tokenizerUsage.IsOptional;
                        leaveNodes = node;

                        if (tokenizerUsage.IsOptional == false)
                        {
                            listeners.ProcessNonOptionalNodes(node);
                        }

                        break;
                    }

                    case GrammarPartUsage partUsage:
                    {
                        // TODO: listeners.ProcessNonOptionalNodes(...);
                        contextStack.Push(context);
                        context = new PartContext(partUsage);
                        break;
                    }

                    case GrammarOrCondition orCondition:
                    {
                        // TODO: listeners.ProcessNonOptionalNodes(node);
                        leaveNodes = new Nodes();
                        foreach (var operand in orCondition.Operands)
                        {
                            switch (operand)
                            {
                                case GrammarTokenizerUsage tokenizerUsage:
                                    var node = new TokenizerNode(tokenizerUsage);
                                    currentNodes.AddChild(node);
                                    leaveNodes.Add(node);
                                    break;
                                case GrammarPartUsage partUsage:
                                    ProcessPart(partUsage, currentNodes, out var partLeaveNodes);
                                    leaveNodes.AddRange(partLeaveNodes);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }

                        context.IsInOptionalScope = false;
                        currentNodes = leaveNodes;
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                MakeOptionalEdgeIfNeeded();
            }

            void MakeOptionalEdgeIfNeeded()
            {
                if (context.WasInOptionalScope && !context.IsInOptionalScope)
                {
                    MakeOptionalEdge(context.LastNonOptionalNodes, currentNodes);
                }
            }

            void MakeOptionalEdge(Nodes from, Nodes to)
            {
                if (from == null || to == null) return;

                foreach (var toNode in to)
                {
                    foreach (var fromNode in from)
                    {
                        fromNode.AddChildImpl(toNode);
                    }
                }
            }
        }

        private sealed class Nodes : List<Node>
        {
            public Nodes()
            {
            }

            /// <summary>
            /// Creates a new Nodes list with single value added.
            /// </summary>
            public Nodes(Node node)
            {
                Add(node);
            }

            public void AddChild(Node node)
            {
                foreach (var item in this)
                {
                    item.AddChildImpl(node);
                }
            }

            public static implicit operator Nodes(Node node)
            {
                return new Nodes(node);
            }
        }

        private sealed class PartContext
        {
            public PartContext(GrammarPartUsage partUsage)
            {
                PartUsage = partUsage;
            }

            public IGrammarBodyItem CurrentBodyItem => PartUsage.Impl.Body[CurrentBodyItemIndex];

            public bool IsInOptionalScope { get; set; }
            public bool WasInOptionalScope { get; set; }
            public Nodes LastNonOptionalNodes { get; set; }

            private GrammarPartUsage PartUsage { get; }
            private int CurrentBodyItemIndex { get; set; } = -1;

            public bool MoveNext()
            {
                if (CurrentBodyItemIndex + 1 >= PartUsage.Impl.Body.Count) return false;

                CurrentBodyItemIndex++;
                return true;
            }
        }

        private class ProcessListeners
        {
            public event Action<Nodes> OnFirstNonOptionalNode;

            public void ProcessNonOptionalNodes(Nodes nodes)
            {
                OnFirstNonOptionalNode?.Invoke(nodes);
                OnFirstNonOptionalNode = null;
            }
        }
    }
}