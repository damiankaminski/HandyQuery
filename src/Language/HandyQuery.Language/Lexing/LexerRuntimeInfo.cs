using System.Globalization;
using HandyQuery.Language.Configuration;

namespace HandyQuery.Language.Lexing
{
    internal struct LexerRuntimeInfo
    {
        public readonly LexerStringReader Reader;
        public readonly ILanguageInternalConfig Config;
        public readonly ICultureConfig CurrentCultureConfig;

        public LexerRuntimeInfo(LexerStringReader reader, ILanguageInternalConfig config, CultureInfo cultureInfo)
        {
            Reader = reader;
            Config = config;
            CurrentCultureConfig = config.GetCultureConfig(cultureInfo);
        }
    }
}