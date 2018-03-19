using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokenizers.Abstract
{
    internal abstract class KeywordTokenizerBase<TKeywordToken> : ITokenizer
        where TKeywordToken : KeywordTokenBase
    {
        private readonly IDictionary<IReadOnlyDictionary<Keyword, string>, KeywordsTree> _keywordsTrees
            = new Dictionary<IReadOnlyDictionary<Keyword, string>, KeywordsTree>();

        [HotPath]
        public TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        {
            var keywordsTree = GetKeywordsTree(ref info);

            var startPosition = info.Reader.CurrentPosition;

            var found = keywordsTree.TryFind(ref info.Reader, info.Config.Syntax, out var keyword);
            var readLength = info.Reader.CurrentPosition - startPosition + 1;

            if (found == false)
            {
                info.Reader.MoveBy(-1 * readLength);
                var word = info.Reader.ReadTillEndOfWord();
                return TokenizationResult.Failed(OnNotFoundError(new string(word)));
            }
            
            var isProperEndOfKeyword = true;

            if (info.Reader.IsEndOfQuery() == false)
            {
                info.Reader.MoveBy(1);
                isProperEndOfKeyword = char.IsWhiteSpace(info.Reader.CurrentChar)
                                       || info.Config.Syntax.ReservedChars.Contains(info.Reader.CurrentChar);
                info.Reader.MoveBy(-1);
            }
            
            if (isProperEndOfKeyword == false)
            {
                info.Reader.MoveBy(-1 * readLength);
                var word = info.Reader.ReadTillEndOfWord();
                return TokenizationResult.Failed(OnNotFoundError(new string(word)));
            }

            var token = CreateToken(startPosition, readLength, keyword);
            return TokenizationResult.Successful(token);
        }

        [HotPath]
        private KeywordsTree GetKeywordsTree(ref LexerRuntimeInfo info)
        {
            // TODO: implement container which will be a member of LanguageConfig
            // and create single tokenizer per language config?
            // This way KeywordsTree could be created once and injected via constructor.
            // It would also help to implement Zero cost extension points via structs (facade of IKeywordCharComparer
            // would be injected)

            var keywordsMap = info.Config.Syntax.KeywordsMap;
            if (_keywordsTrees.TryGetValue(keywordsMap, out var keywordsTree) == false)
            {
                // invoked only once per configuration instance, does not need to be fast
                
                // TODO: make sure this approach is thread safe
                var candidates = GetCandidatesForKeyword(in info);
                var candidatesMap = keywordsMap
                    .Where(x => candidates.Contains(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value);
                keywordsTree = KeywordsTree.Create(candidatesMap);
                _keywordsTrees[keywordsMap] = keywordsTree;
            }

            return keywordsTree;
        }

        /// <summary>
        /// Should create an instance of <see cref="TKeywordToken"/> using provided parameters.
        /// </summary>
        public abstract TKeywordToken CreateToken(int startPosition, int length, Keyword keyword);

        /// <summary>
        /// Should provide possible keywords.
        /// </summary>
        public abstract IEnumerable<Keyword> GetCandidatesForKeyword(in LexerRuntimeInfo info);

        /// <summary>
        /// Allows to add an error in case if keyword is not found.
        /// </summary>
        public abstract Error OnNotFoundError(string word);

        // TODO: do the same for column names to not allocate strings?
        private class KeywordsTree
        {
            private readonly Node _root;

            private KeywordsTree()
            {
                _root = new Node();
            }

            public static KeywordsTree Create(IReadOnlyDictionary<Keyword, string> keywordsMap)
            {
                // invoked only once per configuration instance, does not need to be fast
                
                var keywords = keywordsMap
                    .Select(x => new {Text = x.Value, Value = x.Key})
                    .OrderByDescending(x => x.Text.Length)
                    .ThenBy(x => x.Text)
                    .ToList();

                var tree = new KeywordsTree();
                var current = tree._root;

                foreach (var keyword in keywords)
                {
                    for (var i = 0; i < keyword.Text.Length; i++)
                    {
                        var isLastChar = i == keyword.Text.Length - 1;
                        var c = keyword.Text[i];
                        var indexInCurrent = current.Children.FindIndex(x => x.Value == c);
                        if (indexInCurrent != -1 && !isLastChar)
                        {
                            current = current.Children[indexInCurrent];
                            continue;
                        }

                        var newNode = isLastChar
                            ? new Node(c, keyword.Value)
                            : new Node(c);

                        current.Children.Add(newNode);
                        current = newNode;
                    }

                    current = tree._root;
                }

                return tree;
            }

            [HotPath]
            public bool TryFind(ref LexerStringReader reader, SyntaxConfig syntax, out Keyword keyword)
            {
                if (TryFindNode(ref reader, syntax, out var node))
                {
                    keyword = node.Keyword;
                    if (keyword == null)
                    {
                        return false;
                    }

                    return true;
                }

                keyword = null;
                return false;
            }

            [HotPath]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool TryFindNode(ref LexerStringReader reader, SyntaxConfig syntax, out Node node)
            {
                var currentNode = _root;

                while (currentNode != null)
                {
                    var progressed = false;

                    var currentChar = reader.CurrentChar;
                    if (syntax.ReservedChars.Contains(currentChar)) break;

                    // PERF NOTE: bisection could be implemented here, but since number of children should remain small
                    // then overhead could be larger than actual gains
                    foreach (var child in currentNode.Children)
                    {
                        // TODO: PERF: use zero cost extension point trick (see Performance.md)
                        if (currentChar == child.Value 
                            || (!syntax.KeywordCaseSensitive && char.ToLower(currentChar) == char.ToLower(child.Value)))
                        {
                            if (child.Keyword != null)
                            {
                                node = child;
                                return true;
                            }

                            currentNode = child;
                            progressed = true;
                            reader.MoveBy(1);
                            break;
                        }
                    }

                    if (!progressed) break;
                }

                node = null;
                return false;
            }


            public class Node
            {
                public readonly char Value;
                public readonly List<Node> Children = new List<Node>();

                public readonly Keyword Keyword;

                public Node()
                {
                }

                public Node(char value)
                {
                    Value = value;
                }

                public Node(char value, Keyword keyword)
                {
                    Value = value;
                    Keyword = keyword;
                }

                public override string ToString()
                {
                    return Keyword == null && Value == default ? "ROOT" : Value.ToString();
                }
            }
        }
    }
}