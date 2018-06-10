using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Parsing.Grammar.Structure
{
    internal sealed class GrammarTerminalUsage : IGrammarBodyItem
    {
        public string Name { get; }
        public string ArgumentValue { get; }
        public ITokenizer Impl { get; }

        public GrammarNonTerminalBodyItemType Type => GrammarNonTerminalBodyItemType.TerminalUsage;

        public GrammarTerminalUsage(string name, string argumentValue, ITokenizer impl)
        {
            Name = name;
            ArgumentValue = argumentValue;
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