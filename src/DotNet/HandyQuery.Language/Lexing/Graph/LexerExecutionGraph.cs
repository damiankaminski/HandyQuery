using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using HandyQuery.Language.Lexing.Gramma.Structure;

namespace HandyQuery.Language.Lexing.Graph
{
    internal sealed class LexerExecutionGraph
    {
        private readonly Node _root;

        private LexerExecutionGraph(Node root)
        {
            _root = root;
        }

        public static LexerExecutionGraph Build(GrammaPart grammaRoot)
        {
            var graph = BuildGraph(grammaRoot, null).ToArray();

            if (graph.Length != 1)
            {
                throw new LexerExecutionGraphException("Graph should have a root.");
            }

            return new LexerExecutionGraph(graph[0]);
        }

        private static IEnumerable<Node> BuildGraph(IGrammaElement grammaElement, Node[] parents)
        {
            // TODO: detect and handle cycles (e.g. $Params = $Value ?$MoreParams \n $MoreParams = ParamsSeparator $Params)
            // maybe arrays should be declared explicitly, e.g.
            // $Params = $Value ?$MoreParams[]
            // $MoreParams = ParamsSeparator $Params

            switch (grammaElement.Type)
            {
                case GrammaElementType.Part:
                    var part = grammaElement.As<GrammaPart>();
                    var root = new Node(null, null);
                    BuildGraphFromPartBody(new [] { root }, part.Body);
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

        private static IEnumerable<Node> BuildGraphFromPartBody(IEnumerable<Node> parents, GrammaPartBody body)
        {
            var itemParents = parents.ToArray();
            foreach (var item in body)
            {
                itemParents = BuildGraph(item, itemParents).ToArray();
            }

            return itemParents;
        }
    }

    internal sealed class Node
    {
        public readonly IGrammaBodyItem Item;
        public readonly List<Node> Children = new List<Node>(10);

        public Node(IGrammaBodyItem item, IEnumerable<Node> parents)
        {
            Item = item;

            if (parents != null)
            {
                foreach (var parent in parents)
                {
                    parent?.AddChildNode(this);
                }
            }
        }

        public void AddChildNode(Node child)
        {
            Children.Add(child);
        }

        public override string ToString()
        {
            var name = Item?.Name ?? "Root";
            return $"{name} ({string.Join(",", Children.Select(x => x.Item?.Name))})";
        }
    }
}