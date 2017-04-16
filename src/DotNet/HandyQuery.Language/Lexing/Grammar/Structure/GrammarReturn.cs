namespace HandyQuery.Language.Lexing.Grammar.Structure
{
    internal sealed class GrammarReturn
    {
        public GrammarPartUsage PartUsage { get; }

        public GrammarReturn(GrammarPartUsage partUsage)
        {
            PartUsage = partUsage;
        }
    }
}