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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Utility;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Studio.Core;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    public partial class TimeAndLanguage : UserControl
    {
        public bool WithoutButton;

        public static string Location
        {
            get { return "~/UserControls/Management/TimeAndLanguage/TimeAndLanguage.ascx"; }
        }

        protected Tenant _currentTenant;

        protected string HelpLink { get; set; }

        protected bool ShowHelper { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/UserControls/Management/TimeAndLanguage/js/timeandlanguage.js")
                .RegisterStyle("~/UserControls/Management/TimeAndLanguage/css/timeandlanguage.less");

            _currentTenant = CoreContext.TenantManager.GetCurrentTenant();

            HelpLink = CommonLinkUtility.GetHelpLink();

            ShowHelper = !(CoreContext.Configuration.Standalone && !CompanyWhiteLabelSettings.Instance.IsDefault);
        }

        protected string RenderLanguageSelector()
        {
            var currentCulture = _currentTenant.GetCulture().Name;
            if (SetupInfo.EnabledCultures.All(r => r.Name != currentCulture))
            {
                currentCulture = "en-US";
            }
            var sb = new StringBuilder();
            sb.AppendFormat("<select id=\"studio_lng\" class=\"comboBox\" data-default=\"{0}\">", currentCulture);
            foreach (var ci in SetupInfo.EnabledCultures)
            {
                var displayName = CoreContext.Configuration.CustomMode ? ci.NativeName : ci.DisplayName;
                sb.AppendFormat("<option " + (String.Equals(currentCulture, ci.Name) ? "selected" : "") + " value=\"{0}\">{1}</option>", ci.Name, displayName);
            }

            if (ShowHelper)
                sb.AppendFormat("<option value=\"\">{0}</option>", Resource.MoreLanguages);

            sb.Append("</select>");

            return sb.ToString();
        }

        protected string RenderTimeZoneSelector()
        {
            var sb = new StringBuilder("<select id='studio_timezone' class='comboBox'>");
            foreach (var tz in GetTimeZones().OrderBy(z => z.BaseUtcOffset))
            {
                var displayName = ASC.Common.Utils.TimeZoneConverter.GetTimeZoneName(tz);
                if (!displayName.StartsWith("(UTC") && !displayName.StartsWith("UTC"))
                {
                    if (tz.BaseUtcOffset != TimeSpan.Zero)
                    {
                        displayName = string.Format("(UTC{0}{1}) ", tz.BaseUtcOffset < TimeSpan.Zero ? "-" : "+", tz.BaseUtcOffset.ToString(@"hh\:mm")) + displayName;
                    }
                    else
                    {
                        displayName = "(UTC) " + displayName;
                    }
                }

                sb.AppendFormat("<option {0}value=\"{1}\">{2}</option>", 
                    _currentTenant.TimeZone != null && tz.Id.Equals(_currentTenant.TimeZone.Id) ? "selected " : string.Empty, 
                    tz.Id, 
                    displayName);
            }
            sb.Append("</select>");
            return sb.ToString();
        }


        public static IEnumerable<TimeZoneInfo> GetTimeZones()
        {
            //hack for MONO. In NodaTime.dll method FromTimeZoneInfo throw exception for Europe/Vatican, Europe/San_Marino, Europe/Rome, Europe/Malta
            var timeZones = WorkContext.IsMono
                                ? TimeZoneInfo.GetSystemTimeZones()
                                              .Where(
                                                  tz =>
                                                  tz.Id != "Europe/Vatican" &&
                                                  tz.Id != "Europe/San_Marino" &&
                                                  tz.Id != "Europe/Rome" &&
                                                  tz.Id != "Europe/Malta")
                                              .ToList()
                                : TimeZoneInfo.GetSystemTimeZones().ToList();

            if (timeZones.All(tz => tz.Id != "UTC"))
            {
                timeZones.Add(TimeZoneInfo.Utc);
            }

            return timeZones;
        }
    }
}