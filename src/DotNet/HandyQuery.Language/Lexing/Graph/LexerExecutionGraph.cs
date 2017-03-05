using System.Collections.Generic;
using HandyQuery.Language.Lexing.Grammar.Structure;
using HandyQuery.Language.Lexing.Graph.Builder;

namespace HandyQuery.Language.Lexing.Graph
{
    internal sealed class LexerExecutionGraph
    {
        internal readonly Node Root;

        private HashSet<Node> _getAllChildrenVisitedNodes;

        internal LexerExecutionGraph(Node root)
        {
            Root = root;
        }

        public static LexerExecutionGraph Build(GrammarPart grammarRoot)
        {
            var builder = new LexerExecutionGraphBuilder();
            var root = builder.BuildGraph(grammarRoot);

            return new LexerExecutionGraph(root.ConvertToNode());
        }

        public bool Equals(LexerExecutionGraph expected)
        {
            return Root.Equals(expected.Root) &&
                GetTotalChildrenInstances(Root) == GetTotalChildrenInstances(expected.Root);
        }

        private int GetTotalChildrenInstances(Node node)
        {
            _getAllChildrenVisitedNodes = new HashSet<Node>();
            return GetAllChildren(node).Count;
        }

        private HashSet<Node> GetAllChildren(Node node)
        {
            if (_getAllChildrenVisitedNodes.Contains(node))
            {
                return new HashSet<Node>();
            }

            _getAllChildrenVisitedNodes.Add(node);
            var all = new HashSet<Node>();

            foreach (var nodeChild in node.Children)
            {
                all.Add(nodeChild);
                var childrenOfChild = GetAllChildren(nodeChild);
                foreach (var childOfChild in childrenOfChild)
                {
                    all.Add(childOfChild);
                }
            }
            return all;
        }
    }
}