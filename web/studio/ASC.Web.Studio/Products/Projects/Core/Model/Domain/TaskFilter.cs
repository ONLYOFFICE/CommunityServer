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
                                   "Task", new Dictionary<string, bool>{{"deadline", true}, {"priority", false}, {"create_on", false}, {"start_date", false}, {"title", true}, {"sort_order", true}}
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
                                   "TimeSpend", new Dictionary<string, bool> {{"date", false}, { "create_on", false }, {"hours", false}, {"note", true } }
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
