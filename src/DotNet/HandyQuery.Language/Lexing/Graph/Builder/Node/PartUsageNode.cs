using System.Collections.Generic;

namespace HandyQuery.Language.Lexing.Graph.Builder.Node
{
    internal sealed class PartUsageNode : BuilderNodeBase
    {
        public override BuilderNodeType NodeType { get; } = BuilderNodeType.Part;
        public List<BuilderNodeBase> EntryNodes = new List<BuilderNodeBase>();

        public PartUsageNode(bool isOptional) : base(isOptional)
        {
        }
    }
}