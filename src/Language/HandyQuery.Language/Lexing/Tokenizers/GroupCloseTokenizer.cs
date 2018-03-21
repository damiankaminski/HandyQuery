using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    internal sealed class GroupCloseTokenizer : TokenizerBase
    {
        [HotPath]
        public override TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        {
            throw new System.NotImplementedException();
        }

        [HotPath]
        protected override Error CreateError(ref LexerRuntimeInfo info)
        {
            throw new System.NotImplementedException();
        }
    }
}