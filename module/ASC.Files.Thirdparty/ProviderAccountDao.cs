/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using AppLimit.CloudComputing.SharpBox;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Files.Core;
using ASC.Files.Thirdparty.GoogleDrive;
using ASC.Files.Thirdparty.SharePoint;
using ASC.Files.Thirdparty.Sharpbox;
using ASC.Security.Cryptography;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Files.Thirdparty
{
    internal class ProviderAccountDao : IProviderDao
    {
        private enum ProviderTypes
        {
            DropBox,
            BoxNet,
            WebDav,
            Google,
            Yandex,
            SkyDrive,
            SharePoint,
            GoogleDrive
        }


        private const string TableTitle = "files_thirdparty_account";
        private readonly string storageKey;

        protected int TenantID { get; private set; }

        public ProviderAccountDao(int tenantID, String storageKey)
        {
            TenantID = tenantID;
            this.storageKey = storageKey;
        }

        public virtual IProviderInfo GetProviderInfo(int linkId)
        {
            return GetProvidersInfoInternal(linkId).Single();
        }

        public virtual List<IProviderInfo> GetProvidersInfo()
        {
            return GetProvidersInfoInternal();
        }

        public virtual List<IProviderInfo> GetProvidersInfo(FolderType folderType)
        {
            return GetProvidersInfoInternal(folderType: folderType);
        }


        protected DbManager GetDb()
        {
            return new DbManager(storageKey);
        }

        private List<IProviderInfo> GetProvidersInfoInternal(int linkId = -1, FolderType folderType = FolderType.DEFAULT)
        {
            var querySelect = new SqlQuery(TableTitle)
                .Select("id", "provider", "customer_title", "user_name", "password", "token", "user_id", "folder_type", "create_on", "url")
                .Where("tenant_id", TenantID);

            if (folderType == FolderType.USER || folderType == FolderType.DEFAULT && linkId == -1)
                querySelect.Where(Exp.Eq("user_id", SecurityContext.CurrentAccount.ID.ToString()));

            if (linkId != -1)
                querySelect.Where("id", linkId);

            if (folderType != FolderType.DEFAULT)
                querySelect.Where("folder_type", (int)folderType);

            try
            {
                using (var db = GetDb())
                {
                    return db.ExecuteList(querySelect).ConvertAll(ToProviderInfo);
                }
            }
            catch (Exception e)
            {
                Global.Logger
                      .Error("GetProvidersInfoInternal: linkId = " + linkId + " , folderType = " + folderType + " , user = " + SecurityContext.CurrentAccount.ID, e);
                return new List<IProviderInfo>();
            }
        }

        public virtual int SaveProviderInfo(string providerKey, string customerTitle, AuthData authData, FolderType folderType)
        {
            ProviderTypes prKey;
            try
            {
                prKey = (ProviderTypes)Enum.Parse(typeof(ProviderTypes), providerKey, true);
            }
            catch (Exception)
            {
                throw new ArgumentException("Unrecognize ProviderType");
            }

            authData = GetEncodedAccesToken(authData, prKey);

            if (!CheckProviderInfo(ToProviderInfo(0, providerKey, customerTitle, authData, SecurityContext.CurrentAccount.ID.ToString(), folderType, TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))))
                throw new UnauthorizedAccessException(string.Format(FilesCommonResource.ErrorMassage_SecurityException_Auth, providerKey));

            var queryInsert = new SqlInsert(TableTitle, true)
                .InColumnValue("id", 0)
                .InColumnValue("tenant_id", TenantID)
                .InColumnValue("provider", prKey.ToString())
                .InColumnValue("customer_title", Global.ReplaceInvalidCharsAndTruncate(customerTitle))
                .InColumnValue("user_name", authData.Login)
                .InColumnValue("password", EncryptPassword(authData.Password))
                .InColumnValue("folder_type", (int)folderType)
                .InColumnValue("create_on", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))
                .InColumnValue("user_id", SecurityContext.CurrentAccount.ID.ToString())
                .InColumnValue("token", authData.Token)
                .InColumnValue("url", authData.Url)
                .Identity(0, 0, true);
            using (var db = GetDb())
            {
                return Int32.Parse(db.ExecuteScalar<string>(queryInsert));
            }
        }

        public bool CheckProviderInfo(IProviderInfo providerInfo)
        {
            return providerInfo != null && providerInfo.CheckAccess();
        }

        public virtual int UpdateProviderInfo(int linkId, string customerTitle, FolderType folderType)
        {
            var queryUpdate = new SqlUpdate(TableTitle)
                .Set("customer_title", customerTitle)
                .Set("folder_type", (int)folderType)
                .Where("id", linkId)
                .Where("tenant_id", TenantID);

            using (var db = GetDb())
            {
                return db.ExecuteNonQuery(queryUpdate) == 1 ? linkId : default(int);
            }
        }

        public virtual void RemoveProviderInfo(int linkId)
        {
            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                var folderId = GetProviderInfo(linkId).RootFolderId.ToString();

                var entryIDs = db.ExecuteList(new SqlQuery("files_thirdparty_id_mapping")
                                                         .Select("hash_id")
                                                         .Where(Exp.Eq("tenant_id", TenantID) &
                                                                Exp.Like("id", folderId, SqlLike.StartWith)))
                                        .ConvertAll(x => x[0]);


                db.ExecuteNonQuery(new SqlDelete("files_security")
                                              .Where(
                                                  Exp.Eq("tenant_id", TenantID) &
                                                  Exp.In("entry_id", entryIDs)));

                var sqlQuery = new SqlDelete("files_tag_link")
                    .Where(Exp.Eq("tenant_id", TenantID) & Exp.In("entry_id", entryIDs));

                db.ExecuteNonQuery(sqlQuery);

                sqlQuery = new SqlDelete(TableTitle)
                    .Where("id", linkId)
                    .Where("tenant_id", TenantID);


                db.ExecuteNonQuery(sqlQuery);

                tx.Commit();
            }
        }

        private static IProviderInfo ToProviderInfo(int id, string providerKey, string customerTitle, AuthData authData, string owner, FolderType type, DateTime createOn)
        {
            return ToProviderInfo(new object[] { id, providerKey, customerTitle, authData.Login, EncryptPassword(authData.Password), authData.Token, owner, (int)type, createOn, authData.Url });
        }

        private static IProviderInfo ToProviderInfo(object[] input)
        {
            var key = (ProviderTypes)Enum.Parse(typeof(ProviderTypes), (string)input[1], true);

            if (string.IsNullOrEmpty((string)input[2]))
            {
                throw new ArgumentException("Unrecognize customerTitle");
            }

            if (key == ProviderTypes.SharePoint)
            {
                return new SharePointProviderInfo(
                    Convert.ToInt32(input[0]),
                    input[1] as string,
                    input[2] as string,
                    new AuthData(input[9] as string, input[3] as string, DecryptPassword(input[4] as string), input[5] as string),
                    input[6] == null ? Guid.Empty : new Guid((input[6] as string) ?? ""),
                    (FolderType)Convert.ToInt32(input[7]),
                    TenantUtil.DateTimeFromUtc(Convert.ToDateTime(input[8])));
            }

            if (key == ProviderTypes.GoogleDrive)
            {
                return new GoogleDriveProviderInfo(
                    Convert.ToInt32(input[0]),
                    input[1] as string,
                    input[2] as string,
                    DecryptPassword(input[5] as string),
                    input[6] == null ? Guid.Empty : new Guid((input[6] as string) ?? ""),
                    (FolderType)Convert.ToInt32(input[7]),
                    TenantUtil.DateTimeFromUtc(Convert.ToDateTime(input[8])));
            }

            return new SharpBoxProviderInfo(
                Convert.ToInt32(input[0]),
                input[1] as string,
                input[2] as string,
                new AuthData(input[9] as string, input[3] as string, DecryptPassword(input[4] as string), input[5] as string),
                input[6] == null ? Guid.Empty : new Guid((input[6] as string) ?? ""),
                (FolderType)Convert.ToInt32(input[7]),
                TenantUtil.DateTimeFromUtc(Convert.ToDateTime(input[8])));
        }

        public void Dispose()
        {
        }

        private static AuthData GetEncodedAccesToken(AuthData authData, ProviderTypes provider)
        {
            switch (provider)
            {
                case ProviderTypes.GoogleDrive:

                    var code = authData.Token;

                    var token = OAuth20TokenHelper.GetAccessToken(GoogleLoginProvider.GoogleOauthTokenUrl,
                                                                  GoogleLoginProvider.GoogleOAuth20ClientId,
                                                                  GoogleLoginProvider.GoogleOAuth20ClientSecret,
                                                                  GoogleLoginProvider.GoogleOAuth20RedirectUrl,
                                                                  code);

                    if (token == null) throw new UnauthorizedAccessException(string.Format(FilesCommonResource.ErrorMassage_SecurityException_Auth, provider));

                    authData.Token = EncryptPassword(token.ToJson());

                    break;
                case ProviderTypes.SkyDrive:

                    code = authData.Token;

                    token = OAuth20TokenHelper.GetAccessToken(OneDriveLoginProvider.OneDriveOauthTokenUrl,
                                                              OneDriveLoginProvider.OneDriveOAuth20ClientId,
                                                              OneDriveLoginProvider.OneDriveOAuth20ClientSecret,
                                                              OneDriveLoginProvider.OneDriveOAuth20RedirectUrl,
                                                              code);

                    if (token == null) throw new UnauthorizedAccessException(string.Format(FilesCommonResource.ErrorMassage_SecurityException_Auth, provider));

                    var accessToken = AppLimit.CloudComputing.SharpBox.Common.Net.oAuth20.OAuth20Token.FromJson(token.ToJson());

                    var config = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.SkyDrive);
                    var storage = new CloudStorage();
                    var base64AccessToken = storage.SerializeSecurityTokenToBase64Ex(accessToken, config.GetType(), new Dictionary<string, string>());

                    authData.Token = base64AccessToken;

                    break;
                case ProviderTypes.SharePoint:
                case ProviderTypes.WebDav:
                    break;
                default:
                    authData.Url = null;
                    break;
            }

            return authData;
        }

        private static string EncryptPassword(string password)
        {
            return string.IsNullOrEmpty(password) ? string.Empty : InstanceCrypto.Encrypt(password);
        }

        private static string DecryptPassword(string password)
        {
            return string.IsNullOrEmpty(password) ? string.Empty : InstanceCrypto.Decrypt(password);
        }
    }
}