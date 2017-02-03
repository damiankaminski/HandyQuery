using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing
{
    internal sealed class TokenizationResult
    {
        public bool Success { get; }
        public TokenBase Token { get; }
        public Error Error { get; }

        private TokenizationResult(bool success, TokenBase token, Error error)
        {
            Success = success;
            Error = error;
            Token = token;
        }

        public static TokenizationResult Successful(TokenBase token)
        {
            return new TokenizationResult(true, token, null);
        }

        public static TokenizationResult Failed(Error error)
        {
            return new TokenizationResult(false, null, error);
        }
    }
}