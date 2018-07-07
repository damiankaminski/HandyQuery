using System.Collections.Generic;
using FluentAssertions;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Tests.Model;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace HandyQuery.Language.Tests.Lexing
{
    internal class LexingTests
    {
        [TestCaseSource(nameof(Lexer_should_tokenize_valid_queries_test_cases))]
        public void Lexer_should_tokenize_valid_queries(TestCase testCase)
        {
            var result = Lexer.Build(testCase.Config).Tokenize(testCase.Query);

            result.Tokens.IsSameAs(testCase.ExpectedTokens).Should().BeTrue();
        }

        public class TestCase
        {
            public string Query { get; }
            public TokenList ExpectedTokens { get; }
            public LanguageConfig Config { get; }

            public TestCase(string query, TokenList expectedTokens, LanguageConfig config)
            {
                Query = query;
                ExpectedTokens = expectedTokens;
                Config = config;
            }

            public override string ToString()
            {
                return Query;
            }
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public static IEnumerable<TestCase> Lexer_should_tokenize_valid_queries_test_cases
        {
            get
            {
                var config = HandyQueryLanguage.Configure<Person>().Build();
                var testCases = new List<TestCase>();
                
                TestCase(new TestQueryBuilder().Column("FirstName").IsEmpty());
                TestCase(new TestQueryBuilder().Column("FirstName").IsNotEmpty());
                TestCase(new TestQueryBuilder().Column("Checked").IsTrue());
                TestCase(new TestQueryBuilder().Column("Checked").IsFalse());
                
                // TODO: MOAR tests

                return testCases;       
                void TestCase(TestQueryBuilder.PartBase partBase)
                {
                    var buildResults = partBase.TestQueryBuilder.BuildMultipleVariants(config);
                    foreach (var result in buildResults)
                    {
                        testCases.Add(new TestCase(result.Query, result.ExpectedTokens, config));
                    }
                }
            }
        }
    }
}
