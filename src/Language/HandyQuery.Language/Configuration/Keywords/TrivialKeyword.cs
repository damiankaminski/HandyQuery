namespace HandyQuery.Language.Configuration.Keywords
{
    internal sealed class TrivialKeyword : KeywordBase
    {
        public static readonly TrivialKeyword ParenOpen = new TrivialKeyword(TrivialKeywordType.ParenOpen);
        public static readonly TrivialKeyword ParenClose = new TrivialKeyword(TrivialKeywordType.ParenClose);
        public static readonly TrivialKeyword ParamsSeparator = new TrivialKeyword(TrivialKeywordType.ParamsSeparator);
        public static readonly TrivialKeyword Null = new TrivialKeyword(TrivialKeywordType.Null);

        public TrivialKeywordType TrivialKeywordType { get; }
        public override KeywordType KeywordType { get; } = KeywordType.Trivial;

        private TrivialKeyword(TrivialKeywordType trivialKeywordType)
        {
            TrivialKeywordType = trivialKeywordType;
        }

        public override string ToString()
        {
            return $"{KeywordType}: {TrivialKeywordType.ToString()}";
        }
    }
    
    internal enum TrivialKeywordType
    {
        ParenOpen,
        ParenClose,
        ParamsSeparator,
        Null
    }
}