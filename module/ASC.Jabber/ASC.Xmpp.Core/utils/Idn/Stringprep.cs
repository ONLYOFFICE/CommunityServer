/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using System;
using System.Text;

namespace ASC.Xmpp.Core.utils.Idn
{

    #region usings

    #endregion

    /// <summary>
    /// </summary>
    public class Stringprep
    {
        #region Members

        /// <summary>
        /// </summary>
        private static readonly char[] RFC3920_NODEPREP_PROHIBIT = new[]
                                                                       {
                                                                           '\u0022', '\u0026', '\'', '\u002F',
                                                                           '\u003A', '\u003C', '\u003E', '\u0040'
                                                                       };

        #endregion

        #region Methods

        /// <summary>
        ///   Preps a name according to the Stringprep profile defined in RFC3491. Unassigned code points are not allowed. *
        /// </summary>
        /// <param name="input"> the name to prep. </param>
        /// <returns> the prepped name. @throws StringprepException If the name cannot be prepped with this profile. @throws NullPointerException If the name is null. </returns>
        public static string NamePrep(string input)
        {
            return NamePrep(input, false);
        }

        /// <summary>
        ///   Preps a name according to the Stringprep profile defined in RFC3491. *
        /// </summary>
        /// <param name="input"> the name to prep. </param>
        /// <param name="allowUnassigned"> true if the name may contain unassigned code points. </param>
        /// <returns> the prepped name. @throws StringprepException If the name cannot be prepped with this profile. @throws NullPointerException If the name is null. </returns>
        public static string NamePrep(string input, bool allowUnassigned)
        {
            if (input == null)
            {
                throw new NullReferenceException();
            }

            var s = new StringBuilder(input);

            if (!allowUnassigned && Contains(s, RFC3454.A1))
            {
                throw new StringprepException(StringprepException.CONTAINS_UNASSIGNED);
            }

            Filter(s, RFC3454.B1);
            Map(s, RFC3454.B2search, RFC3454.B2replace);

            s = new StringBuilder(NFKC.NormalizeNFKC(s.ToString()));

            // B.3 is only needed if NFKC is not used, right?
            // map(s, RFC3454.B3search, RFC3454.B3replace);
            if (Contains(s, RFC3454.C12) || Contains(s, RFC3454.C22) || Contains(s, RFC3454.C3) ||
                Contains(s, RFC3454.C4) || Contains(s, RFC3454.C5) || Contains(s, RFC3454.C6) ||
                Contains(s, RFC3454.C7) || Contains(s, RFC3454.C8))
            {
                // Table C.9 only contains code points > 0xFFFF which Java
                // doesn't handle
                throw new StringprepException(StringprepException.CONTAINS_PROHIBITED);
            }

            // Bidi handling
            bool r = Contains(s, RFC3454.D1);
            bool l = Contains(s, RFC3454.D2);

            // RFC 3454, section 6, requirement 1: already handled above (table C.8)

            // RFC 3454, section 6, requirement 2
            if (r && l)
            {
                throw new StringprepException(StringprepException.BIDI_BOTHRAL);
            }

            // RFC 3454, section 6, requirement 3
            if (r)
            {
                if (!Contains(s[0], RFC3454.D1) || !Contains(s[s.Length - 1], RFC3454.D1))
                {
                    throw new StringprepException(StringprepException.BIDI_LTRAL);
                }
            }

            return s.ToString();
        }

        /**
		* Characters prohibited by RFC3920 nodeprep that aren't defined as
		* part of the RFC3454 tables.
		*/

        /// <summary>
        ///   Preps a node name according to the Stringprep profile defined in RFC3920. Unassigned code points are not allowed. *
        /// </summary>
        /// <param name="input"> the node name to prep. </param>
        /// <returns> the prepped node name. @throws StringprepException If the node name cannot be prepped with this profile. @throws NullPointerException If the node name is null. </returns>
        public static string NodePrep(string input)
        {
            return NodePrep(input, false);
        }

