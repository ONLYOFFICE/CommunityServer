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
using System.Collections.Generic;
using System.Diagnostics;

using ASC.Projects.Engine;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Core;

using Autofac;

namespace ASC.Projects.Core.Domain
{
    ///<inherited>ASC.Projects.Core.Domain.ProjectEntity, ASC.Web.Projects</inherited>
    [DebuggerDisplay("Task: ID = {ID}, Title = {Title}, Status = {Status}")]
    public class Task : ProjectEntity
    {
        public override EntityType EntityType { get { return EntityType.Task; } }

        public override string ItemPath { get { return "{0}Tasks.aspx?prjID={1}&ID={2}"; } }

        ///<example type="int">1</example>
        public TaskPriority Priority { get; set; }

        ///<example type="int">1</example>
        public TaskStatus Status { get; set; }

        ///<example type="int">1</example>
        public int? CustomTaskStatus { get; set; }

        ///<example type="int">1</example>
        public int Milestone { get; set; }

        ///<example type="int">1</example>
        public int SortOrder { get; set; }

        ///<example type="int">1</example>
        public DateTime Deadline { get; set; }

        ///<type>ASC.Projects.Core.Domain.Subtask, ASC.Web.Projects</type>
        ///<collection>list</collection>
        public List<Subtask> SubTasks { get; set; }

        ///<example>2fdfe577-3c26-4736-9df9-b5a683bb8520</example>
        ///<collection>list</collection>
        public List<Guid> Responsibles { get; set; }

        ///<type>ASC.Projects.Core.Domain.TaskLink, ASC.Web.Projects</type>
        ///<collection>list</collection>
        public List<TaskLink> Links { get; set; }

        ///<type>ASC.Projects.Core.Domain.TaskLink, ASC.Web.Projects</type>
        public Milestone MilestoneDesc { get; set; }

        ///<example>2020-12-22T04:11:57.0469085+00:00</example>
        public DateTime StatusChangedOn { get; set; }

        ///<example>2020-12-22T04:11:57.0469085+00:00</example>
        public DateTime StartDate { get; set; }

        ///<type>ASC.Web.Projects.Classes.TaskSecurityInfo, ASC.Web.Projects</type>
        public TaskSecurityInfo Security { get; set; }

        ///<example type="int">1</example>
        private int progress;

        ///<example type="int">1</example>
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
