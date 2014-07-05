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
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.UserControls.Common;

namespace ASC.Web.Calendar.UserControls
{
    public partial class CalendarControl : UserControl
    {
        public static string Location
        {
            get { return "~/addons/calendar/usercontrols/calendarcontrol.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitScripts();

            _sharingContainer.Controls.Add(LoadControl(SharingSettings.Location));
        }

        private void InitScripts()
        {
            Page.RegisterStyleControl("~/addons/calendar/app_themes/<theme_folder>/calendar.less", true);
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/addons/calendar/usercontrols/popup/css/popup.css"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/addons/calendar/usercontrols/fullcalendar/css/asc-dialog/jquery-ui-1.8.14.custom.css"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/addons/calendar/usercontrols/fullcalendar/css/asc-datepicker/jquery-ui-1.8.14.custom.css"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/addons/calendar/usercontrols/css/jquery.jscrollpane.css"));

            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/js/uploader/ajaxupload.js"));
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/addons/calendar/usercontrols/popup/popup.js"));
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/addons/calendar/usercontrols/js/calendar_controller.js"));
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/addons/calendar/usercontrols/js/recurrence_rule.js"));
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/addons/calendar/usercontrols/js/jquery.jscrollpane.min.js"));
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/addons/calendar/usercontrols/js/jquery.mousewheel.js"));
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/addons/calendar/usercontrols/js/jquery.cookie.js"));
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/addons/calendar/usercontrols/js/jquery.jscrollpane.min.js"));
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/addons/calendar/usercontrols/fullcalendar/fullcalendar.js"));


            Page.ClientScript.RegisterClientScriptBlock(GetType(), "calendar_full_screen",
                                                        @"<style type=""text/css"">
                    #studioPageContent{padding-bottom:0px;}
                    #studio_sidePanelUpHeight20{display:none;}
                    </style>", false);

            var script = new StringBuilder();
            script.AppendFormat("ASC.CalendarController.init([{0}], '{1}', '{2}');", RenderTimeZones(), VirtualPathUtility.ToAbsolute("~/addons/calendar/usercontrols/fullcalendar/tmpl/notifications.editor.tmpl"), Studio.Core.SetupInfo.IsPersonal.ToString().ToLower());

            Page.RegisterInlineScript(script.ToString());
        }

        protected string RenderTimeZones()
        {
            var sb = new StringBuilder();
            var i = 0;
            foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
            {
                if (i > 0)
                    sb.Append(",");

                sb.AppendFormat("{{name:\"{0}\", id:\"{1}\", offset:{2}}}", tz.DisplayName, tz.Id, (int)tz.BaseUtcOffset.TotalMinutes);
                i++;
            }
            return sb.ToString();
        }
    }
}