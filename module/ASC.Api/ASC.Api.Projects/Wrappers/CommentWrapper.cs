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
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "comment", Namespace = "")]
    public class CommentWrapper : IApiSortableDate
    {
        [DataMember(Order = 1)]
        public Guid Id { get; set; }

        [DataMember(Order = 12)]
        public Guid ParentId { get; set; }

        [DataMember(Order = 10)]
        public string Text { get; set; }

        [DataMember(Order = 50)]
        public ApiDateTime Created { get; set; }

        [DataMember(Order = 9)]
        public EmployeeWraper CreatedBy { get; set; }

        [DataMember(Order = 50, EmitDefaultValue = false)]
        public ApiDateTime Updated { get; set; }

        [DataMember(Order = 13)]
        public bool Inactive { get; set; }

        [DataMember(Order = 14)]
        public bool CanEdit { get; set; }


        private CommentWrapper()
        {
        }

        public CommentWrapper(ProjectApiBase projectApiBase, Comment comment, ProjectEntity entity)
        {
            Id = comment.OldGuidId;
            ParentId = comment.Parent;
            Text = comment.Content;
            Created = Updated = (ApiDateTime)comment.CreateOn;
            CreatedBy = projectApiBase.GetEmployeeWraper(comment.CreateBy);
            Inactive = comment.Inactive;
            CanEdit = projectApiBase.ProjectSecurity.CanEditComment(entity, comment);
        }


        public static CommentWrapper GetSample()
        {
            return new CommentWrapper
                {
                    Id = Guid.Empty,
                    ParentId = Guid.Empty,
                    Text = "comment text",
                    Created = ApiDateTime.GetSample(),
                    CreatedBy = EmployeeWraper.GetSample(),
                    Updated = ApiDateTime.GetSample(),
                    Inactive = false
                };
        }
    }
}