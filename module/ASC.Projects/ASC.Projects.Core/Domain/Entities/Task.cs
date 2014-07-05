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

#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace ASC.Projects.Core.Domain
{
    [DebuggerDisplay("Task: ID = {ID}, Title = {Title}, Status = {Status}")]
    public class Task : ProjectEntity
    {
        public string Description { get; set; }

        public TaskPriority Priority { get; set; }

        public TaskStatus Status { get; set; }

        public int Milestone { get; set; }

        public int SortOrder { get; set; }

        public DateTime Deadline { get; set; }

        public List<Subtask> SubTasks { get; set; }

        public HashSet<Guid> Responsibles { get; set; }

        public List<TaskLink> Links { get; set; }

        public Milestone MilestoneDesc { get; set; }

        public DateTime StatusChangedOn { get; set; }

        public DateTime StartDate { get; set; }

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
            Responsibles = new HashSet<Guid>();
            SubTasks = new List<Subtask>();
            Links = new List<TaskLink>();
        }
    }
}
