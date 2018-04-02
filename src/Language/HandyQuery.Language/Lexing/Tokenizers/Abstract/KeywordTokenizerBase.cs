using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokenizers.Abstract
{
    internal abstract class KeywordTokenizerBase<TKeywordToken> : TokenizerBase
        where TKeywordToken : KeywordTokenBase
    {
        private readonly IDictionary<IReadOnlyDictionary<Keyword, string>, SearchTrie<Keyword>> _keywordsTries
            = new Dictionary<IReadOnlyDictionary<Keyword, string>, SearchTrie<Keyword>>();

        [HotPath]
        public override TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        {
            var keywordsTrie = GetKeywordsTrie(ref info);

            var startPosition = info.Reader.CaptureCurrentPosition();

            var found = keywordsTrie.TryFind(ref info.Reader, out var keyword);
            var readLength = info.Reader.CurrentPosition - startPosition.Value + 1;

            if (found == false)
            {
                info.Reader.MoveTo(startPosition);
                return TokenizationResult.Failed(CreateError(ref info));
            }

            var token = CreateToken(startPosition.Value, readLength, keyword);
            var result = TokenizationResult.Successful(token);
            return EnsureTrailingSpecialChar(ref info, result);
        }

        [HotPath]
        protected override Error CreateError(ref LexerRuntimeInfo info)
        {
            var word = info.Reader.ReadTillEndOfWord();
            return OnNotFoundError(new string(word));
        }

        [HotPath]
        private SearchTrie<Keyword> GetKeywordsTrie(ref LexerRuntimeInfo info)
        {
            // TODO: implement container which will be a member of LanguageConfig
            // and create single tokenizer per language config?
            // This way KeywordsTrie could be created once and injected via constructor.
            // It would also help to implement Zero cost extension points via structs (facade of IKeywordCharComparer
            // would be injected)

            var keywordsMap = info.Config.Syntax.KeywordsMap;
            if (_keywordsTries.TryGetValue(keywordsMap, out var keywordsTrie) == false)
            {
                // invoked only once per configuration instance, does not need to be fast

                // TODO: make sure this approach is thread safe
                var candidates = GetCandidatesForKeyword(in info);
                var candidatesMap = keywordsMap
                    .Where(x => candidates.Contains(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value);
                keywordsTrie = SearchTrie<Keyword>.Create(info.Config.Syntax.KeywordCaseSensitive, candidatesMap);
                _keywordsTries[keywordsMap] = keywordsTrie;
            }

            return keywordsTrie;
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
    }
}