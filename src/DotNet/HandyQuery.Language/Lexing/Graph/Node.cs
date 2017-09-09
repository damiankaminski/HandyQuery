using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Lexing.Graph.Builder;

namespace HandyQuery.Language.Lexing.Graph
{
    // TODO: move to somewhere else (along with node implementations)
    // TODO: get rid of not used methods
    // TODO: order of elements matters so HashSet is not really an option

    internal abstract class Node
    {
        public readonly bool IsOptional;
        public readonly HashSet<Node> Children = new HashSet<Node>();
        public readonly HashSet<Node> Parents = new HashSet<Node>();
        public abstract BuilderNodeType NodeType { get; }

        protected Node(bool isOptional)
        {
            IsOptional = isOptional;
        }

        public void Walk(Func<Node, bool> forEachNode)
        {
            var moveOn = forEachNode(this);
            if (moveOn == false) return;

            foreach (var child in Children)
            {
                child.Walk(forEachNode);
            }
        }

        public void AddParents(IEnumerable<Node> parents)
        {
            if (parents != null)
            {
                foreach (var parent in parents)
                {
                    parent?.AddChildImpl(this);
                }
            }
        }

        internal void AddChildImpl(Node child)
        {
            Children.Add(child);
            child.AddParent(this);
        }

        private Node AddChildren(Node[] children)
        {
            foreach (var child in children)
            {
                Children.Add(child);
                child.AddParent(this);
            }
            return this;
        }

        public IEnumerable<Node> GetDeepChildren()
        {
            foreach (var child in Children)
            {
                yield return child;

                foreach (var node in child.GetDeepChildren())
                {
                    yield return node;
                }
            }
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
            foreach (var parent in Parents.ToArray())
            {
                if (parent.IsOptional == false)
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
            foreach (var child in Children.ToArray())
            {
                if (child.IsOptional == false)
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

        public IEnumerable<Node> FindFirstOptionalChildInAllChildBranches()
        {
            foreach (var child in Children.ToArray())
            {
                if (child.IsOptional)
                {
                    yield return child;
                    continue;
                }

                foreach (var optionalChild in child.FindFirstOptionalChildInAllChildBranches().ToArray())
                {
                    yield return optionalChild;
                }
            }
        }

        public virtual bool Equals(Node node, HashSet<Node> visitedNodes = null)
        {
            if (node.IsOptional != IsOptional)
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

        private static bool AreSame(IEnumerable<Node> items, IEnumerable<Node> items2, HashSet<Node> visitedNodes)
        {
            var i1 = items.ToList();
            var i2 = items2.ToList();

            if (i1.Count != i2.Count)
            {
                return false;
            }

            for (var i = 0; i < i2.Count; i++)
            {
                if (i2[i].Equals(i1[i], visitedNodes) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}