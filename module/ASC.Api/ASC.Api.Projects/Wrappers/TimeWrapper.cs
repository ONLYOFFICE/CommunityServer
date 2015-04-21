/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System;
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

        private TimeWrapper()
        {
        }

        public TimeWrapper(TimeSpend timeSpend)
        {
            Date = (ApiDateTime)timeSpend.Date;
            Hours = timeSpend.Hours;
            Id = timeSpend.ID;
            Note = timeSpend.Note;
            CreatedBy = EmployeeWraper.Get(timeSpend.CreateBy);
            RelatedProject = timeSpend.Task.Project.ID;
            RelatedTask = timeSpend.Task.ID;
            RelatedTaskTitle = timeSpend.Task.Title;
            CanEdit = ProjectSecurity.CanEdit(timeSpend);
            PaymentStatus = timeSpend.PaymentStatus;
            StatusChanged = (ApiDateTime)timeSpend.StatusChangedOn;
            CanEditPaymentStatus = ProjectSecurity.CanEditPaymentStatus(timeSpend);


            if (timeSpend.CreateBy != timeSpend.Person)
            {
                Person = EmployeeWraper.Get(timeSpend.Person);
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