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


#region Usings

using System;
using System.Diagnostics;

using ASC.Web.Projects.Classes;

#endregion

namespace ASC.Projects.Core.Domain
{
    ///<inherited>ASC.Projects.Core.Domain.DomainObject`1, ASC.Web.Projects</inherited>
    [DebuggerDisplay("Project: ID = {ID}, Title = {Title}")]
    public class Project : DomainObject<Int32>
    {
        public override EntityType EntityType { get { return EntityType.Project; } }

        public override string ItemPath { get { return "{0}Tasks.aspx?prjID={1}"; } }

        ///<example>Title</example>
        public string Title { get; set; }

        ///<example>HtmlTitle</example>
        public string HtmlTitle
        {
            get
            {
                if (Title == null) return string.Empty;
                return Title.Length <= 40 ? Title : Title.Remove(37) + "...";
            }
        }

        ///<example>Description</example>
        public String Description { get; set; }

        ///<example type="int">1</example>
        public ProjectStatus Status { get; set; }

        ///<example>2fdfe577-3c26-4736-9df9-b5a683bb8520</example>
        public Guid Responsible { get; set; }

        ///<example>false</example>
        public bool Private { get; set; }

        ///<type>ASC.Web.Projects.Classes.ProjectSecurityInfo, ASC.Web.Projects</type>
        public ProjectSecurityInfo Security { get; set; }

        ///<example>2fdfe577-3c26-4736-9df9-b5a683bb8520</example>
        public Guid CreateBy { get; set; }

        ///<example>2020-12-22T04:11:57.0469085+00:00</example>
        public DateTime CreateOn { get; set; }

        ///<example>2fdfe577-3c26-4736-9df9-b5a683bb8520</example>
        public Guid LastModifiedBy { get; set; }

        ///<example>2020-12-22T04:11:57.0469085+00:00</example>
        public DateTime LastModifiedOn { get; set; }

        /// <summary>
        /// Opened
        /// </summary>
        ///<example type="int">1</example>
        public int TaskCount { get; set; }

        /// <summary>
        /// Total
        /// </summary>
        ///<example type="int">1</example>
        public int TaskCountTotal { get; set; }

        ///<example type="int">1</example>
        public int MilestoneCount { get; set; }

        ///<example type="int">1</example>
        public int ParticipantCount { get; set; }

        ///<example type="int">1</example>
        public int DiscussionCount { get; set; }

        ///<example>TimeTrackingTotal</example>
        public string TimeTrackingTotal { get; set; }

        ///<example type="int">1</example>
        public int DocumentsCount { get; set; }

        ///<example>2020-12-22T04:11:57.0469085+00:00</example>
        public DateTime StatusChangedOn { get; set; }
    }
}
