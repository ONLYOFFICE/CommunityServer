/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


namespace ASC.Mail.Net.IMAP.Server
{
    #region usings

    using System;
    using System.Globalization;
    using MIME;

    #endregion

    /// <summary>
    /// Provides utility methods for IMAP.
    /// </summary>
    public class IMAP_Utils
    {
        #region Methods

        /// <summary>
        /// Parses message flags from string.
        /// </summary>
        /// <param name="flagsString">Message flags string.</param>
        /// <returns></returns>
        public static IMAP_MessageFlags ParseMessageFlags(string flagsString)
        {
            IMAP_MessageFlags mFlags = 0;

            flagsString = flagsString.ToUpper();

            if (flagsString.IndexOf("ANSWERED") > -1)
            {
                mFlags |= IMAP_MessageFlags.Answered;
            }
            if (flagsString.IndexOf("FLAGGED") > -1)
            {
                mFlags |= IMAP_MessageFlags.Flagged;
            }
            if (flagsString.IndexOf("DELETED") > -1)
            {
                mFlags |= IMAP_MessageFlags.Deleted;
            }
            if (flagsString.IndexOf("SEEN") > -1)
            {
                mFlags |= IMAP_MessageFlags.Seen;
            }
            if (flagsString.IndexOf("DRAFT") > -1)
            {
                mFlags |= IMAP_MessageFlags.Draft;
            }

            return mFlags;
        }

        /// <summary>
        /// Converts message flags to string. Eg. \SEEN \DELETED .
        /// </summary>
        /// <returns></returns>
        public static string MessageFlagsToString(IMAP_MessageFlags msgFlags)
        {
            string retVal = "";
            if (((int) IMAP_MessageFlags.Answered & (int) msgFlags) != 0)
            {
                retVal += " \\ANSWERED";
            }
            if (((int) IMAP_MessageFlags.Flagged & (int) msgFlags) != 0)
            {
                retVal += " \\FLAGGED";
            }
            if (((int) IMAP_MessageFlags.Deleted & (int) msgFlags) != 0)
            {
                retVal += " \\DELETED";
            }
            if (((int) IMAP_MessageFlags.Seen & (int) msgFlags) != 0)
            {
                retVal += " \\SEEN";
            }
            if (((int) IMAP_MessageFlags.Draft & (int) msgFlags) != 0)
            {
                retVal += " \\DRAFT";
            }

            return retVal.Trim();
        }

        /// <summary>
        /// Converts IMAP_ACL_Flags to string.
        /// </summary>
        /// <param name="flags">Flags to convert.</param>
        /// <returns></returns>
        public static string ACL_to_String(IMAP_ACL_Flags flags)
        {
            string retVal = "";
            if ((flags & IMAP_ACL_Flags.l) != 0)
            {
                retVal += "l";
            }
            if ((flags & IMAP_ACL_Flags.r) != 0)
            {
                retVal += "r";
            }
            if ((flags & IMAP_ACL_Flags.s) != 0)
            {
                retVal += "s";
            }
            if ((flags & IMAP_ACL_Flags.w) != 0)
            {
                retVal += "w";
            }
            if ((flags & IMAP_ACL_Flags.i) != 0)
            {
                retVal += "i";
            }
            if ((flags & IMAP_ACL_Flags.p) != 0)
            {
                retVal += "p";
            }
            if ((flags & IMAP_ACL_Flags.c) != 0)
            {
                retVal += "c";
            }
            if ((flags & IMAP_ACL_Flags.d) != 0)
            {
                retVal += "d";
            }
            if ((flags & IMAP_ACL_Flags.a) != 0)
            {
                retVal += "a";
            }

            return retVal;
        }

        /// <summary>
        /// Parses IMAP_ACL_Flags from string.
        /// </summary>
        /// <param name="aclString">String from where to convert</param>
        /// <returns></returns>
        public static IMAP_ACL_Flags ACL_From_String(string aclString)
        {
            IMAP_ACL_Flags retVal = IMAP_ACL_Flags.None;
            aclString = aclString.ToLower();
            if (aclString.IndexOf('l') > -1)
            {
                retVal |= IMAP_ACL_Flags.l;
            }
            if (aclString.IndexOf('r') > -1)
            {
                retVal |= IMAP_ACL_Flags.r;
            }
            if (aclString.IndexOf('s') > -1)
            {
                retVal |= IMAP_ACL_Flags.s;
            }
            if (aclString.IndexOf('w') > -1)
            {
                retVal |= IMAP_ACL_Flags.w;
            }
            if (aclString.IndexOf('i') > -1)
            {
                retVal |= IMAP_ACL_Flags.i;
            }
            if (aclString.IndexOf('p') > -1)
            {
                retVal |= IMAP_ACL_Flags.p;
            }
            if (aclString.IndexOf('c') > -1)
            {
                retVal |= IMAP_ACL_Flags.c;
            }
            if (aclString.IndexOf('d') > -1)
            {
                retVal |= IMAP_ACL_Flags.d;
            }
            if (aclString.IndexOf('a') > -1)
            {
                retVal |= IMAP_ACL_Flags.a;
            }

            return retVal;
        }

