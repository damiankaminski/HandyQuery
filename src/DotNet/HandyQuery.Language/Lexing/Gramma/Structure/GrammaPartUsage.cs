namespace HandyQuery.Language.Lexing.Gramma.Structure
{
    internal sealed class GrammaPartUsage : IGrammaBodyItem
    {
        public string Name { get; }
        public bool IsOptional { get; }
        public GrammaPart Impl { get; }
        public GrammaElementType Type => GrammaElementType.PartUsage;

        public GrammaPartUsage(string name, bool isOptional, GrammaPart impl)
        {
            Name = name;
            IsOptional = isOptional;
            Impl = impl;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GrammaPartUsage && Equals((GrammaPartUsage)obj);
        }

        public bool Equals(GrammaPartUsage usage)
        {
            return usage.Name == Name && usage.IsOptional == IsOptional && usage.Impl.Equals(Impl);
        }

        public override string ToString()
        {
            return IsOptional ? "?" + Name : Name;
        }
    }
}