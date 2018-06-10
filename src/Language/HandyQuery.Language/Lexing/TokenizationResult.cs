using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing
{
    // TODO: try to change to ref struct
    internal sealed class TokenizationResult
    {
        public bool Success { get; }
        public TokenBase Token { get; }

        public bool IsPartiallySuccessful => !Success && Token != null;
        
        private TokenizationResult(bool success, TokenBase token)
        {
            Success = success;
            Token = token;
        }

        public static TokenizationResult Successful(TokenBase token)
        {
            return new TokenizationResult(true, token);
        }
        
        public static TokenizationResult PartiallySuccessful(TokenBase token)
        {
            return new TokenizationResult(false, token);
        }

        public static TokenizationResult Failed()
        {
            return new TokenizationResult(false, null);
        }
    }
}