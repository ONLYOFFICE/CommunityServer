using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Ical.Net.Serialization;

namespace Ical.Net.Utility
{
    internal static class TextUtil
    {
        /// <summary> Folds lines at 75 characters, and prepends the next line with a space per RFC https://tools.ietf.org/html/rfc5545#section-3.1 </summary>
        public static string FoldLines(string incoming)
        {
            //The spec says nothing about trimming, but it seems reasonable...
            var trimmed = incoming.Trim();
            if (trimmed.Length <= 75)
            {
                return trimmed + SerializationConstants.LineBreak;
            }

            const int takeLimit = 74;

            var firstLine = trimmed.Substring(0, takeLimit);
            var remainder = trimmed.Substring(takeLimit, trimmed.Length - takeLimit);

            var chunkedRemainder = string.Join(SerializationConstants.LineBreak + " ", Chunk(remainder));
            return firstLine + SerializationConstants.LineBreak + " " + chunkedRemainder + SerializationConstants.LineBreak;
        }

        public static IEnumerable<string> Chunk(string str, int chunkSize = 73)
        {
            for (var index = 0; index < str.Length; index += chunkSize)
            {
                yield return str.Substring(index, Math.Min(chunkSize, str.Length - index));
            }
        }

        /// <summary> Removes blank lines from a string with normalized (\r\n) line endings </summary>
        public static string RemoveEmptyLines(string s)
        {
            var len = -1;
            while (len != s.Length)
            {
                s = s.Replace("\r\n\r\n", SerializationConstants.LineBreak);
                len = s.Length;
            }
            return s;
        }

        internal static readonly Regex NormalizeToCrLf = new Regex(@"((\r(?=[^\n]))|((?<=[^\r])\n))", RegexOptions.Compiled);

        /// <summary>
        /// Normalizes line endings, converting "\r" into "\r\n" and "\n" into "\r\n".        
        /// </summary>
        public static TextReader Normalize(string s, SerializationContext ctx)
        {
            // Replace \r and \n with \r\n.
            s = NormalizeToCrLf.Replace(s, SerializationConstants.LineBreak);

            s = RemoveEmptyLines(UnwrapLines(s));

            return new StringReader(s);
        }

        internal static readonly Regex NewLineMatch = new Regex(@"(\r\n[ \t])", RegexOptions.Compiled);

        /// <summary> Unwraps lines from the RFC 5545 "line folding" technique. </summary>
        public static string UnwrapLines(string s) => NewLineMatch.Replace(s, string.Empty);

        public static bool Contains(this string haystack, string needle, StringComparison stringComparison)
            => haystack.IndexOf(needle, stringComparison) >= 0;
    }
}