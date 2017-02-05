namespace HandyQuery.Language.Lexing.Grammar.Structure
{
    internal interface IGrammarElement
    {
        string Name { get; }
        GrammarElementType Type { get; }
    }

    internal static class GrammarElementExtensions
    {
        public static T As<T>(this IGrammarElement element) where T : class, IGrammarElement
        {
            return element as T;
        }
    }
}