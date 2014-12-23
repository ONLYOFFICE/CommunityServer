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
    [DataContract(Name = "milestone", Namespace = "")]
    public class MilestoneWrapper : ObjectWrapperFullBase
    {
        [DataMember(Order = 14)]
        public SimpleProjectWrapper ProjectOwner { get; set; }

        [DataMember(Order = 20)]
        public ApiDateTime Deadline { get; set; }

        [DataMember(Order = 20)]
        public bool IsKey { get; set; }

        [DataMember(Order = 20)]
        public bool IsNotify { get; set; }

        [DataMember]
        public bool CanEdit { get; set; }

        [DataMember]
        public bool CanDelete { get; set; }

        [DataMember(Order = 20)]
        public int ActiveTaskCount { get; set; }

        [DataMember(Order = 20)]
        public int ClosedTaskCount { get; set; }


        private MilestoneWrapper()
        {
        }

        public MilestoneWrapper(Milestone milestone)
        {
            Id = milestone.ID;
            ProjectOwner = new SimpleProjectWrapper(milestone.Project);
            Title = milestone.Title;
            Description = milestone.Description;
            Created = (ApiDateTime)milestone.CreateOn;
            CreatedBy = EmployeeWraper.Get(milestone.CreateBy);
            Updated = (ApiDateTime)milestone.LastModifiedOn;
            if (milestone.CreateBy != milestone.LastModifiedBy)
            {
                UpdatedBy = EmployeeWraper.Get(milestone.LastModifiedBy);
            }
            if (!milestone.Responsible.Equals(Guid.Empty))
            {
                Responsible = EmployeeWraper.Get(milestone.Responsible);
            }
            Status = (int)milestone.Status;
            Deadline = new ApiDateTime(milestone.DeadLine, TimeZoneInfo.Local);
            IsKey = milestone.IsKey;
            IsNotify = milestone.IsNotify;
            CanEdit = ProjectSecurity.CanEdit(milestone);
            CanDelete = ProjectSecurity.CanDelete(milestone);
            ActiveTaskCount = milestone.ActiveTaskCount;
            ClosedTaskCount = milestone.ClosedTaskCount;
        }

        public static MilestoneWrapper GetSample()
        {
            return new MilestoneWrapper
                {
                    Id = 10,
                    ProjectOwner = SimpleProjectWrapper.GetSample(),
                    Title = "Sample Title",
                    Description = "Sample description",
                    Created = ApiDateTime.GetSample(),
                    CreatedBy = EmployeeWraper.GetSample(),
                    Updated = ApiDateTime.GetSample(),
                    UpdatedBy = EmployeeWraper.GetSample(),
                    Responsible = EmployeeWraper.GetSample(),
                    Status = (int)MilestoneStatus.Open,
                    Deadline = ApiDateTime.GetSample(),
                    IsKey = false,
                    IsNotify = false,
                    CanEdit = true,
                    ActiveTaskCount = 15,
                    ClosedTaskCount = 5
                };
        }
    }
}