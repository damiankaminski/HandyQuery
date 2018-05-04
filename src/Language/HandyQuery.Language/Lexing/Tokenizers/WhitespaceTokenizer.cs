using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    internal sealed class WhitespaceTokenizer : TokenizerBase
    {
        private static readonly TokenizationResult NoWhitespaceResult 
            = TokenizationResult.Successful(new WhitespaceToken(-1, 0));

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

        protected override Error CreateError(ref LexerRuntimeInfo info)
        {
            throw new System.NotSupportedException(
                "Somthing terribly wrong happened. WhitespaceTokenizer should never lead to error.");
        }
    }
}