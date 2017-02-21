using System.Collections.Generic;
using System.Linq;

namespace HandyQuery.Language.Lexing.Graph.Builder.Node
{
    // TODO: get rid of not used methods

    internal abstract class BuilderNodeBase
    {
        public readonly bool IsOptional;
        public readonly HashSet<BuilderNodeBase> Children = new HashSet<BuilderNodeBase>();
        public readonly HashSet<BuilderNodeBase> Parents = new HashSet<BuilderNodeBase>();
        public abstract BuilderNodeType NodeType { get; }

        protected BuilderNodeBase(bool isOptional)
        {
            IsOptional = isOptional;
        }

        public void AddParents(IEnumerable<BuilderNodeBase> parents)
        {
            if (parents != null)
            {
                foreach (var parent in parents)
                {
                    parent?.AddChild(this);
                }
            }
        }

        public BuilderNodeBase AddChild(BuilderNodeBase child)
        {
            Children.Add(child);
            child.AddParent(this);
            return this;
        }

        private BuilderNodeBase AddChildren(BuilderNodeBase[] children)
        {
            foreach (var child in children)
            {
                Children.Add(child);
                child.AddParent(this);
            }
            return this;
        }

        private void AddParent(BuilderNodeBase parent)
        {
            Parents.Add(parent);
        }

        /// <summary>
        /// Finds first non optional parent in all parent branches (single node may have multiple parents).
        /// </summary>
        public IEnumerable<BuilderNodeBase> FindFirstNonOptionalParentInAllParentBranches()
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
        public IEnumerable<BuilderNodeBase> FindFirstNonOptionalChildInAllChildBranches()
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

        public virtual bool Equals(BuilderNodeBase node, HashSet<BuilderNodeBase> visitedNodes = null)
        {
            if (node.IsOptional != IsOptional)
            {
                return false;
            }

            visitedNodes = visitedNodes ?? new HashSet<BuilderNodeBase>();
            if (visitedNodes.Contains(node)) return true;
            visitedNodes.Add(node);

            if (AreSame(node.Children, Children, visitedNodes) == false)
            {
                return false;
            }

            return true;
        }

        private static bool AreSame(IEnumerable<BuilderNodeBase> items, IEnumerable<BuilderNodeBase> items2, HashSet<BuilderNodeBase> visitedNodes)
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