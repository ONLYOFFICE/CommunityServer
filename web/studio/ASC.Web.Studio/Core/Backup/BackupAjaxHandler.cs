/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Web;

using AjaxPro;

using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Common.Configuration;
using ASC.Core.Common.Contracts;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Notify.Cron;
using ASC.Web.Core.Security;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core.Backup
{
    [AjaxNamespace("AjaxPro.Backup")]
    public class BackupAjaxHandler
    {
        #region backup

        [AjaxMethod]
        public BackupProgress StartBackup(BackupStorageType storageType, Dictionary<string, string> storageParams, bool backupMail)
        {
            DemandPermissionsBackup();

            var backupRequest = new StartBackupRequest
            {
                TenantId = GetCurrentTenantId(),
                UserId = SecurityContext.CurrentAccount.ID,
                BackupMail = backupMail,
                StorageType = storageType,
                StorageParams = storageParams
            };

            switch (storageType)
            {
                case BackupStorageType.ThridpartyDocuments:
                case BackupStorageType.Documents:
                    backupRequest.StorageBasePath = storageParams["folderId"];
                    break;
                case BackupStorageType.Local:
                    if (!CoreContext.Configuration.Standalone) throw new Exception("Access denied");
                    backupRequest.StorageBasePath = storageParams["filePath"];
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
        public void CreateSchedule(BackupStorageType storageType, Dictionary<string, string> storageParams, int backupsStored, CronParams cronParams, bool backupMail)
        {
            DemandPermissionsBackup();

            if (!SetupInfo.IsVisibleSettings("AutoBackup"))
                throw new InvalidOperationException(Resource.ErrorNotAllowedOption);

            ValidateCronSettings(cronParams);

            var scheduleRequest = new CreateScheduleRequest
            {
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                BackupMail = backupMail,
                Cron = cronParams.ToString(),
                NumberOfBackupsStored = backupsStored,
                StorageType = storageType,
                StorageParams = storageParams
            };

            switch (storageType)
            {
                case BackupStorageType.ThridpartyDocuments:
                case BackupStorageType.Documents:
                    scheduleRequest.StorageBasePath = storageParams["folderId"];
                    break;
                case BackupStorageType.Local:
                    if (!CoreContext.Configuration.Standalone) throw new Exception("Access denied");
                    scheduleRequest.StorageBasePath = storageParams["filePath"];
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

            ScheduleResponse response;
            using (var service = new BackupServiceClient())
            {
                response = service.GetSchedule(GetCurrentTenantId());
                if (response == null)
                {
                    return null;
                }
            }

            var schedule = new Schedule
            {
                StorageType = response.StorageType,
                StorageParams = response.StorageParams ?? new Dictionary<string, string>(),
                CronParams = new CronParams(response.Cron),
                BackupMail = response.BackupMail,
                BackupsStored = response.NumberOfBackupsStored,
                LastBackupTime = response.LastBackupTime
            };

            if (response.StorageType == BackupStorageType.CustomCloud)
            {
                var amazonSettings = CoreContext.Configuration.GetSection<AmazonS3Settings>();

                var consumer = ConsumerFactory.GetByName<DataStoreConsumer>("S3");
                if (!consumer.IsSet)
                {
                    consumer["acesskey"] = amazonSettings.AccessKeyId;
                    consumer["secretaccesskey"] = amazonSettings.SecretAccessKey;

                    consumer["bucket"] = amazonSettings.Bucket;
                    consumer["region"] = amazonSettings.Region;
                }

                schedule.StorageType = BackupStorageType.ThirdPartyConsumer;
                schedule.StorageParams = consumer.AdditionalKeys.ToDictionary(r => r, r => consumer[r]);
                schedule.StorageParams.Add("module", "S3");

                using (var service = new BackupServiceClient())
                {
                    service.CreateSchedule(new CreateScheduleRequest
                    {
                        TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                        BackupMail = schedule.BackupMail,
                        Cron = schedule.CronParams.ToString(),
                        NumberOfBackupsStored = schedule.BackupsStored,
                        StorageType = schedule.StorageType,
                        StorageParams = schedule.StorageParams
                    });
                }

            }
            else if (response.StorageType != BackupStorageType.ThirdPartyConsumer)
            {
                schedule.StorageParams["folderId"] = response.StorageBasePath;
            }

            return schedule;
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

            if (!CoreContext.Configuration.Standalone && !SetupInfo.IsVisibleSettings(ManagementType.Backup.ToString()))
                throw new BillingException(Resource.ErrorNotAllowedOption, "Backup");
        }

        #endregion

        #region restore

        [AjaxMethod]
        public BackupProgress StartRestore(string backupId, BackupStorageType storageType, Dictionary<string, string> storageParams, bool notify)
        {
            DemandPermissionsRestore();

            var restoreRequest = new StartRestoreRequest
            {
                TenantId = GetCurrentTenantId(),
                NotifyAfterCompletion = notify,
                StorageParams = storageParams
            };

            Guid guidBackupId;
            if (Guid.TryParse(backupId, out guidBackupId))
            {
                restoreRequest.BackupId = guidBackupId;
            }
            else
            {
                restoreRequest.StorageType = storageType;
                restoreRequest.FilePathOrId = storageParams["filePath"];

                if (restoreRequest.StorageType == BackupStorageType.Local && !CoreContext.Configuration.Standalone)
                {
                    restoreRequest.FilePathOrId = BackupFileUploadHandler.GetFilePath();
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

            if (!CoreContext.Configuration.Standalone
                && (!SetupInfo.IsVisibleSettings("Restore")
                    || !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Restore))
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "Restore");
            }
        }

        #endregion

        #region transfer

        [AjaxMethod]
        public BackupProgress StartTransfer(string targetRegion, bool notifyUsers, bool transferMail)
        {
            DemandPermissionsTransfer();

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

        public string GetTmpFolder()
        {
            using (var service = new BackupServiceClient())
            {
                return service.GetTmpFolder();
            }
        }

        private static void ValidateCronSettings(CronParams cronParams)
        {
            new CronExpression(cronParams.ToString());
        }

        private static int GetCurrentTenantId()
        {
            return CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }

        public class Schedule
        {
            ///<example type="int">1</example>
            public BackupStorageType StorageType { get; set; }

            ///<example>null</example>
            public Dictionary<string, string> StorageParams { get; set; }

            ///<type>ASC.Web.Studio.Core.Backup.CronParams, ASC.Web.Studio</type>
            public CronParams CronParams { get; set; }

            ///<example>true</example>
            public bool BackupMail { get; set; }

            ///<example type="int">1</example>
            public int BackupsStored { get; set; }

            ///<example>2019-07-26T00:00:00</example>
            public DateTime LastBackupTime { get; set; }
        }

        public class CronParams
        {
            ///<example type="int">0</example>
            public BackupPeriod Period { get; set; }

            ///<example type="Hour">5</example>
            public int Hour { get; set; }

            ///<example type="Day">4</example>
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