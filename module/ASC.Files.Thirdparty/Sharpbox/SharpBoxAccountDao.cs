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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Caching;
using ASC.Collections;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Files.Thirdparty.GoogleDrive;
using ASC.Files.Thirdparty.SharePoint;
using ASC.Security.Cryptography;
using ASC.Web.Files.Import;
using ASC.Web.Files.Resources;
using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs;

namespace ASC.Files.Thirdparty.Sharpbox
{
    internal class CachedSharpBoxAccountDao : SharpBoxAccountDao
    {
        private static readonly CachedDictionary<IProviderInfo> ProviderCache =
            new CachedDictionary<IProviderInfo>("shrpbox-providers", Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(20), (x) => true);

        private readonly string _rootKey = string.Empty;

        public CachedSharpBoxAccountDao(int tenantID, string storageKey)
            : base(tenantID, storageKey)
        {
            _rootKey = tenantID.ToString(CultureInfo.InvariantCulture);
        }

        public override IProviderInfo GetProviderInfo(int linkId)
        {
            return ProviderCache.Get(_rootKey, linkId.ToString(CultureInfo.InvariantCulture), () => GetProviderInfoBase(linkId));
        }

        private IProviderInfo GetProviderInfoBase(int linkId)
        {
            return base.GetProviderInfo(linkId);
        }

        public override void RemoveProviderInfo(int linkId)
        {
            ProviderCache.Reset(_rootKey, linkId.ToString(CultureInfo.InvariantCulture));
            base.RemoveProviderInfo(linkId);
        }

        public override int UpdateProviderInfo(int linkId, string customerTitle, FolderType folderType)
        {
            ProviderCache.Reset(_rootKey, linkId.ToString(CultureInfo.InvariantCulture));
            return base.UpdateProviderInfo(linkId, customerTitle, folderType);
        }
    }

    internal class SharpBoxAccountDao : IProviderDao
    {
        private enum ProviderTypes
        {
            DropBox,
            StoreGate,
            BoxNet,
            SmartDrive,
            WebDav,
            CloudMe,
            HiDrive,
            Google,
            Yandex,
            SkyDrive,
            SharePoint,
            GoogleDrive
        }


        private const string TableTitle = "files_thirdparty_account";

        protected DbManager DbManager { get; private set; }

        protected int TenantID { get; private set; }

        public SharpBoxAccountDao(int tenantID, String storageKey)
        {
            TenantID = tenantID;
            DbManager = new DbManager(storageKey);
        }

        public virtual IProviderInfo GetProviderInfo(int linkId)
        {
            return GetProviderInfoInernal(linkId);
        }

        private IProviderInfo GetProviderInfoInernal(int linkId)
        {
            var querySelect = new SqlQuery(TableTitle)
                .Select("id", "provider", "customer_title", "user_name", "password", "token", "user_id", "folder_type",
                        "create_on", "url")
                .Where("id", linkId)
                .Where("tenant_id", TenantID);

            //     .Where(Exp.Eq("user_id", SecurityContext.CurrentAccount.ID.ToString()) | Exp.Eq("folder_type", (int) FolderType.COMMON));

            return DbManager.ExecuteList(querySelect).ConvertAll(ToProviderInfo).Single();
        }

        public virtual List<IProviderInfo> GetProvidersInfo()
        {
            return GetProvidersInfoInternal();
        }

        private List<IProviderInfo> GetProvidersInfoInternal()
        {
            var querySelect = new SqlQuery(TableTitle)
                .Select("id", "provider", "customer_title", "user_name", "password", "token", "user_id", "folder_type", "create_on", "url")
                .Where("tenant_id", TenantID)
                .Where(Exp.Eq("user_id", SecurityContext.CurrentAccount.ID.ToString()));

            return DbManager.ExecuteList(querySelect).ConvertAll(ToProviderInfo);
        }

        public virtual List<IProviderInfo> GetProvidersInfo(FolderType folderType)
        {
            return GetProvidersInfoInternal(folderType);
        }

        private List<IProviderInfo> GetProvidersInfoInternal(FolderType folderType)
        {
            var querySelect = new SqlQuery(TableTitle)
                .Select("id", "provider", "customer_title", "user_name", "password", "token", "user_id", "folder_type", "create_on", "url")
                .Where("tenant_id", TenantID)
                .Where("folder_type", (int)folderType);

            if (folderType == FolderType.USER)
                querySelect = querySelect.Where(Exp.Eq("user_id", SecurityContext.CurrentAccount.ID.ToString()));

            return DbManager.ExecuteList(querySelect).ConvertAll(ToProviderInfo);
        }

        public virtual int SaveProviderInfo(string providerKey, string customerTitle, AuthData authData, FolderType folderType)
        {
            var prKey = (ProviderTypes)Enum.Parse(typeof(ProviderTypes), providerKey, true);

            authData = GetEncodedAccesToken(authData, prKey);

            if (!CheckProviderInfo(ToProviderInfo(0, providerKey, customerTitle, authData, SecurityContext.CurrentAccount.ID.ToString(), folderType, TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))))
                throw new UnauthorizedAccessException(string.Format(FilesCommonResource.ErrorMassage_SecurityException_Auth, providerKey));

