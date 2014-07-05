using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Textile
{
    public class StyleReader
    {
        private readonly Regex _styleParser = new Regex(@"(?<selector>[^\{]+)(?<style>[^\}]+)");
        private readonly Regex _minimizer = new Regex(@";\s+");

        private readonly System.Collections.Specialized.StringDictionary _tagStyler = new System.Collections.Specialized.StringDictionary();

        public StyleReader(string styles)
        {
            //Read it
            var matches = _styleParser.Matches(styles.Replace(System.Environment.NewLine, ""));
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    _tagStyler.Add(match.Groups["selector"].Value.Trim('{', '}', ' '), _minimizer.Replace(match.Groups["style"].Value.Trim('{', '}', ' '),";"));
                }
            }
        }

        public string GetStyle(string tag)
        {
            return _tagStyler[tag];
        }
    }
}