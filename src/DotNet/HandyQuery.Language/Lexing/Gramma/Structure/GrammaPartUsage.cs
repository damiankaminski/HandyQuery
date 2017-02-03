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

        public override string ToString()
        {
            return IsOptional ? "?" + Name : Name;
        }
    }
}