using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace HandyQuery.Language.Lexing
{
    // TODO: better support for whitespaces (right now they need to match, e.g. wrong number of spaces will hide an item)
    // or maybe get rid of whitespaces in keywords? that would be the best and probably hardest option

    internal class SearchTrie<T> where T : class
    {
        private readonly Node _root;
        private readonly bool _caseSensitive;

        private SearchTrie(bool caseSensitive)
        {
            _root = new Node();
            _caseSensitive = caseSensitive;
        }

        public static SearchTrie<T> Create(bool caseSensitive, IReadOnlyDictionary<string, T> values)
        {
            var map = values
                .Select(x => new {ExpectedText = x.Key, x.Value})
                .OrderBy(x => x.ExpectedText.Length)
                .ThenBy(x => x.ExpectedText)
                .ToList();

            var trie = new SearchTrie<T>(caseSensitive);
            var current = trie._root;

            foreach (var pair in map)
            {
                for (var i = 0; i < pair.ExpectedText.Length; i++)
                {
                    var isLastChar = i == pair.ExpectedText.Length - 1;
                    var c = pair.ExpectedText[i];
                    var indexInCurrent = current.Children.FindIndex(x => x.Key == c);
                    if (indexInCurrent != -1)
                    {
                        current = current.Children[indexInCurrent];
                        continue;
                    }

                    var newNode = isLastChar
                        ? new Node(c, i + 1, pair.Value)
                        : new Node(c, i + 1);

                    current.Children.Add(newNode);
                    current = newNode;
                }

                current = trie._root;
            }

            return trie;
        }

        /// <summary>
        /// Finds an item in a tree. Uses <see cref="reader"/> to move through the query. Does not specifies what will
        /// be the current position of reader once it finishes its job. If it is relevant then prior to invokation
        /// of this method reader's current position should be stored. 
        /// </summary>
        /// <returns>True if found; otherwise, false.</returns>
        [HotPath]
        public bool TryFind(ref LexerStringReader reader, out T value, out int length)
        {
            if (TryFindNode(ref reader, out var node, out length))
            {
                value = node.Value;
                return true;
            }

            value = null;
            return false;
        }

        [HotPath]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryFindNode(ref LexerStringReader reader, out Node node, out int length)
        {
            var currentNode = _root;
            Node lastSuccessfulNode = null;

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

                    if (currentNode.Value != null)
                    {
                        lastSuccessfulNode = currentNode;
                    }

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

            if (lastSuccessfulNode != null)
            {
                node = lastSuccessfulNode;
                length = node.Length;
                return true;
            }

            node = null;
            length = 0;
            return false;
        }

        private class Node
        {
            public readonly char Key;
            public readonly int Length;
            public readonly List<Node> Children = new List<Node>();

            public readonly T Value;

            public Node()
            {
            }

            public Node(char key, int length)
            {
                Key = key;
                Length = length;
            }

            public Node(char key, int length, T value)
            {
                Key = key;
                Value = value;
                Length = length;
            }

            public override string ToString()
            {
                return Value == null && Key == default ? "ROOT" : Key.ToString();
            }
        }
    }
}