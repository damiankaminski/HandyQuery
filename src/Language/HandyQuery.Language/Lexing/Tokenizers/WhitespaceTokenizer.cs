using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    [Tokenizer(manualUsage: true)]
    internal sealed class WhitespaceTokenizer : TokenizerBase
    {
        private static readonly TokenizationResult NoWhitespaceResult 
            = TokenizationResult.Failed();

        public WhitespaceTokenizer(LanguageConfig languageConfig) : base(languageConfig)
        {
        }

        // TODO: tests

        [HotPath]
        public override TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        {
            var startPosition = info.Reader.CurrentPosition;
            var whitespaceSpan = info.Reader.ReadTillEndOfWhitespace();

            if (whitespaceSpan.Length == 0)
            {
                return NoWhitespaceResult; 
            }
            
            var token = new WhitespaceToken(startPosition, whitespaceSpan.Length);
            return TokenizationResult.Successful(token);
        }
    }
}