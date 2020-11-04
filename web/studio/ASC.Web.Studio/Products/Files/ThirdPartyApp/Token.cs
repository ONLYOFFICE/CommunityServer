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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Helpers;
using ASC.Files.Core;
using ASC.Security.Cryptography;
using ASC.Web.Files.Classes;

namespace ASC.Web.Files.ThirdPartyApp
{
    [DebuggerDisplay("{App} - {AccessToken}")]
    public class Token : OAuth20Token
    {
        public String App { get; private set; }

        public Token(OAuth20Token oAuth20Token, string app)
            : base(oAuth20Token)
        {
            App = app;
        }

        public override string ToString()
        {
            return GetRefreshedToken();
        }

        public String GetRefreshedToken()
        {
            if (IsExpired)
            {
                var app = ThirdPartySelector.GetApp(App);
                try
                {
                    Global.Logger.Debug("Refresh token for app: " + App);

                    var refreshUrl = app.GetRefreshUrl();

                    var refreshed = OAuth20TokenHelper.RefreshToken(refreshUrl, this);

                    if (refreshed != null)
                    {
                        AccessToken = refreshed.AccessToken;
                        RefreshToken = refreshed.RefreshToken;
                        ExpiresIn = refreshed.ExpiresIn;
                        Timestamp = DateTime.UtcNow;

                        SaveToken(this);
                    }
                }
                catch (Exception ex)
                {
                    Global.Logger.Error("Refresh token for app: " + app, ex);
                }
            }
            return AccessToken;
        }

        private const string TableTitle = "files_thirdparty_app";

        public static void SaveToken(Token token)
        {
            using (var db = new DbManager(FileConstant.DatabaseId))
            {
                var queryInsert = new SqlInsert(TableTitle, true)
                    .InColumnValue("tenant_id", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                    .InColumnValue("user_id", SecurityContext.CurrentAccount.ID.ToString())
                    .InColumnValue("app", token.App)
                    .InColumnValue("token", EncryptToken(token));

                db.ExecuteNonQuery(queryInsert);
            }
        }

        public static Token GetToken(string app)
        {
            return GetToken(app, SecurityContext.CurrentAccount.ID.ToString());
        }

        public static Token GetToken(string app, string userId)
        {
            using (var db = new DbManager(FileConstant.DatabaseId))
            {
                var querySelect = new SqlQuery(TableTitle)
                    .Select("token")
                    .Where("tenant_id", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                    .Where("user_id", userId)
                    .Where("app", app);

                var oAuth20Token = db.ExecuteList(querySelect).ConvertAll(r => DecryptToken(r[0] as string)).FirstOrDefault();
                if (oAuth20Token == null) return null;

                return new Token(oAuth20Token, app);
            }
        }

        public static void DeleteToken(string app, Guid? userId = null)
        {
            using (var db = new DbManager(FileConstant.DatabaseId))
            {
                db.ExecuteNonQuery(new SqlDelete(TableTitle)
                                       .Where("tenant_id", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                                       .Where("user_id", (userId ?? SecurityContext.CurrentAccount.ID).ToString())
                                       .Where("app", app));
            }
        }

        private static string EncryptToken(OAuth20Token token)
        {
            var t = token.ToJson();
            return string.IsNullOrEmpty(t) ? string.Empty : InstanceCrypto.Encrypt(t);
        }

        private static OAuth20Token DecryptToken(string token)
        {
            return string.IsNullOrEmpty(token) ? null : FromJson(InstanceCrypto.Decrypt(token));
        }
    }
}