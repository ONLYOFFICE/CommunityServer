/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Configuration;
using System.Text.RegularExpressions;
using System.Web;

using ASC.Security.Cryptography;

namespace ASC.Common.Radicale
{
    public abstract class RadicaleEntity
    {
        public string Uid { get; set; }

        public static readonly string defaultRadicaleUrl = (ConfigurationManagerExtension.AppSettings["radicale.path"] != null) ? ConfigurationManagerExtension.AppSettings["radicale.path"] : "http://localhost:5232";

        public readonly string defaultAddBookName = "11111111-1111-1111-1111-111111111111";

        public readonly string readonlyAddBookName = "11111111-1111-1111-1111-111111111111-readonly";

        public string GetRadicaleUrl(string url, string email, bool isReadonly = false, bool isCardDav = false, bool isRedirectUrl = false, string entityId = "", string itemID = "")
        {
            string requestUrl;
            var currentUserName = url.StartsWith("http") ? email.ToLower() + "@" + new Uri(url).Host : email.ToLower() + "@" + url;
            var protocolType = (!isCardDav) ? "/caldav/" : "/carddav/";
            var serverUrl = isRedirectUrl ? new Uri(url).Scheme + "://" + new Uri(url).Host + protocolType :
                defaultRadicaleUrl;
            if (isCardDav)
            {
                var addbookId = isReadonly ? readonlyAddBookName : defaultAddBookName;
                requestUrl = (itemID != "") ? defaultRadicaleUrl + "/" + HttpUtility.UrlEncode(currentUserName) + "/" + addbookId + "/" + itemID + ".vcf" :
                    (isRedirectUrl) ? serverUrl + HttpUtility.UrlEncode(currentUserName) + "/" + addbookId :
                    defaultRadicaleUrl + "/" + HttpUtility.UrlEncode(currentUserName) + "/" + addbookId;
            }
            else
            {
                requestUrl = (itemID != "") ? serverUrl + HttpUtility.UrlEncode(currentUserName) + "/" + entityId + (isReadonly ? "-readonly" : "") +
                                            "/" + HttpUtility.UrlEncode(itemID) + ".ics" :
                                            serverUrl + HttpUtility.UrlEncode(currentUserName) + "/" + entityId + (isReadonly ? "-readonly" : "");
            }
            return requestUrl;
        }

        public string GetSystemAuthorization()
        {
            return ConfigurationManagerExtension.AppSettings["radicale.admin.data"] + ":" + InstanceCrypto.Encrypt(ConfigurationManagerExtension.AppSettings["radicale.admin.data"]);
        }

        protected string GetData(string sample, string name, string description, string backgroundColor)
        {
            string[] numbers = Regex.Split(backgroundColor, @"\D+");
            var color = numbers.Length > 4 ? HexFromRGB(int.Parse(numbers[1]), int.Parse(numbers[2]), int.Parse(numbers[3])) : "#000000";
            return string.Format(sample, name, color, description);
        }

        private string HexFromRGB(int r, int g, int b)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b);
        }
    }
}
