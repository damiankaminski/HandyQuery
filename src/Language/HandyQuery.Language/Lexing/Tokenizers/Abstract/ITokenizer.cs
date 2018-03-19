﻿namespace HandyQuery.Language.Lexing.Tokenizers.Abstract
{
    internal interface ITokenizer
    {
        TokenizationResult Tokenize(ref LexerRuntimeInfo info);
    }
}