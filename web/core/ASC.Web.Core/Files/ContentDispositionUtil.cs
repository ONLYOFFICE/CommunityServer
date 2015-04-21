/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System.Linq;
using System.Net.Mime;
using System.Text;

namespace ASC.Web.Core.Files
{
    public static class ContentDispositionUtil
    {
        public static string GetHeaderValue(string fileName, bool inline = false, bool withoutBase = false)
        {
            // If fileName contains any Unicode characters, encode according
            // to RFC 2231 (with clarifications from RFC 5987)
            if (fileName.Any(c => (int)c > 127))
            {
                var str = withoutBase
                    ? "{0}; filename*=UTF-8''{2}"
                    : "{0}; filename=\"{1}\"; filename*=UTF-8''{2}";

                return string.Format(str,
                                     inline ? "inline" : "attachment",
                                     fileName,
                                     CreateRfc2231HeaderValue(fileName));
            }

            // Knowing there are no Unicode characters in this fileName, rely on
            // ContentDisposition.ToString() to encode properly.
            // In .Net 4.0, ContentDisposition.ToString() throws FormatException if
            // the file name contains Unicode characters.
            // In .Net 4.5, ContentDisposition.ToString() no longer throws FormatException
            // if it contains Unicode, and it will not encode Unicode as we require here.
            // The Unicode test above is identical to the 4.0 FormatException test,
            // allowing this helper to give the same results in 4.0 and 4.5.         
            var disposition = new ContentDisposition { FileName = fileName, Inline = inline };
            return disposition.ToString();
        }

        private static string CreateRfc2231HeaderValue(string filename)
        {
            var builder = new StringBuilder();

            var filenameBytes = Encoding.UTF8.GetBytes(filename);
            foreach (var b in filenameBytes)
            {
                if (IsByteValidHeaderValueCharacter(b))
                {
                    builder.Append((char)b);
                }
                else
                {
                    AddByteToStringBuilder(b, builder);
                }
            }

            return builder.ToString();
        }

        // Application of RFC 2231 Encoding to Hypertext Transfer Protocol (HTTP) Header Fields, sec. 3.2
        // http://greenbytes.de/tech/webdav/draft-reschke-rfc2231-in-http-latest.html
        private static bool IsByteValidHeaderValueCharacter(byte b)
        {
            if ((byte)'0' <= b && b <= (byte)'9')
            {
                return true; // is digit
            }
            if ((byte)'a' <= b && b <= (byte)'z')
            {
                return true; // lowercase letter
            }
            if ((byte)'A' <= b && b <= (byte)'Z')
            {
                return true; // uppercase letter
            }

            switch (b)
            {
                case (byte)'-':
                case (byte)'.':
                case (byte)'_':
                case (byte)'~':
                case (byte)':':
                case (byte)'!':
                case (byte)'$':
                case (byte)'&':
                case (byte)'+':
                    return true;
            }

            return false;
        }

        private static void AddByteToStringBuilder(byte b, StringBuilder builder)
        {
            builder.Append('%');

            int i = b;
            AddHexDigitToStringBuilder(i >> 4, builder);
            AddHexDigitToStringBuilder(i%16, builder);
        }

        private const string HexDigits = "0123456789ABCDEF";

        private static void AddHexDigitToStringBuilder(int digit, StringBuilder builder)
        {
            builder.Append(HexDigits[digit]);
        }
    }
}