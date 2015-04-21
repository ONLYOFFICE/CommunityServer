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
