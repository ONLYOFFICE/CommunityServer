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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

#endregion

namespace ASC.Projects.Core.Domain.Reports
{
    static class ReportFilterSerializer
    {
        private const string LIST_SEP = "|";


        public static string ToXml(TaskFilter filter)
        {
            var doc = new XDocument();
            var root = new XElement("filter");

            var date = new XElement("date");
            root.Add(date);
            date.Add(new XAttribute("interval", filter.TimeInterval));
            if (HasDate(filter.FromDate)) date.Add(new XAttribute("from", filter.FromDate.ToString("yyyyMMdd")));
            if (HasDate(filter.ToDate)) date.Add(new XAttribute("to", filter.ToDate.ToString("yyyyMMdd")));

            if (filter.TagId > 0) root.Add(new XElement("tag", filter.TagId));

            if (filter.HasProjectIds || filter.HasProjectStatuses)
            {
                var projects = new XElement("projects");
                root.Add(projects);

                foreach (var id in filter.ProjectIds)
                {
                    projects.Add(new XElement("id", id));
                }
                foreach (var status in filter.ProjectStatuses)
                {
                    projects.Add(new XElement("status", (int)status));
                }
            }
            if (filter.HasUserId)
            {
                var users = new XElement("users");
                root.Add(users);
                if (filter.DepartmentId != default(Guid))
                {
                    users.Add(new XAttribute("dep", filter.DepartmentId.ToString("N")));
                }
                if (filter.UserId != default(Guid))
                {
                    users.Add(new XElement("id", filter.UserId.ToString("N")));
                }
            }
            if (filter.HasMilestoneStatuses)
            {
                var milestone = new XElement("milestones");
                root.Add(milestone);

                foreach (var status in filter.MilestoneStatuses)
                {
                    milestone.Add(new XElement("status", (int)status));
                }
            }
            if (filter.HasTaskStatuses)
            {
                var tasks = new XElement("tasks");
                root.Add(tasks);

                foreach (var status in filter.TaskStatuses)
                {
                    tasks.Add(new XElement("status", (int)status));
                }
            }

            if (filter.ViewType != 0)
            {
                root.Add(new XAttribute("view", filter.ViewType));
            }

            if (filter.NoResponsible)
            {
                root.Add(new XAttribute("noResponsible", filter.NoResponsible));
            }

            if (filter.PaymentStatuses.Any())
            {
                var paymentStatuses = new XElement("paymentStatuses");
                root.Add(paymentStatuses);

                foreach (var status in filter.PaymentStatuses)
                {
                    paymentStatuses.Add(new XElement("status", (int)status));
                }
            }

            doc.AddFirst(root);
            return doc.ToString(SaveOptions.DisableFormatting);
        }

        public static TaskFilter FromXml(string xml)
        {
            if (string.IsNullOrEmpty(xml)) throw new ArgumentNullException("xml");

            var filter = new TaskFilter();
            var root = XDocument.Parse(xml).Element("filter");

            var date = root.Element("date");
            if (date != null)
            {
                var attribute = date.Attribute("from");
                if (attribute != null) filter.FromDate = DateTime.ParseExact(attribute.Value, "yyyyMMdd", null);
                attribute = date.Attribute("to");
                if (attribute != null) filter.ToDate = DateTime.ParseExact(attribute.Value, "yyyyMMdd", null);
                attribute = date.Attribute("interval");
                if (attribute != null) filter.TimeInterval = (ReportTimeInterval)Enum.Parse(typeof(ReportTimeInterval), attribute.Value, true);
            }

            var tag = root.Element("tag");
            if (tag != null)
            {
                filter.TagId = int.Parse(tag.Value);
            }

            var projects = root.Element("projects");
            if (projects != null)
            {
                foreach (var id in projects.Elements("id"))
                {
                    filter.ProjectIds.Add(int.Parse(id.Value));
                }
                foreach (var status in projects.Elements("status"))
                {
                    filter.ProjectStatuses.Add((ProjectStatus)int.Parse(status.Value));
                }

                foreach (var tagName in projects.Elements("tag"))
                {
                    filter.ProjectTag = tagName.Value;
                }
            }

            var tasks = root.Element("tasks");
            if (tasks != null)
            {
                foreach (var status in tasks.Elements("status"))
                {
                    filter.TaskStatuses.Add((TaskStatus)int.Parse(status.Value));
                }
            }

            var users = root.Element("users");
            if (users != null)
            {
                if (users.Attribute("dep") != null)
                {
                    filter.DepartmentId = new Guid(users.Attribute("dep").Value);
                }
                foreach (var id in users.Elements("id"))
                {
                    filter.UserId = new Guid(id.Value);
                }
            }

            var milestones = root.Element("milestones");
            if (milestones != null)
            {
                foreach (var status in milestones.Elements("status"))
                {
                    filter.MilestoneStatuses.Add((MilestoneStatus)int.Parse(status.Value));
                }
            }

            var view = root.Attribute("view");
            if (view != null)
            {
                filter.ViewType = int.Parse(view.Value);
            }

            var noResponsible = root.Attribute("noResponsible");
            if (noResponsible != null)
            {
                filter.NoResponsible = bool.Parse(noResponsible.Value);
            }

            var paymentStatuses = root.Element("paymentStatuses");
            if (paymentStatuses != null)
            {
                foreach (var status in paymentStatuses.Elements("status"))
                {
                    filter.PaymentStatuses.Add((PaymentStatus)int.Parse(status.Value));
                }
            }

            return filter;
        }

