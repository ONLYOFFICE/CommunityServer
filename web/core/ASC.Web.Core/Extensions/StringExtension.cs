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
