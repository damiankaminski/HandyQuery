namespace HandyQuery.Language.Lexing.Gramma.Structure
{
    internal interface IGrammaElement
    {
        string Name { get; }
        GrammaElementType Type { get; }
    }

    internal static class GammaElementExtensions
    {
        public static T As<T>(this IGrammaElement element) where T : class, IGrammaElement
        {
            return element as T;
        }
    }
}