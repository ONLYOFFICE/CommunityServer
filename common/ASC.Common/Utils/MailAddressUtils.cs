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


using System.Net.Mail;
using System.Text;

namespace ASC.Common.Utils
{
    public static class MailAddressUtils
    {
        public static MailAddress Create(string address)
        {
            if (!string.IsNullOrEmpty(address))
            {
                var firstPos = address.IndexOf('"');
                var lastPos = address.LastIndexOf('"');
                if (firstPos != -1 && firstPos < lastPos && address.IndexOf('"', firstPos + 1, lastPos - firstPos - 1) != -1)
                {
                    address = new StringBuilder(address).Replace("\"", string.Empty, firstPos + 1, lastPos - firstPos - 1).ToString();
                }
            }
            return new MailAddress(address);
        }

        public static MailAddress Create(string address, string displayName)
        {
            if (!string.IsNullOrEmpty(displayName))
            {
                displayName = displayName.Replace("\"", string.Empty);
                if (125 < displayName.Length)
                {
                    displayName = displayName.Substring(0, 125);
                }
            }
            return Create(ToSmtpAddress(address, displayName));
        }

        public static string ToEncodedString(this MailAddress m)
        {
            return ToSmtpAddress(m.Address, MimeHeaderUtils.EncodeMime(m.DisplayName));
        }


        private static string ToSmtpAddress(string address, string displayName)
        {
            return string.Format("\"{0}\" <{1}>", displayName, address);
        }
    }
}
