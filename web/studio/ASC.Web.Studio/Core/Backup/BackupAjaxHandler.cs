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
using System.Collections.Generic;
using System.Net;
using System.Web;

using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Common.Configuration;
using ASC.Core.Common.Contracts;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Notify.Cron;
using ASC.Web.Core.Security;
using ASC.Web.Studio.Utility;
using Resources;

using AjaxPro;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace ASC.Web.Studio.Core.Backup
{
    [AjaxNamespace("AjaxPro.Backup")]
    public class BackupAjaxHandler
    {
        public BackupAjaxHandler()
        {
            if (WorkContext.IsMono)
            {
                ServicePointManager.ServerCertificateValidationCallback = (s, cert, c, p) => true;
            }
        }

        #region backup

        [AjaxMethod]
        public BackupProgress StartBackup(BackupStorageType storageType, StorageParams storageParams, bool backupMail)
        {
            DemandPermissionsBackup();
            DemandSize();

            var backupRequest = new StartBackupRequest
                {
                    TenantId = GetCurrentTenantId(),
                    UserId = SecurityContext.CurrentAccount.ID,
                    BackupMail = backupMail,
                    StorageType = storageType
                };

            switch (storageType)
            {
                case BackupStorageType.ThridpartyDocuments:
                case BackupStorageType.Documents:
                    backupRequest.StorageBasePath = storageParams.FolderId;
                    break;
                case BackupStorageType.CustomCloud:
                    backupRequest.StorageBasePath = storageParams.FilePath;
                    ValidateS3Settings(storageParams.AccessKeyId, storageParams.SecretAccessKey, storageParams.Bucket, storageParams.Region);
                    CoreContext.Configuration.SaveSection(new AmazonS3Settings
                        {
                            AccessKeyId = storageParams.AccessKeyId,
                            SecretAccessKey = storageParams.SecretAccessKey,
                            Bucket = storageParams.Bucket,
                            Region = storageParams.Region
                        });
                    break;
            }

            MessageService.Send(HttpContext.Current.Request, MessageAction.StartBackupSetting);

            using (var service = new BackupServiceClient())
            {
                return service.StartBackup(backupRequest);
            }
        }

        [AjaxMethod]
        [SecurityPassthrough]
        public BackupProgress GetBackupProgress()
        {
            DemandPermissionsBackup();

            using (var service = new BackupServiceClient())
            {
                return service.GetBackupProgress(GetCurrentTenantId());
            }
        }

        [AjaxMethod]
        public void DeleteBackup(Guid id)
        {
            DemandPermissionsBackup();

            using (var service = new BackupServiceClient())
            {
                service.DeleteBackup(id);
            }
        }

        [AjaxMethod]
        public void DeleteAllBackups()
        {
            DemandPermissionsBackup();

            using (var service = new BackupServiceClient())
            {
                service.DeleteAllBackups(GetCurrentTenantId());
            }
        }

        [AjaxMethod]
        public List<BackupHistoryRecord> GetBackupHistory()
        {
            DemandPermissionsBackup();

            using (var service = new BackupServiceClient())
            {
                return service.GetBackupHistory(GetCurrentTenantId());
            }
        }


        [AjaxMethod]
        public void CreateSchedule(BackupStorageType storageType, StorageParams storageParams, int backupsStored, CronParams cronParams, bool backupMail)
        {
            DemandPermissionsBackup();
            DemandSize();

            if (!SetupInfo.IsVisibleSettings("AutoBackup"))
                throw new InvalidOperationException(Resource.ErrorNotAllowedOption);

            ValidateCronSettings(cronParams);

            var scheduleRequest = new CreateScheduleRequest
                {
                    TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    BackupMail = backupMail,
                    Cron = cronParams.ToString(),
                    NumberOfBackupsStored = backupsStored,
                    StorageType = storageType
                };

            switch (storageType)
            {
                case BackupStorageType.ThridpartyDocuments:
                case BackupStorageType.Documents:
                    scheduleRequest.StorageBasePath = storageParams.FolderId;
                    break;
                case BackupStorageType.CustomCloud:
                    ValidateS3Settings(storageParams.AccessKeyId, storageParams.SecretAccessKey, storageParams.Bucket, storageParams.Region);
                    CoreContext.Configuration.SaveSection(
                        new AmazonS3Settings
                            {
                                AccessKeyId = storageParams.AccessKeyId,
                                SecretAccessKey = storageParams.SecretAccessKey,
                                Bucket = storageParams.Bucket,
                                Region = storageParams.Region
                            });
                    break;
            }

            using (var service = new BackupServiceClient())
            {
                service.CreateSchedule(scheduleRequest);
            }
        }

        [AjaxMethod]
        public Schedule GetSchedule()
        {
            DemandPermissionsBackup();

            using (var service = new BackupServiceClient())
            {
                var response = service.GetSchedule(GetCurrentTenantId());
                if (response == null)
                {
                    return null;
                }

                var schedule = new Schedule
                    {
                        StorageType = response.StorageType,
                        StorageParams = new StorageParams(),
                        CronParams = new CronParams(response.Cron),
                        BackupMail = response.BackupMail,
                        BackupsStored = response.NumberOfBackupsStored,
                        LastBackupTime = response.LastBackupTime
                    };

                if (response.StorageType == BackupStorageType.CustomCloud)
                {
                    var amazonSettings = CoreContext.Configuration.GetSection<AmazonS3Settings>();
                    schedule.StorageParams.AccessKeyId = amazonSettings.AccessKeyId;
                    schedule.StorageParams.SecretAccessKey = amazonSettings.SecretAccessKey;
                    schedule.StorageParams.Bucket = amazonSettings.Bucket;
                    schedule.StorageParams.Region = amazonSettings.Region;
                }
                else
                {
                    schedule.StorageParams.FolderId = response.StorageBasePath;
                }

                return schedule;
            }
        }

        [AjaxMethod]
        public void DeleteSchedule()
        {
            DemandPermissionsBackup();

            using (var service = new BackupServiceClient())
            {
                service.DeleteSchedule(GetCurrentTenantId());
            }
        }

        private static void DemandPermissionsBackup()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!SetupInfo.IsVisibleSettings(ManagementType.Backup.ToString()))
                throw new BillingException(Resource.ErrorNotAllowedOption, "Backup");
        }

        #endregion

        #region restore

        [AjaxMethod]
        public BackupProgress StartRestore(string backupId, BackupStorageType storageType, StorageParams storageParams, bool notify)
        {
            DemandPermissionsRestore();

            var restoreRequest = new StartRestoreRequest
            {
                TenantId = GetCurrentTenantId(),
                NotifyAfterCompletion = notify
            };

            Guid guidBackupId;
            if (Guid.TryParse(backupId, out guidBackupId))
            {
                restoreRequest.BackupId = guidBackupId;
            }
            else
            {
                restoreRequest.StorageType = storageType;
                restoreRequest.FilePathOrId = storageParams.FilePath;
                if (storageType == BackupStorageType.CustomCloud)
                {
                    ValidateS3Settings(storageParams.AccessKeyId, storageParams.SecretAccessKey, storageParams.Bucket, storageParams.Region);
                    CoreContext.Configuration.SaveSection(new AmazonS3Settings
                    {
                        AccessKeyId = storageParams.AccessKeyId,
                        SecretAccessKey = storageParams.SecretAccessKey,
                        Bucket = storageParams.Bucket,
                        Region = storageParams.Region
                    });
                }
            }

            using (var service = new BackupServiceClient())
            {
                return service.StartRestore(restoreRequest);
            }
        }

        [AjaxMethod]
        [SecurityPassthrough]
        public BackupProgress GetRestoreProgress()
        {
            DemandPermissionsRestore();

            BackupProgress result;

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            using (var service = new BackupServiceClient())
            {
                result = service.GetRestoreProgress(tenant.TenantId);
            }

            return result;
        }

        private static void DemandPermissionsRestore()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!SetupInfo.IsVisibleSettings("Restore"))
                throw new BillingException(Resource.ErrorNotAllowedOption, "Restore");
        }

        #endregion

        #region transfer

        [AjaxMethod]
        public BackupProgress StartTransfer(string targetRegion, bool notifyUsers, bool transferMail)
        {
            DemandPermissionsTransfer();
            DemandSize();

            MessageService.Send(HttpContext.Current.Request, MessageAction.StartTransferSetting);

            using (var service = new BackupServiceClient())
            {
                return service.StartTransfer(
                    new StartTransferRequest
                    {
                        TenantId = GetCurrentTenantId(),
                        TargetRegion = targetRegion,
                        BackupMail = transferMail,
                        NotifyUsers = notifyUsers
                    });
            }
        }

        [AjaxMethod]
        [SecurityPassthrough]
        public BackupProgress GetTransferProgress()
        {
            using (var service = new BackupServiceClient())
            {
                return service.GetTransferProgress(GetCurrentTenantId());
            }
        }

        private static void DemandPermissionsTransfer()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            if (!SetupInfo.IsVisibleSettings(ManagementType.Migration.ToString())
                || !currentUser.IsOwner()
                || !SetupInfo.IsSecretEmail(currentUser.Email) && !TenantExtra.GetTenantQuota().HasMigration)
                throw new InvalidOperationException(Resource.ErrorNotAllowedOption);
        }

        #endregion

        private static void DemandSize()
        {
            if (BackupHelper.ExceedsMaxAvailableSize)
                throw new InvalidOperationException(string.Format(UserControlsCommonResource.BackupSpaceExceed,
                    FileSizeComment.FilesSizeToString(BackupHelper.AvailableZipSize),
                    "",
                    ""));
        }

        private static void ValidateCronSettings(CronParams cronParams)
        {
            new CronExpression(cronParams.ToString());
        }

        private static void ValidateS3Settings(string accessKeyId, string secretAccessKey, string bucket, string region)
        {
            if (string.IsNullOrEmpty(accessKeyId))
            {
                throw new ArgumentException("Empty access key id.", "accessKeyId");
            }
            if (string.IsNullOrEmpty(secretAccessKey))
            {
                throw new ArgumentException("Empty secret access key.", "secretAccessKey");
            }
            if (string.IsNullOrEmpty(bucket))
            {
                throw new ArgumentException("Empty s3 bucket.", "bucket");
            }
            if (string.IsNullOrEmpty(region))
            {
                throw new ArgumentException("Empty s3 region.", "region");
            }
            try
            {
      
                using (var s3 = new AmazonS3Client(accessKeyId, secretAccessKey, new AmazonS3Config {RegionEndpoint = RegionEndpoint.GetBySystemName(region)}))
                {
                    s3.GetBucketLocation(new GetBucketLocationRequest {BucketName = bucket});
                }
            }
            catch(AmazonS3Exception error)
            {
                throw new Exception(error.ErrorCode);
            }
        }

        private static int GetCurrentTenantId()
        {
            return CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }

        public class Schedule
        {
            public BackupStorageType StorageType { get; set; }
            public StorageParams StorageParams { get; set; }
            public CronParams CronParams { get; set; }
            public bool BackupMail { get; set; }
            public int BackupsStored { get; set; }
            public DateTime LastBackupTime { get; set; }
        }

        public class StorageParams
        {
            public string AccessKeyId { get; set; }
            public string SecretAccessKey { get; set; }
            public string Bucket { get; set; }
            public string FolderId { get; set; }
            public string FilePath { get; set; }
            public string Region { get; set; }
        }

        public class CronParams
        {
            public BackupPeriod Period { get; set; }
            public int Hour { get; set; }
            public int Day { get; set; }

            public CronParams()
            {
            }

            public CronParams(string cronString)
            {
                var tokens = cronString.Split(' ');
                Hour = Convert.ToInt32(tokens[2]);
                if (tokens[3] != "?")
                {
                    Period = BackupPeriod.EveryMonth;
                    Day = Convert.ToInt32(tokens[3]);
                }
                else if (tokens[5] != "*")
                {
                    Period = BackupPeriod.EveryWeek;
                    Day = Convert.ToInt32(tokens[5]);
                }
                else
                {
                    Period = BackupPeriod.EveryDay;
                }
            }

            public override string ToString()
            {
                switch (Period)
                {
                    case BackupPeriod.EveryDay:
                        return string.Format("0 0 {0} ? * *", Hour);
                    case BackupPeriod.EveryMonth:
                        return string.Format("0 0 {0} {1} * ?", Hour, Day);
                    case BackupPeriod.EveryWeek:
                        return string.Format("0 0 {0} ? * {1}", Hour, Day);
                    default:
                        return base.ToString();
                }
            }
        }

        public enum BackupPeriod
        {
            EveryDay = 0,
            EveryWeek = 1,
            EveryMonth = 2
        }
    }
}