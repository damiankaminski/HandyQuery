using System;
using System.Collections.Generic;
using HandyQuery.Language.Lexing.Grammar.Structure;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing.Graph.Builder.Node
{
    internal sealed class TokenizerNode : BuilderNodeBase
    {
        public readonly ITokenizer Tokenizer;
        public override BuilderNodeType NodeType { get; } = BuilderNodeType.Tokenizer;

        public TokenizerNode(GrammarTokenizerUsage tokenizerUsage) : base(tokenizerUsage?.IsOptional ?? false)
        {
            Tokenizer = tokenizerUsage?.Impl;
        }

        public override bool Equals(BuilderNodeBase nodeBase, HashSet<BuilderNodeBase> visitedNodes = null)
        {
            if (nodeBase.NodeType != BuilderNodeType.Tokenizer)
            {
                return false;
            }

            var node = (TokenizerNode)nodeBase;

            if (node.Tokenizer?.GetType() != Tokenizer?.GetType())
            {
                return false;
            }

            return base.Equals(node, visitedNodes);
        }

        public override string ToString()
        {
            var optional = IsOptional ? "?" : "";
            var name = Tokenizer?.GetType().Name;
            name = name?.Substring(0, name.LastIndexOf("Tokenizer", StringComparison.Ordinal)) ?? "ROOT";
            return $"{optional}{name}";
        }
    }
}