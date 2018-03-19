using System.Globalization;
using HandyQuery.Language.Configuration;

namespace HandyQuery.Language.Lexing
{
    internal ref struct LexerRuntimeInfo
    {
        public LexerStringReader Reader;
        public readonly LanguageConfig Config;

        public LexerRuntimeInfo(LexerStringReader reader, LanguageConfig config)
        {
            Reader = reader;
            Config = config;
        }
    }
}