using System;
using System.Collections.Generic;
using HandyQuery.Language.Lexing.Grammar.Structure;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class TerminalNode : Node
    {
        public readonly ITokenizer Tokenizer;
        public override BuilderNodeType NodeType { get; } = BuilderNodeType.Terminal;

        public TerminalNode(GrammarTerminalUsage terminalUsage)
        {
            Tokenizer = terminalUsage?.Impl;
        }

        public TerminalNode AddChild(Node child)
        {
            AddChildImpl(child);
            return this;
        }

        public override bool Equals(Node nodeBase, HashSet<Node> visitedNodes = null)
        {
            if (nodeBase.NodeType != BuilderNodeType.Terminal)
            {
                return false;
            }

            var node = (TerminalNode)nodeBase;

            if (node.Tokenizer?.GetType() != Tokenizer?.GetType())
            {
                return false;
            }

            return base.Equals(node, visitedNodes);
        }

        public override string ToString()
        {
            var name = Tokenizer?.GetType().Name;
            name = name?.Substring(0, name.LastIndexOf("Terminal", StringComparison.Ordinal)) ?? "ROOT";
            return name;
        }
    }
}