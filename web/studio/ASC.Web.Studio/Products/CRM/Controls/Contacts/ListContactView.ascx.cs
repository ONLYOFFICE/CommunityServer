/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ASC.Web.CRM.Controls.Contacts
{
    public partial class ListContactView : BaseUserControl
    {
        #region Properies

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Contacts/ListContactView.ascx"); }
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (UrlParameters.Action != "export")
            {
                InitPage();
            }
            else
            {
                var contacts = GetContactsByFilter();

                if (UrlParameters.View != "editor")
                {
                    Response.Clear();
                    Response.ContentType = "text/csv; charset=utf-8";
                    Response.ContentEncoding = Encoding.UTF8;
                    Response.Charset = Encoding.UTF8.WebName;
                    const string fileName = "contacts.csv";

                    Response.AppendHeader("Content-Disposition", String.Format("attachment; filename={0}", fileName));
                    Response.Write(ExportToCSV.ExportContactsToCSV(contacts, false));

                    MessageService.Send(HttpContext.Current.Request, MessageAction.ContactsExportedToCsv, MessageTarget.Create(contacts.Select(x => x.ID)), contacts.Select(x => x.GetTitle()));

                    Response.End();
                }
                else
                {
                    var fileUrl = ExportToCSV.ExportContactsToCSV(contacts, true);
                    Response.Redirect(fileUrl);
                }
            }
        }

        #endregion

        #region Methods

        private void InitPage()
        {
            _phListBase.Controls.Add(LoadControl(ListBaseView.Location));
        }

        private class FilterObject
        {
            public string SortBy { get; set; }
            public string SortOrder { get; set; }
            public string FilterValue { get; set; }
            public List<string> Tags { get; set; }
            public string ContactListView { get; set; }
            public int ContactStage { get; set; }
            public int ContactType { get; set; }
            public Guid? ResponsibleID { get; set; }
            public bool? IsShared { get; set; }

            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }

            public String FromDateString { get; set; }
            public String ToDateString { get; set; }

            public FilterObject()
            {
                FromDate = DateTime.MinValue;
                ToDate = DateTime.MinValue;
                ResponsibleID = null;
                ContactStage = -1;
                ContactType = -1;
            }
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

                        JObject filterParam = Global.JObjectParseWithDateAsString(Encoding.UTF8.GetString(Convert.FromBase64String(filterObj.Value<string>("params"))));

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
                            case "noresponsible":
                                result.ResponsibleID = new Guid(filterParam.Value<string>("value"));
                                break;
                            case "tags":
                                result.Tags = filterParam.Value<JArray>("value").ToList().ConvertAll(n => n.ToString());
                                break;
                            case "withopportunity":
                            case "person":
                            case "company":
                                result.ContactListView = filterParam.Value<string>("value");
                                break;
                            case "contactType":
                                result.ContactType = filterParam.Value<int>("value");
                                break;
                            case "contactStage":
                                result.ContactStage = filterParam.Value<int>("value");
                                break;

                            case "lastMonth":
                            case "yesterday":
                            case "today":
                            case "thisMonth":
                                var valueString = filterParam.Value<string>("value");
                                var fromToArray = JsonConvert.DeserializeObject<List<string>>(valueString);
                                if (fromToArray.Count != 2) continue;
                                result.FromDateString = fromToArray[0];
                                result.ToDateString = fromToArray[1];
                                result.FromDate = Global.ApiDateTimeParse(result.FromDateString);
                                result.ToDate = Global.ApiDateTimeParse(result.ToDateString);
                                break;
                            case "fromToDate":
                                result.FromDateString = filterParam.Value<string>("from");
                                result.ToDateString = filterParam.Value<string>("to");
                                result.FromDate = Global.ApiDateTimeParse(result.FromDateString);
                                result.ToDate = Global.ApiDateTimeParse(result.ToDateString);
                                break;
                            case "restricted":
                            case "shared":
                                result.IsShared = filterParam.Value<bool>("value");
                                break;
                        }
                    }
                }
                catch(Exception)
                {
                    result.SortBy = "created";
                    result.SortOrder = "descending";
                }
            }
            else
            {
                result.SortBy = "created";
                result.SortOrder = "descending";
            }

            return result;
        }

        protected List<Contact> GetContactsByFilter()
        {
            var filterObj = GetFilterObjectFromCookie();

            var contactListViewType = ContactListViewType.All;
            if (!String.IsNullOrEmpty(filterObj.ContactListView))
            {
                if (filterObj.ContactListView == ContactListViewType.Company.ToString().ToLower())
                    contactListViewType = ContactListViewType.Company;
                if (filterObj.ContactListView == ContactListViewType.Person.ToString().ToLower())
                    contactListViewType = ContactListViewType.Person;
                if (filterObj.ContactListView == ContactListViewType.WithOpportunity.ToString().ToLower())
                    contactListViewType = ContactListViewType.WithOpportunity;
            }

            ContactSortedByType sortBy;

            if (!Classes.EnumExtension.TryParse(filterObj.SortBy, true, out sortBy))
            {
                sortBy = ContactSortedByType.Created;
            }

            var isAsc = !String.IsNullOrEmpty(filterObj.SortOrder) && filterObj.SortOrder != "descending";

            return DaoFactory.ContactDao.GetContacts(
                filterObj.FilterValue,
                filterObj.Tags,
                filterObj.ContactStage,
                filterObj.ContactType,
                contactListViewType,
                filterObj.FromDate,
                filterObj.ToDate,
                0,
                0,
                new OrderBy(sortBy, isAsc),
                filterObj.ResponsibleID,
                filterObj.IsShared);
        }

        #endregion
    }
}