using System;
using System.Globalization;
using System.Linq.Expressions;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokens
{
    internal sealed class
        NumberLiteralToken : LiteralTokenBase<object> // TODO: change to : TokenBase, IEquatable<NumberLiteralToken>
    {
        public override TokenType TokenType => TokenType.NumberLiteral;

        public NumberLiteralToken(int startPosition, int length, object value) : base(startPosition, length, value)
        {
        }

        public bool TryEvaluate<T>(string query, SyntaxConfig syntaxConfig, out T result) where T : struct
        {
            var number = query.AsSpan().Slice(StartPosition, Length);
            var reader = new LexerStringReader(number, 0);

            result = default;
            switch (result)
            {
                case byte _:
                    // TODO: generate code?
                    var tryParseMethodInfo = typeof(byte).GetMethod("TryParse", new[]
                    {
                        typeof(ReadOnlySpan<char>),
                        typeof(NumberStyles),
                        typeof(IFormatProvider),
                        typeof(byte) // TODO: out... :(
                    });

                    // TODO: generate ParsingResult<T> and return instead of byte
                    var resultExpression = Expression.Variable(typeof(byte));

                    var tryParseCallExpression = Expression.Call(
                        tryParseMethodInfo,
                        Expression.Constant(NumberStyles.None),
                        Expression.Constant(syntaxConfig.CultureInfo.NumberFormat), // TODO: from param
                        resultExpression // TODO: out... :(
                    );

                    // TODO: should return bool and T, create custom struct?
                    var final = Expression.Lambda<Func<ParsingResult<T>>>(
                        body: Expression.Block(
                            new[] {resultExpression},
                            tryParseCallExpression,
                            resultExpression)
                    ).Compile();


                    var r = final();
                    result = r.Value;
                    return r.Parsed;

                // equivalent of:
//                    if (byte.TryParse(number, NumberStyles.None, syntaxConfig.CultureInfo.NumberFormat, out var r))
//                    {
//                        result = (T) r;
//                        return true;
//                    }
//
//                    result = default;
//                    return false;

//            sbyte
//            float
//            decimal
//            double
//            short
//            ushort
//            int
//            uint
//            long
//            ulong
            }
        }

        private struct ParsingResult<T> where T : struct
        {
            public bool Parsed;
            public T Value;
        }
    }
}