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