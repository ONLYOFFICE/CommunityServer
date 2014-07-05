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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core.Common.Logging;
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
            var sb = new StringBuilder();
            sb.Append("<select id=\"studio_lng\" class=\"comboBox\">");
            foreach (var ci in SetupInfo.EnabledCultures)
            {
                sb.AppendFormat("<option " + (String.Equals(_currentTenant.GetCulture().Name, ci.Name) ? "selected" : "") + " value=\"{0}\">{1}</option>", ci.Name, ci.DisplayName);
            }
            sb.Append("</select>");

            return sb.ToString();
        }

        protected string RenderTimeZoneSelector()
        {
            var sb = new StringBuilder("<select id=\"studio_timezone\" class=\"comboBox\">");
            foreach (var timeZone in TimeZoneInfo.GetSystemTimeZones().OrderBy(z => z.BaseUtcOffset))
            {
                sb.AppendFormat("<option " + (timeZone.Equals(_currentTenant.TimeZone) ? "selected" : string.Empty) + " value=\"{0}\">{1}</option>", timeZone.Id, timeZone.DisplayName);
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
                tenant.TimeZone = new List<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones()).Find(tz => String.Equals(tz.Id, timeZoneID));

                CoreContext.TenantManager.SaveTenant(tenant);

                if (!tenant.TimeZone.Id.Equals(oldTimeZone.Id) || changelng)
                {
                    AdminLog.PostAction("Settings: saved language and time zone settings with parameters language={0},time={1}", lng, timeZoneID);

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
                           ? new {Status = 1, Message = String.Empty}
                           : new {Status = 2, Message = Resource.SuccessfullySaveSettingsMessage};
            }
            catch(Exception e)
            {
                return new {Status = 0, Message = e.Message.HtmlEncode()};
            }
        }
    }
}