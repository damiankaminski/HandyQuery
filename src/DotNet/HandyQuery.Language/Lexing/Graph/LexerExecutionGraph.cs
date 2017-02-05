using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Graph
{
    internal sealed class LexerExecutionGraph
    {
        internal readonly Node Root;

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

        public bool Equals(LexerExecutionGraph graph)
        {
            return Root.Equals(graph.Root);
        }

        private sealed class Builder
        {
            public readonly Node Root = new Node(null);

            private readonly Stack<PartContext> _partsUsageStack = new Stack<PartContext>();

            private class PartContext
            {
                public GrammarPartUsage Usage { get; set; }
                public Node[] EntryNodes { get; set; }
            }

            public void BuildGraph(GrammarPart grammarRoot)
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                ProcessGraphPart(new GrammarPartUsage(grammarRoot.Name, false, grammarRoot), new[] {Root}).ToArray();
            }

            // TODO: get rid of yield as it may mess up order of execution, return simply an array
            private IEnumerable<Node> ProcessGraphPart(IGrammarBodyItem grammarElement, Node[] parents)
            {
                switch (grammarElement.Type)
                {
                    case GrammarElementType.TokenizerUsage:
                        var tokenizerUsage = grammarElement.As<GrammarTokenizerUsage>();
                        // TODO: generate additional route if `tokenizerUsage.IsOptional`
                        var node = new Node(tokenizerUsage);
                        node.AddAsChildTo(parents);

                        yield return node;
                        break;

                    case GrammarElementType.PartUsage:
                        foreach (var n in ProcessPartUsage(grammarElement, parents).ToArray()) yield return n;
                        break;

                    case GrammarElementType.OrCondition:
                        var orCondition = grammarElement.As<GrammarOrCondition>();

                        foreach (var operand in orCondition.Operands)
                        {
                            foreach (var n in ProcessGraphPart(operand, parents).ToArray())
                            {
                                yield return n;
                            }
                        }
                        break;

                    default:
                        throw new LexerExecutionGraphException("Cannot process grammar.");
                }
            }

            // TODO: get rid of yield as it may mess up order of execution, return simply an array
            /// <summary>
            /// Processes a single part (which may invoke other parts) and returns result of last item in the body.
            /// </summary>
            private IEnumerable<Node> ProcessPartUsage(IGrammarBodyItem grammarElement, Node[] parents)
            {
                // TODO: generate additional route if `partUsage.IsOptional`

                var partUsage = grammarElement.As<GrammarPartUsage>();

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
                            yield return entryNode;
                            entryNode.AddAsChildTo(parents);
                        }

                        yield break;
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
                    var isLast = i == partUsage.Impl.Body.Count - 1;
                    var item = partUsage.Impl.Body[i];
                    newParents = ProcessGraphPart(item, newParents).ToArray();

                    if (i == 0) context.EntryNodes = newParents;

                    if (isLast)
                    {
                        foreach (var n in newParents)
                        {
                            yield return n;
                        }
                    }
                }

                _partsUsageStack.Pop();
            }
        }
    }
}