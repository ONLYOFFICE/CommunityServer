/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Web.Core.Jabber;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using ASC.Web.Talk.Addon;
using ASC.Web.Talk.Resources;
using AjaxPro;
using log4net;
using System.IO;

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
            _cfg = new TalkConfiguration();

            Utility.RegisterTypeForAjax(GetType());

            Master.DisabledSidePanel = true;
            Master.DisabledTopStudioPanel = true;
            Page.RegisterBodyScripts("~/addons/talk/js/gears.init.js");
            Page.RegisterBodyScripts("~/addons/talk/js/gears.init.js");
            Page.RegisterBodyScripts("~/addons/talk/js/iscroll.js");
            Page.RegisterBodyScripts("~/addons/talk/js/talk.customevents.js");
            Page.RegisterBodyScripts("~/js/third-party/jquery/jquery.notification.js");
            Page.RegisterBodyScripts("~/addons/talk/js/talk.common.js");
            Page.RegisterBodyScripts("~/addons/talk/js/talk.navigationitem.js");
            Page.RegisterBodyScripts("~/addons/talk/js/talk.msmanager.js");
            Page.RegisterBodyScripts("~/addons/talk/js/talk.mucmanager.js");
            Page.RegisterBodyScripts("~/addons/talk/js/talk.roomsmanager.js");
            Page.RegisterBodyScripts("~/addons/talk/js/talk.contactsmanager.js");
            Page.RegisterBodyScripts("~/addons/talk/js/talk.messagesmanager.js");
            Page.RegisterBodyScripts("~/addons/talk/js/talk.connectiomanager.js");
            Page.RegisterBodyScripts("~/addons/talk/js/talk.default.js");

            Page.RegisterStyle("~/addons/talk/css/default/talk.style.css");
            var virtPath = "~/addons/talk/css/default/talk.style." + CultureInfo.CurrentCulture.Name.ToLower() + ".css";
            if (File.Exists(Server.MapPath(virtPath)))
            {
                Page.RegisterStyle(virtPath);
            }
            Page.RegisterStyle("~/addons/talk/css/default/talk.text-overflow.css");


            switch (_cfg.RequestTransportType.ToLower())
            {
                case "flash":
                    Page.RegisterBodyScripts("~/addons/talk/js/jlib/plugins/strophe.flxhr.js");

                    Page.RegisterBodyScripts("~/addons/talk/js/jlib/flxhr/checkplayer.js");
                    Page.RegisterBodyScripts("~/addons/talk/js/jlib/flxhr/flensed.js");
                    Page.RegisterBodyScripts("~/addons/talk/js/jlib/flxhr/flxhr.js");
                    Page.RegisterBodyScripts("~/addons/talk/js/jlib/flxhr/swfobject.js");

                    Page.RegisterBodyScripts("~/addons/talk/js/jlib/strophe/base64.js");
                    Page.RegisterBodyScripts("~/addons/talk/js/jlib/strophe/md5.js");
                    Page.RegisterBodyScripts("~/addons/talk/js/jlib/strophe/core.js");

                    break;
                default:
                    Page.RegisterBodyScripts("~/addons/talk/js/jlib/strophe/base64.js");
                    Page.RegisterBodyScripts("~/addons/talk/js/jlib/strophe/md5.js");
                    Page.RegisterBodyScripts("~/addons/talk/js/jlib/strophe/core.js");

                    Page.RegisterBodyScripts("~/addons/talk/js/jlib/flxhr/swfobject.js");
                    break;
            }

            var jsResources = new StringBuilder();
            jsResources.Append("window.ASC=window.ASC||{};");
            jsResources.Append("window.ASC.TMTalk=window.ASC.TMTalk||{};");
            jsResources.Append("window.ASC.TMTalk.Resources={};");
            jsResources.Append("window.ASC.TMTalk.Resources.statusTitles={}" + ';');
            jsResources.Append("window.ASC.TMTalk.Resources.statusTitles.offline='" + EscapeJsString(TalkResource.StatusOffline) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.statusTitles.online='" + EscapeJsString(TalkResource.StatusOnline) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.statusTitles.away='" + EscapeJsString(TalkResource.StatusAway) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.statusTitles.xa='" + EscapeJsString(TalkResource.StatusNA) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.addonIcon='" + EscapeJsString(WebImageSupplier.GetAbsoluteWebPath("product_logo.png", TalkAddon.AddonID)) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.addonIcon16='" + EscapeJsString(WebImageSupplier.GetAbsoluteWebPath("talk16.png", TalkAddon.AddonID)) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.addonIcon32='" + EscapeJsString(WebImageSupplier.GetAbsoluteWebPath("talk32.png", TalkAddon.AddonID)) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.addonIcon48='" + EscapeJsString(WebImageSupplier.GetAbsoluteWebPath("talk48.png", TalkAddon.AddonID)) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.addonIcon128='" + EscapeJsString(WebImageSupplier.GetAbsoluteWebPath("talk128.png", TalkAddon.AddonID)) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.iconNewMessage='" + EscapeJsString(WebImageSupplier.GetAbsoluteWebPath("icon-new-message.ico", TalkAddon.AddonID)) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.iconTeamlabOffice32='" + EscapeJsString(WebImageSupplier.GetAbsoluteWebPath("icon-teamlab-office32.png", TalkAddon.AddonID)) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.productName='" + EscapeJsString(TalkResource.ProductName) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.updateFlashPlayerUrl='" + EscapeJsString(TalkResource.UpdateFlashPlayerUrl) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.selectUserBookmarkTitle='" + EscapeJsString(TalkResource.SelectUserBookmarkTitle) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.defaultConferenceSubjectTemplate='" + EscapeJsString(TalkResource.DefaultConferenceSubjectTemplate) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.labelNewMessage='" + EscapeJsString(TalkResource.LabelNewMessage) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.labelRecvInvite='" + EscapeJsString(TalkResource.LabelRecvInvite) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.titleRecvInvite='" + EscapeJsString(TalkResource.TitleRecvInvite) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.hintClientConnecting='" + EscapeJsString(TalkResource.HintClientConnecting) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.hintClientDisconnected='" + EscapeJsString(TalkResource.HintClientDisconnected) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.hintEmotions='" + EscapeJsString(TalkResource.HintEmotions) + "',");
            jsResources.Append("window.ASC.TMTalk.Resources.hintFlastPlayerIncorrect='" + EscapeJsString(TalkResource.HintFlastPlayerIncorrect) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.hintGroups='" + EscapeJsString(TalkResource.HintGroups) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.hintNoFlashPlayer='" + EscapeJsString(TalkResource.HintNoFlashPlayer) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.hintOfflineContacts='" + EscapeJsString(TalkResource.HintOfflineContacts) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.hintSounds='" + EscapeJsString(TalkResource.HintSounds) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.hintUpdateHrefText='" + EscapeJsString(TalkResource.HintUpdateHrefText) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.hintSelectContact='" + EscapeJsString(TalkResource.HintSelectContact) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.hintSendInvite='" + EscapeJsString(TalkResource.HintSendInvite) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.hintPossibleClientConflict='" + EscapeJsString(TalkResource.HintPossibleClientConflict) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.hintCreateShortcutDialog='" + EscapeJsString(TalkResource.HintCreateShortcutDialog) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.sendFileMessage='" + EscapeJsString(string.Format(TalkResource.SendFileMessage, "{0}<br/>", "{1}")) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.mailingsGroupName='" + EscapeJsString(TalkResource.MailingsGroupName) + "';");
            jsResources.Append("window.ASC.TMTalk.Resources.conferenceGroupName='" + EscapeJsString(TalkResource.ConferenceGroupName) + "';");

            Page.RegisterInlineScript(jsResources.ToString(), true, false);

            jsResources = new StringBuilder();

            jsResources.Append("TMTalk.init();");
            jsResources.Append("ASC.TMTalk.properties.init(\"2.0\");");
            jsResources.Append("ASC.TMTalk.iconManager.init();");
            jsResources.AppendFormat("ASC.TMTalk.notifications.init(\"{0}\", \"{1}\");", GetUserPhotoHandlerPath(), GetNotificationHandlerPath());
            jsResources.AppendFormat("ASC.TMTalk.msManager.init(\"{0}\");", GetValidSymbols());
            jsResources.AppendFormat("ASC.TMTalk.mucManager.init(\"{0}\");", GetValidSymbols());
            jsResources.Append("ASC.TMTalk.roomsManager.init();");
            jsResources.Append("ASC.TMTalk.contactsManager.init();");
            jsResources.AppendFormat("ASC.TMTalk.messagesManager.init(\"{0}\", \"{1}\", \"{2}\", \"{3}\");", GetShortDateFormat(), GetFullDateFormat(), GetMonthNames(), GetHistoryLength());
            jsResources.AppendFormat("ASC.TMTalk.connectionManager.init(\"{0}\", \"{1}\", \"{2}\", \"{3}\");", GetBoshUri(), GetJabberAccount(), GetResourcePriority(), GetInactivity());
            jsResources.AppendFormat("ASC.TMTalk.properties.item(\"addonID\", \"{0}\");", TalkAddon.AddonID);
            jsResources.AppendFormat("ASC.TMTalk.properties.item(\"enabledMassend\", \"{0}\");", GetMassendState());
            jsResources.AppendFormat("ASC.TMTalk.properties.item(\"enabledConferences\", \"{0}\");", GetConferenceState());
            jsResources.AppendFormat("ASC.TMTalk.properties.item(\"requestTransportType\", \"{0}\");", GetRequestTransportType());
            jsResources.AppendFormat("ASC.TMTalk.properties.item(\"fileTransportType\", \"{0}\");", GetFileTransportType());
            jsResources.AppendFormat("ASC.TMTalk.properties.item(\"maxUploadSize\",\"{0}\");", SetupInfo.MaxImageUploadSize);
            jsResources.AppendFormat("ASC.TMTalk.properties.item(\"maxUploadSizeError\", \"{0}\");", FileSizeComment.FileImageSizeExceptionString);
            jsResources.AppendFormat("ASC.TMTalk.properties.item(\"sounds\", \"{0}\");", WebPath.GetPath("/addons/talk/swf/sounds.swf"));
            jsResources.AppendFormat("ASC.TMTalk.properties.item(\"expressInstall\", \"{0}\");", WebPath.GetPath("/addons/talk/swf/expressinstall.swf"));

            Page.RegisterInlineScript(jsResources.ToString(), onReady: false);

            try
            {
                Page.Title = TalkResource.ProductName + " - " + CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).DisplayUserName();
            }
            catch (System.Security.SecurityException)
            {
                Page.Title = TalkResource.ProductName + " - " + HeaderStringHelper.GetPageTitle(TalkResource.DefaultContactTitle);
            }
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
    }
}