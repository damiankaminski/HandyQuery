using System.Linq;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokenizers.Abstract
{
    internal abstract class KeywordTokenizerBase<TKeywordToken> : TokenizerBase
        where TKeywordToken : KeywordTokenBase
    {
        private readonly SearchTrie<Keyword> _keywordsTrie;

        protected KeywordTokenizerBase(LanguageConfig languageConfig) : base(languageConfig)
        {
            var map = languageConfig.Syntax.KeywordsMap.ToDictionary(x => x.Value, x => x.Key);
            _keywordsTrie = SearchTrie<Keyword>.Create(languageConfig.Syntax.KeywordCaseSensitive, map);
        }
        
        [HotPath]
        public override TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        {
            // TODO: use Param provided by grammar, tokenize single position once and cache result

            var startPosition = info.Reader.CaptureCurrentPosition();

            var found = _keywordsTrie.TryFind(ref info.Reader, out var keyword, out var readLength);

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
        /// Should create an instance of <see cref="TKeywordToken"/> using provided parameters.
        /// </summary>
        public abstract TKeywordToken CreateToken(int startPosition, int length, Keyword keyword);

        /// <summary>
        /// Allows to add an error in case if keyword is not found.
        /// </summary>
        public abstract Error OnNotFoundError(string word, int position);
    }
}