using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Tests.Model;
using NUnit.Framework;

using static HandyQuery.Language.Tests.Lexing.TestQueryBuilder;

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
        
        // ReSharper disable MemberCanBePrivate.Global
        public static IEnumerable<TestCase> Lexer_should_tokenize_valid_queries_test_cases
        {
            // ReSharper disable once UnusedMember.Global
            get
            {
                var config = HandyQueryLanguage.Configure<Person>().Build();
                
                var cases = new List<PartBase>
                {
                    Column("FirstName").IsEmpty(),
                    Column("FirstName").IsNotEmpty(),
                    Column("Checked").IsTrue(),
                    Column("Checked").IsFalse(),
                    
                    // TODO: test all compare operators
                    // TODO: all types
                    Column("Description").Equal("Foo"),
                    Column("Description").Equal("Foo Bar"),
                    
                    // TODO: number tokenizers
//                    Column("Salary").Equal(2000m),
//                    Column("Salary").Equal(2000.56m),
//                    Column("Salary").Equal(decimal.MaxValue),
//                    Column("Salary").Equal(decimal.MinValue),
//                    Column("Salary").Equal(decimal.Zero),
//                    Column("Height").Equal(180f),
//                    Column("Height").Equal(180.56f),
//                    Column("Height").Equal(float.MaxValue),
//                    Column("Height").Equal(float.MinValue),
//                    Column("Height").Equal(0f)
                };
                
                // TODO: MOAR tests

                return cases.SelectMany(BuildTestCase).ToList();
                
                IEnumerable<TestCase> BuildTestCase(PartBase partBase)
                {
                    var buildResults = partBase.TestQueryBuilder.BuildMultipleVariants(config);
                    foreach (var result in buildResults)
                    {
                        yield return new TestCase(result.Query, result.ExpectedTokens, config);
                    }
                }
            }
        }
    }
}
