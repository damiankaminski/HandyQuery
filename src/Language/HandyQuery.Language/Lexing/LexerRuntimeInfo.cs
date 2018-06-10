using System.Globalization;
using HandyQuery.Language.Configuration;

namespace HandyQuery.Language.Lexing
{
    internal ref struct LexerRuntimeInfo
    {
        // PERF: Reader cannot be marked as readonly as it changes while progressing in the query and if
        //       marked as readonly then C# would make defensive copies to make sure that it remains unchanged
        public LexerStringReader Reader;
       
        public LexerRuntimeInfo(LexerStringReader reader)
        {
            Reader = reader;
        }
    }
}