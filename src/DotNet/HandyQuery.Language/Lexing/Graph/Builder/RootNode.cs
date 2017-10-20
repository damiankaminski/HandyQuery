﻿using System.Collections.Generic;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class RootNode : Node
    {
        public Node Child { get; private set; }
        
        public override IEnumerable<Node> Children => _children;
        private IEnumerable<Node> _children = new Node[0];

        public RootNode WithChild(Node child)
        {
            if (Child != null)
            {
                throw new LexerExecutionGraphException("Child change is not allowed.");
            }
            
            Child = child;
            child.AddParent(this);
            _children = new[] {child};
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

            var node = (RootNode) nodeBase;
            
            if (node.Child == null && Child != null)
            {
                return false;
            }
            
            if (node.Child != null && node.Child.Equals(Child, visitedNodes) == false)
            {
                return false;
            }
            
            return true;
        }

        public override string ToString()
        {
            return "ROOT";
        }
    }
}