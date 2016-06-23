/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System.Text;

namespace ASC.Xmpp.Core.utils.Idn
{

    #region usings

    #endregion

    /// <summary>
    /// </summary>
    public class NFKC
    {
        #region Constants

        /// <summary>
        /// </summary>
        internal const int LBase = 0x1100;

        /// <summary>
        /// </summary>
        internal const int LCount = 19;

        /// <summary>
        ///   Entire hangul code copied from: http://www.unicode.org/unicode/reports/tr15/ Several hangul specific constants
        /// </summary>
        internal const int SBase = 0xAC00;

        /// <summary>
        /// </summary>
        internal const int TBase = 0x11A7;

        /// <summary>
        /// </summary>
        internal const int TCount = 28;

        /// <summary>
        /// </summary>
        internal const int VBase = 0x1161;

        /// <summary>
        /// </summary>
        internal const int VCount = 21;

        #endregion

        #region Members

        /// <summary>
        /// </summary>
        internal static readonly int NCount = VCount*TCount;

        /// <summary>
        /// </summary>
        internal static readonly int SCount = LCount*NCount;

        #endregion

        #region Methods

        /// <summary>
        ///   Applies NFKC normalization to a string.
        /// </summary>
        /// <param name="sbIn"> </param>
        /// <returns> An NFKC normalized string. </returns>
        public static string NormalizeNFKC(string sbIn)
        {
            var sbOut = new StringBuilder();

            for (int i = 0; i < sbIn.Length; i++)
            {
                char code = sbIn[i];

                // In Unicode 3.0, Hangul was defined as the block from U+AC00
                // to U+D7A3, however, since Unicode 3.2 the block extends until
                // U+D7AF. The decomposeHangul function only decomposes until
                // U+D7A3. Should this be changed?
                if (code >= 0xAC00 && code <= 0xD7AF)
                {
                    sbOut.Append(decomposeHangul(code));
                }
                else
                {
                    int index = decomposeIndex(code);
                    if (index == -1)
                    {
                        sbOut.Append(code);
                    }
                    else
                    {
                        sbOut.Append(DecompositionMappings.m[index]);
                    }
                }
            }

            // Bring the stringbuffer into canonical order.
            canonicalOrdering(sbOut);

            // Do the canonical composition.
            int last_cc = 0;
            int last_start = 0;

            for (int i = 0; i < sbOut.Length; i++)
            {
                int cc = combiningClass(sbOut[i]);

                if (i > 0 && (last_cc == 0 || last_cc != cc))
                {
                    // Try to combine characters
                    char a = sbOut[last_start];
                    char b = sbOut[i];

                    int c = compose(a, b);

                    if (c != -1)
                    {
                        sbOut[last_start] = (char) c;

                        // sbOut.deleteCharAt(i);
                        sbOut.Remove(i, 1);
                        i--;

                        if (i == last_start)
                        {
                            last_cc = 0;
                        }
                        else
                        {
                            last_cc = combiningClass(sbOut[i - 1]);
                        }

                        continue;
                    }
                }

                if (cc == 0)
                {
                    last_start = i;
                }

                last_cc = cc;
            }

            return sbOut.ToString();
        }

        #endregion

        #region Internal methods

        /// <summary>
        ///   Returns the index inside the decomposition table, implemented using a binary search.
        /// </summary>
        /// <param name="c"> Character to look up. </param>
        /// <returns> Index if found, -1 otherwise. </returns>
        internal static int decomposeIndex(char c)
        {
            int start = 0;
            int end = DecompositionKeys.k.Length/2;

            while (true)
            {
                int half = (start + end)/2;
                int code = DecompositionKeys.k[half*2];

                if (c == code)
                {
                    return DecompositionKeys.k[half*2 + 1];
                }

                if (half == start)
                {
                    // Character not found
                    return -1;
                }
                else if (c > code)
                {
                    start = half;
                }
                else
                {
                    end = half;
                }
            }
        }

