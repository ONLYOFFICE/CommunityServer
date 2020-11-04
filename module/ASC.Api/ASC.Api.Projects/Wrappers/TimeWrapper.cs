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


using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "time", Namespace = "")]
    public class TimeWrapper
    {
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 5)]
        public ApiDateTime Date { get; set; }

        [DataMember(Order = 6)]
        public float Hours { get; set; }

        [DataMember(Order = 6)]
        public string Note { get; set; }

        [DataMember(Order = 7)]
        public int RelatedProject { get; set; }

        [DataMember(Order = 7)]
        public TaskWrapper Task { get; set; }

        [DataMember(Order = 7)]
        public int RelatedTask { get; set; }

        [DataMember(Order = 7)]
        public string RelatedTaskTitle { get; set; }

        [DataMember(Order = 51)]
        public EmployeeWraper CreatedBy { get; set; }

        [DataMember(Order = 52)]
        public EmployeeWraper Person { get; set; }

        [DataMember]
        public bool CanEdit { get; set; }

        [DataMember(Order = 53)]
        public PaymentStatus PaymentStatus { get; set; }

        [DataMember(Order = 54)]
        public ApiDateTime StatusChanged { get; set; }

        [DataMember(Order = 55)]
        public bool CanEditPaymentStatus { get; set; }

        [DataMember(Order = 56)]
        public ApiDateTime CreateOn { get; set; }

        private TimeWrapper()
        {
        }

        public TimeWrapper(ProjectApiBase projectApiBase, TimeSpend timeSpend)
        {
            Date = (ApiDateTime)timeSpend.Date;
            Hours = timeSpend.Hours;
            Id = timeSpend.ID;
            Note = timeSpend.Note;
            CreatedBy = projectApiBase.GetEmployeeWraper(timeSpend.CreateBy);
            RelatedProject = timeSpend.Task.Project.ID;
            RelatedTask = timeSpend.Task.ID;
            RelatedTaskTitle = timeSpend.Task.Title;
            CanEdit = projectApiBase.ProjectSecurity.CanEdit(timeSpend);
            PaymentStatus = timeSpend.PaymentStatus;
            StatusChanged = (ApiDateTime)timeSpend.StatusChangedOn;
            CanEditPaymentStatus = projectApiBase.ProjectSecurity.CanEditPaymentStatus(timeSpend);
            Task = new TaskWrapper(projectApiBase, timeSpend.Task);
            CreateOn = (ApiDateTime)timeSpend.CreateOn;

            if (timeSpend.CreateBy != timeSpend.Person)
            {
                Person = projectApiBase.GetEmployeeWraper(timeSpend.Person);
            }
        }


        public static TimeWrapper GetSample()
        {
            return new TimeWrapper
                {
                    Id = 10,
                    Date = ApiDateTime.GetSample(),
                    Hours = 3.5F,
                    Note = "Sample note",
                    RelatedProject = 123,
                    RelatedTask = 13456,
                    RelatedTaskTitle = "Sample task",
                    CreatedBy = EmployeeWraper.GetSample(),
                    Person = EmployeeWraper.GetSample(),
                    CanEdit = true,
                    PaymentStatus = PaymentStatus.Billed,
                    StatusChanged = ApiDateTime.GetSample(),
                    CanEditPaymentStatus = true
                };
        }
    }
}