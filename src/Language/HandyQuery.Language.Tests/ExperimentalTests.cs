using System;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Tests.Model;
using NUnit.Framework;

namespace HandyQuery.Language.Tests
{
    public class ExperimentalTests
    {
        [Test]
        public void T()
        {
            var config = HandyQueryLanguage.Configure<Person>().Build();
            var tokenizer = new KeywordTokenizer();
            var context = new LexingConfigContext();

            tokenizer.CreateConfigContext(ref context, config);
        }
    }
}

namespace Test2
{
    public interface ITokenizer
    {
        void Tokenize();
    }

    public struct KeywordTokenizer : ITokenizer
    {
        public void Tokenize()
        {
        }
    }

    public class Program
    {
        public static void Main()
        {
            var tokenizer = CreateTokenizer();
            UseTokenizer(tokenizer);
        }

        public static void UseTokenizer<T>(T tokenizer) where T : ITokenizer
        {
            tokenizer.Tokenize();
        }

        public static ITokenizer CreateTokenizer()
        {
            return new KeywordTokenizer();
        }
    }
}