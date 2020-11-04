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


namespace ASC.Mail.Autoreply.Utility
{
    public static class StringExtensions
    {
        public static string TrimStart(this string str, string trimStr)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            while (str.StartsWith(trimStr))
            {
                str = str.Remove(0, trimStr.Length);
            }

            return str;
        }

        public static string TrimEnd(this string str, string trimStr)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            while (str.EndsWith(trimStr))
            {
                str = str.Remove(str.Length - trimStr.Length);
            }

            return str;
        }

        public static string Trim(this string str, string trimStr)
        {
            return str.TrimStart(trimStr).TrimEnd(trimStr);
        }
    }
}
