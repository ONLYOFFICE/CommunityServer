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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;
using ASC.CRM.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;
using ASC.Web.CRM.Controls.Common;

namespace ASC.Web.CRM.Controls.Tasks
{
    public partial class ListTaskView : BaseUserControl
    {
        #region Properies

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Tasks/ListTaskView.ascx"); }
        }

        private const string ExportErrorCookieKey = "export_tasks_error";

        protected readonly ILog _log = LogManager.GetLogger("ASC.CRM");

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (UrlParameters.Action != "export")
            {
                _phListBase.Controls.Add(LoadControl(ListBaseView.Location));
            }
            else // export to csv
            {
                var tasks = GetTasksByFilter();

                if (tasks.Count != 0)
                {
                    if (UrlParameters.View != "editor")
                    {
                        Response.Clear();
                        Response.ContentType = "text/csv; charset=utf-8";
                        Response.ContentEncoding = Encoding.UTF8;
                        Response.Charset = Encoding.UTF8.WebName;

                        const string fileName = "tasks.csv";

                        Response.AppendHeader("Content-Disposition", String.Format("attachment; filename={0}", fileName));
                        Response.Write(ExportToCSV.ExportTasksToCSV(tasks, false));

                        MessageService.Send(HttpContext.Current.Request, MessageAction.CrmTasksExportedToCsv, tasks.Select(x => x.Title));
                        
                        Response.End();
                    }
                    else
                    {
                        var fileUrl = ExportToCSV.ExportTasksToCSV(tasks, true);
                        Response.Redirect(fileUrl);
                    }
                }
                else
                {
                    var cookie = HttpContext.Current.Request.Cookies.Get(ExportErrorCookieKey);
                    if (cookie == null)
                    {
                        cookie = new HttpCookie(ExportErrorCookieKey) {Value = CRMTaskResource.ExportTaskListEmptyError};
                        HttpContext.Current.Response.Cookies.Add(cookie);
                    }
                    Response.Redirect(PathProvider.StartURL() + "tasks.aspx");
                }
            }
        }

        #endregion

        #region Methods

        private class FilterObject
        {
            public string SortBy { get; set; }
            public string SortOrder { get; set; }
            public string FilterValue { get; set; }

            public Guid ResponsibleID { get; set; }

            public bool? IsClosed { get; set; }

            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }

            public int CategoryID { get; set; }

            public int ContactID { get; set; }

            public FilterObject()
            {
                IsClosed = null;
                FromDate = DateTime.MinValue;
                ToDate = DateTime.MinValue;
            }
        };

        private FilterObject GetFilterObjectFromCookie()
        {
            var result = new FilterObject();

            var cookieKey = GetCookieKeyForFilterForExport();

            var cookie = Request.Cookies[HttpUtility.UrlEncode(cookieKey)];
            _log.Debug(String.Format("GetFilterObjectFromCookie. cookieKey={0}", cookieKey));

            if (cookie != null && !String.IsNullOrEmpty(cookie.Value))
            {
                var anchor = cookie.Value;
                _log.Debug(String.Format("GetFilterObjectFromCookie. cookie.Value={0}", anchor));
                try
                {
                    var cookieJson = Encoding.UTF8.GetString(Convert.FromBase64String(HttpUtility.UrlDecode(anchor)));
                    _log.Debug(String.Format("GetFilterObjectFromCookie. cookieJson={0}", cookieJson));
                    var jsonArray = cookieJson.Split(';');

                    foreach (var filterItem in jsonArray)
                    {
                        var filterObj = JObject.Parse(filterItem);

                        var filterParam = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(filterObj.Value<string>("params"))));

                        switch (filterObj.Value<string>("id"))
                        {
                            case "sorter":
                                result.SortBy = filterParam.Value<string>("id");
                                result.SortOrder = filterParam.Value<string>("sortOrder");
                                break;
                            case "text":
                                result.FilterValue = filterParam.Value<string>("value");
                                break;

                            case "my":
                            case "responsibleID":
                                result.ResponsibleID = new Guid(filterParam.Value<string>("value"));
                                break;

                            case "overdue":
                            case "today":
                            case "theNext":
                                var valueString = filterParam.Value<string>("value");
                                var fromToArray = JsonConvert.DeserializeObject<List<string>>(valueString);
                                if (fromToArray.Count != 2) continue;
                                result.FromDate = !String.IsNullOrEmpty(fromToArray[0])
                                                      ? UrlParameters.ApiDateTimeParse(fromToArray[0]) : DateTime.MinValue;
                                result.ToDate = !String.IsNullOrEmpty(fromToArray[1])
                                                    ? UrlParameters.ApiDateTimeParse(fromToArray[1]) : DateTime.MinValue;
                                break;
                            case "fromToDate":
                                result.FromDate = filterParam.Value<DateTime>("from");
                                result.ToDate = filterParam.Value<DateTime>("to");
                                break;
                            case "categoryID":
                                result.CategoryID = filterParam.Value<int>("value");
                                break;
                            case "openTask":
                            case "closedTask":
                                result.IsClosed = filterParam.Value<bool>("value");
                                break;
                            case "contactID":
                                result.ContactID = filterParam.Value<int>("id");
                                break;
                        }
                    }
                }
                catch(Exception)
                {
                    _log.Info("GetFilterObjectFromCookie. Exception! line 206");
                    result.SortBy = "deadline";
                    result.SortOrder = "ascending";
                }
            }
            else
            {
                _log.Info("GetFilterObjectFromCookie. Cookie is null, default filters should be used");
                result.SortBy = "deadline";
                result.SortOrder = "ascending";
            }

            return result;
        }

        protected List<Task> GetTasksByFilter()
        {
            var filterObj = GetFilterObjectFromCookie();

            TaskSortedByType sortBy;
            if (!Classes.EnumExtension.TryParse(filterObj.SortBy, true, out sortBy))
            {
                sortBy = TaskSortedByType.DeadLine;
            }

            var isAsc = !String.IsNullOrEmpty(filterObj.SortOrder) && filterObj.SortOrder != "descending";

            return Global.DaoFactory.GetTaskDao().GetTasks(
                filterObj.FilterValue,
                filterObj.ResponsibleID,
                filterObj.CategoryID,
                filterObj.IsClosed,
                filterObj.FromDate,
                filterObj.ToDate,
                filterObj.ContactID > 0 ? EntityType.Contact : EntityType.Any,
                filterObj.ContactID,
                0, 0,
                new OrderBy(sortBy, isAsc));
        }

        #endregion
    }
}