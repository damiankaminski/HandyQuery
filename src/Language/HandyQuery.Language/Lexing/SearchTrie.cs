using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace HandyQuery.Language.Lexing
{
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
                        ? new Node(c, pair.Value)
                        : new Node(c);

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
        public bool TryFind(LexerStringReader reader, out T value, out int length)
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
            var initialPosition = reader.CurrentPosition;
            var currentNode = _root;
            Node lastSuccessfulNode = null;
            var lastSuccessfulLength = 0;

            var endOfQuery = false;

            while (currentNode != null)
            {
                var progressed = false;

                var currentChar = reader.CurrentChar;

                // PERF NOTE: bisection could be implemented here, but since number of children should remain small
                // then overhead could be larger than actual gains
                foreach (var child in currentNode.Children)
                {
                    var isWhitespace = false;

                    if (char.IsWhiteSpace(child.Key) && char.IsWhiteSpace(currentChar))
                    {
                        isWhitespace = true;
                    }
                    else if (currentChar != child.Key
                             && (_caseSensitive || char.ToLower(currentChar) != char.ToLower(child.Key)))
                    {
                        // TODO: PERF: use zero cost extension point trick to implement case sensivity flag (see Performance.md)
                        continue;
                    }

                    // check if not the end
                    if (reader.IsEndOfQuery()) endOfQuery = true;

                    // read characters
                    if (isWhitespace) reader.ReadTillEndOfWhitespace(); // TODO: multi whitespace read opt in (perhaps use extension point trick?) 
                    else reader.MoveBy(1);
                    
                    // assign results
                    currentNode = child;
                    if (currentNode.Value != null)
                    {
                        lastSuccessfulNode = currentNode;
                        lastSuccessfulLength = reader.CurrentPosition - initialPosition;
                    }

                    // break if nothing more to be processed
                    if (endOfQuery) break;

                    progressed = true;
                    break;
                }

                if (endOfQuery)
                {
                    break;
                }

                if (!progressed)
                {
                    break;
                }
            }

            if (lastSuccessfulNode != null)
            {
                length = lastSuccessfulLength;
                if (endOfQuery) length++;

                node = lastSuccessfulNode;
                return true;
            }

            node = null;
            length = 0;
            return false;
        }

        private class Node
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