        /// <summary>
        /// Parses IMAP date time from string.
        /// </summary>
        /// <param name="date">DateTime string.</param>
        /// <returns>Returns parsed date-time value.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>date</b> is null reference.</exception>
        public static DateTime ParseDate(string date)
        {
            if (date == null)
            {
                throw new ArgumentNullException("date");
            }

            return MIME_Utils.ParseRfc2822DateTime(date);
        }

        /// <summary>
        /// Converts date time to IMAP date time string.
        /// </summary>
        /// <param name="date">DateTime to convert.</param>
        /// <returns></returns>
        public static string DateTimeToString(DateTime date)
        {
            string retVal = "";
            retVal += date.ToString("dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            retVal += " " + date.ToString("zzz", CultureInfo.InvariantCulture).Replace(":", "");

            return retVal;
        }

        /// <summary>
        /// Normalizes folder path.  Example: /Inbox/SubFolder/ will be Inbox/SubFolder.
        /// </summary>
        /// <param name="folder">Folder path to normalize.</param>
        /// <returns>Returns normalized folder path.</returns>
        public static string NormalizeFolder(string folder)
        {
            folder = folder.Replace("\\", "/");
            if (folder.StartsWith("/"))
            {
                folder = folder.Substring(1);
            }
            if (folder.EndsWith("/"))
            {
                folder = folder.Substring(0, folder.Length - 1);
            }

            return folder;
        }

        /// <summary>
        /// Parses [quoted] parameter from args text. Parameter may be not quoted, then parameter is
        /// terminated by SP. Example: argsText="string gdkga agkgs";argsText=stringValue 10.
        /// 
        /// This method also removes parsed parameter from argsText.
        /// </summary>
        /// <param name="argsText">Arguments line from where to parse param.</param>
        /// <returns></returns>
        public static string ParseQuotedParam(ref string argsText)
        {
            string paramValue = "";

            // Get value, it is between ""						
            if (argsText.StartsWith("\""))
            {
                // Find next " not escaped "
                char lastChar = ' ';
                int qIndex = -1;
                for (int i = 1; i < argsText.Length; i++)
                {
                    if (argsText[i] == '\"' && lastChar != '\\')
                    {
                        qIndex = i;
                        break;
                    }
                    lastChar = argsText[i];
                }

                if (qIndex == -1)
                {
                    throw new Exception("qouted-string doesn't have enclosing quote(\")");
                }

                paramValue = argsText.Substring(1, qIndex - 1).Replace("\\\"", "\"");

                // Remove <string> value from argsText
                argsText = argsText.Substring(qIndex + 1).Trim();
            }
            else
            {
                paramValue = argsText.Split(' ')[0];

                // Remove <string> value from argsText
                argsText = argsText.Substring(paramValue.Length).Trim();
            }

            return paramValue;
        }

        /// <summary>
        /// Parses bracket parameter from args text. Parameter may be not between (), then
        /// then args text is considered as value. Example: (test test);test test.
        /// 
        /// This method also removes parsed parameter from argsText.
        /// </summary>
        /// <param name="argsText"></param>
        /// <returns></returns>
        public static string ParseBracketParam(ref string argsText)
        {
            string paramValue = "";
            if (argsText.StartsWith("("))
            {
                // Find matching )
                char lastChar = ' ';
                int bIndex = -1;
                int nestedBracketCount = 0;
                for (int i = 1; i < argsText.Length; i++)
                {
                    // There is nested ()
                    if (argsText[i] == '(')
                    {
                        nestedBracketCount++;
                    }
                    else if (argsText[i] == ')')
                    {
                        if (nestedBracketCount == 0)
                        {
                            bIndex = i;
                            break;
                        }
                            // This was nested bracket )
                        else
                        {
                            nestedBracketCount--;
                        }
                    }
                    lastChar = argsText[i];
                }

                if (bIndex == -1)
                {
                    throw new Exception("bracket doesn't have enclosing bracket ')'");
                }

                paramValue = argsText.Substring(1, bIndex - 1);

                // Remove <string> value from argsText
                argsText = argsText.Substring(bIndex + 1).Trim();
            }
            else
            {
                paramValue = argsText;

                argsText = "";
            }

            return paramValue;
        }

        #endregion
    }
}