        /// <summary>
        ///   Preps a node name according to the Stringprep profile defined in RFC3920.
        /// </summary>
        /// <param name="input"> the node name to prep. </param>
        /// <param name="allowUnassigned"> true if the node name may contain unassigned code points. </param>
        /// <returns> the prepped node name. @throws StringprepException If the node name cannot be prepped with this profile. @throws NullPointerException If the node name is null. </returns>
        public static string NodePrep(string input, bool allowUnassigned)
        {
            if (input == null)
            {
                throw new NullReferenceException();
            }

            var s = new StringBuilder(input);

            if (!allowUnassigned && Contains(s, RFC3454.A1))
            {
                throw new StringprepException(StringprepException.CONTAINS_UNASSIGNED);
            }

            Filter(s, RFC3454.B1);
            Map(s, RFC3454.B2search, RFC3454.B2replace);

            s = new StringBuilder(NFKC.NormalizeNFKC(s.ToString()));

            if (Contains(s, RFC3454.C11) || Contains(s, RFC3454.C12) || Contains(s, RFC3454.C21) ||
                Contains(s, RFC3454.C22) || Contains(s, RFC3454.C3) || Contains(s, RFC3454.C4) ||
                Contains(s, RFC3454.C5) || Contains(s, RFC3454.C6) || Contains(s, RFC3454.C7) ||
                Contains(s, RFC3454.C8) || Contains(s, RFC3920_NODEPREP_PROHIBIT))
            {
                // Table C.9 only contains code points > 0xFFFF which Java
                // doesn't handle
                throw new StringprepException(StringprepException.CONTAINS_PROHIBITED);
            }

            // Bidi handling
            bool r = Contains(s, RFC3454.D1);
            bool l = Contains(s, RFC3454.D2);

            // RFC 3454, section 6, requirement 1: already handled above (table C.8)

            // RFC 3454, section 6, requirement 2
            if (r && l)
            {
                throw new StringprepException(StringprepException.BIDI_BOTHRAL);
            }

            // RFC 3454, section 6, requirement 3
            if (r)
            {
                if (!Contains(s[0], RFC3454.D1) || !Contains(s[s.Length - 1], RFC3454.D1))
                {
                    throw new StringprepException(StringprepException.BIDI_LTRAL);
                }
            }

            return s.ToString();
        }

        /// <summary>
        ///   Preps a resource name according to the Stringprep profile defined in RFC3920. Unassigned code points are not allowed.
        /// </summary>
        /// <param name="input"> the resource name to prep. </param>
        /// <returns> the prepped node name. @throws StringprepException If the resource name cannot be prepped with this profile. @throws NullPointerException If the resource name is null. </returns>
        public static string ResourcePrep(string input)
        {
            return ResourcePrep(input, false);
        }

