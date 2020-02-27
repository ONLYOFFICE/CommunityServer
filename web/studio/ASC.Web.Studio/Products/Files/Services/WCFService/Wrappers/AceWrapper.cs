/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
    }

    [DataContract(Name = "sharingSettings", Namespace = "")]
    public class AceShortWrapper
    {
        [DataMember(Name = "user")]
        public string User { get; set; }

        [DataMember(Name = "permissions")]
        public string Permissions { get; set; }

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
                case FileShare.Restrict:
                    permission = FilesCommonResource.AceStatusEnum_Restrict;
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
            }

            User = aceWrapper.SubjectName;
            if (aceWrapper.SubjectId.Equals(FileConstant.ShareLinkId))
            {
                IsLink = true;
                User = FilesCommonResource.AceShareLink;
            }
            Permissions = permission;
        }
    }
}