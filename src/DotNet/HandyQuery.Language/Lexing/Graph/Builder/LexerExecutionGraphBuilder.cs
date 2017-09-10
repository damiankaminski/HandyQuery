using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class LexerExecutionGraphBuilder
    {
        public RootNode BuildGraph(GrammarReturn grammarRoot)
        {
            var root = new RootNode();

            var context = new PartContext(grammarRoot.PartUsage, null)
            {
                CurrentTail = root
            };
            
            using (var enumerator = ProcessPart(context).GetEnumerator())
                while (enumerator.MoveNext())
                {
                }

            CreateOptionalEdges(root);

            return root;
        }

        private static void CreateOptionalEdges(RootNode root)
        {
            var optionalNodes = new Stack<Node>(root.FindFirstOptionalChildInAllChildBranches());
            if (optionalNodes.Any() == false)
            {
                return;
            }

            var optionalNode = optionalNodes.Pop();
            while (optionalNode != null)
            {
                var from = optionalNode.FindFirstNonOptionalParentInAllParentBranches().ToList();
                var to = optionalNode.FindFirstNonOptionalChildInAllChildBranches().ToList();
                MakeOptionalEdge(new Nodes(from), new Nodes(to));

                foreach (var node in to)
                {
                    var nextOptionalNodes = node.FindFirstOptionalChildInAllChildBranches();
                    foreach (var nextOptionalNode in nextOptionalNodes)
                    {
                        optionalNodes.Push(nextOptionalNode);
                    }
                }

                optionalNode = optionalNodes.Any() ? optionalNodes.Pop() : null;
            }
        }

        private (Nodes Head, IEnumerable<Nodes> Body, Nodes Tail) ProcessPartAndGetNodesInfo(GrammarPartUsage part, 
            PartContext parentContext, Action<Nodes> forEachAction)
        {
            using (var enumerator = ProcessPart(new PartContext(part, parentContext)).GetEnumerator())
            {
                var tail = new Nodes();
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

        private IEnumerable<Nodes> ProcessPart(PartContext context)
        {
            var contextStack = new Stack<PartContext>();

            while (true)
            {
                if (context.MoveNext() == false)
                {
                    if (contextStack.Any() == false)
                    {
                        break;
                    }

                    var parentContext = contextStack.Pop();
                    
                    // on part processing finish current tail gets changed in parent context
                    // after all part created new items 
                    parentContext.CurrentTail = context.CurrentTail;
                    
                    context.OnSwitchToParent(parentContext);
                    context = parentContext;
                    continue;
                }

                var entryContext = context;

                switch (context.CurrentBodyItem)
                {
                    case GrammarTokenizerUsage tokenizerUsage:
                    {
                        var node = new TokenizerNode(tokenizerUsage);
                        context.CurrentTail.AddChild(node);
                        context.CurrentTail = node;
                        yield return node;
                        break;
                    }

                    case GrammarPartUsage partUsage:
                    {
                        var contextCandidate = new PartContext(partUsage, context);
                        if (contextCandidate.IsCyclic)
                        {
                            var parentTail = contextStack.Peek().CurrentTail;
                            context.CurrentTail.AddChild(parentTail);
                            context.CurrentTail = parentTail;
                            break;
                        }
                        
                        if (partUsage.IsOptional)
                        {
                            var latestNonOptional = new List<Node>();
                            foreach (var tailNode in context.CurrentTail)
                            {
                                if (tailNode.IsOptional == false)
                                {
                                    latestNonOptional.Add(tailNode);
                                    continue;
                                }

                                latestNonOptional.AddRange(tailNode.FindFirstNonOptionalParentInAllParentBranches());
                            }

                            context.OnNextNonOptionalInThisOrAnyParentPart += nonOptional =>
                            {
                                MakeOptionalEdge(new Nodes(latestNonOptional), nonOptional);
                            };
                        }
                        
                        contextStack.Push(context);
                        context = contextCandidate;
                        break;
                    }

                    case GrammarOrCondition orCondition:
                    {
                        var initialTail = new Nodes(context.CurrentTail);

                        var headNodes = new Nodes();
                        var bodyNodes = new List<Nodes>();
                        var tailNodes = new Nodes();

                        foreach (var operand in orCondition.Operands)
                        {
                            context.CurrentTail = initialTail;

                            switch (operand)
                            {
                                case GrammarTokenizerUsage tokenizerUsage:
                                    var node = new TokenizerNode(tokenizerUsage);
                                    context.CurrentTail.AddChild(node);
                                    tailNodes.Add(node);
                                    headNodes.Add(node);
                                    break;

                                case GrammarPartUsage partUsage:
                                    // TODO: use without lambda
                                    var info = ProcessPartAndGetNodesInfo(partUsage, context, currentNodes => { });

                                    headNodes.AddRange(info.Head);
                                    bodyNodes.AddRange(info.Body);
                                    tailNodes.AddRange(info.Tail);
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }

                        // if part has only 1 element then is in the headNodes and tailNodes are empty
                        context.CurrentTail = tailNodes.Any() ? tailNodes : headNodes;

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

                entryContext.AfterCurrentBodyItemProcessing(context.CurrentTail);
            }
        }

        private sealed class PartContext
        {
            public event Action<Nodes> OnNextNonOptionalInThisOrAnyParentPart;
            public IGrammarBodyItem CurrentBodyItem => PartUsage.Impl.Body[CurrentBodyItemIndex];

            private GrammarPartUsage PartUsage { get; }
            private int CurrentBodyItemIndex { get; set; } = -1;

            private readonly PartContext _parentContext;
            
            private Nodes _currentTail;
            public Nodes CurrentTail
            {
                get
                {
                    var current = this;
                    while (current != null)
                    {
                        if (current._currentTail != null)
                        {
                            return current._currentTail;
                        }
                        current = current._parentContext;
                    }

                    throw new InvalidOperationException();
                }

                set => _currentTail = value;
            }

            public bool IsCyclic
            {
                get
                {
                    // this is very dummy implementation, could be easily broken
                    // when one part is used multiple times in different parts

                    if (_parentContext == null)
                    {
                        return false;
                    }

                    var parts = new HashSet<GrammarPartUsage>();
                    var current = this;
                    while (current != null)
                    {
                        if (parts.Add(current.PartUsage) == false)
                        {
                            return true;
                        }
                        current = current._parentContext;
                    }

                    return false;
                }
            }

            public PartContext(GrammarPartUsage partUsage, PartContext parentContext)
            {
                _parentContext = parentContext;
                PartUsage = partUsage;
            }

            public bool MoveNext()
            {
                if (CurrentBodyItemIndex + 1 >= PartUsage.Impl.Body.Count) return false;

                CurrentBodyItemIndex++;
                return true;
            }

            public void AfterCurrentBodyItemProcessing(Nodes currentTail)
            {
                if (CurrentBodyItem.IsOptional) return;
                OnNextNonOptionalInThisOrAnyParentPart?.Invoke(currentTail);
                OnNextNonOptionalInThisOrAnyParentPart = null;
            }

            public void OnSwitchToParent(PartContext parent)
            {
                if (OnNextNonOptionalInThisOrAnyParentPart != null)
                {
                    parent.OnNextNonOptionalInThisOrAnyParentPart += OnNextNonOptionalInThisOrAnyParentPart;
                    OnNextNonOptionalInThisOrAnyParentPart = null;
                }
            }

            public override string ToString()
            {
                return PartUsage.ToString();
            }
        }

        private static void MakeOptionalEdge(Nodes from, Nodes to)
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

            public void AddChild(Nodes nodes)
            {
                foreach (var node in nodes)
                {
                    AddChild(node);
                }
            }

            public static implicit operator Nodes(Node node)
            {
                return new Nodes(node);
            }
        }
    }
}