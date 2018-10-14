using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Tokens.Abstract;
using static System.Linq.Expressions.Expression;

namespace HandyQuery.Language.Lexing.Tokens
{
    internal sealed class NumberLiteralToken : TokenBase, IEquatable<NumberLiteralToken>
    {
        public override TokenType TokenType => TokenType.NumberLiteral;

        public NumberLiteralToken(int startPosition, int length) : base(startPosition, length)
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
        }

        public bool Equals(NumberLiteralToken other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is NumberLiteralToken other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        private static class LiteralEvaluator<T> where T : struct
        {
            public static bool IsEvaluable => Evaluate != null;

            private delegate bool EvaluateDelegate(string query, int startPosition, int length,
                NumberFormatInfo formatInfo, out T result);

            private static readonly MethodInfo TryParseMethodInfo;
            private static readonly MethodInfo SliceMethodInfo;
            private static readonly NumberStyles NumberStyles;
            
            private static readonly EvaluateDelegate Evaluate;

            static LiteralEvaluator()
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Byte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        NumberStyles = NumberStyles.None;
                        break;
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        NumberStyles = NumberStyles.AllowLeadingSign;
                        break;
                    case TypeCode.Single: // float
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                        NumberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
                        break;
                    default:
                        return;
                }
                
                TryParseMethodInfo = typeof(T).GetMethod("TryParse", new[]
                {
                    typeof(ReadOnlySpan<char>),
                    typeof(NumberStyles),
                    typeof(IFormatProvider),
                    typeof(T).MakeByRefType()
                });
                
                SliceMethodInfo = typeof(LiteralEvaluator<T>)
                    .GetMethod("Slice", BindingFlags.NonPublic | BindingFlags.Static);
                
                Evaluate = GenerateEvaluateFunction();
            }

            public static bool TryEvaluate(string query, int startPosition, int length, NumberFormatInfo formatInfo,
                out T value)
            {
                return Evaluate(query, startPosition, length, formatInfo, out value);
            }

            // ReSharper disable once UnusedMember.Local
            private static ReadOnlySpan<char> Slice(string query, int startPosition, int length)
                => query.AsSpan().Slice(startPosition, length);

            /// <remarks>
            /// Generates following code:
            /// 
            /// static bool Eval{T}(string query, int startPosition, int length,
            ///   NumberFormatInfo formatInfo, out T result)
            /// {
            ///   return {T}.TryParse(slice, _numberStyles, formatInfo, out var result);
            /// }
            /// </remarks>
            private static EvaluateDelegate GenerateEvaluateFunction()
            {
                if (TryParseMethodInfo == null || SliceMethodInfo == null)
                {
                    return null;
                }

                var instructions = new List<Expression>();

                var queryParam = Parameter(typeof(string), "query");
                var startPositionParam = Parameter(typeof(int), "startPosition");
                var lengthParam = Parameter(typeof(int), "lengthPosition");
                var formatInfoParam = Parameter(typeof(NumberFormatInfo), "format");
                var result = Parameter(typeof(T).MakeByRefType(), "result"); // out T result

                // <T>.TryParse()
                var tryParseCallExpression = Call(
                    TryParseMethodInfo,
                    Call(SliceMethodInfo, queryParam, startPositionParam, lengthParam), // this.Slice(...)
                    Constant(NumberStyles),
                    formatInfoParam,
                    result // out result
                );

                instructions.Add(tryParseCallExpression);

                var final = Lambda<EvaluateDelegate>(
                    body: Block(instructions),
                    parameters: new[] {queryParam, startPositionParam, lengthParam, formatInfoParam, result}
                ).Compile();

                return final;
            }
        }
    }
}