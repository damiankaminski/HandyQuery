using System.Collections.Generic;
using HandyQuery.Language.Extensions;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Graph
{
    internal sealed class Node
    {
        public readonly GrammarTokenizerUsage Item;
        public readonly List<Node> Children = new List<Node>(10);

        public Node(GrammarTokenizerUsage item)
        {
            Item = item;
        }

        public void AddAsChildTo(IEnumerable<Node> parents)
        {
            if (parents != null)
            {
                foreach (var parent in parents)
                {
                    parent?.AddChild(this);
                }
            }
        }

        public Node AddChild(Node child)
        {
            Children.Add(child);
            return this;
        }

        public Node AddChildren(Node[] children)
        {
            Children.AddRange(children);
            return this;
        }

        public bool Equals(Node node, HashSet<Node> visitedNodes = null)
        {
            if (node.Item?.Equals(Item) == false)
            {
                return false;
            }

            visitedNodes = visitedNodes ?? new HashSet<Node>();
            if (visitedNodes.Contains(node)) return true;
            visitedNodes.Add(node);

            if (AreSame(node.Children, Children, visitedNodes) == false)
            {
                return false;
            }

            return true;
        }

        private static bool AreSame(List<Node> items, List<Node> items2, HashSet<Node> visitedNodes)
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
            return Item?.Name ?? "Root";
        }
    }
}