            var queryInsert = new SqlInsert(TableTitle, true)
                .InColumnValue("id", 0)
                .InColumnValue("tenant_id", TenantID)
                .InColumnValue("provider", prKey.ToString())
                .InColumnValue("customer_title", customerTitle)
                .InColumnValue("user_name", authData.Login)
                .InColumnValue("password", EncryptPassword(authData.Password))
                .InColumnValue("folder_type", (int)folderType)
                .InColumnValue("create_on", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))
                .InColumnValue("user_id", SecurityContext.CurrentAccount.ID.ToString())
                .InColumnValue("token", authData.Token)
                .InColumnValue("url", authData.Url)
                .Identity(0, 0, true);

            return Int32.Parse(DbManager.ExecuteScalar<string>(queryInsert));
        }

        public bool CheckProviderInfo(IProviderInfo providerInfo)
        {
            return providerInfo.CheckAccess();
        }

        public virtual int UpdateProviderInfo(int linkId, string customerTitle, FolderType folderType)
        {
            var queryUpdate = new SqlUpdate(TableTitle)
                .Set("customer_title", customerTitle)
                .Set("folder_type", (int)folderType)
                .Where("id", linkId)
                .Where("tenant_id", TenantID);

            return DbManager.ExecuteNonQuery(queryUpdate) == 1 ? linkId : default(int);
        }

        public virtual void RemoveProviderInfo(int linkId)
        {
            using (var tx = DbManager.BeginTransaction())
            {
                var folderId = GetProviderInfoInernal(linkId).RootFolderId.ToString();

                var entryIDs = DbManager.ExecuteList(new SqlQuery("files_thirdparty_id_mapping")
                                                         .Select("hash_id")
                                                         .Where(Exp.Eq("tenant_id", TenantID) &
                                                                Exp.Like("id", folderId, SqlLike.StartWith)))
                                        .ConvertAll(x => x[0]);


                DbManager.ExecuteNonQuery(new SqlDelete("files_security")
                                              .Where(
                                                  Exp.Eq("tenant_id", TenantID) &
                                                  Exp.In("entry_id", entryIDs)));

                var sqlQuery = new SqlDelete("files_tag_link")
                    .Where(Exp.Eq("tenant_id", TenantID) & Exp.In("entry_id", entryIDs));

                DbManager.ExecuteNonQuery(sqlQuery);

                sqlQuery = new SqlDelete(TableTitle)
                    .Where("id", linkId)
                    .Where("tenant_id", TenantID);


                DbManager.ExecuteNonQuery(sqlQuery);

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
            if (key == ProviderTypes.SharePoint)
            {
                return new SharePointProviderInfo(
                    Convert.ToInt32(input[0]),
                    input[1] as string,
                    input[2] as string,
                    new AuthData(input[9] as string, input[3] as string, DecryptPassword(input[4] as string), input[5] as string),
                    input[6] == null ? Guid.Empty : new Guid(input[6] as string),
                    (FolderType)Convert.ToInt32(input[7]),
                    TenantUtil.DateTimeFromUtc(Convert.ToDateTime(input[8])));
            }

            if (key == ProviderTypes.GoogleDrive)
            {
                return new GoogleDriveProviderInfo(
                    Convert.ToInt32(input[0]),
                    input[1] as string,
                    input[2] as string,
                    new AuthData(string.Empty, string.Empty, DecryptPassword(input[5] as string)),
                    input[6] == null ? Guid.Empty : new Guid(input[6] as string),
                    (FolderType)Convert.ToInt32(input[7]),
                    TenantUtil.DateTimeFromUtc(Convert.ToDateTime(input[8])));
            }

            return new SharpBoxProviderInfo(
                Convert.ToInt32(input[0]),
                input[1] as string,
                input[2] as string,
                new AuthData(input[3] as string, DecryptPassword(input[4] as string), input[5] as string),
                input[6] == null ? Guid.Empty : new Guid(input[6] as string),
                (FolderType)Convert.ToInt32(input[7]),
                TenantUtil.DateTimeFromUtc(Convert.ToDateTime(input[8])));
        }

        public void Dispose()
        {
            DbManager.Dispose();
        }

        private static AuthData GetEncodedAccesToken(AuthData authData, ProviderTypes provider)
        {
            switch (provider)
            {
                case ProviderTypes.Google:
                case ProviderTypes.GoogleDrive:

                    var tokenSecret = ImportConfiguration.GoogleTokenManager.GetTokenSecret(authData.Token);
                    var consumerKey = ImportConfiguration.GoogleTokenManager.ConsumerKey;
                    var consumerSecret = ImportConfiguration.GoogleTokenManager.ConsumerSecret;

                    var accessToken = GoogleDocsAuthorizationHelper.BuildToken(authData.Token, tokenSecret, consumerKey, consumerSecret);
                    var storage = new CloudStorage();

                    authData.Token = storage.SerializeSecurityTokenToBase64Ex(accessToken, typeof(GoogleDocsConfiguration), null);

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