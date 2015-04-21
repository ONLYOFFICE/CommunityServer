using System;
using System.Collections.Generic;
using System.Text;

namespace Textile.Blocks
{
    public class CitePhraseBlockModifier : PhraseBlockModifier
    {
        public override string ModifyLine(string line)
        {
            return PhraseModifierFormat(line, @"\?\?", "cite");
        }
    }
}
