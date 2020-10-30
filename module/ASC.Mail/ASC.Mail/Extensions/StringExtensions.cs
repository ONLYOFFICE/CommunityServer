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


using System.Security.Cryptography;
using System.Text;

namespace ASC.Mail.Extensions
{
    public static class StringExtensions
    {
        public static string GetMd5(this string text)
        {
            var bs = Encoding.UTF8.GetBytes(text);
            return bs.GetMd5();
        }

        public static string GetMd5(this byte[] utf8Bytes)
        {
            var x =
                new MD5CryptoServiceProvider();
            var bs = x.ComputeHash(utf8Bytes);
            var s = new StringBuilder(32);
            foreach (var b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            return s.ToString();
        }

        public static string Prefix(this string str, string prefix)
        {
            return string.IsNullOrEmpty(prefix) ? str : string.Format("{0}.{1}", prefix, str);
        }

        public static string Alias(this string str, string alias)
        {
            return string.IsNullOrEmpty(alias) ? str : string.Format("{0} {1}", str, alias);
        }

        public static string Tabs(int n)
        {
            return new string('\t', n);
        }
    }
}
