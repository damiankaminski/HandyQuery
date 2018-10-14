using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Tokens;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Tests.Lexing
{
    internal class TestQueryBuilder
    {
        private readonly List<PartBase> _queryParts = new List<PartBase>();

        public static ColumnPart Column(string columnName)
        {
            return new TestQueryBuilder().AddColumn(columnName);
        }
        
        public ColumnPart AddColumn(string columnName)
        {
            var part = new ColumnPart(columnName, this);
            _queryParts.Add(part);
            return part;
        }

        public IEnumerable<BuildResult> BuildMultipleVariants(LanguageConfig languageConfig)
        {
            // TODO: instead of receiving languageConfig build one here with different settings?

            var variants = new BuildVariants();

            foreach (var part in _queryParts)
            {
                switch (part)
                {
                    case ColumnPart column:
                    {
                        // TODO: quoted column name, especially when with multiple words
                        variants.AddTokenToAll(column.ColumnName,
                            index => new TextLiteralToken(index, column.ColumnName.Length, column.ColumnName));
                        break;
                    }

                    case StatementPart statement:
                    {
                        variants.AppendToAll(" ");

                        var keyword = languageConfig.Syntax.KeywordsMap[statement.StatementKeyword];
                        variants.AddTokenToAll(keyword,
                            index => new KeywordToken(index, keyword.Length, statement.StatementKeyword));
                        break;
                    }

                    case CompareOperatorPart compareOperator:
                    {
                        var keyword = languageConfig.Syntax.KeywordsMap[compareOperator.CompareOperatorKeyword];

                        // produce variant without whitespace if comp op starts with special char
                        var newVariant = char.IsLetterOrDigit(keyword.First()) ? null : variants[0].Copy();
                        variants.AppendToAll(" ", exceptNewVariant: newVariant);

                        variants.AddTokenToAll(keyword,
                            index => new KeywordToken(index, keyword.Length, compareOperator.CompareOperatorKeyword));

                        // produce variant without whitespace if comp op ends with special char
                        newVariant = char.IsLetterOrDigit(keyword.Last()) ? null : variants[0].Copy();
                        variants.AppendToAll(" ", exceptNewVariant: newVariant);

                        string operand;

                        switch (compareOperator.Value)
                        {
                            case string s:
                                var noQuoteVariant = variants[0].Copy();

                                operand = $"\"{s}\"";
                                variants.AddTokenToAll(operand,
                                    index => new TextLiteralToken(index, operand.Length, s));

                                if (s.Trim().Any(char.IsWhiteSpace) == false)
                                {
                                    // produce variant without quote if single word
                                    noQuoteVariant.AddToken(s, index => new TextLiteralToken(index, s.Length, s));
                                    variants.Add(noQuoteVariant);
                                }

                                break;
                            case float v:
                                operand = v.ToString(languageConfig.Syntax.CultureInfo);
                                variants.AddTokenToAll(operand,
                                    index => new NumberLiteralToken(index, operand.Length, v));
                                break;
                            case decimal v:
                                operand = v.ToString(languageConfig.Syntax.CultureInfo);
                                variants.AddTokenToAll(operand,
                                    index => new NumberLiteralToken(index, operand.Length, v));
                                break;
                            case double v:
                                operand = v.ToString(languageConfig.Syntax.CultureInfo);
                                variants.AddTokenToAll(operand,
                                    index => new NumberLiteralToken(index, operand.Length, v));
                                break;
                            // TODO: all types
                            
                            default:
                                throw new NotSupportedException();

                            // TODO: all  other cases
                        }

                        break;
                    }

                    default:
                        throw new NotSupportedException();
                }
            }

            return variants.CreateBuildResults();
        }

        public class BuildVariants : Collection<BuildVariants.Variant>
        {
            public BuildVariants()
            {
                Add(new Variant());
            }
            
            public void AppendToAll(string queryPart, Variant exceptNewVariant = null)
            {
                foreach (var variant in this)
                    variant.ExtendVariant(queryPart);

                if (exceptNewVariant != null)
                    Add(exceptNewVariant);
            }
            
            public void AddTokenToAll(string queryPart, Func<int, TokenBase> createToken)
            {
                foreach (var variant in this)
                    variant.AddToken(queryPart, createToken);
            }

            public IEnumerable<BuildResult> CreateBuildResults()
            {
                return this.Select(x => new BuildResult(x.Query, x.ExpectedTokens)).ToList();
            }
            
            public class Variant
            {
                public string Query { get; private set; } = string.Empty;
                public TokenList ExpectedTokens { get; private set; } = new TokenList();

                public void ExtendVariant(string queryPart)
                {
                    Query += queryPart;
                }

                public void AddToken(string queryPart, Func<int, TokenBase> createToken)
                {
                    ExpectedTokens.Add(createToken(Query.Length));
                    ExtendVariant(queryPart);
                }

                public Variant Copy()
                {
                    return new Variant()
                    {
                        Query = Query,
                        ExpectedTokens = new TokenList(ExpectedTokens)
                    };
                }
            }
        }

        public class BuildResult
        {
            public TokenList ExpectedTokens { get; }
            public string Query { get; }

            public BuildResult(string query, TokenList expectedTokens)
            {
                Query = query;
                ExpectedTokens = expectedTokens;
            }
        }

        public abstract class PartBase
        {
            public TestQueryBuilder TestQueryBuilder { get; }

            protected PartBase(TestQueryBuilder testQueryBuilder)
            {
                TestQueryBuilder = testQueryBuilder;
            }
        }

        public class ColumnPart : PartBase
        {
            public readonly string ColumnName;

            public ColumnPart(string columnName, TestQueryBuilder testQueryBuilder) : base(testQueryBuilder)
            {
                ColumnName = columnName;
            }

            public StatementPart IsEmpty() => Build(StatementKeyword.IsEmpty);
            public StatementPart IsNotEmpty() => Build(StatementKeyword.IsNotEmpty);
            public StatementPart IsTrue() => Build(StatementKeyword.IsTrue);
            public StatementPart IsFalse() => Build(StatementKeyword.IsFalse);

            public CompareOperatorPart Equal(string value) => Build(CompareOperatorKeyword.Equal, value);
            public CompareOperatorPart Equal(char value) => Build(CompareOperatorKeyword.Equal, value); // TODO: not sure if we'll support this one?
            public CompareOperatorPart Equal(byte value) => Build(CompareOperatorKeyword.Equal, value);
            public CompareOperatorPart Equal(sbyte value) => Build(CompareOperatorKeyword.Equal, value);
            public CompareOperatorPart Equal(float value) => Build(CompareOperatorKeyword.Equal, value);
            public CompareOperatorPart Equal(decimal value) => Build(CompareOperatorKeyword.Equal, value);
            public CompareOperatorPart Equal(double value) => Build(CompareOperatorKeyword.Equal, value);
            public CompareOperatorPart Equal(short value) => Build(CompareOperatorKeyword.Equal, value);
            public CompareOperatorPart Equal(ushort value) => Build(CompareOperatorKeyword.Equal, value);
            public CompareOperatorPart Equal(int value) => Build(CompareOperatorKeyword.Equal, value);
            public CompareOperatorPart Equal(uint value) => Build(CompareOperatorKeyword.Equal, value);
            public CompareOperatorPart Equal(long value) => Build(CompareOperatorKeyword.Equal, value);
            public CompareOperatorPart Equal(ulong value) => Build(CompareOperatorKeyword.Equal, value);
            
            // TODO: all operators
//            public CompareOperatorPart NotEqual() => BuildCompareOperatorPart(CompareOperatorKeyword.NotEqual);
//            public CompareOperatorPart GreaterThan() => BuildCompareOperatorPart(CompareOperatorKeyword.GreaterThan);
//            public CompareOperatorPart LessThan() => BuildCompareOperatorPart(CompareOperatorKeyword.LessThan);
//            public CompareOperatorPart GreaterThanOrEqual() => BuildCompareOperatorPart(CompareOperatorKeyword.GreaterThanOrEqual);
//            public CompareOperatorPart LessThanOrEqual() => BuildCompareOperatorPart(CompareOperatorKeyword.LessThanOrEqual);
//            public CompareOperatorPart StartsWith() => BuildCompareOperatorPart(CompareOperatorKeyword.StartsWith);
//            public CompareOperatorPart EndsWith() => BuildCompareOperatorPart(CompareOperatorKeyword.EndsWith);
//            public CompareOperatorPart Contains() => BuildCompareOperatorPart(CompareOperatorKeyword.Contains);

            private StatementPart Build(StatementKeyword statementKeyword)
            {
                var part = new StatementPart(statementKeyword, TestQueryBuilder);
                TestQueryBuilder._queryParts.Add(part);
                return part;
            }

            private CompareOperatorPart Build(CompareOperatorKeyword compareOperatorKeyword,
                object value)
            {
                var part = new CompareOperatorPart(compareOperatorKeyword, value, TestQueryBuilder);
                TestQueryBuilder._queryParts.Add(part);
                return part;
            }
        }

        public class StatementPart : PartBase
        {
            public readonly StatementKeyword StatementKeyword;

            public StatementPart(StatementKeyword statementKeyword, TestQueryBuilder testQueryBuilder) : base(
                testQueryBuilder)
            {
                StatementKeyword = statementKeyword;
            }
        }

        public class CompareOperatorPart : PartBase
        {
            public readonly CompareOperatorKeyword CompareOperatorKeyword;
            public readonly object Value;

            public CompareOperatorPart(CompareOperatorKeyword compareOperatorKeyword, object value,
                TestQueryBuilder testQueryBuilder) : base(testQueryBuilder)
            {
                CompareOperatorKeyword = compareOperatorKeyword;
                Value = value;
            }
        }
    }
}