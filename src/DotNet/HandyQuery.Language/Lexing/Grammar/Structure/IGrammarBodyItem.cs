namespace HandyQuery.Language.Lexing.Grammar.Structure
{
    internal interface IGrammarBodyItem : IGrammarElement
    {
        bool IsOptional { get; }
    }
}