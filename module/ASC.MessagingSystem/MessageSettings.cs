/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


using System.Collections.Generic;
using System.Web;

using UAParser;

namespace ASC.MessagingSystem
{
    public class MessageSettings
    {
        private const string userAgentHeader = "User-Agent";
        private const string forwardedHeader = "X-Forwarded-For";
        private const string hostHeader = "Host";
        private const string refererHeader = "Referer";

        static MessageSettings()
        {
            Parser = Parser.GetDefault();
        }

        private static Parser Parser { get; set; }

        public static ClientInfo GetClientInfo(string uaHeader)
        {
            return Parser.Parse(uaHeader);
        }

        public static string GetUAHeader(HttpRequest request)
        {
            return request != null ? request.Headers[userAgentHeader] : null;
        }

        public static string GetUAHeader(Dictionary<string, string> headers)
        {
            return headers.ContainsKey(userAgentHeader) ? headers[userAgentHeader] : null;
        }

        public static string GetReferer(HttpRequest request)
        {
            return request != null ? request.Headers[refererHeader] : null;
        }

        public static string GetReferer(Dictionary<string, string> headers)
        {
            return headers.ContainsKey(refererHeader) ? headers[refererHeader] : null;
        }

        public static string GetUrlReferer(HttpRequest request)
        {
            return request != null && request.UrlReferrer != null ? request.UrlReferrer.ToString() : null;
        }

        public static string GetIP(Dictionary<string, string> headers)
        {
            var forwarded = headers.ContainsKey(forwardedHeader) ? headers[forwardedHeader] : null;
            var host = headers.ContainsKey(hostHeader) ? headers[hostHeader] : null;
            return forwarded ?? host;
        }

        public static string GetIP(HttpRequest request)
        {
            if (request != null)
            {
                string str = request.Headers[forwardedHeader] ?? request.UserHostAddress;
                return str.Substring(0, str.IndexOf(':') != -1 ? str.IndexOf(':') : str.Length);
            }
            return null;
        }

        public static void AddInfoMessage(EventMessage message, Dictionary<string, ClientInfo> dict = null)
        {
            ClientInfo clientInfo;
            if (dict != null)
            {
                if (!dict.TryGetValue(message.UAHeader, out clientInfo))
                {
                    clientInfo = GetClientInfo(message.UAHeader);
                    dict.Add(message.UAHeader, clientInfo);
                }
            }
            else
            {
                clientInfo = GetClientInfo(message.UAHeader);
            }
            if (clientInfo != null)
            {
                message.Browser = GetBrowser(clientInfo);
                message.Platform = GetPlatformAndDevice(clientInfo);
            }
        }

        public static string GetBrowser(ClientInfo clientInfo)
        {
            return clientInfo == null
                       ? null
                       : string.Format("{0} {1}", clientInfo.UA.Family, clientInfo.UA.Major).Trim();
        }

        public static string GetPlatformAndDevice(ClientInfo clientInfo)
        {
            return clientInfo == null
                       ? null
                       : string.Format("{0} {1} {2} {3}",
                       clientInfo.OS.Family,
                       clientInfo.OS.Major,
                       clientInfo.Device.Brand,
                       clientInfo.Device.Model)
                       .Trim();
        }
    }
}
