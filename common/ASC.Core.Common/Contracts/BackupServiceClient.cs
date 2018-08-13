/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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