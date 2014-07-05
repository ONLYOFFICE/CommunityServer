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
using System.IO;
using ASC.Core;
using ASC.Data.Backup.Logging;
using ASC.Data.Storage;

namespace ASC.Data.Backup.Service.ProgressItems
{
    internal class BackupProgressItem : BackupProgressItemBase
    {
        private readonly Guid notificationReceiverId;

        public string Link { get; private set; }

        public DateTime ExpirationDate { get; private set; }

        public BackupProgressItem(ILog log, int tenantId, Guid notificationReceiverId)
            : base(log, tenantId)
        {
            this.notificationReceiverId = notificationReceiverId;
        }

        protected override void RunInternal()
        {
            var config = BackupConfigurationSection.GetSection();
            var pathToWebConfig = FileUtility.GetRootedPath(config.WebConfigs.GetCurrentConfig());
            var tempFolderPath = FileUtility.GetRootedPath(config.TempFolder);

            if (!pathToWebConfig.EndsWith(".config", StringComparison.InvariantCultureIgnoreCase))
                pathToWebConfig = Path.Combine(pathToWebConfig, "web.config");

            if (!Directory.Exists(tempFolderPath))
                Directory.CreateDirectory(tempFolderPath);
            
            var backupFile = CreateBackupFilePath(tempFolderPath);
            try
            {
                var backuper = new BackupManager(backupFile, pathToWebConfig);
                backuper.ProgressChanged += (sender, args) =>
                    {
                        if (args.Progress > 0)
                        {
                            Progress = Math.Max(0, Math.Min((int)args.Progress/2, 50));
                        }
                    };

                backuper.Save(TenantId);

                using (var stream = new FileStream(backupFile, FileMode.Open))
                using (var progressStream = new ProgressStream(stream))
                {
                    progressStream.OnReadProgress += (sender, args) =>
                    {
                        Progress = Math.Max(0, Math.Min(100, 50 + args / 2));
                    };

                    ExpirationDate = DateTime.UtcNow + config.ExpirePeriod;

                    var storage = StorageFactory.GetStorage(pathToWebConfig, "backupfiles", "backup");
                    Link = storage.SavePrivate(string.Empty, Path.GetFileName(backupFile), progressStream, ExpirationDate);
                }

                NotifyHelper.SendAboutBackupCompleted(TenantId, notificationReceiverId, Link, ExpirationDate);
            }
            finally
            {
                File.Delete(backupFile);
            }
        }

        private string CreateBackupFilePath(string tempFolder)
        {
            var backupFileName = string.Format("{0}-{1:yyyyMMddHHmmss}.zip", CoreContext.TenantManager.GetTenant(TenantId).TenantDomain, DateTime.UtcNow).ToLowerInvariant();
            return Path.Combine(tempFolder, backupFileName);
        }
    }
}
