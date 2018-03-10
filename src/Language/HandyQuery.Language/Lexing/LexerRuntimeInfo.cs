using System.Globalization;
using HandyQuery.Language.Configuration;

namespace HandyQuery.Language.Lexing
{
    internal readonly ref struct LexerRuntimeInfo
    {
        public readonly LexerStringReader Reader;
        public readonly LanguageConfig Config;

        public LexerRuntimeInfo(LexerStringReader reader, LanguageConfig config)
        {
            Reader = reader;
            Config = config;
        }
    }
}