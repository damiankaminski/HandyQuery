using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokenizers.Abstract
{
    internal abstract class KeywordTokenizerBase<TKeywordToken> : TokenizerBase
        where TKeywordToken : KeywordTokenBase
    {
        private readonly Lazy<SearchTrie<Keyword>> _keywordsTrie;

        protected KeywordTokenizerBase(LanguageConfig languageConfig) : base(languageConfig)
        {
            // Lazy is introduced to not call virtual GetCandidatesForKeyword in a constructor
            // Here's why: https://stackoverflow.com/a/119543
            _keywordsTrie = new Lazy<SearchTrie<Keyword>>(() =>
            {
                var candidates = GetCandidatesForKeyword(LanguageConfig);
                var candidatesMap = languageConfig.Syntax.KeywordsMap
                    .Where(x => candidates.Contains(x.Key))
                    .ToDictionary(x => x.Value, x => x.Key);
                return SearchTrie<Keyword>.Create(languageConfig.Syntax.KeywordCaseSensitive, candidatesMap);
            });
        }
        
        [HotPath]
        public override TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        {
            var startPosition = info.Reader.CaptureCurrentPosition();

            var found = _keywordsTrie.Value.TryFind(ref info.Reader, out var keyword, out var readLength);

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
            var position = info.Reader.CurrentPosition;
            var word = info.Reader.ReadTillEndOfWord();
            return OnNotFoundError(new string(word), position);
        }

        /// <summary>
        /// Should provide possible keywords.
        /// </summary>
        public abstract IEnumerable<Keyword> GetCandidatesForKeyword(LanguageConfig languageConfig);
        
        /// <summary>
        /// Should create an instance of <see cref="TKeywordToken"/> using provided parameters.
        /// </summary>
        public abstract TKeywordToken CreateToken(int startPosition, int length, Keyword keyword);

        /// <summary>
        /// Allows to add an error in case if keyword is not found.
        /// </summary>
        public abstract Error OnNotFoundError(string word, int position);
    }
}