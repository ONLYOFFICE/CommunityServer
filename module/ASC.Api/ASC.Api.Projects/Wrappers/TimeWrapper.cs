/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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