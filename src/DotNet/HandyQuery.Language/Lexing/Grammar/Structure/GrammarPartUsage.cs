namespace HandyQuery.Language.Lexing.Grammar.Structure
{
    internal sealed class GrammarPartUsage : IGrammarBodyItem
    {
        public string Name { get; }
        public bool IsOptional { get; }
        public GrammarPart Impl { get; }
        public GrammarElementType Type => GrammarElementType.PartUsage;

        public GrammarPartUsage(string name, bool isOptional, GrammarPart impl)
        {
            Name = name;
            IsOptional = isOptional;
            Impl = impl;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GrammarPartUsage && Equals((GrammarPartUsage)obj);
        }

        public bool Equals(GrammarPartUsage usage)
        {
            return usage.Name == Name && usage.IsOptional == IsOptional && usage.Impl.Equals(Impl);
        }

        public override string ToString()
        {
            return IsOptional ? "?" + Name : Name;
        }
    }
}