/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


namespace ASC.Mail.Net
{
    #region usings

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    #endregion

    /// <summary>
    /// This class provides usefull text methods.
    /// </summary>
    public class TextUtils
    {
        #region Methods

        /// <summary>
        /// Qoutes and escapes fishy(\") chars.
        /// </summary>
        /// <param name="text">Text to quote.</param>
        /// <returns></returns>
        public static string QuoteString(string text)
        {
            StringBuilder retVal = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\\')
                {
                    retVal.Append("\\\\");
                }
                else if (c == '\"')
                {
                    retVal.Append("\\\"");
                }
                else
                {
                    retVal.Append(c);
                }
            }

            return "\"" + retVal + "\"";
        }

        /// <summary>
        /// Unquotes and unescapes escaped chars specified text. For example "xxx" will become to 'xxx', "escaped quote \"", will become to escaped 'quote "'.
        /// </summary>
        /// <param name="text">Text to unquote.</param>
        /// <returns></returns>
        public static string UnQuoteString(string text)
        {
            int startPosInText = 0;
            int endPosInText = text.Length;

            //--- Trim. We can't use standard string.Trim(), it's slow. ----//
            for (int i = 0; i < endPosInText; i++)
            {
                char c = text[i];
                if (c == ' ' || c == '\t')
                {
                    startPosInText++;
                }
                else
                {
                    break;
                }
            }
            for (int i = endPosInText - 1; i > 0; i--)
            {
                char c = text[i];
                if (c == ' ' || c == '\t')
                {
                    endPosInText--;
                }
                else
                {
                    break;
                }
            }
            //--------------------------------------------------------------//

            // All text trimmed
            if ((endPosInText - startPosInText) <= 0)
            {
                return "";
            }

            // Remove starting and ending quotes.         
            if (text[startPosInText] == '\"')
            {
                startPosInText++;
            }
            if (text[endPosInText - 1] == '\"')
            {
                endPosInText--;
            }

            // Just '"'
            if (endPosInText == startPosInText - 1)
            {
                return "";
            }

            char[] chars = new char[endPosInText - startPosInText];

            int posInChars = 0;
            bool charIsEscaped = false;
            for (int i = startPosInText; i < endPosInText; i++)
            {
                char c = text[i];

                // Escaping char
                if (!charIsEscaped && c == '\\')
                {
                    charIsEscaped = true;
                }
                    // Escaped char
                else if (charIsEscaped)
                {
                    // TODO: replace \n,\r,\t,\v ???
                    chars[posInChars] = c;
                    posInChars++;
                    charIsEscaped = false;
                }
                    // Normal char
                else
                {
                    chars[posInChars] = c;
                    posInChars++;
                    charIsEscaped = false;
                }
            }

            return new string(chars, 0, posInChars);
        }

        /// <summary>
        /// Escapes specified chars in the specified string.
        /// </summary>
        /// <param name="text">Text to escape.</param>
        /// <param name="charsToEscape">Chars to escape.</param>
        public static string EscapeString(string text, char[] charsToEscape)
        {
            // Create worst scenario buffer, assume all chars must be escaped
            char[] buffer = new char[text.Length*2];
            int nChars = 0;
            foreach (char c in text)
            {
                foreach (char escapeChar in charsToEscape)
                {
                    if (c == escapeChar)
                    {
                        buffer[nChars] = '\\';
                        nChars++;
                        break;
                    }
                }

                buffer[nChars] = c;
                nChars++;
            }

            return new string(buffer, 0, nChars);
        }

        /// <summary>
        /// Unescapes all escaped chars.
        /// </summary>
        /// <param name="text">Text to unescape.</param>
        /// <returns></returns>
        public static string UnEscapeString(string text)
        {
            // Create worst scenarion buffer, non of the chars escaped.
            char[] buffer = new char[text.Length];
            int nChars = 0;
            bool escapedCahr = false;
            foreach (char c in text)
            {
                if (!escapedCahr && c == '\\')
                {
                    escapedCahr = true;
                }
                else
                {
                    buffer[nChars] = c;
                    nChars++;
                    escapedCahr = false;
                }
            }

            return new string(buffer, 0, nChars);
        }

        /// <summary>
        /// Splits string into string arrays. This split method won't split qouted strings, but only text outside of qouted string.
        /// For example: '"text1, text2",text3' will be 2 parts: "text1, text2" and text3.
        /// </summary>
        /// <param name="text">Text to split.</param>
        /// <param name="splitChar">Char that splits text.</param>
        /// <returns></returns>
        public static string[] SplitQuotedString(string text, char splitChar)
        {
            return SplitQuotedString(text, splitChar, false);
        }

