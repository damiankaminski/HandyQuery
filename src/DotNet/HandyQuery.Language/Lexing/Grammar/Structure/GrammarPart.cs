﻿namespace HandyQuery.Language.Lexing.Grammar.Structure
{
    internal sealed class GrammarPart
    {
        public string Name { get; }

        private GrammarPartBody _body;

        public GrammarPartBody Body
        {
            get => _body;
            set
            {
                if (_body != null)
                    throw new GrammarLexerGeneratorException("Part's body cannot be assigned more than once.");
                _body = value;
            }
        }

        /// <summary>
        /// Indicates whether part has been already parsed, or just used somewhere.
        /// For instane if part A uses part B the latter one will be initialized, but not
        /// fully parsed.
        /// </summary>
        public bool FullyParsed => Body != null;

        public GrammarPart(string name)
        {
            Name = name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GrammarPart && Equals((GrammarPart) obj);
        }

        public bool Equals(GrammarPart part)
        {
            return part.Name == Name && part.Body.Equals(Body);
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
            return $"{Name} = {Body}";
        }
    }
}