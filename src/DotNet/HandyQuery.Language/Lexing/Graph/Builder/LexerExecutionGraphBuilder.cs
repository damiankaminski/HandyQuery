using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class LexerExecutionGraphBuilder
    {
        private Nodes _currentTail;

        public RootNode BuildGraph(GrammarReturn grammarRoot)
        {
            var root = new RootNode();

            _currentTail = new Nodes(root);
            using (var enumerator = ProcessPart(grammarRoot.PartUsage).GetEnumerator())
            while (enumerator.MoveNext()) { }

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

            var context = new PartContext(part);

            while (true)
            {
                if (context.MoveNext() == false)
                {
                    if (contextStack.Any() == false)
                    {
                        break;
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
                                    break;

                                case GrammarPartUsage partUsage:
                                    // TODO: use without lambda
                                    var info = ProcessPartAndGetNodesInfo(partUsage, currentNodes =>
                                    {
                                    });

                                    headNodes.AddRange(info.Head);
                                    bodyNodes.AddRange(info.Body);
                                    tailNodes.AddRange(info.Tail);
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }

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

            public GrammarPartUsage PartUsage { get; }

            private int CurrentBodyItemIndex { get; set; } = -1;

            public bool MoveNext()
            {
                if (CurrentBodyItemIndex + 1 >= PartUsage.Impl.Body.Count) return false;

                CurrentBodyItemIndex++;
                return true;
            }
        }
    }
}