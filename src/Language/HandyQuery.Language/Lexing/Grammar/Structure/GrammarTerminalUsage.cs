using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing.Grammar.Structure
{
    internal sealed class GrammarTerminalUsage : IGrammarBodyItem
    {
        public string Name { get; }
        public ITokenizer Impl { get; }

        public GrammarNonTerminalBodyItemType Type => GrammarNonTerminalBodyItemType.TerminalUsage;

        public GrammarTerminalUsage(string name, ITokenizer impl)
        {
            Name = name;
            Impl = impl;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GrammarTerminalUsage && Equals((GrammarTerminalUsage)obj);
        }

        public bool Equals(GrammarTerminalUsage usage)
        {
            return usage.Name == Name && usage.Impl.GetType() == Impl.GetType();
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Impl != null ? Impl.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}