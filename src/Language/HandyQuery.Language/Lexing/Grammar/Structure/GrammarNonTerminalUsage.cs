namespace HandyQuery.Language.Lexing.Grammar.Structure
{
    internal sealed class GrammarNonTerminalUsage : IGrammarBodyItem
    {
        public string Name { get; }
        public GrammarNonTerminal Impl { get; }
        public GrammarNonTerminalBodyItemType Type => GrammarNonTerminalBodyItemType.NonTerminalUsage;

        public GrammarNonTerminalUsage(string name, GrammarNonTerminal impl)
        {
            Name = name;
            Impl = impl;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GrammarNonTerminalUsage && Equals((GrammarNonTerminalUsage)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Impl != null ? Impl.GetHashCode() : 0);
                return hashCode;
            }
        }

        public bool Equals(GrammarNonTerminalUsage usage)
        {
            return usage.Name == Name && usage.Impl.Equals(Impl);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}