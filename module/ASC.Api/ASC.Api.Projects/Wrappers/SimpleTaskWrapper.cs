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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "task")]
    public class SimpleTaskWrapper
    {
        [DataMember(Name = "id")]
        public int ID { get; set; }

        [DataMember(Name = "title")]
        public String Title { get; set; }

        [DataMember(Name = "description")]
        public String Description { get; set; }

        [DataMember(Name = "deadline")]
        public ApiDateTime Deadline { get; set; }

        [DataMember(Order = 20)]
        public int Status { get; set; }

        [DataMember(Order = 50)]
        public ApiDateTime Created { get; set; }

        [DataMember(Order = 51, EmitDefaultValue = false)]
        public Guid CreatedBy { get; set; }

        private ApiDateTime updated;

        [DataMember(Order = 52, EmitDefaultValue = false)]
        public ApiDateTime Updated
        {
            get { return updated < Created ? Created : updated; }
            set { updated = value; }
        }

        [DataMember(Order = 41, EmitDefaultValue = false)]
        public Guid UpdatedBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ApiDateTime StartDate { get; set; }

        [DataMember(Order = 13, EmitDefaultValue = false)]
        public int MilestoneId { get; set; }

        [DataMember(Order = 12)]
        public TaskPriority Priority { get; set; }

        [DataMember(Order = 14)]
        public int ProjectOwner { get; set; }

        [DataMember(Order = 15, EmitDefaultValue = false)]
        public int Progress { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public int SubtasksCount { get; set; }

        [DataMember(Order = 21, EmitDefaultValue = false)]
        public IEnumerable<TaskLinkWrapper> Links { get; set; }

        [DataMember(Order = 53)]
        public List<Guid> Responsibles { get; set; }

        [DataMember]
        public bool CanEdit { get; set; }

        [DataMember]
        public bool CanCreateSubtask { get; set; }

        [DataMember]
        public bool CanCreateTimeSpend { get; set; }

        [DataMember]
        public bool CanDelete { get; set; }

        public SimpleTaskWrapper(Task task)
        {
            ID = task.ID;
            Title = task.Title;
            Description = task.Description;
            Status = (int)task.Status;

            if (task.Responsibles != null)
            {
                Responsibles = task.Responsibles;
            }

            Deadline = (task.Deadline == DateTime.MinValue ? null : new ApiDateTime(task.Deadline, TimeZoneInfo.Local));
            Priority = task.Priority;
            ProjectOwner = task.Project.ID;
            MilestoneId = task.Milestone;
            Created = (ApiDateTime)task.CreateOn;
            CreatedBy = task.CreateBy;
            Updated = (ApiDateTime)task.LastModifiedOn;
            StartDate = task.StartDate.Equals(DateTime.MinValue) ? null : (ApiDateTime)task.StartDate;

            if (task.CreateBy != task.LastModifiedBy)
            {
                UpdatedBy = task.LastModifiedBy;
            }

            if (task.SubTasks != null)
            {
                SubtasksCount = task.SubTasks.Count(r => r.Status == TaskStatus.Open); // somehow don't work :(
            }

            Progress = task.Progress;
            MilestoneId = task.Milestone;

            if (task.Links.Any())
            {
                Links = task.Links.Select(r => new TaskLinkWrapper(r));
            }

            CanEdit = ProjectSecurity.CanEdit(task);
            CanCreateSubtask = ProjectSecurity.CanCreateSubtask(task);
            CanCreateTimeSpend = ProjectSecurity.CanCreateTimeSpend(task);
            CanDelete = ProjectSecurity.CanDelete(task);
        }
    }
}