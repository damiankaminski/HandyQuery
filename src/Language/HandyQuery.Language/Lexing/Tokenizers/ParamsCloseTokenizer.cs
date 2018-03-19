using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    [PerformanceCritical]
    internal sealed class ParamsCloseTokenizer : ITokenizer
    {
        public TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        {
            throw new System.NotImplementedException();
        }
    }
}