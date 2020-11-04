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


using System;
using System.Text;
using System.Web.UI;
using ASC.Web.Studio.Utility;
using System.Web;
using Resources;

namespace ASC.Web.Studio.UserControls.Statistics
{
    [ManagementControl(ManagementType.Statistic, Location)]
    public partial class VisitorsChart : UserControl
    {
        public const string Location = "~/UserControls/Statistics/VisitorsChart/VisitorsChart.ascx";

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterStyle("~/UserControls/Statistics/VisitorsChart/css/visitorschart_style.less")
                .RegisterBodyScripts("~/UserControls/Statistics/VisitorsChart/js/excanvas.min.js",
                "~/UserControls/Statistics/VisitorsChart/js/jquery.flot.js",
                "~/UserControls/Statistics/VisitorsChart/js/tooltip.js",
                "~/UserControls/Statistics/VisitorsChart/js/visitorschart.js");

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
        }
    }
}