namespace HandyQuery.Language.Lexing.Grammar.Structure
{
    internal sealed class GrammarPartUsage : IGrammarBodyItem
    {
        public string Name { get; }
        public bool IsOptional { get; }
        public bool IsOrConditionOperand { get; }
        public GrammarPart Impl { get; }
        public GrammarElementType Type => GrammarElementType.PartUsage;

        public GrammarPartUsage(string name, bool isOptional, bool isOrConditionOperand, GrammarPart impl)
        {
            Name = name;
            IsOptional = isOptional;
            Impl = impl;
            IsOrConditionOperand = isOrConditionOperand;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GrammarPartUsage && Equals((GrammarPartUsage)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsOptional.GetHashCode();
                hashCode = (hashCode * 397) ^ (Impl != null ? Impl.GetHashCode() : 0);
                return hashCode;
            }
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