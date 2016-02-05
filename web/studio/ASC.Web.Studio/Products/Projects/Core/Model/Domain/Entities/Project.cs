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


#region Usings

using System;
using System.Diagnostics;

#endregion

namespace ASC.Projects.Core.Domain
{
    [DebuggerDisplay("Project: ID = {ID}, Title = {Title}")]
    public class Project : DomainObject<Int32>
    {
        public override EntityType EntityType { get { return EntityType.Project; } }

        public override string ItemPath { get { return "{0}tasks.aspx?prjID={1}"; } }

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


        public Guid CreateBy { get; set; }

        public DateTime CreateOn { get; set; }

        public Guid LastModifiedBy { get; set; }

        public DateTime LastModifiedOn { get; set; }

        public int TaskCount { get; set; }

        public int MilestoneCount { get; set; }

        public int ParticipantCount { get; set; }

        public int DiscussionCount { get; set; }

        public string TimeTrackingTotal { get; set; }

        public int DocumentsCount { get; set; }

        public DateTime StatusChangedOn { get; set; }
    }
}
