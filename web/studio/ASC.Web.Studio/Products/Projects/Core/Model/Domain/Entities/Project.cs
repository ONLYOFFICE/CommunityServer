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


#region Usings

using System;
using System.Diagnostics;
using ASC.Web.Projects.Classes;

#endregion

namespace ASC.Projects.Core.Domain
{
    [DebuggerDisplay("Project: ID = {ID}, Title = {Title}")]
    public class Project : DomainObject<Int32>
    {
        public override EntityType EntityType { get { return EntityType.Project; } }

        public override string ItemPath { get { return "{0}Tasks.aspx?prjID={1}"; } }

        public string Title { get; set; }

        public string HtmlTitle
        {
            get
            {
                if (Title == null) return string.Empty;
                return Title.Length <= 40 ? Title : Title.Remove(37) + "...";
            }
        }

        public String Description { get; set; }

        public ProjectStatus Status { get; set; }

        public Guid Responsible { get; set; }

        public bool Private { get; set; }

        public ProjectSecurityInfo Security { get; set; }


        public Guid CreateBy { get; set; }

        public DateTime CreateOn { get; set; }

        public Guid LastModifiedBy { get; set; }

        public DateTime LastModifiedOn { get; set; }
        
        /// <summary>
        /// Opened
        /// </summary>
        public int TaskCount { get; set; }

        /// <summary>
        /// Total
        /// </summary>
        public int TaskCountTotal { get; set; }

        public int MilestoneCount { get; set; }

        public int ParticipantCount { get; set; }

        public int DiscussionCount { get; set; }

        public string TimeTrackingTotal { get; set; }

        public int DocumentsCount { get; set; }

        public DateTime StatusChangedOn { get; set; }
    }
}
