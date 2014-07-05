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
using System.Text;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Statistic;
using ASC.Web.Studio.Utility;
using AjaxPro;
using System.Web;
using Resources;

namespace ASC.Web.Studio.UserControls.Statistics
{
    public sealed class ChartPoint
    {
        public String DisplayDate { get; set; }
        public DateTime Date { get; set; }
        public Int32 Hosts { get; set; }
        public Int32 Hits { get; set; }
    }

    [ManagementControl(ManagementType.Statistic, Location)]
    [AjaxNamespace("VisitorsChart")]
    public partial class VisitorsChart : UserControl
    {
        public const string Location = "~/UserControls/Statistics/VisitorsChart/VisitorsChart.ascx";

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/statistics/visitorschart/css/visitorschart_style.less"));

            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/statistics/visitorschart/js/excanvas.min.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/statistics/visitorschart/js/jquery.flot.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/statistics/visitorschart/js/jquery.flot.resize.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/statistics/visitorschart/js/tooltip.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/statistics/visitorschart/js/visitorschart.js"));

            var jsResources = new StringBuilder();
            jsResources.Append("jq(document).ready(function(){");
            jsResources.Append("if(typeof window.ASC==='undefined')window.ASC={};");
            jsResources.Append("if(typeof window.ASC.Resources==='undefined')window.ASC.Resources={};");
            jsResources.Append("window.ASC.Resources.chartDateFormat='" + Resource.ChartDateFormat + "';");
            jsResources.Append("window.ASC.Resources.chartMonthNames='" + Resource.ChartMonthNames + "';");
            jsResources.Append("window.ASC.Resources.hitLabel='" + Resource.VisitorsChartHitLabel + "';");
            jsResources.Append("window.ASC.Resources.hostLabel='" + Resource.VisitorsChartHostLabel + "';");
            jsResources.Append("window.ASC.Resources.visitsLabel='" + Resource.VisitorsChartVisitsLabel + "';");
            jsResources.Append("});");

            Page.RegisterInlineScript(jsResources.ToString());

            jQueryDateMask.Value = DateTimeExtension.DateMaskForJQuery;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public List<ChartPoint> GetVisitStatistics(DateTime from, DateTime to)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var points = new List<ChartPoint>();
            if (from.CompareTo(to) >= 0) return points;
            for (var d = new DateTime(from.Ticks); d.Date.CompareTo(to.Date) <= 0; d = d.AddDays(1))
            {
                points.Add(new ChartPoint
                    {
                        Hosts = 0,
                        Hits = 0,
                        Date = TenantUtil.DateTimeFromUtc(d.Date),
                        DisplayDate = TenantUtil.DateTimeFromUtc(d.Date).ToShortDateString()
                    });
            }
            var hits = StatisticManager.GetHitsByPeriod(TenantProvider.CurrentTenantID, from, to);
            var hosts = StatisticManager.GetHostsByPeriod(TenantProvider.CurrentTenantID, from, to);
            if (hits.Count == 0 || hosts.Count == 0) return points;

            hits.Sort((x, y) => x.VisitDate.CompareTo(y.VisitDate));
            hosts.Sort((x, y) => x.VisitDate.CompareTo(y.VisitDate));

            for (int i = 0, n = points.Count, hitsNum = 0, hostsNum = 0; i < n; i++)
            {
                while (hitsNum < hits.Count && points[i].Date.Date.CompareTo(hits[hitsNum].VisitDate.Date) == 0)
                {
                    points[i].Hits += hits[hitsNum].VisitCount;
                    hitsNum++;
                }
                while (hostsNum < hosts.Count && points[i].Date.Date.CompareTo(hosts[hostsNum].VisitDate.Date) == 0)
                {
                    points[i].Hosts++;
                    hostsNum++;
                }
            }

            return points;
        }
    }
}