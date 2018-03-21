using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    internal sealed class ColumnNameTokenizer : TokenizerBase
    {
        [HotPath]
        public override TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        {
            var startPosition = info.Reader.CurrentPosition;
            
            var columnNameSpan = info.Reader.ReadTillIvalidCharOrWhitespace(info.Config.Syntax.ReservedChars);
            var columnName = new string(columnNameSpan); // TODO: get rid of this allocation

            var token = new ColumnToken(
                startPosition, 
                columnNameSpan.Length,
                info.Config.GetColumnInfo(columnName));

            if (token.ColumnInfo == null)
            {
                return TokenizationResult.Failed(CreateError(ref info));
            }

            return TokenizationResult.Successful(token);
        }

        [HotPath]
        protected override Error CreateError(ref LexerRuntimeInfo info)
        {
            var columnNameSpan = info.Reader.ReadTillIvalidCharOrWhitespace(info.Config.Syntax.ReservedChars);
            var columnName = new string(columnNameSpan); // TODO: get rid of this allocation
            return new Error($"\"{columnName}\" is not a column.", ErrorId.ColumnNotFound, columnName);
        }
    }
}