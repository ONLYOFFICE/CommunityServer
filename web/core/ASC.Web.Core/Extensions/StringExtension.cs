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


using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace System
{
    public static class StringExtension
    {
        private static readonly Regex reStrict = new Regex(@"^(([^<>()[\]\\.,;:\s@\""]+"
                  + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                  + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$");


        public static string HtmlEncode(this string str)
        {
            return !string.IsNullOrEmpty(str) ? HttpUtility.HtmlEncode(str) : str;
        }

        /// <summary>
        /// Replace ' on ′
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceSingleQuote(this string str)
        {
            return str == null ? null : str.Replace("'", "′");
        }

        public static bool TestEmailRegex(this string emailAddress)
        {
            emailAddress = (emailAddress ?? "").Trim();
            return !string.IsNullOrEmpty(emailAddress) && reStrict.IsMatch(emailAddress);
        }

        public static string GetMD5Hash(this string str)
        {
            var bytes = Encoding.Unicode.GetBytes(str);

            var CSP = new MD5CryptoServiceProvider();

            var byteHash = CSP.ComputeHash(bytes);

            return byteHash.Aggregate(String.Empty, (current, b) => current + String.Format("{0:x2}", b));
        }

        public static int EnumerableComparer(this string x, string y)
        {
            var xIndex = 0;
            var yIndex = 0;

            while (xIndex < x.Length)
            {
                if (yIndex >= y.Length)
                    return 1;

                if (char.IsDigit(x[xIndex]) && char.IsDigit(y[yIndex]))
                {
                    var xBuilder = new StringBuilder();
                    while (xIndex < x.Length && char.IsDigit(x[xIndex]))
                    {
                        xBuilder.Append(x[xIndex++]);
                    }

                    var yBuilder = new StringBuilder();
                    while (yIndex < y.Length && char.IsDigit(y[yIndex]))
                    {
                        yBuilder.Append(y[yIndex++]);
                    }

                    long xValue;
                    if (!long.TryParse(xBuilder.ToString(), out xValue))
                    {
                        xValue = Int64.MaxValue;
                    }

                    long yValue;
                    if (!long.TryParse(yBuilder.ToString(), out yValue))
                    {
                        yValue = Int64.MaxValue;
                    }

                    int difference;
                    if ((difference = xValue.CompareTo(yValue)) != 0)
                        return difference;
                }
                else
                {
                    int difference;
                    if ((difference = string.Compare(x[xIndex].ToString(CultureInfo.InvariantCulture), y[yIndex].ToString(CultureInfo.InvariantCulture), StringComparison.InvariantCultureIgnoreCase)) != 0)
                        return difference;

                    xIndex++;
                    yIndex++;
                }
            }

            if (yIndex < y.Length)
                return -1;

            return 0;
        }
    }
}
