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


using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Files.Core;
using ASC.Files.Thirdparty.Box;
using ASC.Files.Thirdparty.Dropbox;
using ASC.Files.Thirdparty.GoogleDrive;
using ASC.Files.Thirdparty.SharePoint;
using ASC.Files.Thirdparty.Sharpbox;
using ASC.Security.Cryptography;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Import;
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
            Box,
            BoxNet,
            DropBox,
            DropboxV2,
            Google,
            GoogleDrive,
            SharePoint,
            SkyDrive,
            WebDav,
            Yandex,
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

        public virtual List<IProviderInfo> GetProvidersInfo(FolderType folderType, string searchText = null)
        {
            return GetProvidersInfoInternal(folderType: folderType, searchText: searchText);
        }


        protected DbManager GetDb()
        {
            return new DbManager(storageKey);
        }

        private List<IProviderInfo> GetProvidersInfoInternal(int linkId = -1, FolderType folderType = FolderType.DEFAULT, string searchText = null)
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

            if (!string.IsNullOrEmpty(searchText))
                querySelect.Where(Exp.Like("lower(customer_title)", searchText.ToLower().Trim()));

            try
            {
                using (var db = GetDb())
                {
                    return db.ExecuteList(querySelect).ConvertAll(ToProviderInfo).Where(p => p != null).ToList();
                }
            }
            catch (Exception e)
            {
                Global.Logger.Error(string.Format("GetProvidersInfoInternal: linkId = {0} , folderType = {1} , user = {2}",
                                                  linkId, folderType, SecurityContext.CurrentAccount.ID), e);
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
                .InColumnValue("user_name", authData.Login ?? "")
                .InColumnValue("password", EncryptPassword(authData.Password))
                .InColumnValue("folder_type", (int)folderType)
                .InColumnValue("create_on", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))
                .InColumnValue("user_id", SecurityContext.CurrentAccount.ID.ToString())
                .InColumnValue("token", EncryptPassword(authData.Token ?? ""))
                .InColumnValue("url", authData.Url ?? "")
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

        public virtual int UpdateProviderInfo(int linkId, AuthData authData)
        {
            var queryUpdate = new SqlUpdate(TableTitle)
                .Set("user_name", authData.Login ?? "")
                .Set("password", EncryptPassword(authData.Password))
                .Set("token", EncryptPassword(authData.Token ?? ""))
                .Set("url", authData.Url ?? "")
                .Where("id", linkId)
                .Where("tenant_id", TenantID);

            using (var db = GetDb())
            {
                return db.ExecuteNonQuery(queryUpdate) == 1 ? linkId : default(int);
            }
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
            return ToProviderInfo(new object[] { id, providerKey, customerTitle, authData.Login, EncryptPassword(authData.Password), EncryptPassword(authData.Token), owner, (int)type, createOn, authData.Url });
        }

        private static IProviderInfo ToProviderInfo(object[] input)
        {
            ProviderTypes key;
            if (!Enum.TryParse((string) input[1], true, out key)) return null;

            var id = Convert.ToInt32(input[0]);
            var providerTitle = (string) input[2] ?? string.Empty;
            var token = DecryptToken(input[5] as string);
            var owner = input[6] == null ? Guid.Empty : new Guid((input[6] as string) ?? "");
            var folderType = (FolderType) Convert.ToInt32(input[7]);
            var createOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(input[8]));

            if (key == ProviderTypes.Box)
            {
                return new BoxProviderInfo(
                    id,
                    key.ToString(),
                    providerTitle,
                    token,
                    owner,
                    folderType,
                    createOn);
            }

            if (key == ProviderTypes.DropboxV2)
            {
                return new DropboxProviderInfo(
                    id,
                    key.ToString(),
                    providerTitle,
                    token,
                    owner,
                    folderType,
                    createOn);
            }

            if (key == ProviderTypes.SharePoint)
            {
                return new SharePointProviderInfo(
                    id,
                    key.ToString(),
                    providerTitle,
                    new AuthData(input[9] as string, input[3] as string, DecryptPassword(input[4] as string), token),
                    owner,
                    folderType,
                    createOn);
            }

            if (key == ProviderTypes.GoogleDrive)
            {
                return new GoogleDriveProviderInfo(
                    id,
                    key.ToString(),
                    providerTitle,
                    token,
                    owner,
                    folderType,
                    createOn);
            }

            return new SharpBoxProviderInfo(
                id,
                key.ToString(),
                providerTitle,
                new AuthData(input[9] as string, input[3] as string, DecryptPassword(input[4] as string), token),
                owner,
                folderType,
                createOn);
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

                    return new AuthData(token: token.ToJson());

                case ProviderTypes.Box:

                    code = authData.Token;

                    token = OAuth20TokenHelper.GetAccessToken(BoxLoginProvider.BoxOauthTokenUrl,
                                                              BoxLoginProvider.BoxOAuth20ClientId,
                                                              BoxLoginProvider.BoxOAuth20ClientSecret,
                                                              BoxLoginProvider.BoxOAuth20RedirectUrl,
                                                              code);

                    if (token == null) throw new UnauthorizedAccessException(string.Format(FilesCommonResource.ErrorMassage_SecurityException_Auth, provider));

                    return new AuthData(token: token.ToJson());

                case ProviderTypes.DropboxV2:

                    code = authData.Token;

                    token = OAuth20TokenHelper.GetAccessToken(DropboxLoginProvider.DropboxOauthTokenUrl,
                                                              DropboxLoginProvider.DropboxOAuth20ClientId,
                                                              DropboxLoginProvider.DropboxOAuth20ClientSecret,
                                                              DropboxLoginProvider.DropboxOAuth20RedirectUrl,
                                                              code);

                    if (token == null) throw new UnauthorizedAccessException(string.Format(FilesCommonResource.ErrorMassage_SecurityException_Auth, provider));

                    return new AuthData(token: token.ToJson());

                case ProviderTypes.DropBox:

                    var dropBoxRequestToken = DropBoxRequestToken.Parse(authData.Token);

                    var config = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.DropBox);
                    var accessToken = DropBoxStorageProviderTools.ExchangeDropBoxRequestTokenIntoAccessToken(config as DropBoxConfiguration,
                                                                                                             ImportConfiguration.DropboxAppKey,
                                                                                                             ImportConfiguration.DropboxAppSecret,
                                                                                                             dropBoxRequestToken);

                    var base64Token = new CloudStorage().SerializeSecurityTokenToBase64Ex(accessToken, config.GetType(), new Dictionary<string, string>());

                    return new AuthData(token: base64Token);

                case ProviderTypes.SkyDrive:

                    code = authData.Token;

                    token = OAuth20TokenHelper.GetAccessToken(OneDriveLoginProvider.OneDriveOauthTokenUrl,
                                                              OneDriveLoginProvider.OneDriveOAuth20ClientId,
                                                              OneDriveLoginProvider.OneDriveOAuth20ClientSecret,
                                                              OneDriveLoginProvider.OneDriveOAuth20RedirectUrl,
                                                              code);

                    if (token == null) throw new UnauthorizedAccessException(string.Format(FilesCommonResource.ErrorMassage_SecurityException_Auth, provider));

                    accessToken = AppLimit.CloudComputing.SharpBox.Common.Net.oAuth20.OAuth20Token.FromJson(token.ToJson());

                    if (accessToken == null) throw new UnauthorizedAccessException(string.Format(FilesCommonResource.ErrorMassage_SecurityException_Auth, provider));

                    config = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.SkyDrive);
                    var storage = new CloudStorage();
                    base64Token = storage.SerializeSecurityTokenToBase64Ex(accessToken, config.GetType(), new Dictionary<string, string>());

                    return new AuthData(token: base64Token);

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

        private static string DecryptToken(string token)
        {
            try
            {
                return DecryptPassword(token);
            }
            catch
            {
                //old token in base64 without encrypt
                return token ?? "";
            }
        }
    }
}