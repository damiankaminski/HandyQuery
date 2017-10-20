using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Lexing.Graph.Builder;

namespace HandyQuery.Language.Lexing.Graph
{
    // TODO: move to somewhere else (along with node implementations)
    // TODO: get rid of not used methods
    // TODO: order of elements matters so HashSet is not really an option
    // TODO: use NodesCollection instead of List<Node> ?

    internal abstract class Node
    {
        public readonly List<Node> Children = new List<Node>();
        public readonly List<Node> Parents = new List<Node>();
        public abstract BuilderNodeType NodeType { get; }

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

        public virtual bool Equals(Node node, HashSet<Node> visitedNodes = null)
        {
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