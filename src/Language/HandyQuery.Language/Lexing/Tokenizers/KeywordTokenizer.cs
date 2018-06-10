using System.Linq;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    internal sealed class KeywordTokenizer : TokenizerBase
    {
        private readonly SearchTrie<Keyword> _keywordsTrie;

        public KeywordTokenizer(LanguageConfig languageConfig) : base(languageConfig)
        {
            var candidatesMap = languageConfig.Syntax.KeywordsMap
                .ToDictionary(x => x.Value, x => x.Key);
            _keywordsTrie = SearchTrie<Keyword>.Create(languageConfig.Syntax.KeywordCaseSensitive, candidatesMap);
        }
        
        [HotPath]
        public override TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        {
            var startPosition = info.Reader.CaptureCurrentPosition();

            var found = _keywordsTrie.TryFind(info.Reader, out var keyword, out var readLength);

            if (found == false)
            {
                return TokenizationResult.Failed();
            }

            var token = new KeywordToken(startPosition.Value, readLength, keyword);
            var result = TokenizationResult.Successful(token);
            return EnsureTrailingSpecialChar(ref info, result);
        }
    }
}