        /// <summary>
        /// Splits string into string arrays. This split method won't split qouted strings, but only text outside of qouted string.
        /// For example: '"text1, text2",text3' will be 2 parts: "text1, text2" and text3.
        /// </summary>
        /// <param name="text">Text to split.</param>
        /// <param name="splitChar">Char that splits text.</param>
        /// <param name="unquote">If true, splitted parst will be unqouted if they are qouted.</param>
        /// <returns></returns>
        public static string[] SplitQuotedString(string text, char splitChar, bool unquote)
        {
            List<string> splitParts = new List<string>(); // Holds splitted parts
            int startPos = 0;
            bool inQuotedString = false; // Holds flag if position is quoted string or not
            char lastChar = '0';

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // Start/end quoted string area. Ingonre escaped \".
                if (lastChar != '\\' && c == '\"')
                {
                    inQuotedString = !inQuotedString;
                }

                // Current char is split char and it isn't in quoted string, do split
                if (!inQuotedString && c == splitChar)
                {
                    // Add current currentSplitBuffer value to splitted parts list
                    if (unquote)
                    {
                        splitParts.Add(UnQuoteString(text.Substring(startPos, i - startPos)));
                    }
                    else
                    {
                        splitParts.Add(text.Substring(startPos, i - startPos));
                    }

                    // Store new split part start position.
                    startPos = i + 1;
                }
                lastChar = c;
            }
            // Add last split part to splitted parts list
            if (unquote)
            {
                splitParts.Add(UnQuoteString(text.Substring(startPos, text.Length - startPos)));
            }
            else
            {
                splitParts.Add(text.Substring(startPos, text.Length - startPos));
            }

            return splitParts.ToArray();
        }

        /// <summary>
        /// Gets first index of specified char. The specified char in quoted string is skipped.
        /// Returns -1 if specified char doesn't exist.
        /// </summary>
        /// <param name="text">Text in what to check.</param>
        /// <param name="indexChar">Char what index to get.</param>
        /// <returns></returns>
        public static int QuotedIndexOf(string text, char indexChar)
        {
            int retVal = -1;
            bool inQuotedString = false; // Holds flag if position is quoted string or not			
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\"')
                {
                    // Start/end quoted string area
                    inQuotedString = !inQuotedString;
                }

                // Current char is what index we want and it isn't in quoted string, return it's index
                if (!inQuotedString && c == indexChar)
                {
                    return i;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Splits string into string arrays.
        /// </summary>
        /// <param name="text">Text to split.</param>
        /// <param name="splitChar">Char Char that splits text.</param>
        /// <returns></returns>
        public static string[] SplitString(string text, char splitChar)
        {
            ArrayList splitParts = new ArrayList(); // Holds splitted parts

            int lastSplitPoint = 0;
            int textLength = text.Length;
            for (int i = 0; i < textLength; i++)
            {
                if (text[i] == splitChar)
                {
                    // Add current currentSplitBuffer value to splitted parts list
                    splitParts.Add(text.Substring(lastSplitPoint, i - lastSplitPoint));

                    lastSplitPoint = i + 1;
                }
            }
            // Add last split part to splitted parts list
            if (lastSplitPoint <= textLength)
            {
                splitParts.Add(text.Substring(lastSplitPoint));
            }

            string[] retVal = new string[splitParts.Count];
            splitParts.CopyTo(retVal, 0);

            return retVal;
        }

        /// <summary>
        /// Gets if specified string is valid "token" value.
        /// </summary>
        /// <param name="value">String value to check.</param>
        /// <returns>Returns true if specified string value is valid "token" value.</returns>
        /// <exception cref="ArgumentNullException">Is raised if <b>value</b> is null.</exception>
        public static bool IsToken(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(value);
            }

            /* This syntax is taken from rfc 3261, but token must be universal so ... .
                token    =  1*(alphanum / "-" / "." / "!" / "%" / "*" / "_" / "+" / "`" / "'" / "~" )
                alphanum = ALPHA / DIGIT
                ALPHA    =  %x41-5A / %x61-7A   ; A-Z / a-z
                DIGIT    =  %x30-39             ; 0-9
            */

            char[] tokenChars = new[] {'-', '.', '!', '%', '*', '_', '+', '`', '\'', '~'};
            foreach (char c in value)
            {
                // We don't have letter or digit, so we only may have token char.
                if (!((c >= 0x41 && c <= 0x5A) || (c >= 0x61 && c <= 0x7A) || (c >= 0x30 && c <= 0x39)))
                {
                    bool validTokenChar = false;
                    foreach (char tokenChar in tokenChars)
                    {
                        if (c == tokenChar)
                        {
                            validTokenChar = true;
                            break;
                        }
                    }
                    if (!validTokenChar)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion
    }
}