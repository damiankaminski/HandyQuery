using System.Collections.Generic;
using System.Linq;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class BranchNode : Node
    {
        public IEnumerable<Node> Heads => _heads;
        private readonly List<Node> _heads = new List<Node>();

        public BranchNode()
        {
        }
        
        public BranchNode(IEnumerable<Node> heads)
        {
            foreach (var head in heads)
            {
                AddHead(head);
            }
        }

        public BranchNode AddHead(Node head)
        {
            if (_heads.Contains(head))
            {
                return this;
            }
            
            _heads.Add(head);
            return this;
        }

        public override string ToString()
        {
            return "[ᚶ] " + string.Join(" | ", Heads.Select(x => x.ToString()));
        }
    }
}