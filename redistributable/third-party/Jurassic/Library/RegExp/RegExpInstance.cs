using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents an instance of the RegExp object.
    /// </summary>
    [Serializable]
    public class RegExpInstance : ObjectInstance
    {
        private Regex value;
        private bool globalSearch;



        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new regular expression instance.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        /// <param name="pattern"> The regular expression pattern. </param>
        /// <param name="flags"> Available flags, which may be combined, are:
        /// g (global search for all occurrences of pattern)
        /// i (ignore case)
        /// m (multiline search)</param>
        internal RegExpInstance(ObjectInstance prototype, string pattern, string flags = null)
            : base(prototype)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");

            try
            {
                this.value = new Regex(pattern, ParseFlags(flags));
            }
            catch (ArgumentException ex)
            {
                // Wrap the exception so that it can be caught within javascript code.
                throw new JavaScriptException(this.Engine, "SyntaxError", "Invalid regular expression - " + ex.Message);
            }

            // Initialize the javascript properties.
            this.FastSetProperty("source", pattern);
            this.FastSetProperty("global", this.Global);
            this.FastSetProperty("multiline", this.Multiline);
            this.FastSetProperty("ignoreCase", this.IgnoreCase);
            this.FastSetProperty("lastIndex", 0.0, PropertyAttributes.Writable);
        }

        /// <summary>
        /// Creates a new regular expression instance by copying the pattern and flags from another
        /// RegExp instance.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        /// <param name="existingInstance"> The instance to copy the pattern and flags from. </param>
        internal RegExpInstance(ObjectInstance prototype, RegExpInstance existingInstance)
            : base(prototype)
        {
            if (existingInstance == null)
                throw new ArgumentNullException("existingInstance");
            this.value = existingInstance.value;
            this.globalSearch = existingInstance.globalSearch;

            // Initialize the javascript properties.
            this.FastSetProperty("source", existingInstance.Source);
            this.FastSetProperty("global", existingInstance.Global);
            this.FastSetProperty("multiline", existingInstance.Multiline);
            this.FastSetProperty("ignoreCase", existingInstance.IgnoreCase);
            this.FastSetProperty("lastIndex", 0.0, PropertyAttributes.Writable);
        }



        //     .NET ACCESSOR PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the internal class name of the object.  Used by the default toString()
        /// implementation.
        /// </summary>
        protected override string InternalClassName
        {
            get { return "RegExp"; }
        }

        /// <summary>
        /// Gets the primitive value of this object.
        /// </summary>
        public Regex Value
        {
            get { return this.value; }
        }

        /// <summary>
        /// Gets a string that contains the flags.
        /// </summary>
        public string Flags
        {
            get
            {
                var result = new System.Text.StringBuilder(3);
                if (this.Global)
                    result.Append("g");
                if (this.IgnoreCase)
                    result.Append("i");
                if (this.Multiline)
                    result.Append("m");
                return result.ToString();
            }
        }



        //     JAVASCRIPT PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the regular expression pattern.
        /// </summary>
        public string Source
        {
            get { return this.value.ToString(); }
        }

        /// <summary>
        /// Gets a value that indicates whether the global flag is set.  If this flag is set it
        /// indicates that a search should find all occurrences of the pattern within the searched
        /// string, not just the first one.
        /// </summary>
        public bool Global
        {
            get { return this.globalSearch; }
        }

        /// <summary>
        /// Gets a value that indicates whether the multiline flag is set.  If this flag is set it
        /// indicates that the ^ and $ tokens should match the start and end of lines and not just
        /// the start and end of the string.
        /// </summary>
        public bool Multiline
        {
            get { return (this.value.Options & RegexOptions.Multiline) != 0;}
        }

        /// <summary>
        /// Gets a value that indicates whether the ignoreCase flag is set.  If this flag is set it
        /// indicates that a search should ignore differences in case between the pattern and the
        /// matched string.
        /// </summary>
        public bool IgnoreCase
        {
            get { return (this.value.Options & RegexOptions.IgnoreCase) != 0; }
        }

        /// <summary>
        /// Gets the character position to start searching when the global flag is set.
        /// </summary>
        public int LastIndex
        {
            get { return TypeConverter.ToInteger(this["lastIndex"]); }
            set { this["lastIndex"] = value; }
        }



        //     JAVASCRIPT FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Compiles the regular expression for faster execution.
        /// </summary>
        /// <param name="pattern"> The regular expression pattern. </param>
        /// <param name="flags"> Available flags, which may be combined, are:
        /// g (global search for all occurrences of pattern)
        /// i (ignore case)
        /// m (multiline search)</param>
        [JSInternalFunction(Deprecated = true, Name = "compile")]
        public void Compile(string pattern, [DefaultParameterValue(null)] string flags = null)
        {
#if !SILVERLIGHT
            this.value = new Regex(pattern, ParseFlags(flags) | RegexOptions.Compiled);
#endif

            // Update the javascript properties.
            this.FastSetProperty("source", pattern);
            this.FastSetProperty("global", this.Global);
            this.FastSetProperty("multiline", this.Multiline);
            this.FastSetProperty("ignoreCase", this.IgnoreCase);
            this.LastIndex = 0;
        }

        /// <summary>
        /// Returns a boolean value that indicates whether or not a pattern exists in a searched string.
        /// </summary>
        /// <param name="input"> The string on which to perform the search. </param>
        /// <returns> <c>true</c> if the regular expression has at least one match in the given
        /// string; <c>false</c> otherwise. </returns>
        [JSInternalFunction(Name = "test")]
        public bool Test(string input)
        {
            // Check if there is a match.
            var match = this.value.Match(input, CalculateStartPosition(input));
            
            // If the regex is global, update the lastIndex property.
            if (this.Global == true)
                this.LastIndex = match.Success == true ? match.Index + match.Length : 0;

            // Set the deprecated RegExp properties.
            if (match.Success == true)
                this.Engine.RegExp.SetDeprecatedProperties(input, match);

            return match.Success;
        }

        /// <summary>
        /// Executes a search on a string using a regular expression pattern, and returns an array
        /// containing the results of that search.
        /// </summary>
        /// <param name="input"> The string on which to perform the search. </param>
        /// <returns> Returns an array containing the match and submatch details, or <c>null</c> if
        /// no match was found.  The array returned by the exec method has three properties, input,
        /// index and lastIndex. The input property contains the entire searched string. The index
        /// property contains the position of the matched substring within the complete searched
        /// string. The lastIndex property contains the position following the last character in
        /// the match. </returns>
        [JSInternalFunction(Name = "exec")]
        public object Exec(string input)
        {
            // Perform the regular expression matching.
            var match = this.value.Match(input, CalculateStartPosition(input));

            // Return null if no match was found.
            if (match.Success == false)
            {
                // Reset the lastIndex property.
                if (this.Global == true)
                    this.LastIndex = 0;
                return Null.Value;
            }

            // If the global flag is set, update the lastIndex property.
            if (this.Global == true)
                this.LastIndex = match.Index + match.Length;

            // Set the deprecated RegExp properties.
            this.Engine.RegExp.SetDeprecatedProperties(input, match);

            // Otherwise, return an array.
            object[] array = new object[match.Groups.Count];
            for (int i = 0; i < match.Groups.Count; i++)
            {
                var group = match.Groups[i];
                array[i] = group.Value;
                if (group.Captures.Count == 0)
                    array[i] = Undefined.Value;
            }
            var result = this.Engine.Array.New(array);
            result["index"] = match.Index;
            result["input"] = input;

            return result;
        }

        /// <summary>
        /// Calculates the position to start searching.
        /// </summary>
        /// <param name="input"> The string on which to perform the search. </param>
        /// <returns> The character position to start searching. </returns>
        private int CalculateStartPosition(string input)
        {
            if (this.Global == false)
                return 0;
            return Math.Min(Math.Max(this.LastIndex, 0), input.Length);
        }

        /// <summary>
        /// Finds all regular expression matches within the given string.
        /// </summary>
        /// <param name="input"> The string on which to perform the search. </param>
        /// <returns> An array containing the matched strings. </returns>
        public object Match(string input)
        {
            // If the global flag is not set, returns a single match.
            if (this.Global == false)
                return Exec(input);

            // Otherwise, find all matches.
            var matches = this.value.Matches(input);
            if (matches.Count == 0)
                return Null.Value;

            // Set the deprecated RegExp properties (using the last match).
            this.Engine.RegExp.SetDeprecatedProperties(input, matches[matches.Count - 1]);

            // Construct the array to return.
            object[] matchValues = new object[matches.Count];
            for (int i = 0; i < matches.Count; i++)
                matchValues[i] = matches[i].Value;
            return this.Engine.Array.New(matchValues);
        }

        /// <summary>
        /// Returns a copy of the given string with text replaced using a regular expression.
        /// </summary>
        /// <param name="input"> The string on which to perform the search. </param>
        /// <param name="replaceText"> A string containing the text to replace for every successful match. </param>
        /// <returns> A copy of the given string with text replaced using a regular expression. </returns>
        public string Replace(string input, string replaceText)
        {
            // Check if the replacement string contains any patterns.
            bool replaceTextContainsPattern = replaceText.IndexOf('$') >= 0;

            // Replace the input string with replaceText, recording the last match found.
            Match lastMatch = null;
            string result = this.value.Replace(input, match =>
            {
                lastMatch = match;

                // If there is no pattern, replace the pattern as is.
                if (replaceTextContainsPattern == false)
                    return replaceText;

                // Patterns
                // $$	Inserts a "$".
                // $&	Inserts the matched substring.
                // $`	Inserts the portion of the string that precedes the matched substring.
                // $'	Inserts the portion of the string that follows the matched substring.
                // $n or $nn	Where n or nn are decimal digits, inserts the nth parenthesized submatch string, provided the first argument was a RegExp object.
                var replacementBuilder = new System.Text.StringBuilder();
                for (int i = 0; i < replaceText.Length; i++)
                {
                    char c = replaceText[i];
                    if (c == '$' && i < replaceText.Length - 1)
                    {
                        c = replaceText[++i];
                        if (c == '$')
                            replacementBuilder.Append('$');
                        else if (c == '&')
                            replacementBuilder.Append(match.Value);
                        else if (c == '`')
                            replacementBuilder.Append(input.Substring(0, match.Index));
                        else if (c == '\'')
                            replacementBuilder.Append(input.Substring(match.Index + match.Length));
                        else if (c >= '0' && c <= '9')
                        {
                            int matchNumber1 = c - '0';

                            // The match number can be one or two digits long.
                            int matchNumber2 = 0;
                            if (i < replaceText.Length - 1 && replaceText[i + 1] >= '0' && replaceText[i + 1] <= '9')
                                matchNumber2 = matchNumber1 * 10 + (replaceText[i + 1] - '0');

                            // Try the two digit capture first.
                            if (matchNumber2 > 0 && matchNumber2 < match.Groups.Count)
                            {
                                // Two digit capture replacement.
                                replacementBuilder.Append(match.Groups[matchNumber2].Value);
                                i++;
                            }
                            else if (matchNumber1 > 0 && matchNumber1 < match.Groups.Count)
                            {
                                // Single digit capture replacement.
                                replacementBuilder.Append(match.Groups[matchNumber1].Value);
                            }
                            else
                            {
                                // Capture does not exist.
                                replacementBuilder.Append('$');
                                i--;
                            }
                        }
                        else
                        {
                            // Unknown replacement pattern.
                            replacementBuilder.Append('$');
                            replacementBuilder.Append(c);
                        }
                    }
                    else
                        replacementBuilder.Append(c);
                }

                return replacementBuilder.ToString();
            }, this.Global == true ? -1 : 1);

            // Set the deprecated RegExp properties if at least one match was found.
            if (lastMatch != null)
                this.Engine.RegExp.SetDeprecatedProperties(input, lastMatch);

            return result;
        }

        /// <summary>
        /// Returns a copy of the given string with text replaced using a regular expression.
        /// </summary>
        /// <param name="input"> The string on which to perform the search. </param>
        /// <param name="replaceFunction"> A function that is called to produce the text to replace
        /// for every successful match. </param>
        /// <returns> A copy of the given string with text replaced using a regular expression. </returns>
        public string Replace(string input, FunctionInstance replaceFunction)
        {
            return this.value.Replace(input, match =>
            {
                // Set the deprecated RegExp properties.
                this.Engine.RegExp.SetDeprecatedProperties(input, match);

                object[] parameters = new object[match.Groups.Count + 2];
                for (int i = 0; i < match.Groups.Count; i++)
                {
                    if (match.Groups[i].Success == false)
                        parameters[i] = Undefined.Value;
                    else
                        parameters[i] = match.Groups[i].Value;
                }
                parameters[match.Groups.Count] = match.Index;
                parameters[match.Groups.Count + 1] = input;
                return TypeConverter.ToString(replaceFunction.CallFromNative("replace", null, parameters));
            }, this.Global == true ? int.MaxValue : 1);
        }

        /// <summary>
        /// Returns the position of the first substring match in a regular expression search.
        /// </summary>
        /// <param name="input"> The string on which to perform the search. </param>
        /// <returns> The character position of the first match, or -1 if no match was found. </returns>
        public int Search(string input)
        {
            // Perform the regular expression matching.
            var match = this.value.Match(input);

            // Return -1 if no match was found.
            if (match.Success == false)
                return -1;

            // Set the deprecated RegExp properties.
            this.Engine.RegExp.SetDeprecatedProperties(input, match);

            // Otherwise, return the position of the match.
            return match.Index;
        }

        /// <summary>
        /// Splits the given string into an array of strings by separating the string into substrings.
        /// </summary>
        /// <param name="input"> The string to split. </param>
        /// <param name="limit"> The maximum number of array items to return.  Defaults to unlimited. </param>
        /// <returns> An array containing the split strings. </returns>
        public ArrayInstance Split(string input, [DefaultParameterValue(uint.MaxValue)] uint limit = uint.MaxValue)
        {
            // Return an empty array if limit = 0.
            if (limit == 0)
                return this.Engine.Array.New(new object[0]);

            // Find the first match.
            Match match = this.value.Match(input, 0);

            var results = new List<object>();
            int startIndex = 0;
            Match lastMatch = null;
            while (match.Success == true)
            {
                // Do not match the an empty substring at the start or end of the string or at the
                // end of the previous match.
                if (match.Length == 0 && (match.Index == 0 || match.Index == input.Length || match.Index == startIndex))
                {
                    // Find the next match.
                    match = match.NextMatch();
                    continue;
                }

                // Add the match results to the array.
                results.Add(input.Substring(startIndex, match.Index - startIndex));
                if (results.Count >= limit)
                    return this.Engine.Array.New(results.ToArray());
                startIndex = match.Index + match.Length;
                for (int i = 1; i < match.Groups.Count; i++)
                {
                    var group = match.Groups[i];
                    if (group.Captures.Count == 0)
                        results.Add(Undefined.Value);       // Non-capturing groups return "undefined".
                    else
                        results.Add(match.Groups[i].Value);
                    if (results.Count >= limit)
                        return this.Engine.Array.New(results.ToArray());
                }

                // Record the last match.
                lastMatch = match;

                // Find the next match.
                match = match.NextMatch();
            }
            results.Add(input.Substring(startIndex, input.Length - startIndex));

            // Set the deprecated RegExp properties.
            if (lastMatch != null)
                this.Engine.RegExp.SetDeprecatedProperties(input, lastMatch);

            return this.Engine.Array.New(results.ToArray());
        }

        /// <summary>
        /// Returns a string representing the current object.
        /// </summary>
        /// <returns> A string representing the current object. </returns>
        [JSInternalFunction(Name = "toString")]
        public new string ToString()
        {
            return string.Format("/{0}/{1}", this.Source, this.Flags);
        }



        //     PRIVATE IMPLEMENTATION METHODS
        //_________________________________________________________________________________________

        /// <summary>
        /// Parses the flags parameter into an enum.
        /// </summary>
        /// <param name="flags"> Available flags, which may be combined, are:
        /// g (global search for all occurrences of pattern)
        /// i (ignore case)
        /// m (multiline search)</param>
        /// <returns> RegexOptions flags that correspond to the given flags. </returns>
        private RegexOptions ParseFlags(string flags)
        {
            var options = RegexOptions.ECMAScript;
            this.globalSearch = false;

            if (flags != null)
            {
                for (int i = 0; i < flags.Length; i++)
                {
                    char flag = flags[i];
                    if (flag == 'g')
                    {
                        if (this.globalSearch == true)
                            throw new JavaScriptException(this.Engine, "SyntaxError", "The 'g' flag cannot be specified twice");
                        this.globalSearch = true;
                    }
                    else if (flag == 'i')
                    {
                        if ((options & RegexOptions.IgnoreCase) == RegexOptions.IgnoreCase)
                            throw new JavaScriptException(this.Engine, "SyntaxError", "The 'i' flag cannot be specified twice");
                        options |= RegexOptions.IgnoreCase;
                    }
                    else if (flag == 'm')
                    {
                        if ((options & RegexOptions.Multiline) == RegexOptions.Multiline)
                            throw new JavaScriptException(this.Engine, "SyntaxError", "The 'm' flag cannot be specified twice");
                        options |= RegexOptions.Multiline;
                    }
                    else
                    {
                        throw new JavaScriptException(this.Engine, "SyntaxError", string.Format("Unknown flag '{0}'", flag));
                    }
                }
            }
            return options;
        }
    }
}
