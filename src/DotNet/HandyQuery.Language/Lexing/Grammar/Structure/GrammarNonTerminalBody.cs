using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Extensions;

namespace HandyQuery.Language.Lexing.Grammar.Structure
{
    internal sealed class GrammarNonTerminalBody
    {
        public readonly List<Operand> Operands = new List<Operand>();
        
        public override string ToString()
        {
            return string.Join(" | ", Operands.Select(x => x.ToString()));
        }

        public class Operand : List<IGrammarBodyItem>
        {
            public override string ToString()
            {
                return string.Join(" ", this.Select(x => x.ToString()));
            }
        }
    }
}