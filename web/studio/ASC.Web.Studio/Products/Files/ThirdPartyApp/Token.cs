/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
                    .InColumnValue("user_id", SecurityContext.CurrentAccount.ID.ToString())
                    .InColumnValue("app", token.App)
                    .InColumnValue("token", EncryptToken(token))
                    .InColumnValue("tenant_id", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                    .InColumnValue("modified_on", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()));

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

        public static void DeleteToken(string app)
        {
            using (var db = new DbManager(FileConstant.DatabaseId))
            {
                db.ExecuteNonQuery(new SqlDelete(TableTitle)
                                       .Where("tenant_id", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                                       .Where("user_id", SecurityContext.CurrentAccount.ID.ToString())
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