        /// <summary>
        ///   Returns the combining class of a given character.
        /// </summary>
        /// <param name="c"> The character. </param>
        /// <returns> The combining class. </returns>
        internal static int combiningClass(char c)
        {
            int h = c >> 8;
            int l = c & 0xff;

            int i = CombiningClass.i[h];
            if (i > -1)
            {
                return CombiningClass.c[i, l];
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        ///   Rearranges characters in a stringbuffer in order to respect the canonical ordering properties.
        /// </summary>
        /// <param name="sbIn"> </param>
        internal static void canonicalOrdering(StringBuilder sbIn)
        {
            bool isOrdered = false;

            while (!isOrdered)
            {
                isOrdered = true;

                // 24.10.2005
                int lastCC = 0;
                if (sbIn.Length > 0)
                {
                    lastCC = combiningClass(sbIn[0]);
                }

                for (int i = 0; i < sbIn.Length - 1; i++)
                {
                    int nextCC = combiningClass(sbIn[i + 1]);
                    if (nextCC != 0 && lastCC > nextCC)
                    {
                        for (int j = i + 1; j > 0; j--)
                        {
                            if (combiningClass(sbIn[j - 1]) <= nextCC)
                            {
                                break;
                            }

                            char t = sbIn[j];
                            sbIn[j] = sbIn[j - 1];
                            sbIn[j - 1] = t;
                            isOrdered = false;
                        }

                        nextCC = lastCC;
                    }

                    lastCC = nextCC;
                }
            }
        }

        /// <summary>
        ///   Returns the index inside the composition table.
        /// </summary>
        /// <param name="a"> Character to look up. </param>
        /// <returns> Index if found, -1 otherwise. </returns>
        internal static int composeIndex(char a)
        {
            if (a >> 8 >= Composition.composePage.Length)
            {
                return -1;
            }

            int ap = Composition.composePage[a >> 8];
            if (ap == -1)
            {
                return -1;
            }

            return Composition.composeData[ap, a & 0xff];
        }

        /// <summary>
        ///   Tries to compose two characters canonically.
        /// </summary>
        /// <param name="a"> First character. </param>
        /// <param name="b"> Second character. </param>
        /// <returns> The composed character or -1 if no composition could be found. </returns>
        internal static int compose(char a, char b)
        {
            int h = composeHangul(a, b);
            if (h != -1)
            {
                return h;
            }

            int ai = composeIndex(a);

            if (ai >= Composition.singleFirstStart && ai < Composition.singleSecondStart)
            {
                if (b == Composition.singleFirst[ai - Composition.singleFirstStart, 0])
                {
                    return Composition.singleFirst[ai - Composition.singleFirstStart, 1];
                }
                else
                {
                    return -1;
                }
            }

            int bi = composeIndex(b);

            if (bi >= Composition.singleSecondStart)
            {
                if (a == Composition.singleSecond[bi - Composition.singleSecondStart, 0])
                {
                    return Composition.singleSecond[bi - Composition.singleSecondStart, 1];
                }
                else
                {
                    return -1;
                }
            }

            if (ai >= 0 && ai < Composition.multiSecondStart && bi >= Composition.multiSecondStart &&
                bi < Composition.singleFirstStart)
            {
                char[] f = Composition.multiFirst[ai];

                if (bi - Composition.multiSecondStart < f.Length)
                {
                    char r = f[bi - Composition.multiSecondStart];
                    if (r == 0)
                    {
                        return -1;
                    }
                    else
                    {
                        return r;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        ///   Decomposes a hangul character.
        /// </summary>
        /// <param name="s"> A character to decompose. </param>
        /// <returns> A string containing the hangul decomposition of the input character. If no hangul decomposition can be found, a string containing the character itself is returned. </returns>
        internal static string decomposeHangul(char s)
        {
            int SIndex = s - SBase;
            if (SIndex < 0 || SIndex >= SCount)
            {
                return s.ToString();
            }

            var result = new StringBuilder();
            int L = LBase + SIndex/NCount;
            int V = VBase + (SIndex%NCount)/TCount;
            int T = TBase + SIndex%TCount;
            result.Append((char) L);
            result.Append((char) V);
            if (T != TBase)
            {
                result.Append((char) T);
            }

            return result.ToString();
        }

        /// <summary>
        ///   Composes two hangul characters.
        /// </summary>
        /// <param name="a"> First character. </param>
        /// <param name="b"> Second character. </param>
        /// <returns> Returns the composed character or -1 if the two characters cannot be composed. </returns>
        internal static int composeHangul(char a, char b)
        {
            // 1. check to see if two current characters are L and V
            int LIndex = a - LBase;
            if (0 <= LIndex && LIndex < LCount)
            {
                int VIndex = b - VBase;
                if (0 <= VIndex && VIndex < VCount)
                {
                    // make syllable of form LV
                    return SBase + (LIndex*VCount + VIndex)*TCount;
                }
            }

            // 2. check to see if two current characters are LV and T
            int SIndex = a - SBase;
            if (0 <= SIndex && SIndex < SCount && (SIndex%TCount) == 0)
            {
                int TIndex = b - TBase;
                if (0 <= TIndex && TIndex <= TCount)
                {
                    // make syllable of form LVT
                    return a + TIndex;
                }
            }

            return -1;
        }

        #endregion
    }
}