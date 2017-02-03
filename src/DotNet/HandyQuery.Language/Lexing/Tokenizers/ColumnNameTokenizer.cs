using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    internal sealed class ColumnNameTokenizer : ITokenizer
    {
        public TokenizationResult Tokenize(LexerRuntimeInfo info)
        {
            var columnName = info.Reader.ReadTillIvalidCharOrWhitespace(info.Config.SyntaxInfo.ReservedChars);

            var token = new ColumnToken(info.Reader.CurrentPosition, info.Reader.ReadLength, info.Config.GetColumnInfo(columnName));

            if (token.ColumnInfo == null)
            {
                var error = new Error($"\"{columnName}\" is not a column.", ErrorId.ColumnNotFound, columnName);
                return TokenizationResult.Failed(error);
            }

            return TokenizationResult.Successful(token);
        }
    }
}