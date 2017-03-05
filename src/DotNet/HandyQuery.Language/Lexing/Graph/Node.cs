using System.Collections.Generic;
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

        public Node(ITokenizer tokenizer, bool isOptional, IEnumerable<Node> children, IEnumerable<Node> parents)
        {
            Tokenizer = tokenizer;
            IsOptional = isOptional;
            Children = children;
            Parents = parents;
        }

        public Node()
        {
            Children = new List<Node>();
            Parents = new List<Node>();
        }

        public Node WithChild(Node child)
        {
            var newChildren = new HashSet<Node>(new Node[] {}) { child };
            return new Node(Tokenizer, IsOptional, newChildren, Parents);
        }

        public bool Equals(Node node, HashSet<Node> visitedNodes = null)
        {
            if (node.Tokenizer?.GetType() != Tokenizer?.GetType())
            {
                return false;
            }

            visitedNodes = visitedNodes ?? new HashSet<Node>();
            if (visitedNodes.Contains(node)) return true;
            visitedNodes.Add(node);

            if (AreSame(node.Children.ToList(), Children.ToList(), visitedNodes) == false)
            {
                return false;
            }

            return true;
        }

        private static bool AreSame(IReadOnlyList<Node> items, IReadOnlyList<Node> items2, HashSet<Node> visitedNodes)
        {
            if (items.Count != items2.Count)
            {
                return false;
            }

            for (var i = 0; i < items2.Count; i++)
            {
                if (items2[i].Equals(items[i], visitedNodes) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return Tokenizer?.GetType().Name ?? "Root";
        }
    }
}