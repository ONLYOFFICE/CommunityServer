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
using System.Runtime.Serialization;

using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Files.Resources;

namespace ASC.Web.Files.Services.WCFService
{
    [DataContract(Name = "ace_collection", Namespace = "")]
    public class AceCollection
    {
        [DataMember(Name = "entries", Order = 1)]
        public ItemList<string> Entries { get; set; }

        [DataMember(Name = "aces", Order = 2)]
        public ItemList<AceWrapper> Aces { get; set; }

        [DataMember(Name = "message", Order = 3, IsRequired = false)]
        public string Message { get; set; }

        [DataMember(Name = "advancedSettings", Order = 4, IsRequired = false)]
        public AceAdvancedSettingsWrapper AdvancedSettings { get; set; }
    }

    [DataContract(Name = "ace_wrapper", Namespace = "")]
    public class AceWrapper
    {
        [DataMember(Name = "id", Order = 1)]
        public Guid SubjectId { get; set; }

        [DataMember(Name = "title", Order = 2, EmitDefaultValue = false)]
        public string SubjectName { get; set; }

        [DataMember(Name = "link", Order = 3, EmitDefaultValue = false)]
        public string Link { get; set; }

        [DataMember(Name = "is_group", Order = 4)]
        public bool SubjectGroup { get; set; }

        [DataMember(Name = "owner", Order = 5)]
        public bool Owner { get; set; }

        [DataMember(Name = "ace_status", Order = 6)]
        public FileShare Share { get; set; }

        [DataMember(Name = "locked", Order = 7)]
        public bool LockedRights { get; set; }

        [DataMember(Name = "disable_remove", Order = 8)]
        public bool DisableRemove { get; set; }

        [DataMember(Name = "linkSettings", Order = 9, EmitDefaultValue = false, IsRequired = false)]
        public LinkSettingsWrapper LinkSettings { get; set; }

        [DataMember(Name = "entryType", Order = 10)]
        public FileEntryType EntryType { get; set; }
        
        [DataMember(Name = "inherited", Order = 11)]
        public bool Inherited { get; set; }

        public bool IsLink
        {
            get
            {
                return SubjectId == FileConstant.ShareLinkId || LinkSettings != null;
            }
        }
    }

    [DataContract(Name = "linkSettings", Namespace = "")]
    public class LinkSettingsWrapper
    {
        [DataMember(Name = "autoDelete")]
        public bool AutoDelete { get; set; }

        [DataMember(Name = "expirationDate")]
        public string ExpirationDate { get; set; }

        [DataMember(Name = "expired")]
        public bool Expired { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }
    }

    [DataContract(Name = "sharingSettings", Namespace = "")]
    public class AceShortWrapper
    {
        ///<example name="user">user</example>
        [DataMember(Name = "user")]
        public string User { get; set; }

        ///<example name="permissions">permissions</example>
        [DataMember(Name = "permissions")]
        public string Permissions { get; set; }

        ///<example name="isLink">true</example>
        [DataMember(Name = "isLink", EmitDefaultValue = false, IsRequired = false)]
        public bool IsLink { get; set; }

        public AceShortWrapper(AceWrapper aceWrapper)
        {
            var permission = string.Empty;

            switch (aceWrapper.Share)
            {
                case FileShare.Read:
                    permission = FilesCommonResource.AceStatusEnum_Read;
                    break;
                case FileShare.ReadWrite:
                    permission = FilesCommonResource.AceStatusEnum_ReadWrite;
                    break;
                case FileShare.CustomFilter:
                    permission = FilesCommonResource.AceStatusEnum_CustomFilter;
                    break;
                case FileShare.Review:
                    permission = FilesCommonResource.AceStatusEnum_Review;
                    break;
                case FileShare.FillForms:
                    permission = FilesCommonResource.AceStatusEnum_FillForms;
                    break;
                case FileShare.Comment:
                    permission = FilesCommonResource.AceStatusEnum_Comment;
                    break;
                case FileShare.Restrict:
                    permission = FilesCommonResource.AceStatusEnum_Restrict;
                    break;
            }

            User = aceWrapper.SubjectName;
            if (aceWrapper.IsLink)
            {
                IsLink = true;
                User = FilesCommonResource.AceShareLink;
            }
            Permissions = permission;
        }
    }

    [DataContract(Name = "advancedSettings", Namespace = "")]
    public class AceAdvancedSettingsWrapper
    {
        [DataMember(Name = "denyDownload")]
        public bool DenyDownload { get; set; }

        [DataMember(Name = "denySharing")]
        public bool DenySharing { get; set; }
    }
}