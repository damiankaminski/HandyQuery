namespace HandyQuery.Language.Lexing
{
    internal sealed class LexerConfig
    {
        public static readonly LexerConfig Default = new LexerConfig();
        public bool AllowUnknownToken { get; set; }
    }
}