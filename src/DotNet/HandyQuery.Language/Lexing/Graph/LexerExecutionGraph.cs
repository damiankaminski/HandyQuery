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
            var graph = new Builder().BuildGraph(grammaRoot, null).ToArray();

            if (graph.Length != 1)
            {
                throw new LexerExecutionGraphException("Graph should have a root.");
            }

            return new LexerExecutionGraph(graph[0]);
        }

        internal sealed class Builder
        {
            private readonly List<IGrammaElement> _visitedElements = new List<IGrammaElement>();

            public IEnumerable<Node> BuildGraph(IGrammaElement grammaElement, Node[] parents)
            {
                // TODO: detect and handle cycles (e.g. $Params = $Value ?$MoreParams \n $MoreParams = ParamsSeparator $Params)
                // maybe arrays should be declared explicitly, e.g.
                // $Params = $Value ?$MoreParams[]
                // $MoreParams = ParamsSeparator $Params

                if (_visitedElements.Contains(grammaElement))
                {
                    yield break;
                }

                _visitedElements.Add(grammaElement);

                switch (grammaElement.Type)
                {
                    case GrammaElementType.Part:
                        var part = grammaElement.As<GrammaPart>();
                        var root = new Node(null, null);
                        BuildGraphFromPartBody(new[] { root }, part.Body);
                        yield return root;
                        break;

                    case GrammaElementType.PartUsage:
                        var partUsage = grammaElement.As<GrammaPartUsage>();
                        // TODO: generate additional route if `partUsage.IsOptional`
                        foreach (var node in BuildGraphFromPartBody(parents, partUsage.Impl.Body).ToArray())
                        {
                            yield return node;
                        }
                        break;

                    case GrammaElementType.TokenizerUsage:
                        var tokenizerUsage = grammaElement.As<GrammaTokenizerUsage>();
                        // TODO: generate additional route if `tokenizerUsage.IsOptional`
                        yield return new Node(tokenizerUsage, parents);
                        break;

                    case GrammaElementType.OrCondition:
                        var orCondition = grammaElement.As<GrammaOrCondition>();
                        foreach (var operand in orCondition.Operands)
                        {
                            foreach (var node in BuildGraph(operand, parents).ToArray())
                            {
                                yield return node;
                            }
                        }
                        break;

                    default:
                        throw new LexerExecutionGraphException("Cannot process gramma.");
                }
            }

            private IEnumerable<Node> BuildGraphFromPartBody(IEnumerable<Node> parents, GrammaPartBody body)
            {
                var itemParents = parents.ToArray();
                foreach (var item in body)
                {
                    itemParents = BuildGraph(item, itemParents).ToArray();
                }

                return itemParents;
            }
        }
    }
}