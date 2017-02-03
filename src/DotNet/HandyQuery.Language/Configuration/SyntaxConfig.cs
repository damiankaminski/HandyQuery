namespace HandyQuery.Language.Configuration
{
    public sealed class SyntaxConfig : ISyntaxConfig
    {
        public bool KeywordCaseSensitive { get; set; } = false;
        public bool ColumnNameCaseSensitive { get; set; } = false;
        public bool NullConstantCaseSensitive { get; set; } = false;

        public char ParenOpen { get; set; } = '(';
        public char ParenClose { get; set; } = ')';

        public char ParamsOpen { get; set; } = '(';
        public char ParamsClose { get; set; } = ')';

        public char StringLiteralIdentifier { get; set; } = '"';
    }
}