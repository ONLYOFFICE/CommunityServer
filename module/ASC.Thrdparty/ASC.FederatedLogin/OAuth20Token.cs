/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ASC.FederatedLogin
{
    public class OAuth20Token
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public long ExpiresIn { get; set; }

        public string ClientID { get; set; }

        public string ClientSecret { get; set; }

        public string RedirectUri { get; set; }

        public DateTime Timestamp { get; set; }

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
                };

            long expiresIn;
            if (long.TryParse(parser.Value<string>("expires_in"), out expiresIn))
                token.ExpiresIn = expiresIn;

            DateTime timestamp;
            token.Timestamp =
                DateTime.TryParse(parser.Value<string>("timestamp"), out timestamp)
                    ? timestamp
                    : DateTime.UtcNow;

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
            sb.AppendFormat(", \"timestamp\": \"{0}\"", Timestamp);
            sb.Append("}");
            return sb.ToString();
        }

        public override string ToString()
        {
            return IsExpired.ToString();
        }
    }
}
