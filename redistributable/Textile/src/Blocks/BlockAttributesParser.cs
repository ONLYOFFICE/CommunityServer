#region License Statement
// Copyright (c) L.A.B.Soft.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
#endregion


namespace Textile.Blocks
{
    public class BlockAttributesParser
    {
        public static StyleReader Styler { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static public string ParseBlockAttributes(string input)
        {
            return ParseBlockAttributes(input, "");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        static public string ParseBlockAttributes(string input, string element)
        {
            string style = string.Empty;
            string cssClass = string.Empty;
            string lang = string.Empty;
            string colspan = string.Empty;
            string rowspan = string.Empty;
            string id = string.Empty;
            string atts = string.Empty;

            if (Styler != null)
            {
                style = GetStyle(element, style);
            }

            if (input.Length == 0)
                return (style.Length > 0 ? " style=\"" + style + "\"" : "");


            Match m;
            string matched = input;
            if (element == "td")
            {
				// column span
                m = Regex.Match(matched, @"\\(\d+)");
                if (m.Success)
                    colspan = m.Groups[1].Value;
				// row span
                m = Regex.Match(matched, @"/(\d+)");
                if (m.Success)
                    rowspan = m.Groups[1].Value;
				// vertical align
                m = Regex.Match(matched, @"(" + Globals.VerticalAlignPattern + @")");
                if (m.Success)
                    style += "vertical-align:" + Globals.VerticalAlign[m.Captures[0].Value] + ";";
            }

            // First, match custom styles
            m = Regex.Match(matched, @"\{([^}]*)\}");
            if (m.Success)
            {
                style += m.Groups[1].Value + ";";
                matched = matched.Replace(m.ToString(), "");
            }

            // Then match the language
            m = Regex.Match(matched, @"\[([^()]+)\]");
            if (m.Success)
            {
                lang = m.Groups[1].Value;
                matched = matched.Replace(m.ToString(), "");
            }

            // Match classes and IDs after that
            m = Regex.Match(matched, @"\(([^()]+)\)");
            if (m.Success)
            {
                cssClass = m.Groups[1].Value;
                matched = matched.Replace(m.ToString(), "");

                // Separate the public class and the ID
                m = Regex.Match(cssClass, @"^(.*)#(.*)$");
                if (m.Success)
                {
                    cssClass = m.Groups[1].Value;
                    id = m.Groups[2].Value;
                }
                if (Styler != null && !string.IsNullOrEmpty(cssClass))
                {
                    style = GetStyle("." + cssClass,style);
                }

            }

            // Get the padding on the left
            m = Regex.Match(matched, @"([(]+)");
            if (m.Success)
            {
                style += "padding-left:" + m.Groups[1].Length + "em;";
                matched = matched.Replace(m.ToString(), "");
            }

            // Get the padding on the right
            m = Regex.Match(matched, @"([)]+)");
            if (m.Success)
            {
                style += "padding-right:" + m.Groups[1].Length + "em;";
                matched = matched.Replace(m.ToString(), "");
            }

            // Get the text alignment
            m = Regex.Match(matched, "(" + Globals.HorizontalAlignPattern + ")");
            if (m.Success)
                style += "text-align:" + Globals.HorizontalAlign[m.Groups[1].Value] + ";";

            

            return (
                    (style.Length > 0 ? " style=\"" + style + "\"" : "") +
                    (cssClass.Length > 0 ? " class=\"" + cssClass + "\"" : "") +
                    (lang.Length > 0 ? " lang=\"" + lang + "\"" : "") +
                    (id.Length > 0 ? " id=\"" + id + "\"" : "") +
                    (colspan.Length > 0 ? " colspan=\"" + colspan + "\"" : "") +
                    (rowspan.Length > 0 ? " rowspan=\"" + rowspan + "\"" : "")
                   );
        }

        private static string GetStyle(string element, string style)
        {
            var styled = Styler.GetStyle(element);
            if (!string.IsNullOrEmpty(styled))
            {
                style += styled;
            }
            return style;
        }
    }
}
