using System.Collections.Generic;
using FluentAssertions;
using HandyQuery.Language.Lexing.Tokens;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Lexing.Tokens
{
    public class NumberLiteralTokenTests
    {
        [TestCaseSource(nameof(TestCases))]
        public void NumberLiteralToken_TryEvaluate<T>(string number, T expectedResult, bool expectedEvaluated) 
            where T : struct 
        {
            var token = new NumberLiteralToken(0, number.Length);
            var syntaxConfig = HandyQueryLanguage.BuildSyntax().Build();
            
            var evaluated = token.TryEvaluate<T>(number, syntaxConfig, out var result);

            result.Should().Be(expectedResult);
            evaluated.Should().Be(expectedEvaluated);
        }
        
        public static IEnumerable<TestCaseData> TestCases()
        {
            // float
            yield return new TestCaseData("0", 0f, true);
            yield return new TestCaseData("0.01", 0.01f, true);
            yield return new TestCaseData("-0.5", -0.5f, true);
            yield return new TestCaseData("15", 15f, true);
            yield return new TestCaseData("-15", -15f, true);
            yield return new TestCaseData(long.MaxValue.ToString(), (float)long.MaxValue, true);
            yield return new TestCaseData(ulong.MaxValue.ToString(), (float)ulong.MaxValue, true);
            yield return new TestCaseData(long.MinValue.ToString(), (float)long.MinValue, true);
            yield return new TestCaseData("a", 0f, false);
            yield return new TestCaseData("not_a_number", 0f, false);
            
            // decimal
            yield return new TestCaseData("0", 0m, true);
            yield return new TestCaseData("0.01", 0.01m, true);
            yield return new TestCaseData("-0.5", -0.5m, true);
            yield return new TestCaseData("15", 15m, true);
            yield return new TestCaseData("-15", -15m, true);
            yield return new TestCaseData(long.MaxValue.ToString(), (decimal)long.MaxValue, true);
            yield return new TestCaseData(ulong.MaxValue.ToString(), (decimal)ulong.MaxValue, true);
            yield return new TestCaseData(long.MinValue.ToString(), (decimal)long.MinValue, true);
            yield return new TestCaseData("a", 0m, false);
            yield return new TestCaseData("not_a_number", 0m, false);
            
            // double
            yield return new TestCaseData("0", 0d, true);
            yield return new TestCaseData("0.01", 0.01d, true);
            yield return new TestCaseData("-0.5", -0.5d, true);
            yield return new TestCaseData("15", 15d, true);
            yield return new TestCaseData("-15", -15d, true);
            yield return new TestCaseData(long.MaxValue.ToString(), (double)long.MaxValue, true);
            yield return new TestCaseData(ulong.MaxValue.ToString(), (double)ulong.MaxValue, true);
            yield return new TestCaseData(long.MinValue.ToString(), (double)long.MinValue, true);
            yield return new TestCaseData("a", 0d, false);
            yield return new TestCaseData("not_a_number", 0d, false);
            
            // byte
            yield return new TestCaseData("0", (byte)0, true);
            yield return new TestCaseData("15", (byte)15, true);
            yield return new TestCaseData("-15", default(byte), false);
            yield return new TestCaseData(byte.MaxValue.ToString(), byte.MaxValue, true);
            yield return new TestCaseData(byte.MinValue.ToString(), byte.MinValue, true);
            yield return new TestCaseData(long.MaxValue.ToString(), default(byte), false);
            yield return new TestCaseData(long.MinValue.ToString(), default(byte), false);
            yield return new TestCaseData("a", default(byte), false);
            yield return new TestCaseData("not_a_number", default(byte), false);
            
            // uint16
            yield return new TestCaseData("0", (ushort)0, true);
            yield return new TestCaseData("15", (ushort)15, true);
            yield return new TestCaseData("-15", default(ushort), false);
            yield return new TestCaseData(ushort.MaxValue.ToString(), ushort.MaxValue, true);
            yield return new TestCaseData(ushort.MinValue.ToString(), ushort.MinValue, true);
            yield return new TestCaseData(long.MaxValue.ToString(), default(ushort), false);
            yield return new TestCaseData(long.MinValue.ToString(), default(ushort), false);
            yield return new TestCaseData("a", default(ushort), false);
            yield return new TestCaseData("not_a_number", default(ushort), false);
            
            // uint32
            yield return new TestCaseData("0", (uint)0, true);
            yield return new TestCaseData("15", (uint)15, true);
            yield return new TestCaseData("-15", default(uint), false);
            yield return new TestCaseData(uint.MaxValue.ToString(), uint.MaxValue, true);
            yield return new TestCaseData(uint.MinValue.ToString(), uint.MinValue, true);
            yield return new TestCaseData(long.MaxValue.ToString(), default(uint), false);
            yield return new TestCaseData(long.MinValue.ToString(), default(uint), false);
            yield return new TestCaseData("a", default(uint), false);
            yield return new TestCaseData("not_a_number", default(uint), false);
            
            // uint64
            yield return new TestCaseData("0", (ulong)0, true);
            yield return new TestCaseData("15", (ulong)15, true);
            yield return new TestCaseData("-15", default(ulong), false);
            yield return new TestCaseData(ulong.MaxValue.ToString(), ulong.MaxValue, true);
            yield return new TestCaseData(ulong.MinValue.ToString(), ulong.MinValue, true);
            yield return new TestCaseData(long.MaxValue.ToString(), (ulong)long.MaxValue, true);
            yield return new TestCaseData(long.MinValue.ToString(), default(ulong), false);
            yield return new TestCaseData("a", default(ulong), false);
            yield return new TestCaseData("not_a_number", default(ulong), false);
            
            // sbyte
            yield return new TestCaseData("0", (sbyte)0, true);
            yield return new TestCaseData("15", (sbyte)15, true);
            yield return new TestCaseData("-15", (sbyte)-15, true);
            yield return new TestCaseData(sbyte.MaxValue.ToString(), sbyte.MaxValue, true);
            yield return new TestCaseData(sbyte.MinValue.ToString(), sbyte.MinValue, true);
            yield return new TestCaseData(long.MaxValue.ToString(), default(sbyte), false);
            yield return new TestCaseData(long.MinValue.ToString(), default(sbyte), false);
            yield return new TestCaseData("a", default(sbyte), false);
            yield return new TestCaseData("not_a_number", default(sbyte), false);
            
            // int16
            yield return new TestCaseData("0", (short)0, true);
            yield return new TestCaseData("15", (short)15, true);
            yield return new TestCaseData("-15", (short)-15, true);
            yield return new TestCaseData(short.MaxValue.ToString(), short.MaxValue, true);
            yield return new TestCaseData(short.MinValue.ToString(), short.MinValue, true);
            yield return new TestCaseData(long.MaxValue.ToString(), default(short), false);
            yield return new TestCaseData(long.MinValue.ToString(), default(short), false);
            yield return new TestCaseData("a", default(short), false);
            yield return new TestCaseData("not_a_number", default(short), false);
            
            // int32
            yield return new TestCaseData("0", 0, true);
            yield return new TestCaseData("15", 15, true);
            yield return new TestCaseData("-15", -15, true);
            yield return new TestCaseData(int.MaxValue.ToString(), int.MaxValue, true);
            yield return new TestCaseData(int.MinValue.ToString(), int.MinValue, true);
            yield return new TestCaseData(long.MaxValue.ToString(), default(int), false);
            yield return new TestCaseData(long.MinValue.ToString(), default(int), false);
            yield return new TestCaseData("a", default(int), false);
            yield return new TestCaseData("not_a_number", default(int), false);
            
            // int64
            yield return new TestCaseData("0", (long)0, true);
            yield return new TestCaseData("15", (long)15, true);
            yield return new TestCaseData("-15", (long)-15, true);
            yield return new TestCaseData(long.MaxValue.ToString(), long.MaxValue, true);
            yield return new TestCaseData(long.MinValue.ToString(), long.MinValue, true);
            yield return new TestCaseData("-9223372036854775809", default(long), false);
            yield return new TestCaseData("9223372036854775808", default(long), false);
            yield return new TestCaseData("a", default(long), false);
            yield return new TestCaseData("not_a_number", default(long), false);
            
            // char
            yield return new TestCaseData("1", default(char), false);
        }
    }
}