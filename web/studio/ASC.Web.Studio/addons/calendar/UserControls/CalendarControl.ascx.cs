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
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core;
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
            Page
                .RegisterStyle("~/addons/calendar/app_themes/<theme_folder>/calendar.less")
                .RegisterStyle("~/addons/calendar/usercontrols/popup/css/popup.css",
                    "~/addons/calendar/usercontrols/fullcalendar/css/asc-dialog/jquery-ui-1.8.14.custom.css",
                    "~/addons/calendar/usercontrols/fullcalendar/css/asc-datepicker/jquery-ui-1.8.14.custom.css",
                    "~/addons/calendar/usercontrols/css/jquery.jscrollpane.css")
                .RegisterBodyScripts("~/js/uploader/ajaxupload.js",
                    "~/addons/calendar/usercontrols/popup/popup.js",
                    "~/addons/calendar/usercontrols/js/calendar_controller.js",
                    "~/addons/calendar/usercontrols/js/recurrence_rule.js",
                    "~/addons/calendar/usercontrols/js/calendar_event_page.js",
                    "~/addons/calendar/usercontrols/js/jquery.jscrollpane.min.js",
                    "~/addons/calendar/usercontrols/js/jquery.mousewheel.js",
                    "~/addons/calendar/usercontrols/js/jquery.cookie.js",
                    "~/addons/calendar/usercontrols/fullcalendar/fullcalendar.js");


            Page.ClientScript.RegisterClientScriptBlock(GetType(), "calendar_full_screen",
                                                        @"<style type=""text/css"">
                    #studioPageContent{padding-bottom:0px;}
                    #studio_sidePanelUpHeight20{display:none;}
                    </style>", false);

            var script = new StringBuilder();
            script.AppendFormat("ASC.CalendarController.init([{0}], '{1}');", RenderTimeZones(), VirtualPathUtility.ToAbsolute("~/addons/calendar/usercontrols/fullcalendar/tmpl/notifications.editor.tmpl"));

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