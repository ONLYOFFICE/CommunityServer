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
using ASC.Common.Module;

namespace ASC.Core.Common.Contracts
{
    public class BackupServiceClient : BaseWcfClient<IBackupService>, IBackupService
    {
        public BackupProgress StartBackup(StartBackupRequest request)
        {
            return Channel.StartBackup(request);
        }

        public BackupProgress GetBackupProgress(int tenantId)
        {
            return Channel.GetBackupProgress(tenantId);
        }

        public void DeleteBackup(Guid backupId)
        {
            Channel.DeleteBackup(backupId);
        }

        public void DeleteAllBackups(int tenantId)
        {
            Channel.DeleteAllBackups(tenantId);
        }

        public List<BackupHistoryRecord> GetBackupHistory(int tenantId)
        {
            return Channel.GetBackupHistory(tenantId);
        }

        public BackupProgress StartTransfer(StartTransferRequest request)
        {
            return Channel.StartTransfer(request);
        }

        public BackupProgress GetTransferProgress(int tenantID)
        {
            return Channel.GetTransferProgress(tenantID);
        }

        public List<TransferRegion> GetTransferRegions()
        {
            return Channel.GetTransferRegions();
        }

        public BackupProgress StartRestore(StartRestoreRequest request)
        {
            return Channel.StartRestore(request);
        }

        public BackupProgress GetRestoreProgress(int tenantId)
        {
            return Channel.GetRestoreProgress(tenantId);
        }

        public string GetTmpFolder()
        {
            return Channel.GetTmpFolder();
        }

        public void CreateSchedule(CreateScheduleRequest request)
        {
            Channel.CreateSchedule(request);
        }

        public void DeleteSchedule(int tenantId)
        {
            Channel.DeleteSchedule(tenantId);
        }

        public ScheduleResponse GetSchedule(int tenantId)
        {
            return Channel.GetSchedule(tenantId);
        }
    }
}