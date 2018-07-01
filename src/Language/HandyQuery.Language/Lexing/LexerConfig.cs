namespace HandyQuery.Language.Lexing
{
    // TODO: remove it?
    internal sealed class LexerConfig
    {
        public static readonly LexerConfig Default = new LexerConfig();
        public bool AllowUnknownToken { get; set; }
    }
}