using System.Collections.Generic;

namespace HandyQuery.Language.Lexing.Graph.Builder.Node
{
    internal sealed class PartNode : BuilderNodeBase
    {
        public override BuilderNodeType NodeType { get; } = BuilderNodeType.Part;
        public readonly HashSet<BuilderNodeBase> EntryNodes = new HashSet<BuilderNodeBase>();

        public PartNode(bool isOptional) : base(isOptional)
        {
        }
    }
}