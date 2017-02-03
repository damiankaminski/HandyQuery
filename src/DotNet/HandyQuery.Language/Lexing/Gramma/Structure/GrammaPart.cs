namespace HandyQuery.Language.Lexing.Gramma.Structure
{
    internal sealed class GrammaPart : IGrammaElement
    {
        public string Name { get; }

        private GrammaPartBody _body;
        public GrammaPartBody Body
        {
            get { return _body; }
            set
            {
                if(_body != null) throw new GrammaLexerGeneratorException("Part's body cannot be assigned more than once.");
                _body = value;
            }
        }

        /// <summary>
        /// Indicates whether part has been already parsed, or just used somewhere.
        /// For instane if part A uses part B the latter one will be initialized, but not
        /// fully parsed.
        /// </summary>
        public bool FullyParsed => Body != null;

        public GrammaElementType Type => GrammaElementType.Part;

        public GrammaPart(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"{Name} = {Body}";
        }
    }
}