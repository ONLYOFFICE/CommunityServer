/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Ascensio System Limited" file="IDNA.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using System.Text;

namespace ASC.Xmpp.Core.utils.Idn
{

    #region usings

    #endregion

    /// <summary>
    /// </summary>
    public class IDNA
    {
        #region Constants

        /// <summary>
        /// </summary>
        public const string ACE_PREFIX = "xn--";

        #endregion

        #region Methods

        /// <summary>
        ///   Converts a Unicode string to ASCII using the procedure in RFC3490 section 4.1. Unassigned characters are not allowed and STD3 ASCII rules are enforced. The input string may be a domain name containing dots.
        /// </summary>
        /// <param name="input"> Unicode string. </param>
        /// <returns> Encoded string. </returns>
        public static string ToASCII(string input)
        {
            var o = new StringBuilder();
            var h = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '.' || c == '\u3002' || c == '\uff0e' || c == '\uff61')
                {
                    o.Append(ToASCII(h.ToString(), false, true));
                    o.Append('.');
                    h = new StringBuilder();
                }
                else
                {
                    h.Append(c);
                }
            }

            o.Append(ToASCII(h.ToString(), false, true));
            return o.ToString();
        }

        /// <summary>
        ///   Converts a Unicode string to ASCII using the procedure in RFC3490 section 4.1. Unassigned characters are not allowed and STD3 ASCII rules are enforced.
        /// </summary>
        /// <param name="input"> Unicode string. </param>
        /// <param name="allowUnassigned"> Unassigned characters, allowed or not? </param>
        /// <param name="useSTD3ASCIIRules"> STD3 ASCII rules, enforced or not? </param>
        /// <returns> Encoded string. </returns>
        public static string ToASCII(string input, bool allowUnassigned, bool useSTD3ASCIIRules)
        {
            // Step 1: Check if the string contains code points outside
            // the ASCII range 0..0x7c.
            bool nonASCII = false;

            for (int i = 0; i < input.Length; i++)
            {
                int c = input[i];
                if (c > 0x7f)
                {
                    nonASCII = true;
                    break;
                }
            }

            // Step 2: Perform the nameprep operation.
            if (nonASCII)
            {
                try
                {
                    input = Stringprep.NamePrep(input, allowUnassigned);
                }
                catch (StringprepException e)
                {
                    // TODO 
                    throw new IDNAException(e);
                }
            }

            // Step 3: - Verify the absence of non-LDH ASCII code points
            // (char) 0..0x2c, 0x2e..0x2f, 0x3a..0x40, 0x5b..0x60,
            // (char) 0x7b..0x7f
            // - Verify the absence of leading and trailing
            // hyphen-minus

            if (useSTD3ASCIIRules)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    int c = input[i];
                    if ((c <= 0x2c) || (c >= 0x2e && c <= 0x2f) || (c >= 0x3a && c <= 0x40) ||
                        (c >= 0x5b && c <= 0x60) || (c >= 0x7b && c <= 0x7f))
                    {
                        throw new IDNAException(IDNAException.CONTAINS_NON_LDH);
                    }
                }

                if (input.StartsWith("-") || input.EndsWith("-"))
                {
                    throw new IDNAException(IDNAException.CONTAINS_HYPHEN);
                }
            }

            // Step 4: If all code points are inside 0..0x7f, skip to step 8
            nonASCII = false;

            for (int i = 0; i < input.Length; i++)
            {
                int c = input[i];
                if (c > 0x7f)
                {
                    nonASCII = true;
                    break;
                }
            }

            string output = input;

            if (nonASCII)
            {
                // Step 5: Verify that the sequence does not begin with the ACE prefix.
                if (input.StartsWith(ACE_PREFIX))
                {
                    throw new IDNAException(IDNAException.CONTAINS_ACE_PREFIX);
                }

                // Step 6: Punycode
                try
                {
                    output = Punycode.Encode(input);
                }
                catch (PunycodeException e)
                {
                    // TODO
                    throw new IDNAException(e);
                }

                // Step 7: Prepend the ACE prefix.
                output = ACE_PREFIX + output;
            }

            // Step 8: Check that the length is inside 1..63.
            if (output.Length < 1 || output.Length > 63)
            {
                throw new IDNAException(IDNAException.TOO_LONG);
            }

            return output;
        }

        /// <summary>
        ///   Converts an ASCII-encoded string to Unicode. Unassigned characters are not allowed and STD3 hostnames are enforced. Input may be domain name containing dots.
        /// </summary>
        /// <param name="input"> ASCII input string. </param>
        /// <returns> Unicode string. </returns>
        public static string ToUnicode(string input)
        {
            input = input.ToLower();
            var o = new StringBuilder();
            var h = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '.' || c == '\u3002' || c == '\uff0e' || c == '\uff61')
                {
                    o.Append(ToUnicode(h.ToString(), false, true));
                    o.Append(c);
                    h = new StringBuilder();
                }
                else
                {
                    h.Append(c);
                }
            }

            o.Append(ToUnicode(h.ToString(), false, true));
            return o.ToString();
        }

        /// <summary>
        ///   Converts an ASCII-encoded string to Unicode.
        /// </summary>
        /// <param name="input"> ASCII input string. </param>
        /// <param name="allowUnassigned"> Allow unassigned Unicode characters. </param>
        /// <param name="useSTD3ASCIIRules"> Check that the output conforms to STD3. </param>
        /// <returns> Unicode string. </returns>
        public static string ToUnicode(string input, bool allowUnassigned, bool useSTD3ASCIIRules)
        {
            string original = input;
            bool nonASCII = false;

            // Step 1: If all code points are inside 0..0x7f, skip to step 3.
            for (int i = 0; i < input.Length; i++)
            {
                int c = input[i];
                if (c > 0x7f)
                {
                    nonASCII = true;
                    break;
                }
            }

            // Step 2: Perform the Nameprep operation.
            if (nonASCII)
            {
                try
                {
                    input = Stringprep.NamePrep(input, allowUnassigned);
                }
                catch (StringprepException)
                {
                    // ToUnicode never fails!
                    return original;
                }
            }

            // Step 3: Verify the sequence starts with the ACE prefix.
            if (!input.StartsWith(ACE_PREFIX))
            {
                // ToUnicode never fails!
                return original;
            }

            string stored = input;

            // Step 4: Remove the ACE prefix.
            input = input.Substring(ACE_PREFIX.Length);

            // Step 5: Decode using punycode
            string output;

            try
            {
                output = Punycode.Decode(input);
            }
            catch (PunycodeException)
            {
                // ToUnicode never fails!
                return original;
            }

            // Step 6: Apply toASCII
            string ascii;

            try
            {
                ascii = ToASCII(output, allowUnassigned, useSTD3ASCIIRules);
            }
            catch (IDNAException)
            {
                // ToUnicode never fails!
                return original;
            }

            // Step 7: Compare case-insensitively.
            if (!ascii.ToUpper().Equals(stored.ToUpper()))
            {
                // ToUnicode never fails!
                return original;
            }

            // Step 8: Return the result.
            return output;
        }

        #endregion
    }
}