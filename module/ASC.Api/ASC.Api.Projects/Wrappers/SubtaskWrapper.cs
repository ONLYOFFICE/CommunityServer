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
    [DataContract(Name = "subtask", Namespace = "")]
    public class SubtaskWrapper : ObjectWrapperFullBase
    {
        [DataMember]
        public bool CanEdit { get; set; }

        [DataMember]
        public int TaskId { get; set; }

        private SubtaskWrapper()
        {
        }

        public SubtaskWrapper(ProjectApiBase projectApiBase, Subtask subtask, Task task)
        {
            Id = subtask.ID;
            Title = subtask.Title;
            Status = (int)subtask.Status;
            if (subtask.Responsible != Guid.Empty)
            {
                Responsible = projectApiBase.GetEmployeeWraper(subtask.Responsible);
            }
            Created = (ApiDateTime)subtask.CreateOn;
            CreatedBy = projectApiBase.GetEmployeeWraper(subtask.CreateBy);
            Updated = (ApiDateTime)subtask.LastModifiedOn;
            if (subtask.CreateBy != subtask.LastModifiedBy)
            {
                UpdatedBy = projectApiBase.GetEmployeeWraper(subtask.LastModifiedBy);
            }
            CanEdit = projectApiBase.ProjectSecurity.CanEdit(task, subtask);

            TaskId = task.ID;
        }


        public static SubtaskWrapper GetSample()
        {
            return new SubtaskWrapper
                {
                    Id = 1233,
                    Title = "Sample subtask",
                    Description = "Sample description",
                    Status = (int)TaskStatus.Open,
                    Responsible = EmployeeWraper.GetSample(),
                    Created = ApiDateTime.GetSample(),
                    CreatedBy = EmployeeWraper.GetSample(),
                    Updated = ApiDateTime.GetSample(),
                    UpdatedBy = EmployeeWraper.GetSample(),
                };
        }
    }
}