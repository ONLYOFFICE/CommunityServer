/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;

using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Jabber;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using ASC.Web.Talk.Addon;
using ASC.Web.Talk.ClientScript;
using ASC.Web.Talk.Resources;

using AjaxPro;
using ASC.Common.Logging;

namespace ASC.Web.Talk
{
    [AjaxNamespace("JabberClient")]
    public partial class JabberClient : MainPage
    {
        private static string JabberResource
        {
            get { return "TMTalk"; }
        }

        private TalkConfiguration _cfg;

        private static String EscapeJsString(String s)
        {
            return s.Replace("'", "\\'");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["web.third-party-chat"]))
            {
                var thirdPartyChat = ConfigurationManager.AppSettings["web.third-party-chat-url"];
                if (Convert.ToBoolean(ConfigurationManager.AppSettings["web.third-party-chat"]) && !String.IsNullOrEmpty(thirdPartyChat))
                {
                    Response.Redirect(thirdPartyChat, false);
                }
                
            }
            _cfg = new TalkConfiguration();

            Utility.RegisterTypeForAjax(GetType());

            Master.DisabledSidePanel = true;
            Master.DisabledTopStudioPanel = true;

            Page
                .RegisterBodyScripts("~/addons/talk/js/gears.init.js",
                    "~/addons/talk/js/gears.init.js",
                    "~/addons/talk/js/iscroll.js",
                    "~/addons/talk/js/talk.customevents.js",
                    "~/js/third-party/jquery/jquery.notification.js",
                    "~/js/third-party/moment.min.js",
                    "~/js/third-party/moment-timezone.min.js",
                    "~/js/third-party/firebase.js",
                    "~/js/third-party/firebase-app.js",
                    "~/js/third-party/firebase-auth.js",
                    "~/js/third-party/firebase-database.js",
                    "~/js/third-party/firebase-messaging.js",
                    "~/addons/talk/js/talk.common.js",
                    "~/addons/talk/js/talk.navigationitem.js",
                    "~/addons/talk/js/talk.msmanager.js",
                    "~/addons/talk/js/talk.mucmanager.js",
                    "~/addons/talk/js/talk.roomsmanager.js",
                    "~/addons/talk/js/talk.contactsmanager.js",
                    "~/addons/talk/js/talk.messagesmanager.js",
                    "~/addons/talk/js/talk.connectiomanager.js",
                    "~/addons/talk/js/talk.default.js",
                    "~/addons/talk/js/talk.init.js")
                .RegisterStyle("~/addons/talk/css/default/talk.style.css");

            var virtPath = "~/addons/talk/css/default/talk.style." + CultureInfo.CurrentCulture.Name.ToLower() + ".css";
            if (File.Exists(Server.MapPath(virtPath)))
            {
                Page.RegisterStyle(virtPath);
            }
            Page.RegisterStyle("~/addons/talk/css/default/talk.text-overflow.css");


            switch (_cfg.RequestTransportType.ToLower())
            {
                case "flash":
                    Page.RegisterBodyScripts("~/addons/talk/js/jlib/plugins/strophe.flxhr.js",

                        "~/addons/talk/js/jlib/flxhr/checkplayer.js",
                        "~/addons/talk/js/jlib/flxhr/flensed.js",
                        "~/addons/talk/js/jlib/flxhr/flxhr.js",
                        "~/addons/talk/js/jlib/flxhr/swfobject.js",

                        "~/js/third-party/xregexp.js",

                        "~/addons/talk/js/jlib/strophe/base64.js",
                        "~/addons/talk/js/jlib/strophe/md5.js",
                        "~/addons/talk/js/jlib/strophe/core.js");

                    break;
                default:
                    Page.RegisterBodyScripts(
                        "~/addons/talk/js/jlib/strophe/base64.js",
                        "~/addons/talk/js/jlib/strophe/md5.js",
                        "~/addons/talk/js/jlib/strophe/core.js",
                        "~/js/third-party/xregexp.js",
                        "~/addons/talk/js/jlib/flxhr/swfobject.js");
                    break;
            }

            Master.AddClientScript(new TalkClientScript(), new TalkClientScriptLocalization());

