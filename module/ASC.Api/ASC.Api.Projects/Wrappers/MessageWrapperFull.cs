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
using System.Linq;
using System.Runtime.Serialization;
using ASC.Api.Documents;
using ASC.Api.Employee;
using ASC.Core;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility.HtmlUtility;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "message", Namespace = "")]
    public class MessageWrapperFull : MessageWrapper
    {
        [DataMember]
        public bool CanEditFiles { get; set; }

        [DataMember]
        public bool CanReadFiles { get; set; }

        [DataMember]
        public List<EmployeeWraperFull> Subscribers { get; set; }

        [DataMember]
        public List<FileWrapper> Files { get; set; }

        [DataMember]
        public List<CommentInfo> Comments { get; set; }

        [DataMember]
        public ProjectWrapperFull Project { get; set; }

        public MessageWrapperFull(ProjectApiBase projectApiBase, Message message, ProjectWrapperFull project, IEnumerable<EmployeeWraperFull> subscribers)
            : base(projectApiBase, message)
        {
            CanEditFiles = projectApiBase.ProjectSecurity.CanEditFiles(message);
            CanReadFiles = projectApiBase.ProjectSecurity.CanReadFiles(message.Project);
            Text = HtmlUtility.GetFull(Text);
            Project = project;
            Subscribers = subscribers.ToList();
        }

        public MessageWrapperFull(ProjectApiBase projectApiBase, Message message, ProjectWrapperFull project, IEnumerable<EmployeeWraperFull> subscribers, IEnumerable<FileWrapper> files, IEnumerable<CommentInfo> comments) :
            this(projectApiBase, message, project, subscribers)
        {
            Files = files.ToList();
            var creator = CoreContext.UserManager.GetUsers(message.CreateBy);
            Comments = new List<CommentInfo>(comments.Count() + 1)
            {
                new CommentInfo
                {
                    TimeStamp = message.CreateOn,
                    TimeStampStr = message.CreateOn.Ago(),
                    CommentBody = HtmlUtility.GetFull(message.Description),
                    CommentID = SecurityContext.CurrentAccount.ID.ToString() + "1",
                    UserID = message.CreateBy,
                    UserFullName = creator.DisplayUserName(),
                    UserProfileLink = creator.GetUserProfilePageURL(),
                    Inactive = false,
                    IsEditPermissions = false,
                    IsResponsePermissions = false,
                    IsRead = true,
                    UserAvatarPath = creator.GetBigPhotoURL(),
                    UserPost = creator.Title,
                    CommentList = new List<CommentInfo>()
                }
            };
            Comments.AddRange(comments);
            
        }
    }
}