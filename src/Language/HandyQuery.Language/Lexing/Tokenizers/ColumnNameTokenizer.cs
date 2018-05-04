using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    internal sealed class ColumnNameTokenizer : TokenizerBase
    {
        public ColumnNameTokenizer(LanguageConfig languageConfig) : base(languageConfig)
        {
        }

        [HotPath]
        public override TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        {
            var startPosition = info.Reader.CaptureCurrentPosition();
            
            var columnNameSpan = info.Reader.ReadTillIvalidCharOrWhitespace(info.Config.Syntax.ReservedChars);
            var columnName = new string(columnNameSpan); // TODO: get rid of this allocation, use trie like with keywords?

            var token = new ColumnToken(
                startPosition.Value, 
                columnNameSpan.Length,
                info.Config.GetColumnInfo(columnName));

            if (token.ColumnInfo == null)
            {
                info.Reader.MoveTo(startPosition);
                return TokenizationResult.Failed(CreateError(ref info));
            }

            return TokenizationResult.Successful(token); // TODO: EnsureTrailingSpecialChar?
        }

        [HotPath]
        protected override Error CreateError(ref LexerRuntimeInfo info)
        {
            var position = info.Reader.CurrentPosition;
            var columnNameSpan = info.Reader.ReadTillIvalidCharOrWhitespace(info.Config.Syntax.ReservedChars);
            var columnName = new string(columnNameSpan); // TODO: get rid of this allocation
            return new Error(
                $"\"{columnName}\" is not a column.", 
                ErrorId.ColumnNotFound, 
                new Error.RangeInfo(position, columnNameSpan.Length), 
                columnName);
        }
    }
}