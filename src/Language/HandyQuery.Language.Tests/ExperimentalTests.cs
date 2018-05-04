using System.Linq;
using System.Runtime.InteropServices;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Tests.Model;
using NUnit.Framework;

namespace HandyQuery.Language.Tests
{
    public class ExperimentalTests
    {
        // TODO: other
        // - create new tokenizer for each language configuration? it makes sense as it could affect the way things
        //   are tokenized

        // TODO: concept 5
        // use context struct similar to the one in concept 2, but this time with struct that is created for each 
        // new position (like in concept 4)

        // TODO: concept 4 (doesn't make sense in multi threaded environment, field cannot be set to anything to keep state, that would affect other threads)
        // leave tokenizers as classes as they are, but in case of keyword tokenizer create a sepearate struct
        // for each new position (information if it's new position or not should be given by Lexer)

        // TODO: concept 3 (doesn't make sense, because of no real workarounds for boxing issues)
        // change tokenizers to structs and create new one for each position
        // then it would allow to store keyword tokenization result for a given position
        // and keyword tokenizer would became multiinvoke capable (would calculate actual token only once)
        // interface is out of scope with structs though :/

        // TODO: concept 2
//        internal class KeywordTokenizer : TokenizerBase2
//        {
//            public TokenizationResult Tokenize(ref LexingConfigContext lexingConfigContext)
//            {
//                var context = new KeywordLexingConfigContext(ref lexingConfigContext);
//                var searchTrie = context.KeywordsSearchTrie;
//                return null;
//            }
//
//            public override LexingConfigContext CreateConfigContext(ref LexingConfigContext memory,
//                LanguageConfig config)
//            {
//                var keywordsTrie = SearchTrie<Keyword>.Create(
//                    config.Syntax.KeywordCaseSensitive,
//                    config.Syntax.KeywordsMap.ToDictionary(x => x.Value,
//                        x => x.Key)); // TODO: maybe change typeof KeywordsMap so that this map won't be needed?
//
//                var lexingContext = new KeywordLexingConfigContext(ref memory);
//                lexingContext.KeywordsSearchTrie = keywordsTrie;
//
//                return memory;
//            }
//        }
//
//        internal class TokenizerBase2
//        {
//            public virtual LexingConfigContext CreateConfigContext(ref LexingConfigContext memory,
//                LanguageConfig config)
//            {
//                return memory;
//            }
//        }
//
//        [StructLayout(LayoutKind.Explicit)]
//        internal ref struct KeywordLexingConfigContext
//        {
//            // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
//            // Used as pointer to stack memory.
//            // ReSharper disable once FieldCanBeMadeReadOnly.Local
//            // It is mutated by _keywordsSearchTrie which is at the very same offset.
//            [FieldOffset(0)] private LexingConfigContext _memory;
//
//            [FieldOffset(0)] private SearchTrie<Keyword> _keywordsSearchTrie;
//
//            public SearchTrie<Keyword> KeywordsSearchTrie
//            {
//                get => _keywordsSearchTrie;
//                set => _keywordsSearchTrie = value;
//            }
//
//            public KeywordLexingConfigContext(ref LexingConfigContext memory)
//            {
//                _keywordsSearchTrie = null;
//                _memory = memory;
//            }
//        }
//
//        /// <summary>
//        /// Provides stack memory to be used by tokenizers if needed.
//        /// </summary>
//        [StructLayout(LayoutKind.Explicit)]
//        internal ref struct LexingConfigContext
//        {
//            // ReSharper disable once FieldCanBeMadeReadOnly.Local
//            [FieldOffset(0)] private long _item1;
//        }

        // TODO: concept:
        //    internal struct KeywordTokenizer : ITokenizer
        //    {
        //        private readonly SearchTrie<Keyword> _keywordsTrie;
        //
        //        public KeywordTokenizer(SearchTrie<Keyword> keywordsTrie)
        //        {
        //            _keywordsTrie = keywordsTrie;
        //        }
        //
        //        public TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        //        {
        //            throw new System.NotImplementedException();
        //        }
        //    }
        //
        //    internal sealed class KeywordTokenizerFactory : TokenizerFactoryBase
        //    {
        //        private readonly SearchTrie<Keyword> _keywordsTrie;
        //        
        //        public KeywordTokenizerFactory(LanguageConfig config) : base(config)
        //        {
        //            _keywordsTrie = SearchTrie<Keyword>.Create(
        //                Config.Syntax.KeywordCaseSensitive, 
        //                config.Syntax.KeywordsMap.ToDictionary(x => x.Value, x => x.Key)); // TODO: maybe change typeof KeywordsMap so that this map won't be needed?
        //        }
        //
        //        public override ITokenizer Create()
        //        {
        //            return new KeywordTokenizer(); // TODO: damn... will need to return proper type...
        //        }
        //    }
        //    
        //    internal sealed class DefaultTokenizerFactory : TokenizerFactoryBase
        //    {
        //        public DefaultTokenizerFactory(LanguageConfig config) : base(config)
        //        {
        //        }
        //
        //        public override ITokenizer Create()
        //        {
        //            throw new System.NotImplementedException();
        //        }
        //    }
        //    
        //    // TODO: should be created once per LanguageConfig
        //    internal abstract class TokenizerFactoryBase
        //    {
        //        protected readonly LanguageConfig Config;
        //
        //        protected TokenizerFactoryBase(LanguageConfig config)
        //        {
        //            Config = config;
        //        }
        //
        //        public abstract ITokenizer Create();
        //    }

        [Test]
        public void T()
        {
//            var config = HandyQueryLanguage.Configure<Person>().Build();
//            var tokenizer = new KeywordTokenizer();
//            var context = new LexingConfigContext();
//
//            tokenizer.CreateConfigContext(ref context, config);
        }
    }
}