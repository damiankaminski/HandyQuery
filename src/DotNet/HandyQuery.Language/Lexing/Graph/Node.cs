using System;
using System.Collections.Generic;
using System.Linq;

namespace HandyQuery.Language.Lexing.Graph
{
    // TODO: use NodesCollection instead of List<Node> ?

    internal abstract class Node
    {
        public readonly List<Node> Parents = new List<Node>();
        public abstract IEnumerable<Node> Children { get; }

        public void AddParent(Node parent)
        {
            Parents.Add(parent);
        }

        public abstract bool Equals(Node nodeBase, HashSet<Node> visitedNodes = null);

        protected static bool AreSame(IEnumerable<Node> items, IEnumerable<Node> items2, HashSet<Node> visitedNodes)
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