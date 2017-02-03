using System.Collections.Generic;
using HandyQuery.Language.Lexing.Gramma.Structure;

namespace HandyQuery.Language.Lexing.Graph
{
    internal sealed class Node
    {
        public readonly IGrammaBodyItem Item;
        public readonly List<Node> Children = new List<Node>(10);

        public Node(IGrammaBodyItem item, IEnumerable<Node> parents)
        {
            Item = item;

            if (parents != null)
            {
                foreach (var parent in parents)
                {
                    parent?.AddChildNode(this);
                }
            }
        }

        public void AddChildNode(Node child)
        {
            Children.Add(child);
        }

        public override string ToString()
        {
            return Item?.Name ?? "Root";
        }
    }
}