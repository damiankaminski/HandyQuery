using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing
{
    // TODO: try to change to ref struct
    internal sealed class TokenizationResult
    {
        public bool Success { get; }
        public TokenBase Token { get; }
        
        // TODO: lazy evaluation? should be possible if changed to Action<LexerStringReader> and if
        //       reader's postion would be properly adjusted 
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