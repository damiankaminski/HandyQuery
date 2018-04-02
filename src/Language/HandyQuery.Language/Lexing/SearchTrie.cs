using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace HandyQuery.Language.Lexing
{
    // TODO: tests

    internal class SearchTrie<T> where T : class
    {
        private readonly Node _root;
        private readonly bool _caseSensitive;

        private SearchTrie(bool caseSensitive)
        {
            _root = new Node();
            _caseSensitive = caseSensitive;
        }

        public static SearchTrie<T> Create(bool caseSensitive, IReadOnlyDictionary<T, string> values)
        {
            var map = values
                .Select(x => new {Text = x.Value, Value = x.Key})
                .OrderBy(x => x.Text.Length)
                .ThenBy(x => x.Text)
                .ToList();

            var trie = new SearchTrie<T>(caseSensitive);
            var current = trie._root;

            foreach (var pair in map)
            {
                for (var i = 0; i < pair.Text.Length; i++)
                {
                    var isLastChar = i == pair.Text.Length - 1;
                    var c = pair.Text[i];
                    var indexInCurrent = current.Children.FindIndex(x => x.Key == c);
                    if (indexInCurrent != -1)
                    {
                        current = current.Children[indexInCurrent];
                        continue;
                    }

                    var newNode = isLastChar
                        ? new Node(c, pair.Value)
                        : new Node(c);

                    current.Children.Add(newNode);
                    current = newNode;
                }

                current = trie._root;
            }

            return trie;
        }

        [HotPath]
        public bool TryFind(ref LexerStringReader reader, out T value)
        {
            if (TryFindNode(ref reader, out var node))
            {
                value = node.Value;
                if (value == null)
                {
                    return false;
                }

                return true;
            }

            value = null;
            return false;
        }

        [HotPath]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryFindNode(ref LexerStringReader reader, out Node node)
        {
            // TODO: FIXME: Current implementation has a very subtle bug which is not a problem right now
            // but might be if syntax would change.
            // Lets assume we've got following keywords:
            // `=`
            // `==`
            // `==t=`
            // and following input: `==ta`
            // It would result with false even though `==` is present.
            // currentNode.Keyword would be null because currentNode would be set to `t`.
            // It should be set to second `=` and when failed to process `==t=` it should go back using MoveBy(-2)

            var currentNode = _root;

            while (currentNode != null)
            {
                var progressed = false;
                var endOfQuery = false;

                var currentChar = reader.CurrentChar;

                // PERF NOTE: bisection could be implemented here, but since number of children should remain small
                // then overhead could be larger than actual gains
                foreach (var child in currentNode.Children)
                {
                    // TODO: PERF: use zero cost extension point trick (see Performance.md)
                    if (currentChar != child.Key
                        && (_caseSensitive || char.ToLower(currentChar) != char.ToLower(child.Key)))
                    {
                        continue;
                    }

                    currentNode = child;

                    if (reader.IsEndOfQuery())
                    {
                        endOfQuery = true;
                        break;
                    }

                    progressed = true;
                    reader.MoveBy(1);
                    break;
                }

                if (endOfQuery)
                {
                    break;
                }

                if (!progressed)
                {
                    reader.MoveBy(-1);
                    break;
                }
            }

            if (currentNode?.Value != null)
            {
                node = currentNode;
                return true;
            }

            node = null;
            return false;
        }

        public class Node
        {
            public readonly char Key;
            public readonly List<Node> Children = new List<Node>();

            public readonly T Value;

            public Node()
            {
            }

            public Node(char key)
            {
                Key = key;
            }

            public Node(char key, T value)
            {
                Key = key;
                Value = value;
            }

            public override string ToString()
            {
                return Value == null && Key == default ? "ROOT" : Key.ToString();
            }
        }
    }
}