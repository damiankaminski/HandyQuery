using System.Collections.Generic;
using System.Collections.ObjectModel;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class LexerExecutionGraphBuilder
    {
        public RootNode BuildGraph(GrammarReturn grammarRoot)
        {
            var root = new RootNode();

            // TODO
            
            var t = new NodesCollection();

            return root;
        }

        private sealed class NodesCollection : Collection<Node>
        {
            public NodesCollection()
            {
            }

            /// <summary>
            /// Creates a new Nodes list with single value added.
            /// </summary>
            public NodesCollection(Node node)
            {
                Add(node);
            }

            /// <summary>
            /// Creates a new Nodes list with multiple values added.
            /// </summary>
            public NodesCollection(IEnumerable<Node> nodes)
            {
                foreach (var node in nodes)
                {
                    Add(node);
                }
            }

            public void AddChild(Node node)
            {
                foreach (var item in this)
                {
                    item.AddChildImpl(node);
                }
            }

            public void AddChild(NodesCollection nodes)
            {
                foreach (var node in nodes)
                {
                    AddChild(node);
                }
            }

            public static implicit operator NodesCollection(Node node)
            {
                return new NodesCollection(node);
            }
        }
    }
}