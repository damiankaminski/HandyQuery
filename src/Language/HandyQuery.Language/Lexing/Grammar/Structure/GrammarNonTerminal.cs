namespace HandyQuery.Language.Lexing.Grammar.Structure
{
    internal sealed class GrammarNonTerminal
    {
        public string Name { get; }

        private GrammarNonTerminalBody _body;

        public GrammarNonTerminalBody Body
        {
            get => _body;
            set
            {
                if (_body != null)
                    throw new GrammarParserException("NonTerminal's body cannot be assigned more than once.");
                _body = value;
            }
        }
        
        /// <summary>
        /// Indicates whether nonTerminal has been already parsed, or just used somewhere.
        /// For instane if nonTerminal A uses nonTerminal B the latter one will be initialized, but not
        /// fully parsed.
        /// </summary>
        public bool FullyParsed => Body != null;

        public GrammarNonTerminal(string name)
        {
            Name = name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GrammarNonTerminal && Equals((GrammarNonTerminal) obj);
        }

        public bool Equals(GrammarNonTerminal nonTerminal)
        {
            return nonTerminal.Name == Name && nonTerminal.Body.Equals(Body);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                return ((_body != null ? _body.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"{Name} ::= {Body}";
        }
    }
}