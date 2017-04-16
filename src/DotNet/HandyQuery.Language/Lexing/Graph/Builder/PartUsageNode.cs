using System.Collections.Generic;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class PartUsageNode : Node
    {
        public override BuilderNodeType NodeType { get; } = BuilderNodeType.Part;
        public List<Node> EntryNodes = new List<Node>();

        public PartUsageNode(bool isOptional) : base(isOptional)
        {
        }
    }
}