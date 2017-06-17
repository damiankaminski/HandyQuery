using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class LexerExecutionGraphBuilder : IDisposable
    {
        private readonly ProcessListeners _listeners = new ProcessListeners();
        private Nodes _currentTail;

        public RootNode BuildGraph(GrammarReturn grammarRoot)
        {
            var root = new RootNode();

            _currentTail = new Nodes(root);
            ProcessPart(grammarRoot.PartUsage).ToList();

            return root;
        }

        private Nodes ProcessPartAndGetLeaveNodes(GrammarPartUsage part)
        {
            using (var enumerator = ProcessPart(part).GetEnumerator())
            {
                Nodes leaveNodes = null;
                while (enumerator.MoveNext())
                {
                    leaveNodes = enumerator.Current;
                }

                return leaveNodes;
            }
        }

        private Nodes ProcessPartAndGetEntryNodes(GrammarPartUsage part)
        {
            using (var enumerator = ProcessPart(part).GetEnumerator())
            {
                enumerator.MoveNext();
                var entryNodes = enumerator.Current;

                while (enumerator.MoveNext())
                {
                }

                return entryNodes;
            }
        }

        private (Nodes Head, IEnumerable<Nodes> Body, Nodes Tail) ProcessPartAndGetNodesInfo(GrammarPartUsage part, Action<Nodes> forEachAction)
        {
            using (var enumerator = ProcessPart(part).GetEnumerator())
            {
                Nodes tail = null;
                var body = new List<Nodes>();
                enumerator.MoveNext();
                var head = enumerator.Current;

                while (enumerator.MoveNext())
                {
                    body.Add(enumerator.Current);
                    tail = enumerator.Current;
                    forEachAction(enumerator.Current);
                }

                if (body.Any())
                {
                    // remove tail from body
                    body.RemoveAt(body.Count - 1);
                }

                return (head, body, tail);
            }
        }

        private IEnumerable<Nodes> ProcessPart(GrammarPartUsage part)
        {
            var contextStack = new Stack<PartContext>();

            var context = new PartContext(part)
            {
                IsInOptionalScope = false
            };

            while (true)
            {
                context.WasInOptionalScope = context.IsInOptionalScope;
                context.LastNonOptionalNodes = context.IsInOptionalScope ? context.LastNonOptionalNodes : _currentTail;

                if (context.MoveNext() == false)
                {
                    if (contextStack.Any() == false)
                    {
                        break;
                    }

                    if (context.IsInOptionalScope)
                    {
                        // ended with optional tokenizer

                        // TODO: I think that in case of multiple optional nodes this should be invoked only for first one (not sure though)

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
                        _currentTail.AddChild(node);
                        _currentTail = node;
                        yield return node;

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
                        var initialTail = new Nodes(_currentTail);

                        var headNodes = new Nodes();
                        var bodyNodes = new List<Nodes>();
                        var tailNodes = new Nodes();

                        var nonOptionalHeadNodes = new Nodes();

                        foreach (var operand in orCondition.Operands)
                        {
                            _currentTail = initialTail;

                            switch (operand)
                            {
                                case GrammarTokenizerUsage tokenizerUsage:
                                    var node = new TokenizerNode(tokenizerUsage);
                                    _currentTail.AddChild(node);
                                    tailNodes.Add(node);
                                    headNodes.Add(node);
                                    if (tokenizerUsage.IsOptional == false) nonOptionalHeadNodes.Add(node);
                                    break;

                                case GrammarPartUsage partUsage:
                                    var nonOptionalInPartFound = false;
                                    var info = ProcessPartAndGetNodesInfo(partUsage, currentNodes =>
                                    {
                                        if (nonOptionalInPartFound || currentNodes.All(x => x.IsOptional)) return;

                                        nonOptionalHeadNodes.AddRange(currentNodes);
                                        nonOptionalInPartFound = true;

                                        // BUG: here's a problem:
                                        _listeners.HandleNonOptionalNodes(nonOptionalHeadNodes);
                                        // Execution of part processing should stop for a while here and
                                        // `_listeners.HandleNonOptionalNodes(nonOptionalHeadNodes);` should be executed.
                                        // Otherwise `HandleNonOptionalNodes` might (and will in some cases) be called
                                        // from other places.
                                        // Unfortunately `HandleNonOptionalNodes` cannot be simply called here because
                                        // it needs to know all first non optional heads in `or` operands 
                                        // (juts imagine `$SomeOperand|$OtherOperand|$DamnOperand`, each part may have non
                                        // optional at different index, it simply needs to be stoped once found).
                                    });

                                    headNodes.AddRange(info.Head);
                                    bodyNodes.AddRange(info.Body);
                                    tailNodes.AddRange(info.Tail);
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }

                        // TODO: not sure if it will be still needed once part operand handling will be fixed
                        if (part.IsOrConditionOperand == false)
                        {
                            // if it is an or condition operand then non optional nodes stuff will be handled by the caller
                            _listeners.HandleNonOptionalNodes(nonOptionalHeadNodes);
                        }

                        context.IsInOptionalScope = false;

                        // if part has only 1 element then is in the headNodes and tailNodes are empty
                        _currentTail = tailNodes.Any() ? tailNodes : headNodes;

                        yield return headNodes;
                        foreach (var bodyNode in bodyNodes)
                        {
                            yield return bodyNode;
                        }
                        yield return tailNodes;

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

            /// <summary>
            /// Creates a new Nodes list with multiple values added.
            /// </summary>
            public Nodes(IEnumerable<Node> nodes)
            {
                AddRange(nodes);
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