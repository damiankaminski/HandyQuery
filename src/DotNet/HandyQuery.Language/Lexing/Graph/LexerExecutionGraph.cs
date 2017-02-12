using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Extensions;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Graph
{
    internal sealed class LexerExecutionGraph
    {
        internal readonly Node Root;

        private HashSet<Node> _getAllChildrenVisitedNodes;

        internal LexerExecutionGraph(Node root)
        {
            Root = root;
        }

        public static LexerExecutionGraph Build(GrammarPart grammarRoot)
        {
            var builder = new Builder();
            builder.BuildGraph(grammarRoot);

            return new LexerExecutionGraph(builder.Root);
        }

        public bool Equals(LexerExecutionGraph expected)
        {
            return Root.Equals(expected.Root) &&
                GetTotalChildrenInstances(Root) == GetTotalChildrenInstances(expected.Root);
        }

        private int GetTotalChildrenInstances(Node node)
        {
            _getAllChildrenVisitedNodes = new HashSet<Node>();
            return GetAllChildren(node).Count;
        }

        private HashSet<Node> GetAllChildren(Node node)
        {
            if (_getAllChildrenVisitedNodes.Contains(node))
            {
                return new HashSet<Node>();
            }

            _getAllChildrenVisitedNodes.Add(node);
            var all = new HashSet<Node>();

            foreach (var nodeChild in node.Children)
            {
                all.Add(nodeChild);
                var childrenOfChild = GetAllChildren(nodeChild);
                foreach (var childOfChild in childrenOfChild)
                {
                    all.Add(childOfChild);
                }
            }
            return all;
        }

        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable ParameterTypeCanBeEnumerable.Local
        private sealed class Builder
        {
            public readonly Node Root = new Node(null);

            private readonly Stack<PartContext> _partsUsageStack = new Stack<PartContext>();

            private sealed class PartContext
            {
                public GrammarPartUsage Usage { get; set; }
                public Node[] EntryNodes => BodyItemsNodes.Count > 0 ? BodyItemsNodes[0] : null;
                public Node[] LeaveNodes => BodyItemsNodes.Count > 0 ? BodyItemsNodes[BodyItemsNodes.Count - 1] : null;
                public List<Node[]> BodyItemsNodes { get; set; } = new List<Node[]>();
            }

            public sealed class VisitResult
            {
                public readonly Node[] LeaveNodes;

                public VisitResult(Node[] leaveNodes)
                {
                    LeaveNodes = leaveNodes;
                }
            }

            public void BuildGraph(GrammarPart grammarRoot)
            {
                Visit(new GrammarPartUsage(grammarRoot.Name, false, grammarRoot), new[] {Root});
            }
            
            public VisitResult Visit(GrammarTokenizerUsage tokenizerUsage, Node[] parents)
            {
                var node = new Node(tokenizerUsage);
                node.AddAsChildTo(parents);
                return new VisitResult(new[] {node});
            }

            public VisitResult Visit(GrammarOrCondition orCondition, Node[] parents)
            {
                var leaveNodes = new List<Node>();
                foreach (var operand in orCondition.Operands)
                {
                    leaveNodes.AddRange(Visit(operand, parents).LeaveNodes);
                }

                return new VisitResult(leaveNodes.ToArray());
            }

            public VisitResult Visit(GrammarPartUsage partUsage, Node[] parents)
            {
                if (_partsUsageStack.Count > 1)
                {
                    var cycleCandidate = _partsUsageStack.ToArray()
                        .FirstOrDefault(x => ReferenceEquals(x.Usage.Impl, partUsage.Impl));
                    var isCycle = cycleCandidate != null && cycleCandidate.EntryNodes != null;

                    if (isCycle)
                    {
                        // TODO: $FunctionInvokation usage in $FunctionInvokation seems to be bad
                        foreach (var entryNode in cycleCandidate.EntryNodes)
                        {
                            entryNode.AddAsChildTo(parents);
                        }

                        return new VisitResult(cycleCandidate.EntryNodes);
                    }
                }

                var currentPartContext = new PartContext()
                {
                    Usage = partUsage
                };
                _partsUsageStack.Push(currentPartContext);

                Node[] lastNonOptional = null;
                var optionalBodyItemsLeaveNodes = new List<Node>();
                var nodes = parents;
                var body = partUsage.Impl.Body;
                for (var i = 0; i < body.Count; i++)
                {
                    var item = body[i];

                    var prev = i == 0 ? null : body[i-1];
                    if (item.IsOptional && (prev == null || prev.IsOptional == false))
                    {
                        // If current item is optional and previous item was not optional (if there is
                        // no previous then it couldn't be optional) then save previous nodes to create a new edge.
                        // It skip all optional nodes and land in not optional.
                        lastNonOptional = nodes;
                    }

                    var visitResult = Visit(item, nodes);
                    nodes = visitResult.LeaveNodes;

                    if (item.Type == GrammarElementType.PartUsage && item.IsOptional)
                    {
                        // if item is a part usage and it is optional then all nodes of this part are considered to be optional as well
                        // TODO: optionalBodyItemsLeaveNodes.AddRange(visitResult.AllNodes);
                    }
                    else
                    {
                        optionalBodyItemsLeaveNodes.AddRange(nodes.Where(x => x.Item != null && x.Item.IsOptional));
                    }

                    if (nodes.Any(x => x.Item == null || x.Item.IsOptional) == false && optionalBodyItemsLeaveNodes.Any())
                    {
                        // if visited nodes are not optional and there are some optional elements left
                        // to be processed

                        var nonOptionalParents = optionalBodyItemsLeaveNodes
                            .Select(x => x.FindFirstNonOptionalParentInAllParentBranches())
                            .SelectMany(x => x)
                            .WithoutDuplicates()
                            .ToArray();

                        foreach (var nonOptionalParent in nonOptionalParents)
                        {
                            nonOptionalParent.AddChildren(nodes);
                        }

                        optionalBodyItemsLeaveNodes = new List<Node>();
                    }

                    /*if (lastNonOptional != null && item.IsOptional == false)
                    {
                        // Create an edge which will skip the optional elements
                        foreach (var node in lastNonOptional)
                        {
                            node.AddChildren(nodes);
                        }
                        lastNonOptional = null;
                    }*/

                    currentPartContext.BodyItemsNodes.Add(nodes);
                }

                _partsUsageStack.Pop();

                var prevPartContext = _partsUsageStack.Any() ? _partsUsageStack.Peek() : null;
                if (prevPartContext?.LeaveNodes != null)
                {
                    // TODO: not sure if it is needed, describe if it is
                   
                    var prevPartOptionalLeaveNodes = prevPartContext.LeaveNodes.Where(x => x.Item != null && x.Item.IsOptional).ToArray();
                    var nonOptionalEntryNodes =
                        currentPartContext.EntryNodes
                            .Select(x => x.FindFirstNonOptionalChildInAllChildBranches())
                            .SelectMany(x => x)
                            .ToArray();

                    foreach (var prevPartOptionalLeaveNode in prevPartOptionalLeaveNodes)
                    {
                        foreach (var firstNonOptionalInPrevPart in prevPartOptionalLeaveNode.FindFirstNonOptionalParentInAllParentBranches())
                        {
                            firstNonOptionalInPrevPart.AddChildren(nonOptionalEntryNodes);
                        }
                    }
                }

                return new VisitResult(nodes);
            }

            private VisitResult Visit(IGrammarElement any, Node[] parents)
            {
                var method = GetType().GetMethod(nameof(Visit), new[] {any.GetType(), parents.GetType()});
                if (method == null || method.ReturnType != typeof(VisitResult))
                {
                    throw new ArgumentOutOfRangeException($"Grammar element of type {any.GetType().FullName} is not supported by graph builder.");
                }

                return (VisitResult) method.Invoke(this, new object[] {any, parents});
            }
        }
        // ReSharper restore MemberCanBePrivate.Local
        // ReSharper restore ParameterTypeCanBeEnumerable.Local
    }
}