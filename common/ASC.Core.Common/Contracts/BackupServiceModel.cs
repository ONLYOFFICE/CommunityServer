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
using System.Runtime.Serialization;

namespace ASC.Core.Common.Contracts
{
    [DataContract]
    public enum BackupStorageType
    {
        [EnumMember] Documents = 0,

        [EnumMember] ThridpartyDocuments = 1,

        [EnumMember] CustomCloud = 2,

        [EnumMember] Local = 3,

        [EnumMember] DataStore = 4,

        [EnumMember] ThirdPartyConsumer = 5
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

        [DataMember]
        public Dictionary<string, string> StorageParams { get; set; }
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

        [DataMember]
        public Dictionary<string, string> StorageParams { get; set; }
    }

    [DataContract]
    public class CreateScheduleRequest : StartBackupRequest
    {
        [DataMember]
        public string Cron { get; set; }

        [DataMember]
        public int NumberOfBackupsStored { get; set; }
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

        [DataMember]
        public Dictionary<string, string> StorageParams { get; set; }
    }
}