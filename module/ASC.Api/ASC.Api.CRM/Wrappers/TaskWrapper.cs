/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public EmployeeWraper CreateBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ApiDateTime Created { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ContactBaseWithEmailWrapper Contact { get; set; }

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

        public static TaskWrapper GetSample()
        {
            return new TaskWrapper(0)
                {
                    Created = (ApiDateTime)DateTime.UtcNow,
                    CreateBy = EmployeeWraper.GetSample(),
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