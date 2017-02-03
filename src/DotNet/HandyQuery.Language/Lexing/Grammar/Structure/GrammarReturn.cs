﻿namespace HandyQuery.Language.Lexing.Grammar.Structure
{
    internal sealed class GrammarReturn
    {
        public GrammarNonTerminalUsage NonTerminalUsage { get; }

        public GrammarReturn(GrammarNonTerminalUsage nonTerminalUsage)
        {
            NonTerminalUsage = nonTerminalUsage;
        }
    }
}