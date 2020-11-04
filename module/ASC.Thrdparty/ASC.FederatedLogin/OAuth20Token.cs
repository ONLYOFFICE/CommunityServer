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