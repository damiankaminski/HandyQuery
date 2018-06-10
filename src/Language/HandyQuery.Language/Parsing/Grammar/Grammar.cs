using System.Collections.Generic;
using HandyQuery.Language.Parsing.Grammar.Structure;

namespace HandyQuery.Language.Parsing.Grammar
{
    internal sealed class Grammar
    {
        public readonly GrammarReturn Root;
        public readonly IEnumerable<GrammarNonTerminal> NonTerminals;

        public Grammar(GrammarReturn root, IEnumerable<GrammarNonTerminal> nonTerminals)
        {
            Root = root;
            NonTerminals = nonTerminals;
        }
    }
}