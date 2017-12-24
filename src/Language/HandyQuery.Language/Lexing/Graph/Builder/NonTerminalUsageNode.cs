namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class NonTerminalUsageNode : Node
    {
        public readonly string Name;
        
        public Node Child { get; private set; }

        public Node Head { get; }

        public NonTerminalUsageNode(string name, Node head)
        {
            Name = name;
            Head = head;
        }

        public NonTerminalUsageNode WithChild(Node child)
        {
            if (Child != null)
            {
                throw new LexerExecutionGraphException("Child change is not allowed.", 
                    LexerExecutionGraphException.Id.NodeChildChangeNotAllowed);
            }
            
            Child = child;
            return this;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}