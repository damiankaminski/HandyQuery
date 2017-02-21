using System;
using System.Collections.Generic;

namespace HandyQuery.Language.Lexing.Graph.Builder.Node
{
    internal sealed class RootNode : BuilderNodeBase
    {
        public override BuilderNodeType NodeType { get; } = BuilderNodeType.Root;

        public RootNode() : base(false)
        {
        }

        public Graph.Node ConvertToNode()
        {
            Graph.Node empty = null;
            return new FinalNodeBuilder().Build(this, ref empty);
        }

        private sealed class FinalNodeBuilder
        {
            private readonly Dictionary<BuilderNodeBase, Graph.Node> _createdNodes 
                = new Dictionary<BuilderNodeBase, Graph.Node>();

            public Graph.Node Build(BuilderNodeBase builderNode, ref Graph.Node parent)
            {
                switch (builderNode.NodeType)
                {
                    case BuilderNodeType.Part:
                        throw new NotImplementedException();
                    case BuilderNodeType.Tokenizer:
                        var tokenizerNode = (TokenizerNode)builderNode;

                        Graph.Node node;
                        if (_createdNodes.TryGetValue(tokenizerNode, out node) == false)
                        {
                            node = new Graph.Node(
                                tokenizerNode.Tokenizer,
                                tokenizerNode.IsOptional,
                                new HashSet<Graph.Node>(),
                                new HashSet<Graph.Node>());

                            _createdNodes[tokenizerNode] = node;
                        }

                        foreach (var child in tokenizerNode.Children)
                        {
                            Build(child, ref node);
                        }

                        parent = parent.AddChild(node);
                        return node;
                    case BuilderNodeType.Root:
                        var root = new Graph.Node();
                        foreach (var child in builderNode.Children)
                        {
                            Build(child, ref root);
                        }
                        return root;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}