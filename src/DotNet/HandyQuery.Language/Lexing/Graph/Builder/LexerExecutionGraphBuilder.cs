using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class LexerExecutionGraphBuilder : IDisposable
    {
        private readonly ProcessListeners _listeners = new ProcessListeners();

        public RootNode BuildGraph(GrammarReturn grammarRoot)
        {
            var root = new RootNode();

            var currentNodes = new Nodes(root);
            ProcessPart(grammarRoot.PartUsage, currentNodes, out var _);

            return root;
        }

        private void ProcessPart(GrammarPartUsage part, Nodes currentNodes, out Nodes leaveNodes)
        {
            leaveNodes = new Nodes();

            var contextStack = new Stack<PartContext>();

            var context = new PartContext(part)
            {
                IsInOptionalScope = false
            };

            while (true)
            {
                context.WasInOptionalScope = context.IsInOptionalScope;
                context.LastNonOptionalNodes = context.IsInOptionalScope ? context.LastNonOptionalNodes : currentNodes;

                if (context.MoveNext() == false)
                {
                    if (contextStack.Any() == false)
                    {
                        break;
                    }

                    if (context.IsInOptionalScope)
                    {
                        // ended with optional tokenizer

                        var savedContext = context;
                        _listeners.OnFirstNonOptionalNode += nodes =>
                        {
                            MakeOptionalEdge(savedContext.LastNonOptionalNodes, nodes);
                        };
                    }

                    if (context.PartUsage.IsOptional)
                    {
                        // part is optional
                        var savedFromNodes = contextStack.Peek().LastNonOptionalNodes;
                        _listeners.OnFirstNonOptionalNode += nodes =>
                        {
                            MakeOptionalEdge(savedFromNodes, nodes);
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

                        leaveNodes = node;

                        if (tokenizerUsage.IsOptional)
                        {
                            if (context.IsInOptionalScope == false)
                            {
                                var savedFromNodes = context.LastNonOptionalNodes;
                                _listeners.OnFirstNonOptionalNode += nodes =>
                                {
                                    MakeOptionalEdge(savedFromNodes, nodes);
                                };
                            }
                        }
                        else
                        {    
                            _listeners.HandleNonOptionalNodes(node);
                        }

                        context.IsInOptionalScope = tokenizerUsage.IsOptional;
                        break;
                    }

                    case GrammarPartUsage partUsage:
                    {
                        contextStack.Push(context);
                        context = new PartContext(partUsage);
                        break;
                    }

                    case GrammarOrCondition orCondition:
                    {
                        leaveNodes = new Nodes();
                        foreach (var operand in orCondition.Operands)
                        {
                            switch (operand)
                            {
                                case GrammarTokenizerUsage tokenizerUsage:
                                    var node = new TokenizerNode(tokenizerUsage);
                                    currentNodes.AddChild(node);
                                    leaveNodes.Add(node);
                                    if (tokenizerUsage.IsOptional == false) _listeners.HandleNonOptionalNodes(node);
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

        public void Dispose()
        {
            _listeners?.Dispose();
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
            public GrammarPartUsage PartUsage { get; }

            private int CurrentBodyItemIndex { get; set; } = -1;

            public bool MoveNext()
            {
                if (CurrentBodyItemIndex + 1 >= PartUsage.Impl.Body.Count) return false;

                CurrentBodyItemIndex++;
                return true;
            }
        }

        private class ProcessListeners : IDisposable
        {
            public event Action<Nodes> OnFirstNonOptionalNode;

            public void HandleNonOptionalNodes(Nodes nodes)
            {
                OnFirstNonOptionalNode?.Invoke(nodes);
                OnFirstNonOptionalNode = delegate { };
            }

            public void Dispose()
            {
                OnFirstNonOptionalNode = null;
            }
        }
    }
}