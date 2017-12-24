using System;
using System.Collections.Generic;
using System.Reflection;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language
{
    internal sealed class TokenizersSource
    {
        private readonly Dictionary<string, ITokenizer> _tokenizers = new Dictionary<string, ITokenizer>();

        public TokenizersSource()
        {
            foreach (var type in Assembly.GetExecutingAssembly().DefinedTypes)
            {
                if (type.IsInterface || type.IsAbstract)
                {
                    continue;
                }

                if (typeof(ITokenizer).IsAssignableFrom(type))
                {
                    const string suffix = "Tokenizer";
                    var index = type.Name.LastIndexOf(suffix, StringComparison.Ordinal);
                    if (index == -1)
                    {
                        throw new HandyQueryInitializationException($"Tokenizer {type.FullName} implements ITokenizer, but " +
                                                                    $"it does not follow naming convention. Each tokenizer should end " +
                                                                    $"with '{suffix}' suffix.");
                    }

                    var name = type.Name.Substring(0, index);
                    _tokenizers.Add(name, (ITokenizer)Activator.CreateInstance(type));
                }
            }
        }

        public bool TryGetTokenizer(string name, out ITokenizer tokenizer)
        {
            return _tokenizers.TryGetValue(name, out tokenizer);
        }

        public ITokenizer GetTokenizer(string name)
        {
            ITokenizer tokenizer;
            if (TryGetTokenizer(name, out tokenizer) == false)
            {
                throw new QueryLanguageException($"Terminal '{name}' not found.");
            }

            return tokenizer;
        }
    }
}