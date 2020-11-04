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
using System.Diagnostics;
using ASC.Projects.Engine;
using ASC.Web.Projects;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Core;
using Autofac;

namespace ASC.Projects.Core.Domain
{
    [DebuggerDisplay("Task: ID = {ID}, Title = {Title}, Status = {Status}")]
    public class Task : ProjectEntity
    {
        public override EntityType EntityType { get{ return EntityType.Task;} }

        public override string ItemPath { get { return "{0}Tasks.aspx?prjID={1}&ID={2}"; } }

        public TaskPriority Priority { get; set; }

        public TaskStatus Status { get; set; }

        public int? CustomTaskStatus { get; set; }

        public int Milestone { get; set; }

        public int SortOrder { get; set; }

        public DateTime Deadline { get; set; }

        public List<Subtask> SubTasks { get; set; }

        public List<Guid> Responsibles { get; set; }

        public List<TaskLink> Links { get; set; }

        public Milestone MilestoneDesc { get; set; }

        public DateTime StatusChangedOn { get; set; }

        public DateTime StartDate { get; set; }

        public TaskSecurityInfo Security { get; set; }

        private int progress;

        public int Progress
        {
            get { return progress; }
            set
            {
                if (value < 0)
                {
                    progress = 0;
                }
                else if (value > 100)
                {
                    progress = 100;
                }
                else
                {
                    progress = value;
                }
            }
        }

        public Task()
        {
            Responsibles = new List<Guid>();
            SubTasks = new List<Subtask>();
            Links = new List<TaskLink>();
        }

        public override bool CanEdit()
        {
            if (Security != null) return Security.CanEdit;

            using (var scope = DIHelper.Resolve())
            {
                return scope.Resolve<ProjectSecurity>().CanEdit(this);
            }
        }
    }
}
