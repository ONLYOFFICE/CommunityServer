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
using System.Collections.Generic;
using System.Linq;
using ASC.Files.Core.Data;
using ASC.Web.Files.Helpers;
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
using ASC.Files.Thirdparty.OneDrive;
using ASC.Files.Thirdparty.SharePoint;
using ASC.Files.Thirdparty.Sharpbox;
using ASC.Security.Cryptography;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;

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
            OneDrive,
            SharePoint,
            SkyDrive,
            WebDav,
            kDrive,
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

        public virtual List<IProviderInfo> GetProvidersInfo(Guid userId)
        {
            var querySelect = new SqlQuery(TableTitle)
                .Select("id", "provider", "customer_title", "user_name", "password", "token", "user_id", "folder_type", "create_on", "url")
                .Where("tenant_id", TenantID)
                .Where(Exp.Eq("user_id", userId.ToString()));

            try
            {
                using (var db = GetDb())
                {
                    return db.ExecuteList(querySelect).ConvertAll(ToProviderInfo).Where(p => p != null).ToList();
                }
            }
            catch (Exception e)
            {
                Global.Logger.Error(string.Format("GetProvidersInfoInternal: user = {0}", userId), e);
                return new List<IProviderInfo>();
            }
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
                querySelect.Where(AbstractDao.BuildSearch("customer_title", searchText));

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

        public virtual int UpdateProviderInfo(int linkId, string customerTitle, AuthData newAuthData, FolderType folderType, Guid? userId = null)
        {
            var authData = new AuthData();
            if (newAuthData != null && !newAuthData.IsEmpty())
            {
                var querySelect = new SqlQuery(TableTitle)
                    .Select("provider", "url", "user_name", "password")
                    .Where("tenant_id", TenantID)
                    .Where("id", linkId);

                object[] input;
                try
                {
                    using (var db = GetDb())
                    {
                        input = db.ExecuteList(querySelect).Single();
                    }
                }
                catch (Exception e)
                {
                    Global.Logger.Error(string.Format("UpdateProviderInfo: linkId = {0} , user = {1}", linkId, SecurityContext.CurrentAccount.ID), e);
                    throw;
                }

                var providerKey = (string)input[0];
                ProviderTypes key;
                if (!Enum.TryParse(providerKey, true, out key))
                {
                    throw new ArgumentException("Unrecognize ProviderType");
                }

                authData = new AuthData(
                    !string.IsNullOrEmpty(newAuthData.Url) ? newAuthData.Url : (string)input[1],
                    (string)input[2],
                    !string.IsNullOrEmpty(newAuthData.Password) ? newAuthData.Password : DecryptPassword(input[3] as string),
                    newAuthData.Token);

                if (!string.IsNullOrEmpty(newAuthData.Token))
                {
                    authData = GetEncodedAccesToken(authData, key);
                }

                if (!CheckProviderInfo(ToProviderInfo(0, providerKey, customerTitle, authData, SecurityContext.CurrentAccount.ID.ToString(), folderType, TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))))
                    throw new UnauthorizedAccessException(string.Format(FilesCommonResource.ErrorMassage_SecurityException_Auth, providerKey));
            }

            var queryUpdate = new SqlUpdate(TableTitle)
                .Where("id", linkId)
                .Where("tenant_id", TenantID);

            if (!string.IsNullOrEmpty(customerTitle))
            {
                queryUpdate
                    .Set("customer_title", customerTitle);
            }

            if (folderType != FolderType.DEFAULT)
            {
                queryUpdate
                    .Set("folder_type", (int)folderType);
            }

            if (userId.HasValue)
            {
                queryUpdate
                    .Set("user_id", userId.Value.ToString());
            }

            if (!authData.IsEmpty())
            {
                queryUpdate
                    .Set("user_name", authData.Login ?? "")
                    .Set("password", EncryptPassword(authData.Password))
                    .Set("token", EncryptPassword(authData.Token ?? ""))
                    .Set("url", authData.Url ?? "");
            }

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

            if (key == ProviderTypes.OneDrive)
            {
                return new OneDriveProviderInfo(
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

                    var token = OAuth20TokenHelper.GetAccessToken<GoogleLoginProvider>(code);

                    if (token == null) throw new UnauthorizedAccessException(string.Format(FilesCommonResource.ErrorMassage_SecurityException_Auth, provider));

                    return new AuthData(token: token.ToJson());

                case ProviderTypes.Box:

                    code = authData.Token;

                    token = OAuth20TokenHelper.GetAccessToken<BoxLoginProvider>(code);

                    if (token == null) throw new UnauthorizedAccessException(string.Format(FilesCommonResource.ErrorMassage_SecurityException_Auth, provider));

                    return new AuthData(token: token.ToJson());

                case ProviderTypes.DropboxV2:

                    code = authData.Token;

                    token = OAuth20TokenHelper.GetAccessToken<DropboxLoginProvider>(code);

                    if (token == null) throw new UnauthorizedAccessException(string.Format(FilesCommonResource.ErrorMassage_SecurityException_Auth, provider));

                    return new AuthData(token: token.ToJson());

                case ProviderTypes.DropBox:

                    var dropBoxRequestToken = DropBoxRequestToken.Parse(authData.Token);

                    var config = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.DropBox);
                    var accessToken = DropBoxStorageProviderTools.ExchangeDropBoxRequestTokenIntoAccessToken(config as DropBoxConfiguration,
                                                                                                             ThirdpartyConfiguration.DropboxAppKey,
                                                                                                             ThirdpartyConfiguration.DropboxAppSecret,
                                                                                                             dropBoxRequestToken);

                    var base64Token = new CloudStorage().SerializeSecurityTokenToBase64Ex(accessToken, config.GetType(), new Dictionary<string, string>());

                    return new AuthData(token: base64Token);

                case ProviderTypes.OneDrive:

                    code = authData.Token;

                    token = OAuth20TokenHelper.GetAccessToken<OneDriveLoginProvider>(code);

                    if (token == null) throw new UnauthorizedAccessException(string.Format(FilesCommonResource.ErrorMassage_SecurityException_Auth, provider));

                    return new AuthData(token: token.ToJson());

                case ProviderTypes.SkyDrive:

                    code = authData.Token;

                    token = OAuth20TokenHelper.GetAccessToken<OneDriveLoginProvider>(code);

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