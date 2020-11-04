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