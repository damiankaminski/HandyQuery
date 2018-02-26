namespace HandyQuery.Language.Configuration
{
    public sealed class SyntaxBuilder
    {
        private bool _keywordCaseSensitive;
        private bool _columnNameCaseSensitive;

        private SyntaxConfig _syntaxConfig;
        
        internal SyntaxConfig Build()
        {
            _syntaxConfig = _syntaxConfig ?? new SyntaxConfig(_keywordCaseSensitive, _columnNameCaseSensitive);
            return _syntaxConfig;
        }
        
        public SyntaxBuilder WithCaseSensitiveKeywords()
        {
            _keywordCaseSensitive = true;
            return this;
        }
        
        public SyntaxBuilder WithCaseSensitiveColumnNames()
        {
            _columnNameCaseSensitive = true;
            return this;
        } 
    }
}