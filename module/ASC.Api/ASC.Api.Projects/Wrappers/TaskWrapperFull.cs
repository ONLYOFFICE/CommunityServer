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


using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Api.Documents;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Studio.UserControls.Common.Comments;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "task", Namespace = "")]
    public class TaskWrapperFull : TaskWrapper
    {
        [DataMember]
        public List<FileWrapper> Files { get; set; }

        [DataMember]
        public List<CommentInfo> Comments { get; set; }

        [DataMember]
        public int CommentsCount { get; set; }

        [DataMember]
        public bool IsSubscribed { get; set; }

        [DataMember]
        public bool CanEditFiles { get; set; }

        [DataMember]
        public bool CanCreateComment { get; set; }

        [DataMember]
        public ProjectWrapperFull Project { get; set; }

        [DataMember]
        public float TimeSpend { get; set; }

        public TaskWrapperFull(ProjectApiBase projectApiBase, Task task, Milestone milestone, ProjectWrapperFull project, IEnumerable<FileWrapper> files, IEnumerable<CommentInfo> comments, int commentsCount, bool isSubscribed, float timeSpend)
            : base(projectApiBase, task, milestone)
        {
            Files = files.ToList();
            CommentsCount = commentsCount;
            IsSubscribed = isSubscribed;
            Project = project;
            CanEditFiles = projectApiBase.ProjectSecurity.CanEditFiles(task);
            CanCreateComment = projectApiBase.ProjectSecurity.CanCreateComment(task);
            TimeSpend = timeSpend;
            Comments = comments.ToList();
        }
    }
}