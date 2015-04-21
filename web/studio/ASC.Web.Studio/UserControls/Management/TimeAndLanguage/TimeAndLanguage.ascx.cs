/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.MessagingSystem;
using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Studio.Core;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("TimeAndLanguageSettingsController")]
    public partial class TimeAndLanguage : UserControl
    {
        public bool WithoutButton;

        public static string Location
        {
            get { return "~/UserControls/Management/TimeAndLanguage/TimeAndLanguage.ascx"; }
        }

        protected Tenant _currentTenant;

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/TimeAndLanguage/js/TimeAndLanguage.js"));

            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/TimeAndLanguage/css/TimeAndLanguage.less"));

            _currentTenant = CoreContext.TenantManager.GetCurrentTenant();
        }

        protected string RenderLanguageSelector()
        {
            var currentCulture = _currentTenant.GetCulture().Name;
            if (SetupInfo.EnabledCultures.All(r => r.Name != currentCulture))
            {
                currentCulture = "en-US";
            }
            var sb = new StringBuilder();
            sb.Append("<select id=\"studio_lng\" class=\"comboBox\">");
            foreach (var ci in SetupInfo.EnabledCultures)
            {
                sb.AppendFormat("<option " + (String.Equals(currentCulture, ci.Name) ? "selected" : "") + " value=\"{0}\">{1}</option>", ci.Name, ci.DisplayName);
            }
            sb.Append("</select>");

            return sb.ToString();
        }

        protected string RenderTimeZoneSelector()
        {
            var sb = new StringBuilder("<select id='studio_timezone' class='comboBox'>");
            foreach (var tz in GetTimeZones().OrderBy(z => z.BaseUtcOffset))
            {
                var displayName = tz.DisplayName;
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

                sb.AppendFormat("<option {0}value=\"{1}\">{2}</option>", tz.Equals(_currentTenant.TimeZone) ? "selected " : string.Empty, tz.Id, displayName);
            }
            sb.Append("</select>");
            return sb.ToString();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveLanguageTimeSettings(string lng, string timeZoneID)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                var culture = CultureInfo.GetCultureInfo(lng);

                var changelng = false;
                if (SetupInfo.EnabledCultures.Find(c => String.Equals(c.Name, culture.Name, StringComparison.InvariantCultureIgnoreCase)) != null)
                {
                    if (!String.Equals(tenant.Language, culture.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        tenant.Language = culture.Name;
                        changelng = true;
                    }
                }

                var oldTimeZone = tenant.TimeZone;
                tenant.TimeZone = GetTimeZones().FirstOrDefault(tz => tz.Id == timeZoneID) ?? TimeZoneInfo.Utc;

                CoreContext.TenantManager.SaveTenant(tenant);

                if (!tenant.TimeZone.Id.Equals(oldTimeZone.Id) || changelng)
                {
                    if (!tenant.TimeZone.Id.Equals(oldTimeZone.Id))
                    {
                        MessageService.Send(HttpContext.Current.Request, MessageAction.TimeZoneSettingsUpdated);
                    }
                    if (changelng)
                    {
                        MessageService.Send(HttpContext.Current.Request, MessageAction.LanguageSettingsUpdated);
                    }
                }

                return changelng
                           ? new { Status = 1, Message = String.Empty }
                           : new { Status = 2, Message = Resource.SuccessfullySaveSettingsMessage };
            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message.HtmlEncode() };
            }
        }


        private IEnumerable<TimeZoneInfo> GetTimeZones()
        {
            var timeZones = TimeZoneInfo.GetSystemTimeZones().ToList();
            if (!timeZones.Any(tz => tz.Id == "UTC"))
            {
                timeZones.Add(TimeZoneInfo.Utc);
            }
            return timeZones;
        }
    }
}