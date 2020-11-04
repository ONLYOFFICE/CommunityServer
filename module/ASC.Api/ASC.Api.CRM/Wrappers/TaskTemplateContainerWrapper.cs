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
using ASC.Api.Employee;

namespace ASC.Api.CRM.Wrappers
{
    [DataContract(Namespace = "taskTemplateContainer")]
    public class TaskTemplateContainerWrapper : ObjectWrapperBase
    {
        public TaskTemplateContainerWrapper() :
            base(0)
        {
        }

        public TaskTemplateContainerWrapper(int id)
            : base(id)
        {
        }

        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public String Title { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public String EntityType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IEnumerable<TaskTemplateWrapper> Items { get; set; }

        public static TaskTemplateContainerWrapper GetSample()
        {
            return new TaskTemplateContainerWrapper
                {
                    EntityType = "contact",
                    Title = "Birthday greetings",
                    Items = new List<TaskTemplateWrapper>
                        {
                            TaskTemplateWrapper.GetSample()
                        }
                };
        }
    }

    [DataContract(Namespace = "taskTemplate")]
    public class TaskTemplateWrapper : ObjectWrapperBase
    {
        public TaskTemplateWrapper() : base(0)
        {
        }

        public TaskTemplateWrapper(int id) :
            base(id)
        {
        }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public int ContainerID { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public String Title { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Description { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public EmployeeWraper Responsible { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public TaskCategoryWrapper Category { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool isNotify { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public long OffsetTicks { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool DeadLineIsFixed { get; set; }

        public static TaskTemplateWrapper GetSample()
        {
            return new TaskTemplateWrapper
                {
                    Title = "Send an Email",
                    Category = TaskCategoryWrapper.GetSample(),
                    isNotify = true,
                    Responsible = EmployeeWraper.GetSample(),
                    ContainerID = 12,
                    DeadLineIsFixed = false,
                    OffsetTicks = TimeSpan.FromDays(10).Ticks,
                    Description = ""
                };
        }
    }
}