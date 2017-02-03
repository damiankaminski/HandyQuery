namespace HandyQuery.Language.Configuration.Keywords
{
    internal abstract class Keyword
    {
        public abstract TokenType TokenType { get; }

        public override string ToString()
        {
            return TokenType.ToString();
        }
    }
}