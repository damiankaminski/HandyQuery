using HandyQuery.Language.Parsing.StateTransition.Nodes;

namespace HandyQuery.Language.Parsing.StateTransition
{
    internal static class NodeExtensions
    {
        public static void AddChild(this Node self, Node node)
        {
            switch (self)
            {
//                case BranchNode n:
//                    n.AddHead(node);
//                    break;
                case RootNode n:
                    n.WithChild(node);
                    break;
                case TerminalNode n:
                    n.WithChild(node);
                    break;
                case NonTerminalUsageNode n:
                    n.WithChild(node);
                    break;
                default:
                    throw new StateTransitionException($"Unkown type: {self.GetType()}", 
                        StateTransitionException.Id.UnknownNodeType);
            }
        }
    }
}