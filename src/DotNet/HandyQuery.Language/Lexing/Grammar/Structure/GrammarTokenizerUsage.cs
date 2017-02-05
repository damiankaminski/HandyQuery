using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing.Grammar.Structure
{
    internal sealed class GrammarTokenizerUsage : IGrammarBodyItem
    {
        public string Name { get; }
        public bool IsOptional { get; }
        public ITokenizer Impl { get; }

        public GrammarElementType Type => GrammarElementType.TokenizerUsage;

        public GrammarTokenizerUsage(string name, bool isOptional, ITokenizer impl)
        {
            Name = name;
            IsOptional = isOptional;
            Impl = impl;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GrammarTokenizerUsage && Equals((GrammarTokenizerUsage)obj);
        }

        public bool Equals(GrammarTokenizerUsage usage)
        {
            return usage.Name == Name && usage.IsOptional == IsOptional && usage.Impl.GetType() == Impl.GetType();
        }

        public override string ToString()
        {
            return IsOptional ? "?" + Name : Name;
        }
    }
}