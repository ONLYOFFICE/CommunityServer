using System;
using System.Text.RegularExpressions;

namespace Textile.Blocks
{
    public static class NoTextileEncoder
    {
        private static readonly string[,] TextileModifiers = {
								{ "\"", "&#34;" },
								{ "%", "&#37;" },
            					{ "*", "&#42;" },
            					{ "+", "&#43;" },
            					{ "-", "&#45;" },
            					{ "<", "&lt;" },   // or "&#60;"
            					{ "=", "&#61;" },
            					{ ">", "&gt;" },   // or "&#62;"
            					{ "?", "&#63;" },     
            					{ "^", "&#94;" },
            					{ "_", "&#95;" },
            					{ "~", "&#126;" },
                                { "@", "&#64;" },
                                { "'", "&#39;" },
                                { "|", "&#124;" },
                                { "!", "&#33;" },
                                { "(", "&#40;" },
                                { ")", "&#41;" },
                                { ".", "&#46;" },
                                { "x", "&#120;" }
							};


        public static string EncodeNoTextileZones(string tmp, string patternPrefix, string patternSuffix)
        {
            return EncodeNoTextileZones(tmp, patternPrefix, patternSuffix, null);
        }

        public static string EncodeNoTextileZones(string tmp, string patternPrefix, string patternSuffix, string[] exceptions)
        {
            Func<Match, string> evaluator = m =>
            {
                var toEncode = m.Groups["notex"].Value;
                if (toEncode == string.Empty)
                {
                    return string.Empty;
                }
                for (var i = 0; i < TextileModifiers.GetLength(0); ++i)
                {
                    if (exceptions == null || Array.IndexOf(exceptions, TextileModifiers[i, 0]) < 0)
                    {
                        toEncode = toEncode.Replace(TextileModifiers[i, 0], TextileModifiers[i, 1]);
                    }
                }
                return patternPrefix + toEncode + patternSuffix;
            };
            tmp = Regex.Replace(tmp, string.Format("({0}(?<notex>.+?){1})*", patternPrefix, patternSuffix), new MatchEvaluator(evaluator));
            return tmp;
        }

        public static string DecodeNoTextileZones(string tmp, string patternPrefix, string patternSuffix)
        {
            return DecodeNoTextileZones(tmp, patternPrefix, patternSuffix, null);
        }

        public static string DecodeNoTextileZones(string tmp, string patternPrefix, string patternSuffix, string[] exceptions)
        {
            Func<Match, string> evaluator = m =>
            {
                var toEncode = m.Groups["notex"].Value;
                for (var i = 0; i < TextileModifiers.GetLength(0); ++i)
                {
                    if (exceptions == null || Array.IndexOf(exceptions, TextileModifiers[i, 0]) < 0)
                    {
                        toEncode = toEncode.Replace(TextileModifiers[i, 1], TextileModifiers[i, 0]);
                    }
                }
                return toEncode;
            };
            tmp = Regex.Replace(tmp, string.Format("({0}(?<notex>.+?){1})*", patternPrefix, patternSuffix), new MatchEvaluator(evaluator));
            return tmp;
        }
    }
}
