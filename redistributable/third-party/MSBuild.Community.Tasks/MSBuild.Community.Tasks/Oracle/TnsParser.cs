

using System;
using System.Text.RegularExpressions;

namespace MSBuild.Community.Tasks.Oracle
{

    /// <summary>
    /// Locates host entries within a TNSNAMES.ORA file
    /// </summary>
    /// <exclude />
    public class TnsParser
    {
        /// <summary>
        /// Initializes a new instance of the parser using the contents of a TNSNAMES.ORA file.
        /// </summary>
        /// <param name="content"></param>
        public TnsParser(string content)
        {
            this.content = content;
        }

        private string content;

        /// <summary>
        /// Locates a host entry by its name.
        /// </summary>
        /// <param name="name">The name of the entry to find.</param>
        /// <returns>A <see cref="TnsEntry" /> which contains information about the location of the entry within the file.</returns>
        public TnsEntry FindEntry(string name)
        {
            string findEntryPattern = String.Format(FIND_ENTRY_REGEX_FORMAT, Regex.Escape(name));
            Match match = Regex.Match(content, findEntryPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            if (!match.Success) return null;
            //TODO: make sure to exclude entries that are contained within comments
            //string matchComments = @"/\* (.*?) \*\/";

            int startPosition = match.Index;
            int length = match.Length;

            return new TnsEntry(startPosition, length);
        }

        // Makes use of the "balancing group definition" Grouping Construct available in .NET
        // Based on sample from http://www.oreilly.com/catalog/regex2/chapter/ch09.pdf
        const string FIND_ENTRY_REGEX_FORMAT = @"^(?<entryName>{0})\s*=\s*
(?<definition>
\(
(?>
[^()]+
|
\( (?'DEPTH')
|
\) (?'-DEPTH')
)*
(?(DEPTH)(?!))
\)
)";

    }
}