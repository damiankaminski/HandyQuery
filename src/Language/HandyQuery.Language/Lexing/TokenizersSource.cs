using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing
{
    internal sealed class TokenizersSource
    {
        public IEnumerable<ITokenizer> OrderedTokenizers => _orderedTokenizers;
        private readonly List<ITokenizer> _orderedTokenizers = new List<ITokenizer>();

        public TokenizersSource(LanguageConfig languageConfig)
        {
            // TODO: tests
            var tokenizers = new List<ITokenizer>();

            foreach (var type in Assembly.GetExecutingAssembly().DefinedTypes)
            {
                if (type.IsInterface || type.IsAbstract) continue;
                if (!typeof(ITokenizer).IsAssignableFrom(type)) continue;

                const string suffix = "Tokenizer";
                var index = type.Name.LastIndexOf(suffix, StringComparison.Ordinal);
                if (index == -1)
                {
                    throw new HandyQueryInitializationException(
                        $"Tokenizer {type.FullName} implements ITokenizer, but it does not follow naming " +
                        $"convention. Each tokenizer should end with \'{suffix}\' suffix.");
                }

                if (type.GetCustomAttribute<TokenizerAttribute>() == null)
                {
                    throw new HandyQueryInitializationException(
                        $"Tokenizer {type.FullName} implements ITokenizer, but it does not follow attribute " +
                        $"convention. Each tokenizer should define TokenizerAttribute .");
                }

                var tokenizer = (ITokenizer) Activator.CreateInstance(type, languageConfig);
                tokenizers.Add(tokenizer);
            }

            var waitingTokenizers = new List<ITokenizer>();
            foreach (var tokenizer in tokenizers)
            {
                EvaluateWaiting();
                var tokenizerAttribute = GetTokenizerAttribute(tokenizer);
                if (tokenizerAttribute.ManualUsage) continue;
                
                var after = tokenizerAttribute.AfterTokenizer;

                if (after == null || _orderedTokenizers.Any(x => x.GetType() == after))
                {
                    _orderedTokenizers.Add(tokenizer);
                    continue;
                }
                
                waitingTokenizers.Add(tokenizer);
            }
            
            EvaluateWaiting();

            if (waitingTokenizers.Any())
            {
                throw new HandyQueryInitializationException(
                    "Unable to create tokenizers in proper order. Inifnite loop via TokenizerAttribute.AfterTokenizer?");
            }
            
            void EvaluateWaiting()
            {
                var evaluatedTokenizers = new List<ITokenizer>();
                
                foreach (var tokenizer in waitingTokenizers)
                {
                    var after = GetTokenizerAttribute(tokenizer).AfterTokenizer;
                    if (_orderedTokenizers.Any(x => x.GetType() == after))
                    {
                        _orderedTokenizers.Add(tokenizer);
                        evaluatedTokenizers.Add(tokenizer);
                    }                    
                }
                
                foreach (var tokenizer in evaluatedTokenizers)
                {
                    waitingTokenizers.Remove(tokenizer);
                }
            }
            
            TokenizerAttribute GetTokenizerAttribute(ITokenizer tokenizer)
            {
                return tokenizer.GetType().GetCustomAttribute<TokenizerAttribute>();
            }
        }
    }
}