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
using ASC.Projects.Core.Domain.Reports;

#endregion

namespace ASC.Projects.Core.Domain
{
    public class TaskFilter : ReportFilter, ICloneable
    {
        public int? Milestone { get; set; }

        public bool Follow { get; set; }

        public string SortBy { get; set; }

        public bool SortOrder { get; set; }

        public string SearchText { get; set; }

        public long Offset { get; set; }

        public long Max { get; set; }

        public int LastId { get; set; }

        public bool MyProjects { get; set; }

        public bool MyMilestones { get; set; }

        public Guid? ParticipantId { get; set; }

        public TaskFilter()
        {
            Max = 150001;
        }

        public Dictionary<string, Dictionary<string, bool>> SortColumns
        {
            get
            {
                return new Dictionary<string, Dictionary<string, bool>>
                           {
                               {
                                   "Task", new Dictionary<string, bool>{{"deadline", true}, {"priority", false}, {"create_on", false}, {"start_date", false}, {"title", true}}
                                   },
                               {
                                   "Milestone", new Dictionary<string, bool> {{"deadline", true}, {"create_on", false}, {"title", true}}
                                   },
                               {
                                   "Project", new Dictionary<string, bool> {{"create_on", false}, {"title", true}}
                                   },
                               {
                                   "Message", new Dictionary<string, bool> {{"comments", false}, {"create_on", false}, {"title", true}}
                                   },
                               {
                                   "TimeSpend", new Dictionary<string, bool> {{"date", false}, {"hours", false}, {"note", true}}
                                   }
                           };
            }
        }

        public string ToXml()
        {
            return ReportFilterSerializer.ToXml(this);
        }

        public static TaskFilter FromXml(string xml)
        {
            return ReportFilterSerializer.FromXml(xml);
        }

        public string ToUri()
        {
            return ReportFilterSerializer.ToUri(this);
        }

        public static TaskFilter FromUri(string uri)
        {
            return ReportFilterSerializer.FromUri(uri);
        }

        public static TaskFilter FromUri(Uri uri)
        {
            return FromUri(uri.Query);
        }

        public object Clone()
        {
            return FromXml(ToXml());
        }
    }
}
