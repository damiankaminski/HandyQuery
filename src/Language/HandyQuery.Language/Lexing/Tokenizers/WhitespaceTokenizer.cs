using HandyQuery.Language.Extensions;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    [PerformanceCritical]
    internal sealed class WhitespaceTokenizer : ITokenizer
    {
        // TODO: tests
        public TokenizationResult Tokenize(LexerRuntimeInfo info)
        {
            var startPosition = info.Reader.CurrentPosition;
            var whitespaceSpan = info.Reader.ReadTillEndOfWhitespace();

            var token = new WhitespaceToken(startPosition, whitespaceSpan.Length);

            return TokenizationResult.Successful(token);
        }
    }
}