namespace HandyQuery.Language.Lexing.Tokenizers.Abstract
{
    // TODO: use pool in all tokenizers to get rid of allocations

    internal interface ITokenizer
    {
        TokenizationResult Tokenize(LexerRuntimeInfo info);
    }
}