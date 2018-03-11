using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    [PerformanceCritical]
    internal sealed class ColumnNameTokenizer : ITokenizer
    {
        public TokenizationResult Tokenize(LexerRuntimeInfo info)
        {
            var startPosition = info.Reader.CurrentPosition;
            
            // TODO: make sure there is no struct copy here
            // TODO: it probably is copied because of readonly on Reader in LexerRuntimeInfo...
            var columnNameSpan = info.Reader.ReadTillIvalidCharOrWhitespace(info.Config.SyntaxConfig.ReservedChars);
            var columnName = new string(columnNameSpan); // TODO: get rid of this allocation

            var token = new ColumnToken(
                startPosition, 
                columnNameSpan.Length,
                info.Config.GetColumnInfo(columnName));

            if (token.ColumnInfo == null)
            {
                var error = new Error($"\"{columnName}\" is not a column.", ErrorId.ColumnNotFound, columnName);
                return TokenizationResult.Failed(error);
            }

            return TokenizationResult.Successful(token);
        }
    }
}