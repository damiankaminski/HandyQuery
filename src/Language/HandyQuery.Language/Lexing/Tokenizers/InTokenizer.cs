using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    internal sealed class InTokenizer : ITokenizer
    {
        [HotPath]
        public TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        {
            throw new System.NotImplementedException();
        }
    }
}