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
        ///<example name="commentID">12261949-db62-43c2-b956-91e12c412d5a</example>
        [DataMember(Name = "commentID")]
        public string CommentID { get; set; }

        ///<example name="userID">15985c13-ad91-4f2c-9286-cf991448e796</example>
        [DataMember(Name = "userID")]
        public Guid UserID { get; set; }

        ///<example name="userPost">null</example>
        [DataMember(Name = "userPost")]
        public string UserPost { get; set; }

        ///<example name="userFullName">Administrator</example>
        [DataMember(Name = "userFullName")]
        public string UserFullName { get; set; }

        ///<example name="userProfileLink">\/Products\/People\/Profile.aspx?user=administrator</example>
        [DataMember(Name = "userProfileLink")]
        public string UserProfileLink { get; set; }

        ///<example name="userAvatarPath">\/skins\/default\/images\/default_user_photo_size_82-82.png</example>
        [DataMember(Name = "userAvatarPath")]
        public string UserAvatarPath { get; set; }

        ///<example name="commentBody"><p>das</p>\u000a</example>
        [DataMember(Name = "commentBody")]
        public string CommentBody { get; set; }

        ///<example name="inactive">false</example>
        [DataMember(Name = "inactive")]
        public bool Inactive { get; set; }

        ///<example name="isRead">true</example>
        [DataMember(Name = "isRead")]
        public bool IsRead { get; set; }

        ///<example name="isEditPermissions">true</example>
        [DataMember(Name = "isEditPermissions")]
        public bool IsEditPermissions { get; set; }

        ///<example name="isResponsePermissions">true</example>
        [DataMember(Name = "isResponsePermissions")]
        public bool IsResponsePermissions { get; set; }

        public DateTime TimeStamp { get; set; }

        ///<example name="timeStampStr">15:39 Today</example>
        [DataMember(Name = "timeStampStr")]
        public string TimeStampStr { get; set; }

        ///<example name="commentList">null</example>
        [DataMember(Name = "commentList")]
        public IList<CommentInfo> CommentList { get; set; }

        ///<example name="attachments">null</example>
        [DataMember(Name = "attachments")]
        public IList<Attachment> Attachments { get; set; }
    }
}