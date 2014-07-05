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
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ASC.Web.CRM.Controls.Deals
{
    public partial class ListDealView : BaseUserControl
    {
        #region Properies

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Deals/ListDealView.ascx"); }
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
                var deals = GetDealsByFilter();

                if (UrlParameters.View != "editor")
                {
                    Response.Clear();
                    Response.ContentType = "text/csv; charset=utf-8";
                    Response.ContentEncoding = Encoding.UTF8;
                    Response.Charset = Encoding.UTF8.WebName;
                    const string fileName = "opportunity.csv";

                    Response.AppendHeader("Content-Disposition", String.Format("attachment; filename={0}", fileName));
                    Response.Write(ExportToCSV.ExportDealsToCSV(deals, false));

                    MessageService.Send(HttpContext.Current.Request, MessageAction.OpportunitiesExportedToCsv, deals.Select(x => x.Title));
                    
                    Response.End();
                }
                else
                {
                    var fileUrl = ExportToCSV.ExportDealsToCSV(deals, true);
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

            public Guid ResponsibleID { get; set; }

            public String StageType { get; set; }
            public int OpportunityStagesID { get; set; }

            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }

            public String FromDateString { get; set; }
            public String ToDateString { get; set; }

            public int ContactID { get; set; }
            public bool? ContactAlsoIsParticipant { get; set; }

            public List<string> Tags { get; set; }

            public FilterObject()
            {
                ContactAlsoIsParticipant = null;
                FromDate = DateTime.MinValue;
                ToDate = DateTime.MinValue;
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

                            case "stageTypeOpen":
                            case "stageTypeClosedAndWon":
                            case "stageTypeClosedAndLost":
                                result.StageType = filterParam.Value<string>("value");
                                break;
                            case "opportunityStagesID":
                                result.OpportunityStagesID = filterParam.Value<int>("value");
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
                                result.FromDate = UrlParameters.ApiDateTimeParse(result.FromDateString);
                                result.ToDate = UrlParameters.ApiDateTimeParse(result.ToDateString);
                                break;
                            case "fromToDate":
                                result.FromDateString = filterParam.Value<string>("from");
                                result.ToDateString = filterParam.Value<string>("to");
                                result.FromDate = UrlParameters.ApiDateTimeParse(result.FromDateString);
                                result.ToDate = UrlParameters.ApiDateTimeParse(result.ToDateString);
                                break;

                            case "participantID":
                                result.ContactID = filterParam.Value<int>("id");
                                result.ContactAlsoIsParticipant = true;
                                break;
                            case "contactID":
                                result.ContactID = filterParam.Value<int>("id");
                                result.ContactAlsoIsParticipant = false;
                                break;

                            case "tags":
                                result.Tags = new List<string>();
                                result.Tags = filterParam.Value<JArray>("value").ToList().ConvertAll(n => n.ToString());
                                break;
                        }
                    }
                }
                catch(Exception)
                {
                    result.SortBy = "stage";
                    result.SortOrder = "ascending";
                }
            }
            else
            {
                result.SortBy = "stage";
                result.SortOrder = "ascending";
            }

            return result;
        }

        protected List<Deal> GetDealsByFilter()
        {
            var filterObj = GetFilterObjectFromCookie();

            DealSortedByType sortBy;
            if (!Classes.EnumExtension.TryParse(filterObj.SortBy, true, out sortBy))
            {
                sortBy = DealSortedByType.Title;
            }

            var isAsc = !String.IsNullOrEmpty(filterObj.SortOrder) && filterObj.SortOrder != "descending";

            DealMilestoneStatus? stageType = null;
            if (!String.IsNullOrEmpty(filterObj.StageType))
            {
                if (filterObj.StageType.ToLower() == DealMilestoneStatus.Open.ToString().ToLower())
                    stageType = DealMilestoneStatus.Open;
                if (filterObj.StageType.ToLower() == DealMilestoneStatus.ClosedAndLost.ToString().ToLower())
                    stageType = DealMilestoneStatus.ClosedAndLost;
                if (filterObj.StageType.ToLower() == DealMilestoneStatus.ClosedAndWon.ToString().ToLower())
                    stageType = DealMilestoneStatus.ClosedAndWon;
            }

            return Global.DaoFactory.GetDealDao().GetDeals(
                filterObj.FilterValue,
                filterObj.ResponsibleID,
                filterObj.OpportunityStagesID,
                filterObj.Tags,
                filterObj.ContactID,
                stageType,
                filterObj.ContactAlsoIsParticipant,
                filterObj.FromDate,
                filterObj.ToDate,
                0, 0,
                new OrderBy(sortBy, isAsc));
        }

        #endregion
    }
}