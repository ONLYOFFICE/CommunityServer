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


using System;
using System.Linq;
using System.Text;

namespace ASC.Common.Utils
{
    public static class MimeHeaderUtils
    {
        public static string EncodeMime(string mimeHeaderValue)
        {
            return EncodeMime(mimeHeaderValue, Encoding.UTF8, false);
        }

        public static string EncodeMime(string mimeHeaderValue, Encoding charset, bool split)
        {
            if (MustEncode(mimeHeaderValue))
            {
                var result = new StringBuilder();
                var data = charset.GetBytes(mimeHeaderValue);
                var maxEncodedTextSize = split ? 75 - ("=?" + charset.WebName + "?" + "B"/*Base64 encode*/ + "?" + "?=").Length : int.MaxValue;

                result.Append("=?" + charset.WebName + "?B?");
                var stored = 0;
                var base64 = Convert.ToBase64String(data);
                for (var i = 0; i < base64.Length; i += 4)
                {
                    // Encoding buffer full, create new encoded-word.
                    if (stored + 4 > maxEncodedTextSize)
                    {
                        result.Append("?=\r\n =?" + charset.WebName + "?B?");
                        stored = 0;
                    }

                    result.Append(base64, i, 4);
                    stored += 4;
                }
                result.Append("?=");
                return result.ToString();
            }
            return mimeHeaderValue;
        }

        public static bool MustEncode(string text)
        {
            return !string.IsNullOrEmpty(text) && text.Any(c => c > 127);
        }
    }
}