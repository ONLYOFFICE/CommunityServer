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
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "project", Namespace = "")]
    public class ProjectWrapperFull : ObjectWrapperFullBase
    {
        [DataMember]
        public bool CanEdit { get; set; }

        [DataMember]
        public ProjectSecurityInfo Security { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public object ProjectFolder { get; set; }

        [DataMember(Order = 32)]
        public bool IsPrivate { get; set; }

        [DataMember(Order = 33)]
        public int TaskCount { get; set; }

        [DataMember(Order = 34)]
        public int MilestoneCount { get; set; }

        [DataMember(Order = 34)]
        public int DiscussionCount { get; set; }

        [DataMember(Order = 35)]
        public int ParticipantCount { get; set; }

        [DataMember(Order = 35)]
        public string TimeTrackingTotal { get; set; }

        [DataMember(Order = 35)]
        public int DocumentsCount { get; set; }


        private ProjectWrapperFull()
        {
        }

        public ProjectWrapperFull(Project project, object filesRoot)
        {
            Id = project.ID;
            Title = project.Title;
            Description = project.Description;
            Status = (int)project.Status;
            Responsible = EmployeeWraper.Get(project.Responsible);
            Created = (ApiDateTime)project.CreateOn;
            CreatedBy = EmployeeWraper.Get(project.CreateBy);
            Updated = (ApiDateTime)project.LastModifiedOn;
            if (project.CreateBy != project.LastModifiedBy)
            {
                UpdatedBy = EmployeeWraper.Get(project.LastModifiedBy);
            }
            Security = new ProjectSecurityInfo(project);
            CanEdit = ProjectSecurity.CanEdit(project);
            ProjectFolder = filesRoot;
            IsPrivate = project.Private;

            TaskCount = project.TaskCount;
            MilestoneCount = project.MilestoneCount;
            DiscussionCount = project.DiscussionCount;
            TimeTrackingTotal = project.TimeTrackingTotal;
            DocumentsCount = project.DocumentsCount;
            ParticipantCount = project.ParticipantCount;
        }

        public ProjectWrapperFull(Project project) : this(project, 0)
        {
        }


        public static ProjectWrapperFull GetSample()
        {
            return new ProjectWrapperFull
                {
                    Id = 10,
                    Title = "Sample Title",
                    Description = "Sample description",
                    Status = (int)MilestoneStatus.Open,
                    Responsible = EmployeeWraper.GetSample(),
                    Created = (ApiDateTime)DateTime.Now,
                    CreatedBy = EmployeeWraper.GetSample(),
                    Updated = (ApiDateTime)DateTime.Now,
                    UpdatedBy = EmployeeWraper.GetSample(),
                    ProjectFolder = 13234
                };
        }
    }
}