        /// <summary>
        ///   Preps a resource name according to the Stringprep profile defined in RFC3920.
        /// </summary>
        /// <param name="input"> the resource name to prep. </param>
        /// <param name="allowUnassigned"> true if the resource name may contain unassigned code points. </param>
        /// <returns> the prepped node name. @throws StringprepException If the resource name cannot be prepped with this profile. @throws NullPointerException If the resource name is null. </returns>
        public static string ResourcePrep(string input, bool allowUnassigned)
        {
            if (input == null)
            {
                throw new NullReferenceException();
            }

            var s = new StringBuilder(input);

            if (!allowUnassigned && Contains(s, RFC3454.A1))
            {
                throw new StringprepException(StringprepException.CONTAINS_UNASSIGNED);
            }

            Filter(s, RFC3454.B1);

            s = new StringBuilder(NFKC.NormalizeNFKC(s.ToString()));

            if (Contains(s, RFC3454.C12) || Contains(s, RFC3454.C21) || Contains(s, RFC3454.C22) ||
                Contains(s, RFC3454.C3) || Contains(s, RFC3454.C4) || Contains(s, RFC3454.C5) ||
                Contains(s, RFC3454.C6) || Contains(s, RFC3454.C7) || Contains(s, RFC3454.C8))
            {
                // Table C.9 only contains code points > 0xFFFF which Java
                // doesn't handle
                throw new StringprepException(StringprepException.CONTAINS_PROHIBITED);
            }

            // Bidi handling
            bool r = Contains(s, RFC3454.D1);
            bool l = Contains(s, RFC3454.D2);

            // RFC 3454, section 6, requirement 1: already handled above (table C.8)

            // RFC 3454, section 6, requirement 2
            if (r && l)
            {
                throw new StringprepException(StringprepException.BIDI_BOTHRAL);
            }

            // RFC 3454, section 6, requirement 3
            if (r)
            {
                if (!Contains(s[0], RFC3454.D1) || !Contains(s[s.Length - 1], RFC3454.D1))
                {
                    throw new StringprepException(StringprepException.BIDI_LTRAL);
                }
            }

            return s.ToString();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// </summary>
        /// <param name="s"> </param>
        /// <param name="p"> </param>
        /// <returns> </returns>
        internal static bool Contains(StringBuilder s, char[] p)
        {
            for (int i = 0; i < p.Length; i++)
            {
                char c = p[i];
                for (int j = 0; j < s.Length; j++)
                {
                    if (c == s[j])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="s"> </param>
        /// <param name="p"> </param>
        /// <returns> </returns>
        internal static bool Contains(StringBuilder s, char[][] p)
        {
            for (int i = 0; i < p.Length; i++)
            {
                char[] r = p[i];
                if (1 == r.Length)
                {
                    char c = r[0];
                    for (int j = 0; j < s.Length; j++)
                    {
                        if (c == s[j])
                        {
                            return true;
                        }
                    }
                }
                else if (2 == r.Length)
                {
                    char f = r[0];
                    char t = r[1];
                    for (int j = 0; j < s.Length; j++)
                    {
                        if (f <= s[j] && t >= s[j])
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="c"> </param>
        /// <param name="p"> </param>
        /// <returns> </returns>
        internal static bool Contains(char c, char[][] p)
        {
            for (int i = 0; i < p.Length; i++)
            {
                char[] r = p[i];
                if (1 == r.Length)
                {
                    if (c == r[0])
                    {
                        return true;
                    }
                }
                else if (2 == r.Length)
                {
                    char f = r[0];
                    char t = r[1];
                    if (f <= c && t >= c)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="s"> </param>
        /// <param name="f"> </param>
        internal static void Filter(StringBuilder s, char[] f)
        {
            for (int i = 0; i < f.Length; i++)
            {
                char c = f[i];

                int j = 0;
                while (j < s.Length)
                {
                    if (c == s[j])
                    {
                        // s.deleteCharAt(j);
                        s.Remove(j, 1);
                    }
                    else
                    {
                        j++;
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="s"> </param>
        /// <param name="f"> </param>
        internal static void Filter(StringBuilder s, char[][] f)
        {
            for (int i = 0; i < f.Length; i++)
            {
                char[] r = f[i];

                if (1 == r.Length)
                {
                    char c = r[0];

                    int j = 0;
                    while (j < s.Length)
                    {
                        if (c == s[j])
                        {
                            // s.deleteCharAt(j);
                            s.Remove(j, 1);
                        }
                        else
                        {
                            j++;
                        }
                    }
                }
                else if (2 == r.Length)
                {
                    char from = r[0];
                    char to = r[1];

                    int j = 0;
                    while (j < s.Length)
                    {
                        if (from <= s[j] && to >= s[j])
                        {
                            // s.deleteCharAt(j);
                            s.Remove(j, 1);
                        }
                        else
                        {
                            j++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="s"> </param>
        /// <param name="search"> </param>
        /// <param name="replace"> </param>
        internal static void Map(StringBuilder s, char[] search, string[] replace)
        {
            for (int i = 0; i < search.Length; i++)
            {
                char c = search[i];

                int j = 0;
                while (j < s.Length)
                {
                    if (c == s[j])
                    {
                        // s.deleteCharAt(j);
                        s.Remove(j, 1);
                        if (null != replace[i])
                        {
                            s.Insert(j, replace[i]);
                            j += replace[i].Length - 1;
                        }
                    }
                    else
                    {
                        j++;
                    }
                }
            }
        }

        #endregion
    }
}