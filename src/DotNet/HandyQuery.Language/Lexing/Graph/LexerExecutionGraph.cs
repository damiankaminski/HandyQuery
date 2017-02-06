using System;
using System.Collections.Generic;
using System.Linq;
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
                public Node[] EntryNodes { get; set; }
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
                // TODO: generate additional edge if `tokenizerUsage.IsOptional`
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
                // TODO: generate additional edge if `partUsage.IsOptional`

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

                var context = new PartContext()
                {
                    Usage = partUsage
                };
                _partsUsageStack.Push(context);

                var newParents = parents;
                for (var i = 0; i < partUsage.Impl.Body.Count; i++)
                {
                    var item = partUsage.Impl.Body[i];
                    newParents = Visit(item, newParents).LeaveNodes;

                    if (i == 0) context.EntryNodes = newParents;
                }

                _partsUsageStack.Pop();
                return new VisitResult(newParents);
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