            try
            {
                Page.Title = TalkResource.ProductName + " - " + CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).DisplayUserName(false);
            }
            catch (System.Security.SecurityException)
            {
                Page.Title = TalkResource.ProductName + " - " + HeaderStringHelper.GetPageTitle(TalkResource.DefaultContactTitle);
            }
            try
            {
                Page.RegisterInlineScript("ASC.TMTalk.notifications.initialiseFirebase(" + GetFirebaseConfig() + ");");
            }
            catch (Exception){}
            
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite), Core.Security.SecurityPassthrough]
        public string GetAuthToken()
        {
            try
            {
                return new JabberServiceClient().GetAuthToken(TenantProvider.CurrentTenantID);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Talk").Error(ex);
                return String.Empty;
            }
        }

        [AjaxMethod]
        public string GetSpaceUsage()
        {
            try
            {
                var spaceUsage = TalkSpaceUsageStatManager.GetSpaceUsage();
                return spaceUsage > 0 ? FileSizeComment.FilesSizeToString(spaceUsage) : String.Empty;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Talk").Error(ex);
                return String.Empty;
            }
        }

        [AjaxMethod]
        public string ClearSpaceUsage(TalkSpaceUsageStatManager.ClearType type)
        {
            try
            {
                TalkSpaceUsageStatManager.ClearSpaceUsage(type);
                return GetSpaceUsage();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Talk").Error(ex);
                return String.Empty;
            }
        }

        protected String GetBoshUri()
        {
            return _cfg.BoshUri;
        }

        protected String GetResourcePriority()
        {
            return _cfg.ResourcePriority;
        }

        protected String GetInactivity()
        {
            return _cfg.ClientInactivity;
        }

        protected String GetFileTransportType()
        {
            return _cfg.FileTransportType ?? String.Empty;
        }

        protected String GetRequestTransportType()
        {
            return _cfg.RequestTransportType ?? String.Empty;
        }

        protected String GetMassendState()
        {
            return _cfg.EnabledMassend.ToString().ToLower();
        }

        protected String GetConferenceState()
        {
            return _cfg.EnabledConferences.ToString().ToLower();
        }

        protected String GetMonthNames()
        {
            return String.Join(",", CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames);
        }
        protected String GetDayNames()
        {
            return String.Join(",", CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames);
        }
        protected String GetHistoryLength()
        {
            return _cfg.HistoryLength ?? String.Empty;
        }

        protected String GetValidSymbols()
        {
            return _cfg.ValidSymbols ?? String.Empty;
        }

        protected String GetFullDateFormat()
        {
            return TalkResource.FullDateFormat;
        }

        protected String GetShortDateFormat()
        {
            return TalkResource.ShortDateFormat;
        }

        protected String GetUserPhotoHandlerPath()
        {
            return VirtualPathUtility.ToAbsolute("~/addons/talk/userphoto.ashx");
        }

        protected String GetNotificationHandlerPath()
        {
            return VirtualPathUtility.ToAbsolute("~/addons/talk/notification.html");
        }

        protected string GetJidServerPartOfJid()
        {
            string tenantDomain = CoreContext.TenantManager.GetCurrentTenant().TenantDomain;
            if (_cfg.ReplaceDomain && tenantDomain != null && tenantDomain.EndsWith(_cfg.ReplaceToDomain))
            {
                int place = tenantDomain.LastIndexOf(_cfg.ReplaceToDomain);
                if (place >= 0)
                {
                    return tenantDomain.Remove(place, _cfg.ReplaceToDomain.Length).Insert(place, _cfg.ReplaceFromDomain);
                }
            }
            return tenantDomain;
        }

        protected String GetJabberAccount()
        {
            try
            {
                return EscapeJsString(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).UserName.ToLower()) + "@" +
                       GetJidServerPartOfJid() + "/" + JabberResource;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }
        private String GetFirebaseConfig()
        {
            string firebase_projectId = FireBase.Instance.ProjectId;
            firebase_projectId = firebase_projectId.Trim();
            var script = new StringBuilder();

            if (firebase_projectId != String.Empty)
            {
                script.AppendLine("{apiKey: '" + FireBase.Instance.ApiKey.Trim() + "',");
                script.AppendLine(" authDomain: '" + firebase_projectId + ".firebaseapp.com',");
                script.AppendLine(" databaseURL: 'https://" + firebase_projectId + ".firebaseapp.com',");
                script.AppendLine(" projectId: '" + firebase_projectId + "',");
                script.AppendLine(" storageBucket: '" + firebase_projectId + ".appspot.com',");
                script.AppendLine(" messagingSenderId: '" + FireBase.Instance.MessagingSenderId.Trim() + "'}");

                return script.ToString();
            }
            else
            {
                return null;
            }

        }
    }
}