using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Lexing.Gramma.Structure;

namespace HandyQuery.Language.Lexing.Graph
{
    internal sealed class LexerExecutionGraph
    {
        internal readonly Node Root;

        private LexerExecutionGraph(Node root)
        {
            Root = root;
        }

        public static LexerExecutionGraph Build(GrammaPart grammaRoot)
        {
            var builder = new Builder();
            builder.BuildGraph(grammaRoot);

            return new LexerExecutionGraph(builder.Root);
        }

        internal sealed class Builder
        {
            public readonly Node Root = new Node(null);

            private readonly Stack<PartContext> _partsUsageStack = new Stack<PartContext>();

            private class PartContext
            {
                public GrammaPartUsage Usage { get; set; }
                public Node[] EntryNodes;
            }

            public void BuildGraph(GrammaPart grammaRoot)
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                ProcessGraphPart(new GrammaPartUsage(grammaRoot.Name, false, grammaRoot), new[] {Root}).ToArray();
            }

            private IEnumerable<Node> ProcessGraphPart(IGrammaBodyItem grammaElement, Node[] parents)
            {
                switch (grammaElement.Type)
                {
                    case GrammaElementType.TokenizerUsage:
                        var tokenizerUsage = grammaElement.As<GrammaTokenizerUsage>();
                        // TODO: generate additional route if `tokenizerUsage.IsOptional`
                        var node = new Node(tokenizerUsage);
                        node.AddAsChildTo(parents);

                        yield return node;
                        break;

                    case GrammaElementType.PartUsage:
                        foreach (var n in ProcessPartUsage(grammaElement, parents)) yield return n;
                        break;

                    case GrammaElementType.OrCondition:
                        var orCondition = grammaElement.As<GrammaOrCondition>();

                        foreach (var operand in orCondition.Operands)
                        {
                            foreach (var n in ProcessGraphPart(operand, parents))
                            {
                                yield return n;
                            }
                        }
                        break;

                    default:
                        throw new LexerExecutionGraphException("Cannot process gramma.");
                }
            }

            private IEnumerable<Node> ProcessPartUsage(IGrammaBodyItem grammaElement, Node[] parents)
            {
                // TODO: generate additional route if `partUsage.IsOptional`

                var partUsage = grammaElement.As<GrammaPartUsage>();

                if (_partsUsageStack.Count > 1)
                {
                    var cycleCandidate = _partsUsageStack.ToArray()
                        .FirstOrDefault(x => ReferenceEquals(x.Usage.Impl, partUsage.Impl));
                    var isCycle = cycleCandidate != null && cycleCandidate.EntryNodes != null;

                    if (isCycle)
                    {
                        foreach (var entryNode in cycleCandidate.EntryNodes)
                        {
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
                    var item = partUsage.Impl.Body[i];
                    newParents = ProcessGraphPart(item, newParents).ToArray();

                    if (i == 0) context.EntryNodes = newParents;

                    foreach (var n in newParents)
                    {
                        yield return n;
                    }
                }

                _partsUsageStack.Pop();
            }
        }
    }
}