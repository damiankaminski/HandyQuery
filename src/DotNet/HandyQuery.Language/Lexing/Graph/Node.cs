using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Graph
{
    internal sealed class Node
    {
        public readonly GrammarTokenizerUsage Item;
        public readonly List<Node> Children = new List<Node>(3);
        public readonly List<Node> Parents = new List<Node>(1);

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
            child.AddParent(this);
            return this;
        }

        public Node AddChildren(Node[] children)
        {
            Children.AddRange(children);
            foreach (var child in children)
            {
                child.AddParent(this);
            }
            return this;
        }

        private void AddParent(Node parent)
        {
            Parents.Add(parent);
        }

        /// <summary>
        /// Finds first non optional parent in all parent branches (single node may have multiple parents).
        /// </summary>
        public IEnumerable<Node> FindFirstNonOptionalParentInAllParentBranches()
        {
            foreach (var parent in Parents)
            {
                // if root or not optional
                if (parent.Item == null || parent.Item.IsOptional == false)
                {
                    yield return parent;
                    continue;
                }

                foreach (var nonOptionalParent in parent.FindFirstNonOptionalParentInAllParentBranches().ToArray())
                {
                    yield return nonOptionalParent;
                }
            }
        }

        /// <summary>
        /// Finds first non optional child in all child branches (single node may have multiple children).
        /// </summary>
        public IEnumerable<Node> FindFirstNonOptionalChildInAllChildBranches()
        {
            foreach (var child in Children)
            {
                // if root or not optional
                if (child.Item == null || child.Item.IsOptional == false)
                {
                    yield return child;
                    continue;
                }

                foreach (var nonOptionalChild in child.FindFirstNonOptionalChildInAllChildBranches().ToArray())
                {
                    yield return nonOptionalChild;
                }
            }
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