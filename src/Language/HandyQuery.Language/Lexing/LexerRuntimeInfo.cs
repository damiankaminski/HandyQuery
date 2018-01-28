using System.Globalization;
using HandyQuery.Language.Configuration;

namespace HandyQuery.Language.Lexing
{
    internal struct LexerRuntimeInfo
    {
        public readonly LexerStringReader Reader;
        public readonly ILanguageInternalConfig Config;
        public readonly ICultureConfig CurrentCulture;

        public LexerRuntimeInfo(LexerStringReader reader, ILanguageInternalConfig config, CultureInfo cultureInfo)
        {
            Reader = reader;
            Config = config;
            CurrentCulture = config.GetCulture(cultureInfo);
        }
    }
}