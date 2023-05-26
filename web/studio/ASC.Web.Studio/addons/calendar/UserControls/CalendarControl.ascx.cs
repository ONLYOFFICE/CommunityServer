/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

using ASC.Web.Calendar.Controls;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.UserControls.Common;

namespace ASC.Web.Calendar.UserControls
{
    public partial class CalendarControl : UserControl
    {
        public static string Location
        {
            get { return "~/addons/calendar/UserControls/CalendarControl.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitScripts();

            _sharingContainer.Controls.Add(LoadControl(SharingSettings.Location));
            _phDocUploader.Controls.Add(LoadControl(DocumentsPopup.Location));
            _commonPopup.Options.IsPopup = true;
        }

        private void InitScripts()
        {
            if (ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
            {
                Page
                .RegisterStyle("~/addons/calendar/App_Themes/<theme_folder>/calendar.less")
                .RegisterStyle("~/addons/calendar/UserControls/popup/css/dark-popup.less",
                    "~/addons/calendar/UserControls/fullcalendar/css/asc-dialog/jquery-ui-1.8.14.custom.css",
                    "~/addons/calendar/UserControls/fullcalendar/css/asc-datepicker/jquery-ui-1.8.14.custom.css",
                    "~/addons/calendar/UserControls/css/jquery.jscrollpane.css",
                    "~/addons/calendar/UserControls/fullcalendar/css/dark-attachments.less");
            }
            else
            {
                Page
                .RegisterStyle("~/addons/calendar/App_Themes/<theme_folder>/calendar.less")
                .RegisterStyle("~/addons/calendar/UserControls/popup/css/popup.less",
                    "~/addons/calendar/UserControls/fullcalendar/css/asc-dialog/jquery-ui-1.8.14.custom.css",
                    "~/addons/calendar/UserControls/fullcalendar/css/asc-datepicker/jquery-ui-1.8.14.custom.css",
                    "~/addons/calendar/UserControls/css/jquery.jscrollpane.css",
                    "~/addons/calendar/UserControls/fullcalendar/css/attachments.less");
            }
            Page
                .RegisterBodyScripts("~/js/uploader/ajaxupload.js",
                    "~/js/uploader/jquery.fileupload.js",
                    "~/js/third-party/ical.js",
                    "~/js/third-party/moment.min.js",
                    "~/js/third-party/moment-timezone.min.js",
                    "~/js/third-party/rrule.js",
                    "~/js/third-party/nlp.js",
                    "~/js/uploader/jquery.fileuploadmanager.js",
                    "~/addons/calendar/UserControls/js/bluebird.min.js",
                    "~/addons/calendar/UserControls/popup/popup.js",
                    "~/addons/calendar/UserControls/js/calendar_controller.js",
                    "~/addons/calendar/UserControls/js/recurrence_rule.js",
                    "~/addons/calendar/UserControls/js/calendar_event_page.js",
                    "~/addons/calendar/UserControls/js/calendar.attachments.js",
                    "~/addons/calendar/UserControls/js/calendar.popupqueue.js",
                    "~/addons/calendar/UserControls/js/jquery.jscrollpane.min.js",
                    "~/addons/calendar/UserControls/js/jquery.mousewheel.js",
                    "~/addons/calendar/UserControls/js/jquery.cookie.js",
                    "~/addons/calendar/UserControls/fullcalendar/fullcalendar.js",
                    "~/Products/Files/js/common.js",
                    "~/Products/Files/js/ui.js",
                    "~/UserControls/Common/ckeditor/ckeditor-connector.js")
                .RegisterClientScript(new Files.Masters.ClientScripts.FilesConstantsResources());


            Page.ClientScript.RegisterClientScriptBlock(GetType(), "calendar_full_screen",
                                                        @"<style type=""text/css"">
                    #studioPageContent{padding-bottom:0px;}
                    #studio_sidePanelUpHeight20{display:none;}
                    </style>", false);

            var script = new StringBuilder();

            script.Append("ckeditorConnector.load(function(){console.log('ckeditor loaded')});");
            script.AppendFormat("ASC.CalendarController.init([{0}], '{1}');", RenderTimeZones(), VirtualPathUtility.ToAbsolute("~/addons/calendar/UserControls/fullcalendar/tmpl/notifications.editor.tmpl"));

            Page.RegisterInlineScript(script.ToString());
        }

        protected string RenderTimeZones()
        {
            return string.Join(",",
                               Studio.UserControls.Management.TimeAndLanguage.GetTimeZones()
                                     .Select(tz => string.Format("{{name:\"{0}\", id:\"{1}\", offset:{2}}}",
                                                                 Common.Utils.TimeZoneConverter.GetTimeZoneName(tz),
                                                                 tz.Id,
                                                                 (int)tz.GetOffset().TotalMinutes)));
        }
    }
}