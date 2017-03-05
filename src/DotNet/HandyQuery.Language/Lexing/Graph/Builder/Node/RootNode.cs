using System;
using System.Collections.Generic;
using System.Linq;

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
            return new FinalNodeBuilder().Build(this);
        }

        private sealed class FinalNodeBuilder
        {
            private readonly Dictionary<BuilderNodeBase, Graph.Node> _createdNodes 
                = new Dictionary<BuilderNodeBase, Graph.Node>();

            public Graph.Node Build(RootNode builderNode)
            {
                var root = new Graph.Node();
                foreach (var child in builderNode.Children)
                {
                    Process(child, ref root);
                }
                return root;
            }

            private Graph.Node Process(BuilderNodeBase builderNode, ref Graph.Node parent)
            {
                switch (builderNode.NodeType)
                {
                    case BuilderNodeType.Part:
                        var partNode = (PartUsageNode)builderNode;
                        foreach (var entryNode in partNode.EntryNodes)
                        {
                            // build nodes inside part
                            var leaveNode = Process(entryNode, ref parent);

                            foreach (var x in entryNode.GetDeepChildren())
                            {
                                leaveNode = Process(x, ref leaveNode);
                            }

                            foreach (var child in partNode.Children)
                            {
                                // build outside of part
                                Process(child, ref leaveNode);
                            }
                        }

                        return null;

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

                        parent = parent.WithChild(node);

                        return node;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}