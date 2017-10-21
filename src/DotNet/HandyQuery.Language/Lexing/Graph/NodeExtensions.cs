using HandyQuery.Language.Lexing.Graph.Builder;

namespace HandyQuery.Language.Lexing.Graph
{
    internal static class NodeExtensions
    {
        public static void AddChild(this Node @this, Node node)
        {
            switch (@this)
            {
                case BranchNode n:
                    n.AddChild(node);
                    break;
                case RootNode n:
                    n.WithChild(node);
                    break;
                case TerminalNode n:
                    n.WithChild(node);
                    break;
                default:
                    throw new LexerExecutionGraphException($"Unkown type: {@this.GetType()}");
            }
        }
    }
}