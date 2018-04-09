namespace HandyQuery.Language.Lexing.Tokenizers.Abstract
{
    internal interface ITokenizer
    {
        // TODO: pass LexerRuntimeInfo by value? 
        TokenizationResult Tokenize(ref LexerRuntimeInfo info);
    }
}