        public static string ToUri(TaskFilter filter)
        {
            var uri = new StringBuilder();

            uri.AppendFormat("&ftime={0}", (int)filter.TimeInterval);
            if (HasDate(filter.FromDate))
            {
                uri.AppendFormat("&ffrom={0}", filter.FromDate.ToString("yyyyMMdd"));
            }
            if (HasDate(filter.ToDate))
            {
                uri.AppendFormat("&fto={0}", filter.ToDate.ToString("yyyyMMdd"));
            }
            if (filter.HasProjectIds)
            {
                uri.AppendFormat("&fpid={0}", string.Join(LIST_SEP, filter.ProjectIds.Select(id => id.ToString()).ToArray()));
            }
            if (filter.HasProjectStatuses)
            {
                uri.AppendFormat("&fps={0}", (int)filter.ProjectStatuses.FirstOrDefault());
            }
            if (filter.TagId > 0)
            {
                uri.AppendFormat("&fpt={0}", filter.TagId);
            }
            if (filter.UserId != default(Guid))
            {
                uri.AppendFormat("&fu={0}", filter.UserId);
            }
            if (filter.DepartmentId != default(Guid))
            {
                uri.AppendFormat("&fd={0}", filter.DepartmentId);
            }
            if (filter.HasMilestoneStatuses)
            {
                uri.AppendFormat("&fms={0}", (int)filter.MilestoneStatuses.FirstOrDefault());
            }
            if (filter.HasTaskStatuses)
            {
                uri.AppendFormat("&fts={0}", (int)filter.TaskStatuses.FirstOrDefault());
            }
            if (filter.ViewType != 0)
            {
                uri.AppendFormat("&fv={0}", filter.ViewType);
            }
            if (filter.NoResponsible)
            {
                uri.AppendFormat("&noRes={0}", filter.NoResponsible);
            }
            if (filter.PaymentStatuses.Any())
            {
                uri.AppendFormat("&fpays={0}", string.Join(LIST_SEP, filter.PaymentStatuses.Select(id => ((int)id).ToString(CultureInfo.InvariantCulture)).ToArray()));
            }

            return uri.ToString().Trim('&').ToLower();
        }

        public static TaskFilter FromUri(string uri)
        {
            var filter = new TaskFilter();

            var p = GetParameterFromUri(uri, "ftime");
            if (!string.IsNullOrEmpty(p)) filter.TimeInterval = (ReportTimeInterval)Enum.Parse(typeof(ReportTimeInterval), p, true);

            p = GetParameterFromUri(uri, "ffrom");
            if (!string.IsNullOrEmpty(p)) filter.FromDate = DateTime.ParseExact(p, "yyyyMMdd", null);

            p = GetParameterFromUri(uri, "fto");
            if (!string.IsNullOrEmpty(p)) filter.ToDate = DateTime.ParseExact(p, "yyyyMMdd", null);

            p = GetParameterFromUri(uri, "fu");
            if (!string.IsNullOrEmpty(p)) filter.UserId = new Guid(p);

            p = GetParameterFromUri(uri, "fd");
            if (!string.IsNullOrEmpty(p)) filter.DepartmentId = new Guid(p);

            p = GetParameterFromUri(uri, "fpid");
            if (!string.IsNullOrEmpty(p)) filter.ProjectIds = Split(p).Select(v => int.Parse(v)).ToList();

            p = GetParameterFromUri(uri, "fps");
            if (!string.IsNullOrEmpty(p)) filter.ProjectStatuses = ToEnumsArray<ProjectStatus>(p);

            p = GetParameterFromUri(uri, "fpt");
            if (!string.IsNullOrEmpty(p)) filter.TagId = int.Parse(p);

            p = GetParameterFromUri(uri, "fms");
            if (!string.IsNullOrEmpty(p)) filter.MilestoneStatuses = ToEnumsArray<MilestoneStatus>(p);

            p = GetParameterFromUri(uri, "fts");
            if (!string.IsNullOrEmpty(p)) filter.TaskStatuses = ToEnumsArray<TaskStatus>(p);

            p = GetParameterFromUri(uri, "fv");
            if (!string.IsNullOrEmpty(p)) filter.ViewType = int.Parse(p);

            p = GetParameterFromUri(uri, "nores");
            if (!string.IsNullOrEmpty(p)) filter.NoResponsible = bool.Parse(p);

            p = GetParameterFromUri(uri, "fpays");
            if (!string.IsNullOrEmpty(p)) filter.PaymentStatuses = ToEnumsArray<PaymentStatus>(p);

            return filter;
        }

        private static string GetParameterFromUri(string uri, string paramName)
        {
            foreach (var parameter in (uri ?? string.Empty).Split(new[] { '?', '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = HttpUtility.UrlDecode(parameter).Split('=');
                if (parts.Length == 2 && String.Compare(parts[0], paramName, StringComparison.OrdinalIgnoreCase) == 0) return parts[1];
            }
            return null;
        }

        private static IEnumerable<string> Split(string value)
        {
            return value.Split(new[] { LIST_SEP }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static bool HasDate(DateTime dateTime)
        {
            return dateTime != DateTime.MinValue && dateTime != DateTime.MaxValue;
        }

        private static List<T> ToEnumsArray<T>(string s)
        {
            var result = new List<T>();
            foreach (var v in Split(s))
            {
                try
                {
                    result.Add((T)Enum.Parse(typeof(T), v, true));
                }
                catch (ArgumentException) { }
            }
            return result;
        }
    }
}
