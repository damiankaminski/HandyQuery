﻿using System;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class RootNode : Node
    {
        public override BuilderNodeType NodeType { get; } = BuilderNodeType.Root;

        public RootNode()
        {
        }

        public RootNode AddChild(Node child)
        {
            AddChildImpl(child);
            return this;
        }

        public override string ToString()
        {
            return "ROOT";
        }
    }
}