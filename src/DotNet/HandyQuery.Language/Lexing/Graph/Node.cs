using System.Collections.Generic;
using HandyQuery.Language.Extensions;
using HandyQuery.Language.Lexing.Gramma.Structure;

namespace HandyQuery.Language.Lexing.Graph
{
    internal sealed class Node
    {
        public readonly IGrammaBodyItem Item;
        public readonly List<Node> Children = new List<Node>(10);

        public Node(IGrammaBodyItem item)
        {
            Item = item;
        }

        public void AddAsChildTo(IEnumerable<Node> parents)
        {
            if (parents != null)
            {
                foreach (var parent in parents)
                {
                    parent?.AddChildNode(this);
                }
            }
        }

        public Node AddChild(Node child)
        {
            Children.Add(child);
            return this;
        }

        private void AddChildNode(Node child)
        {
            Children.Add(child);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Node && Equals((Node) obj);
        }

        public bool Equals(Node node)
        {
            if (node.Item?.Equals(Item) == false)
            {
                return false;
            }

            if (node.Children.IsSameAs(Children) == false)
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return Item?.Name ?? "Root";
        }
    }
}