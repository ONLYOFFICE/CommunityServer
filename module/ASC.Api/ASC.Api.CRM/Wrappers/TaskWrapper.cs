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

using ASC.Api.Employee;
using ASC.CRM.Core.Entities;
using ASC.Specific;

namespace ASC.Api.CRM.Wrappers
{
    /// <summary>
    ///  Task
    /// </summary>
    /// <inherited>ASC.Api.CRM.Wrappers.ObjectWrapperBase, ASC.Api.CRM</inherited>
    [DataContract(Name = "task", Namespace = "")]
    public class TaskWrapper : ObjectWrapperBase
    {
        public TaskWrapper(int id) : base(id)
        {
        }

        public TaskWrapper(Task task) : base(task.ID)
        {
            CreateBy = EmployeeWraper.Get(task.CreateBy);
            Created = (ApiDateTime)task.CreateOn;
            Title = task.Title;
            Description = task.Description;
            DeadLine = (ApiDateTime)task.DeadLine;
            Responsible = EmployeeWraper.Get(task.ResponsibleID);
            IsClosed = task.IsClosed;
            AlertValue = task.AlertValue;
        }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public EmployeeWraper CreateBy { get; set; }

        ///<example>2020-12-16T03:11:54.3573531Z</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ApiDateTime Created { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.ContactBaseWithEmailWrapper, ASC.Api.CRM</type>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ContactBaseWithEmailWrapper Contact { get; set; }

        ///<example>Send a commercial offer</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Title { get; set; }

        ///<example>Description</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Description { get; set; }

        ///<example>2020-12-16T03:11:54.3573531Z</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ApiDateTime DeadLine { get; set; }

        ///<example type="int">1</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int AlertValue { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public EmployeeWraper Responsible { get; set; }

        ///<example>true</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsClosed { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.TaskCategoryBaseWrapper, ASC.Api.CRM</type>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public TaskCategoryBaseWrapper Category { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.EntityWrapper, ASC.Api.CRM</type>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public EntityWrapper Entity { get; set; }

        ///<example>true</example>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool CanEdit { get; set; }

        public static TaskWrapper GetSample()
        {
            return new TaskWrapper(0)
            {
                Created = ApiDateTime.GetSample(),
                CreateBy = EmployeeWraper.GetSample(),
                DeadLine = ApiDateTime.GetSample(),
                IsClosed = false,
                Responsible = EmployeeWraper.GetSample(),
                Category = TaskCategoryBaseWrapper.GetSample(),
                CanEdit = true,
                Title = "Send a commercial offer",
                AlertValue = 0
            };
        }
    }

    [DataContract(Name = "taskBase", Namespace = "")]
    public class TaskBaseWrapper : ObjectWrapperBase
    {
        public TaskBaseWrapper(int id) : base(id)
        {
        }

        public TaskBaseWrapper(Task task) : base(task.ID)
        {
            Title = task.Title;
            Description = task.Description;
            DeadLine = (ApiDateTime)task.DeadLine;
            Responsible = EmployeeWraper.Get(task.ResponsibleID);
            IsClosed = task.IsClosed;
            AlertValue = task.AlertValue;
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Title { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Description { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ApiDateTime DeadLine { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int AlertValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public EmployeeWraper Responsible { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsClosed { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public TaskCategoryBaseWrapper Category { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public EntityWrapper Entity { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool CanEdit { get; set; }

        public static TaskBaseWrapper GetSample()
        {
            return new TaskBaseWrapper(0)
            {
                DeadLine = (ApiDateTime)DateTime.UtcNow.AddMonths(1),
                IsClosed = false,
                Responsible = EmployeeWraper.GetSample(),
                Category = TaskCategoryBaseWrapper.GetSample(),
                CanEdit = true,
                Title = "Send a commercial offer",
                AlertValue = 0
            };
        }
    }
}