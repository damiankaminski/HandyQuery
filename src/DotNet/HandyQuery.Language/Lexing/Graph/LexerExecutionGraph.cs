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
            private readonly List<IGrammaElement> _visitedElements = new List<IGrammaElement>();

            public readonly Node Root = new Node(null);

            public void BuildGraph(GrammaPart grammaRoot)
            {
                foreach (var item in grammaRoot.Body)
                {
                    Process(item, new[] {Root});
                }
            }

            public void Process(IGrammaBodyItem grammaElement, Node[] parents)
            {
                switch (grammaElement.Type)
                {
                    case GrammaElementType.TokenizerUsage:
                        ProcessGraphPart(grammaElement, parents).ToArray();
                        break;

                    case GrammaElementType.PartUsage:
                        ProcessGraphPart(grammaElement, parents).ToArray();
                        break;

                    case GrammaElementType.OrCondition:
                        var orCondition = grammaElement.As<GrammaOrCondition>();
                        foreach (var operand in orCondition.Operands)
                        {
                            ProcessGraphPart(operand, parents).ToArray();
                        }
                        break;

                    default:
                        throw new LexerExecutionGraphException("Cannot process gramma.");
                }
            }

            private IEnumerable<Node> ProcessGraphPart(IGrammaBodyItem grammaElement, Node[] parents)
            {
                if (_visitedElements.Contains(grammaElement))
                {
                    yield break;
                }

                _visitedElements.Add(grammaElement);

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
                        var partUsage = grammaElement.As<GrammaPartUsage>();
                        // TODO: generate additional route if `partUsage.IsOptional`
                        foreach (var item in partUsage.Impl.Body)
                        {
                            foreach (var n in ProcessGraphPart(item, parents))
                            {
                                yield return n;
                            }
                        }
                        break;

                    default:
                        throw new LexerExecutionGraphException("Cannot process gramma.");
                }
            }
        }
    }
}