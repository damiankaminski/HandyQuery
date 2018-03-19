using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    internal sealed class WhitespaceTokenizer : ITokenizer
    {
        // TODO: tests
        [HotPath]
        public TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        {
            var startPosition = info.Reader.CurrentPosition;
            var whitespaceSpan = info.Reader.ReadTillEndOfWhitespace();

            var token = new WhitespaceToken(startPosition, whitespaceSpan.Length);

            return TokenizationResult.Successful(token);
        }
    }
}