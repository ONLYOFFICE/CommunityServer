/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Runtime.Serialization;

namespace ASC.Core.Common.Contracts
{
    [DataContract]
    public enum BackupStorageType
    {
        [EnumMember]
        Documents = 0,

        [EnumMember]
        ThridpartyDocuments = 1,

        [EnumMember]
        CustomCloud = 2,
        
        [EnumMember]
        Local = 3,

        [EnumMember]
        DataStore = 4
    }

    [DataContract]
    public class StartBackupRequest
    {
        [DataMember]
        public int TenantId { get; set; }

        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public bool BackupMail { get; set; }

        [DataMember]
        public BackupStorageType StorageType { get; set; }

        [DataMember]
        public string StorageBasePath { get; set; }
    }

    [DataContract]
    public class BackupProgress
    {
        [DataMember]
        public bool IsCompleted { get; set; }

        [DataMember]
        public int Progress { get; set; }

        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public string Link { get; set; }
    }

    [DataContract]
    public class BackupHistoryRecord
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public BackupStorageType StorageType { get; set; }

        [DataMember]
        public DateTime CreatedOn { get; set; }

        [DataMember]
        public DateTime ExpiresOn { get; set; }
    }

    [DataContract]
    public class StartTransferRequest
    {
        [DataMember]
        public int TenantId { get; set; }

        [DataMember]
        public string TargetRegion { get; set; }

        [DataMember]
        public bool NotifyUsers { get; set; }

        [DataMember]
        public bool BackupMail { get; set; }
    }

    [DataContract]
    public class TransferRegion
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string BaseDomain { get; set; }

        [DataMember]
        public bool IsCurrentRegion { get; set; }
    }

    [DataContract]
    public class StartRestoreRequest
    {
        [DataMember]
        public int TenantId { get; set; }

        [DataMember]
        public Guid BackupId { get; set; }

        [DataMember]
        public BackupStorageType StorageType { get; set; }

        [DataMember]
        public string FilePathOrId { get; set; }

        [DataMember]
        public bool NotifyAfterCompletion { get; set; }
    }

    [DataContract]
    public class CreateScheduleRequest
    {
        [DataMember]
        public int TenantId { get; set; }

        [DataMember]
        public string Cron { get; set; }

        [DataMember]
        public bool BackupMail { get; set; }

        [DataMember]
        public int NumberOfBackupsStored { get; set; }

        [DataMember]
        public BackupStorageType StorageType { get; set; }

        [DataMember]
        public string StorageBasePath { get; set; }
    }

    [DataContract]
    public class ScheduleResponse
    {
        [DataMember]
        public BackupStorageType StorageType { get; set; }

        [DataMember]
        public string StorageBasePath { get; set; }

        [DataMember]
        public bool BackupMail { get; set; }

        [DataMember]
        public int NumberOfBackupsStored { get; set; }

        [DataMember]
        public string Cron { get; set; }

        [DataMember]
        public DateTime LastBackupTime { get; set; }
    }
}