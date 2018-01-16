/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ASC.Web.CRM.Controls.Cases
{
    public partial class ListCasesView : BaseUserControl
    {
        #region Properies

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Cases/ListCasesView.ascx"); }
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (UrlParameters.Action != "export")
            {
                _phListBase.Controls.Add(LoadControl(ListBaseView.Location));
            }
            else
            {
                var cases = GetCasesByFilter();

                if (UrlParameters.View != "editor")
                {
                    Response.Clear();
                    Response.ContentType = "text/csv; charset=utf-8";
                    Response.ContentEncoding = Encoding.UTF8;
                    Response.Charset = Encoding.UTF8.WebName;

                    const string fileName = "cases.csv";

                    Response.AppendHeader("Content-Disposition", String.Format("attachment; filename={0}", fileName));
                    Response.Write(ExportToCSV.ExportCasesToCSV(cases, false));

                    MessageService.Send(HttpContext.Current.Request, MessageAction.CasesExportedToCsv, MessageTarget.Create(cases.Select(x => x.ID)), cases.Select(x => x.Title));

                    Response.End();
                }
                else
                {
                    var fileUrl = ExportToCSV.ExportCasesToCSV(cases, true);
                    Response.Redirect(fileUrl);
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
            public bool? IsClosed { get; set; }
            public List<string> Tags { get; set; }
        };

        private FilterObject GetFilterObjectFromCookie()
        {
            var result = new FilterObject();

            var cookieKey = GetCookieKeyForFilterForExport();
            var cookie = Request.Cookies[HttpUtility.UrlEncode(cookieKey)];

            if (cookie != null && !String.IsNullOrEmpty(cookie.Value))
            {
                var anchor = cookie.Value;

                try
                {
                    var cookieJson = Encoding.UTF8.GetString(Convert.FromBase64String(HttpUtility.UrlDecode(anchor)));

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
                            case "closed":
                            case "opened":
                                result.IsClosed = filterParam.Value<bool>("value");
                                break;
                            case "tags":

                                result.Tags = filterParam.Value<JArray>("value").ToList().ConvertAll(
                                    n => n.ToString());
                                break;
                        }
                    }
                }
                catch(Exception)
                {
                    result.SortBy = "title";
                    result.SortOrder = "ascending";
                }
            }
            else
            {
                result.SortBy = "title";
                result.SortOrder = "ascending";
            }
            return result;
        }

        protected List<ASC.CRM.Core.Entities.Cases> GetCasesByFilter()
        {
            var filterObj = GetFilterObjectFromCookie();

            SortedByType sortBy;
            if (!Classes.EnumExtension.TryParse(filterObj.SortBy, true, out sortBy))
            {
                sortBy = SortedByType.Title;
            }

            var isAsc = !String.IsNullOrEmpty(filterObj.SortOrder) && filterObj.SortOrder != "descending";

            return Global.DaoFactory.GetCasesDao().GetCases(
                filterObj.FilterValue,
                0,
                filterObj.IsClosed,
                filterObj.Tags,
                0, 0,
                new OrderBy(sortBy, isAsc));
        }

        #endregion
    }
}