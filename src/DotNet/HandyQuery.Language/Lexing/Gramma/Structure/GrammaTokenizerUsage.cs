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

        public override string ToString()
        {
            return IsOptional ? "?" + Name : Name;
        }
    }
}