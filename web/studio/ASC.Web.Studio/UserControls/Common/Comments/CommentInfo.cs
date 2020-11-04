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

namespace ASC.Web.Studio.UserControls.Common.Comments
{
    [DataContract]
    public class Attachment
    {
        [DataMember(Name = "FileName")]
        public string FileName { get; set; }

        [DataMember(Name = "FilePath")]
        public string FilePath { get; set; }
    }

    [DataContract]
    public class CommentInfo
    {
        [DataMember(Name = "commentID")]
        public string CommentID { get; set; }

        [DataMember(Name = "userID")]
        public Guid UserID { get; set; }

        [DataMember(Name = "userPost")]
        public string UserPost { get; set; }

        [DataMember(Name = "userFullName")]
        public string UserFullName { get; set; }

        [DataMember(Name = "userProfileLink")]
        public string UserProfileLink { get; set; }

        [DataMember(Name = "userAvatarPath")]
        public string UserAvatarPath { get; set; }

        [DataMember(Name = "commentBody")]
        public string CommentBody { get; set; }

        [DataMember(Name = "inactive")]
        public bool Inactive { get; set; }

        [DataMember(Name = "isRead")]
        public bool IsRead { get; set; }

        [DataMember(Name = "isEditPermissions")]
        public bool IsEditPermissions { get; set; }

        [DataMember(Name = "isResponsePermissions")]
        public bool IsResponsePermissions { get; set; }

        public DateTime TimeStamp { get; set; }

        [DataMember(Name = "timeStampStr")]
        public string TimeStampStr { get; set; }

        [DataMember(Name = "commentList")]
        public IList<CommentInfo> CommentList { get; set; }

        [DataMember(Name = "attachments")]
        public IList<Attachment> Attachments { get; set; }
    }
}