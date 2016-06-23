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


using System;
using System.Text;

namespace ASC.Xmpp.Core.utils.Idn
{

    #region usings

    #endregion

    /// <summary>
    /// </summary>
    public class Punycode
    {
        #region Constants

        /// <summary>
        /// </summary>
        internal const int BASE = 36;

        /// <summary>
        /// </summary>
        internal const int DAMP = 700;

        /// <summary>
        /// </summary>
        internal const char DELIMITER = '-';

        /// <summary>
        /// </summary>
        internal const int INITIAL_BIAS = 72;

        /// <summary>
        /// </summary>
        internal const int INITIAL_N = 128;

        /// <summary>
        /// </summary>
        internal const int SKEW = 38;

        /// <summary>
        /// </summary>
        internal const int TMAX = 26;

        /// <summary>
        /// </summary>
        internal const int TMIN = 1;

        #endregion

        #region Methods

        /// <summary>
        ///   Punycodes a unicode string.
        /// </summary>
        /// <param name="input"> Unicode string. </param>
        /// <returns> Punycoded string. </returns>
        public static string Encode(string input)
        {
            int n = INITIAL_N;
            int delta = 0;
            int bias = INITIAL_BIAS;
            var output = new StringBuilder();

            // Copy all basic code points to the output
            int b = 0;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (IsBasic(c))
                {
                    output.Append(c);
                    b++;
                }
            }

            // Append delimiter
            if (b > 0)
            {
                output.Append(DELIMITER);
            }

            int h = b;
            while (h < input.Length)
            {
                int m = Int32.MaxValue;

                // Find the minimum code point >= n
                for (int i = 0; i < input.Length; i++)
                {
                    int c = input[i];
                    if (c >= n && c < m)
                    {
                        m = c;
                    }
                }

                if (m - n > (Int32.MaxValue - delta)/(h + 1))
                {
                    throw new PunycodeException(PunycodeException.OVERFLOW);
                }

                delta = delta + (m - n)*(h + 1);
                n = m;

                for (int j = 0; j < input.Length; j++)
                {
                    int c = input[j];
                    if (c < n)
                    {
                        delta++;
                        if (0 == delta)
                        {
                            throw new PunycodeException(PunycodeException.OVERFLOW);
                        }
                    }

                    if (c == n)
                    {
                        int q = delta;

                        for (int k = BASE;; k += BASE)
                        {
                            int t;
                            if (k <= bias)
                            {
                                t = TMIN;
                            }
                            else if (k >= bias + TMAX)
                            {
                                t = TMAX;
                            }
                            else
                            {
                                t = k - bias;
                            }

                            if (q < t)
                            {
                                break;
                            }

                            output.Append((char) Digit2Codepoint(t + (q - t)%(BASE - t)));
                            q = (q - t)/(BASE - t);
                        }

                        output.Append((char) Digit2Codepoint(q));
                        bias = Adapt(delta, h + 1, h == b);
                        delta = 0;
                        h++;
                    }
                }

                delta++;
                n++;
            }

            return output.ToString();
        }

        /// <summary>
        ///   Decode a punycoded string.
        /// </summary>
        /// <param name="input"> Punycode string </param>
        /// <returns> Unicode string. </returns>
        public static string Decode(string input)
        {
            int n = INITIAL_N;
            int i = 0;
            int bias = INITIAL_BIAS;
            var output = new StringBuilder();

            int d = input.LastIndexOf(DELIMITER);
            if (d > 0)
            {
                for (int j = 0; j < d; j++)
                {
                    char c = input[j];
                    if (!IsBasic(c))
                    {
                        throw new PunycodeException(PunycodeException.BAD_INPUT);
                    }

                    output.Append(c);
                }

                d++;
            }
            else
            {
                d = 0;
            }

            while (d < input.Length)
            {
                int oldi = i;
                int w = 1;

                for (int k = BASE;; k += BASE)
                {
                    if (d == input.Length)
                    {
                        throw new PunycodeException(PunycodeException.BAD_INPUT);
                    }

                    int c = input[d++];
                    int digit = Codepoint2Digit(c);
                    if (digit > (Int32.MaxValue - i)/w)
                    {
                        throw new PunycodeException(PunycodeException.OVERFLOW);
                    }

                    i = i + digit*w;

                    int t;
                    if (k <= bias)
                    {
                        t = TMIN;
                    }
                    else if (k >= bias + TMAX)
                    {
                        t = TMAX;
                    }
                    else
                    {
                        t = k - bias;
                    }

                    if (digit < t)
                    {
                        break;
                    }

                    w = w*(BASE - t);
                }

                bias = Adapt(i - oldi, output.Length + 1, oldi == 0);

                if (i/(output.Length + 1) > Int32.MaxValue - n)
                {
                    throw new PunycodeException(PunycodeException.OVERFLOW);
                }

                n = n + i/(output.Length + 1);
                i = i%(output.Length + 1);

                // following overload is not supported on CF
                // output.Insert(i,(char) n);
                output.Insert(i, new[] {(char) n});
                i++;
            }

            return output.ToString();
        }

        /// <summary>
        /// </summary>
        /// <param name="delta"> </param>
        /// <param name="numpoints"> </param>
        /// <param name="first"> </param>
        /// <returns> </returns>
        public static int Adapt(int delta, int numpoints, bool first)
        {
            if (first)
            {
                delta = delta/DAMP;
            }
            else
            {
                delta = delta/2;
            }

            delta = delta + (delta/numpoints);

            int k = 0;
            while (delta > ((BASE - TMIN)*TMAX)/2)
            {
                delta = delta/(BASE - TMIN);
                k = k + BASE;
            }

            return k + ((BASE - TMIN + 1)*delta)/(delta + SKEW);
        }

        /// <summary>
        /// </summary>
        /// <param name="c"> </param>
        /// <returns> </returns>
        public static bool IsBasic(char c)
        {
            return c < 0x80;
        }

        /// <summary>
        /// </summary>
        /// <param name="d"> </param>
        /// <returns> </returns>
        /// <exception cref="PunycodeException"></exception>
        public static int Digit2Codepoint(int d)
        {
            if (d < 26)
            {
                // 0..25 : 'a'..'z'
                return d + 'a';
            }
            else if (d < 36)
            {
                // 26..35 : '0'..'9';
                return d - 26 + '0';
            }
            else
            {
                throw new PunycodeException(PunycodeException.BAD_INPUT);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="c"> </param>
        /// <returns> </returns>
        /// <exception cref="PunycodeException"></exception>
        public static int Codepoint2Digit(int c)
        {
            if (c - '0' < 10)
            {
                // '0'..'9' : 26..35
                return c - '0' + 26;
            }
            else if (c - 'a' < 26)
            {
                // 'a'..'z' : 0..25
                return c - 'a';
            }
            else
            {
                throw new PunycodeException(PunycodeException.BAD_INPUT);
            }
        }

        #endregion

        /* Punycode parameters */
    }
}