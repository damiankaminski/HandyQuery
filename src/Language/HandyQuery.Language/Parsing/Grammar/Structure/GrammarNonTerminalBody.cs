using System.Collections.Generic;
using System.Linq;

namespace HandyQuery.Language.Parsing.Grammar.Structure
{
    internal sealed class GrammarNonTerminalBody
    {
        public readonly List<OrConditionOperand> OrConditionOperands = new List<OrConditionOperand>();
        
        public override string ToString()
        {
            return string.Join(" | ", OrConditionOperands.Select(x => x.ToString()));
        }

        public class OrConditionOperand : List<IGrammarBodyItem>
        {
            public bool IsRecursive { get; set; }
            
            public override string ToString()
            {
                return string.Join(" ", this.Select(x => x.ToString()));
            }
        }
    }
}