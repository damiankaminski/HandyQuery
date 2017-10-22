using System.Collections.Generic;
using System.Linq;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal class BranchNode : Node
    {
        public override IEnumerable<Node> Children => _children;
        private readonly List<Node> _children = new List<Node>();

        public BranchNode AddChild(Node child)
        {
            if (_children.Contains(child))
            {
                return this;
            }
            
            _children.Add(child);
            child.AddParent(this);
            return this;
        }

        public override bool Equals(Node nodeBase, HashSet<Node> visitedNodes = null)
        {
            visitedNodes = visitedNodes ?? new HashSet<Node>();
            if (visitedNodes.Contains(nodeBase)) return true;
            visitedNodes.Add(nodeBase);
            
            if (nodeBase.GetType() != GetType())
            {
                return false;
            }

            var node = (BranchNode) nodeBase;
            
            if (AreSame(Children, node.Children, visitedNodes) == false)
            {
                return false;
            }
            
            return true;
        }

        public override string ToString()
        {
            return string.Join(" | ", Children.Select(x => x.ToString()));
        }
    }
}