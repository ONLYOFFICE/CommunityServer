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
using ASC.Core;
using ASC.Core.Common.Configuration;
using ASC.Core.Common.Contracts;
using ASC.Data.Backup.Service;
using ASC.Data.Backup.Utils;

namespace ASC.Data.Backup.Storage
{
    internal static class BackupStorageFactory
    {
        public static IBackupStorage GetBackupStorage(BackupStorageType type, int tenantId)
        {
            var config = BackupConfigurationSection.GetSection();
            var webConfigPath = PathHelper.ToRootedConfigPath(config.WebConfigs.CurrentPath);
            switch (type)
            {
                case BackupStorageType.Documents:
                case BackupStorageType.ThridpartyDocuments:
                    return new DocumentsBackupStorage(tenantId, webConfigPath);
                case BackupStorageType.DataStore:
                    return new DataStoreBackupStorage(tenantId, webConfigPath);
                case BackupStorageType.CustomCloud:
                    var s3Config = CoreContext.Configuration.GetSection<AmazonS3Settings>(tenantId);
                    return new S3BackupStorage(s3Config.AccessKeyId, s3Config.SecretAccessKey, s3Config.Bucket, s3Config.Region);
                case BackupStorageType.Local:
                    return new LocalBackupStorage();
                default:
                    throw new InvalidOperationException("Unknown storage type.");
            }
        }

        public static IBackupRepository GetBackupRepository()
        {
            return new BackupRepository();
        }
    }
}
