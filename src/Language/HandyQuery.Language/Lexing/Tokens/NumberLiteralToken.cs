using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Tokens.Abstract;
using static System.Linq.Expressions.Expression;

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
            if (LiteralEvaluator<T>.IsEvaluable == false)
            {
                result = default;
                return false;
            }
            
            return LiteralEvaluator<T>.TryEvaluate(query, StartPosition, Length,
                syntaxConfig.CultureInfo.NumberFormat, out result);

            // TODO: support these types:
            // byte
            // sbyte
            // float
            // decimal
            // double
            // short
            // ushort
            // int
            // uint
            // long
            // ulong
        }

        private static class LiteralEvaluator<T> where T : struct
        {
            private static readonly EvaluateDelegate Evaluate = GenerateEvaluateFunction();

            private static readonly MethodInfo TryParseMethodInfo = typeof(T).GetMethod("TryParse", new[]
            {
                typeof(ReadOnlySpan<char>),
                typeof(NumberStyles),
                typeof(IFormatProvider),
                typeof(T)
            });

            public static bool IsEvaluable => TryParseMethodInfo != null;
            
            public static bool TryEvaluate(string query, int startPosition, int length, NumberFormatInfo formatInfo,
                out T value)
            {
                var res = Evaluate(query, startPosition, length, formatInfo);
                value = res.Value;
                return res.Parsed;
            }

            private delegate ParsingResult EvaluateDelegate(string query, int startPosition, int length,
                NumberFormatInfo formatInfo);

            /// <remarks>
            /// Generates following code:
            /// 
            /// static ParsingResult{T} Eval{T}(string query, int startPosition, int length, NumberFormatInfo formatInfo) {
            ///   var result = new ParsingResult{T}();
            ///   var number = query.AsSpan().Slice(startPosition, length);
            ///   var parsed = {T}.TryParse(number, NumberStyles.None, formatInfo, out var value);
            ///   result.Parsed = parsed;
            ///   result.Value = value;
            ///   return result;
            /// }
            /// </remarks>
            private static EvaluateDelegate GenerateEvaluateFunction()
            {
                // TODO: implement!
                
                
                
                
                var formatInfoExpression = Parameter(typeof(NumberFormatInfo), "format");

                // TODO: generate ParsingResult<T> and return instead of byte

                // var result = new ParsingResult<T>();
                var resultExpression = Variable(typeof(ParsingResult), "result");

                // result.Parsed
                var parsedFieldAccess = MakeMemberAccess(resultExpression, typeof(ParsingResult).GetField("Parsed"));

                // result.Value
                var valueFieldAccess = MakeMemberAccess(resultExpression, typeof(ParsingResult).GetField("Value"));

                var valueExpression = Variable(typeof(T), "value");

                // <T>.TryParse()
                var tryParseCallExpression = Call(
                    TryParseMethodInfo,
                    Constant(NumberStyles.None),
                    formatInfoExpression,
                    valueExpression // TODO: out? could it be an issue?
                );

                // TODO: should return bool and T, create custom struct?
                var final = Lambda<EvaluateDelegate>(
                    body: Block(
                        new[] {resultExpression},
                        tryParseCallExpression,
                        resultExpression),
                    parameters: new[] {formatInfoExpression}
                ).Compile();

                return final;
            }

            private struct ParsingResult
            {
                public bool Parsed;
                public T Value;
            }
        }
    }
}