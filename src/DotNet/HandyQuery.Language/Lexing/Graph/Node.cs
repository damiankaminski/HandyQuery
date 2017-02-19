﻿using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing.Graph
{
    internal sealed class Node
    {
        public readonly ITokenizer Tokenizer;
        public readonly bool IsOptional;
        public readonly IEnumerable<Node> Children;
        public readonly IEnumerable<Node> Parents;

        public Node(ITokenizer tokenizer, bool isOptional, HashSet<Node> children, HashSet<Node> parents)
        {
            Tokenizer = tokenizer;
            IsOptional = isOptional;
            Children = children;
            Parents = parents;
        }

        public override string ToString()
        {
            return Tokenizer?.GetType().Name ?? "Root";
        }
    }
}