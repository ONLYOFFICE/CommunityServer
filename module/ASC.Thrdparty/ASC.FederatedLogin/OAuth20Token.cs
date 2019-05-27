/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ASC.FederatedLogin
{
    [DebuggerDisplay("{AccessToken} (expired: {IsExpired})")]
    public class OAuth20Token
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public long ExpiresIn { get; set; }

        public string ClientID { get; set; }

        public string ClientSecret { get; set; }

        public string RedirectUri { get; set; }

        public DateTime Timestamp { get; set; }

        public string OriginJson { get; set; }

        public OAuth20Token()
        {
        }

        public OAuth20Token(OAuth20Token oAuth20Token)
        {
            Copy(oAuth20Token);
        }

        public OAuth20Token(string json)
        {
            Copy(FromJson(json));
        }

        private void Copy(OAuth20Token oAuth20Token)
        {
            if (oAuth20Token == null) return;

            AccessToken = oAuth20Token.AccessToken;
            RefreshToken = oAuth20Token.RefreshToken;
            ExpiresIn = oAuth20Token.ExpiresIn;
            ClientID = oAuth20Token.ClientID;
            ClientSecret = oAuth20Token.ClientSecret;
            RedirectUri = oAuth20Token.RedirectUri;
            Timestamp = oAuth20Token.Timestamp;
        }

        public bool IsExpired
        {
            get
            {
                if (!ExpiresIn.Equals(default(long)))
                    return DateTime.UtcNow > Timestamp + TimeSpan.FromSeconds(ExpiresIn);
                return true;
            }
        }

        public static OAuth20Token FromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            var parser = JObject.Parse(json);
            if (parser == null) return null;

            var accessToken = parser.Value<string>("access_token");

            if (string.IsNullOrEmpty(accessToken))
                return null;

            var token = new OAuth20Token
                {
                    AccessToken = accessToken,
                    RefreshToken = parser.Value<string>("refresh_token"),
                    ClientID = parser.Value<string>("client_id"),
                    ClientSecret = parser.Value<string>("client_secret"),
                    RedirectUri = parser.Value<string>("redirect_uri"),
                    OriginJson = json,
                };

            long expiresIn;
            if (long.TryParse(parser.Value<string>("expires_in"), out expiresIn))
                token.ExpiresIn = expiresIn;

            try
            {
                token.Timestamp =
                    !string.IsNullOrEmpty(parser.Value<string>("timestamp"))
                        ? parser.Value<DateTime>("timestamp")
                        : DateTime.UtcNow;
            }
            catch (Exception)
            {
                token.Timestamp = DateTime.MinValue;
            }

            return token;
        }

        public string ToJson()
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat(" \"access_token\": \"{0}\"", AccessToken);
            sb.AppendFormat(", \"refresh_token\": \"{0}\"", RefreshToken);
            sb.AppendFormat(", \"expires_in\": \"{0}\"", ExpiresIn);
            sb.AppendFormat(", \"client_id\": \"{0}\"", ClientID);
            sb.AppendFormat(", \"client_secret\": \"{0}\"", ClientSecret);
            sb.AppendFormat(", \"redirect_uri\": \"{0}\"", RedirectUri);
            sb.AppendFormat(", \"timestamp\": \"{0}\"", Timestamp.ToString("o", new CultureInfo("en-US")));
            sb.Append("}");
            return sb.ToString();
        }

        public override string ToString()
        {
            return AccessToken;
        }
    }
}