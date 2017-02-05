using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing.Gramma.Structure
{
    internal sealed class GrammaTokenizerUsage : IGrammaBodyItem
    {
        public string Name { get; }
        public bool IsOptional { get; }
        public ITokenizer Impl { get; }

        public GrammaElementType Type => GrammaElementType.TokenizerUsage;

        public GrammaTokenizerUsage(string name, bool isOptional, ITokenizer impl)
        {
            Name = name;
            IsOptional = isOptional;
            Impl = impl;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GrammaTokenizerUsage && Equals((GrammaTokenizerUsage)obj);
        }

        public bool Equals(GrammaTokenizerUsage usage)
        {
            return usage.Name == Name && usage.IsOptional == IsOptional && usage.Impl.GetType() == Impl.GetType();
        }

        public override string ToString()
        {
            return IsOptional ? "?" + Name : Name;
        }
    }
}