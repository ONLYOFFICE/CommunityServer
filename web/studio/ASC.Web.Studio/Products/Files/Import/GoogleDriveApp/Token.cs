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
using System.Linq;
using System.Text;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Core;
using ASC.Files.Core;
using ASC.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace ASC.Web.Files.GoogleDriveApp
{
    internal class Token
    {
        private String _accessToken;

        public String AccessToken
        {
            get
            {
                if (!IsExpired)
                {
                    var refreshed = GoogleApiHelper.RefreshToken(RefreshToken);

                    if (refreshed != null)
                    {
                        _accessToken = refreshed._accessToken;
                        RefreshToken = refreshed.RefreshToken;
                        ExpiresIn = refreshed.ExpiresIn;
                        Timestamp = DateTime.UtcNow;
                    }
                }
                return _accessToken;
            }
            set { _accessToken = value; }
        }

        public String RefreshToken { get; set; }

        public double ExpiresIn { get; set; }

        public DateTime? Timestamp { get; set; }

        public bool IsExpired
        {
            get
            {
                if (Timestamp.HasValue && !ExpiresIn.Equals(default(double)))
                    return DateTime.UtcNow > Timestamp + TimeSpan.FromSeconds(ExpiresIn);
                return true;
            }
        }

        public static Token FromJson(String json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            var parser = JObject.Parse(json);
            if (parser == null) return null;

            var accessToken = parser.Value<string>("access_token");

            if (String.IsNullOrEmpty(accessToken))
                return null;

            var token = new Token
                {
                    _accessToken = accessToken,
                    RefreshToken = parser.Value<string>("refresh_token"),
                };

            double expiresIn;
            if (double.TryParse(parser.Value<string>("expires_in"), out expiresIn))
                token.ExpiresIn = expiresIn;

            DateTime timestamp;
            if (DateTime.TryParse(parser.Value<string>("timestamp"), out timestamp))
                token.Timestamp = timestamp;

            return token;
        }

        public String ToJson()
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat(" \"access_token\": \"{0}\"", _accessToken);
            sb.AppendFormat(", \"refresh_token\": \"{0}\"", RefreshToken);
            sb.AppendFormat(", \"expires_in\": \"{0}\"", ExpiresIn);
            sb.AppendFormat(", \"timestamp\": \"{0}\"", Timestamp);
            sb.Append("}");
            return sb.ToString();
        }

        public override string ToString()
        {
            return AccessToken;
        }

        private const string TableTitle = "files_thirdparty_app";

        public static void SaveToken(Token token)
        {
            using (var db = new DbManager(FileConstant.DatabaseId))
            {
                var queryInsert = new SqlInsert(TableTitle, true)
                    .InColumnValue("user_id", SecurityContext.CurrentAccount.ID.ToString())
                    .InColumnValue("token", EncryptToken(token))
                    .InColumnValue("tenant_id", CoreContext.TenantManager.GetCurrentTenant().TenantId);

                db.ExecuteNonQuery(queryInsert);
            }
        }

        public static Token GetToken()
        {
            return GetToken(SecurityContext.CurrentAccount.ID.ToString());
        }

        public static Token GetToken(string userId)
        {
            using (var db = new DbManager(FileConstant.DatabaseId))
            {
                var querySelect = new SqlQuery(TableTitle)
                    .Select("token")
                    .Where("tenant_id", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                    .Where("user_id", userId);

                return db.ExecuteList(querySelect).ConvertAll(r => DecryptToken(r[0] as string)).FirstOrDefault();
            }
        }

        private static string EncryptToken(Token token)
        {
            var t = token.ToJson();
            return string.IsNullOrEmpty(t) ? string.Empty : InstanceCrypto.Encrypt(t);
        }

        private static Token DecryptToken(string token)
        {
            return string.IsNullOrEmpty(token) ? null : FromJson(InstanceCrypto.Decrypt(token));
        }
    }
}