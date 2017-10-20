using System;
using System.Collections.Generic;
using System.Data.Common;
using HandyQuery.Language.Lexing.Grammar.Structure;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class TerminalNode : Node
    {
        public readonly ITokenizer Tokenizer;
        public Node Child { get; private set; }
        
        public override IEnumerable<Node> Children => _children;
        private IEnumerable<Node> _children = new Node[0];
        
        public TerminalNode(GrammarTerminalUsage terminalUsage)
        {
            Tokenizer = terminalUsage?.Impl;
        }

        public TerminalNode WithChild(Node child)
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

            var node = (TerminalNode)nodeBase;

            if (node.Tokenizer?.GetType() != Tokenizer?.GetType())
            {
                return false;
            }

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
            var name = Tokenizer?.GetType().Name;
            name = name?.Substring(0, name.LastIndexOf("Tokenizer", StringComparison.Ordinal)) ?? "ROOT";
            return name;
        }
    }
}