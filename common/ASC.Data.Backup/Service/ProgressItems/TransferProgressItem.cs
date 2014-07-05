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
using ASC.Core.Tenants;
using ASC.Data.Backup.Logging;
using ASC.Data.Backup.Tasks;
using ASC.Data.Backup.Tasks.Modules;

namespace ASC.Data.Backup.Service.ProgressItems
{
    internal class TransferProgressItem : BackupProgressItemBase
    {
        private readonly string targetRegion;
        private readonly Tenant tenant;

        public bool NotifyOnlyOwner { get; set; }
        public bool TransferMail { get; set; }

        public TransferProgressItem(ILog log, int tenantId, string targetRegion)
            : base(log, tenantId)
        {
            if (string.IsNullOrEmpty(targetRegion))
                throw new ArgumentException("Empty target region.", "targetRegion");

            this.targetRegion = targetRegion;

            TransferMail = true;
            tenant = CoreContext.TenantManager.GetTenant(tenantId);
        }

        protected override void RunInternal()
        {
            var config = BackupConfigurationSection.GetSection();
            var pathToCurrentWebConfig = FileUtility.GetRootedPath(config.WebConfigs.GetCurrentConfig());
            var pathToTargetWebConfig = FileUtility.GetRootedPath(config.WebConfigs.GetPathForRegion(targetRegion));
            var tempFolderPath = config.TempFolder;

            if (!Directory.Exists(tempFolderPath))
                Directory.CreateDirectory(tempFolderPath);

            try
            {
                NotifyHelper.SendAboutTransferStart(TenantId, targetRegion, NotifyOnlyOwner);

                var transferTask = new TransferPortalTask(CoreContext.TenantManager.GetTenant(TenantId), pathToCurrentWebConfig, pathToTargetWebConfig)
                    {
                        BackupDirectory = FileUtility.GetRootedPath(config.TempFolder)
                    };

                if (!TransferMail)
                    transferTask.IgnoreModule(ModuleName.Mail);

                transferTask.ProgressChanged += (sender, args) => Progress = args.Progress;
                transferTask.Message += (sender, args) =>
                {
                    if (args.Reason == MessageReason.Info && Log != null)
                    {
                        Log.Debug(args.Message);
                    }
                    else if (args.Reason == MessageReason.Warning && Log != null)
                    {
                        Log.Warn(args.Message);
                    }
                };

                transferTask.Run();

                NotifyHelper.SendAboutTransferComplete(TenantId, targetRegion, GetPortalAddress(pathToTargetWebConfig), NotifyOnlyOwner);
            }
            catch
            {
                NotifyHelper.SendAboutTransferError(TenantId, targetRegion, GetPortalAddress(pathToCurrentWebConfig), NotifyOnlyOwner);
                throw;
            }
        }

        private string GetPortalAddress(string configPath)
        {
            var baseDomain = FileUtility.OpenConfigurationFile(configPath).AppSettings.Settings["core.base-domain"].Value;
            return string.Format("http://{0}.{1}", tenant.TenantAlias, baseDomain);
        }

    }
}
