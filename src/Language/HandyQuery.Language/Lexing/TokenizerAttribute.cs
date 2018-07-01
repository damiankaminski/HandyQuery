using System;
using System.Diagnostics;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing
{
    public class TokenizerAttribute : Attribute
    {
        public readonly Type AfterTokenizer;
        public readonly bool ManualUsage;

        public TokenizerAttribute(Type afterTokenizer = null, bool manualUsage = false)
        {
            Debug.Assert(afterTokenizer == null || typeof(ITokenizer).IsAssignableFrom(afterTokenizer));
            AfterTokenizer = afterTokenizer;
            ManualUsage = manualUsage;
        }
    }
}