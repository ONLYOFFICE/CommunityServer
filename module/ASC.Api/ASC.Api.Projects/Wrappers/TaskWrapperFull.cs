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


using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using ASC.Api.Documents;
using ASC.Projects.Core.Domain;
using ASC.Web.Studio.UserControls.Common.Comments;

namespace ASC.Api.Projects.Wrappers
{
    ///<inherited>ASC.Api.Projects.Wrappers.TaskWrapper, ASC.Api.Projects</inherited>
    [DataContract(Name = "task", Namespace = "")]
    public class TaskWrapperFull : TaskWrapper
    {
        ///<type>ASC.Api.Documents.FileWrapper, ASC.Api.Documents</type>
        ///<collection>list</collection>
        [DataMember]
        public List<FileWrapper> Files { get; set; }

        ///<type>ASC.Web.Studio.UserControls.Common.Comments.CommentInfo, ASC.Web.Studio</type>
        ///<collection>list</collection>
        [DataMember]
        public List<CommentInfo> Comments { get; set; }

        ///<example type="int">1</example>
        [DataMember]
        public int CommentsCount { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool IsSubscribed { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool CanEditFiles { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool CanCreateComment { get; set; }

        ///<type>ASC.Api.Projects.Wrappers.ProjectWrapperFull, ASC.Api.Projects</type>
        [DataMember]
        public ProjectWrapperFull Project { get; set; }
        
        ///<example type="double">3.5</example>
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