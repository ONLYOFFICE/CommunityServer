/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
            CanEditFiles = ProjectSecurity.CanEditFiles(task);
            CanCreateComment = ProjectSecurity.CanCreateComment(task);
            TimeSpend = timeSpend;
            Comments = comments.ToList();
        }
    }
}