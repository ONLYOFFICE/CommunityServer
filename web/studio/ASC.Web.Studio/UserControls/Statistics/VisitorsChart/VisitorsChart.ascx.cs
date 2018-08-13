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
    [ManagementControl(ManagementType.Statistic, Location)]
    [AjaxNamespace("VisitorsChart")]
    public partial class VisitorsChart : UserControl
    {
        public const string Location = "~/UserControls/Statistics/VisitorsChart/VisitorsChart.ascx";

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterStyle("~/usercontrols/statistics/visitorschart/css/visitorschart_style.less")
                .RegisterBodyScripts("~/usercontrols/statistics/visitorschart/js/excanvas.min.js",
                "~/usercontrols/statistics/visitorschart/js/jquery.flot.js",
                "~/usercontrols/statistics/visitorschart/js/tooltip.js",
                "~/usercontrols/statistics/visitorschart/js/visitorschart.js");

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

    public sealed class ChartPoint
    {
        public String DisplayDate { get; set; }
        public DateTime Date { get; set; }
        public Int32 Hosts { get; set; }
        public Int32 Hits { get; set; }
    }
}