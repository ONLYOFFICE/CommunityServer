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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Authentication;
using System.Text;
using System.Web;
using System.Web.Configuration;
using ASC.Api.Mail.DataContracts;
using ASC.Api.Mail.Extensions;
using ASC.Core;
using ASC.Core.Users;
using ASC.FederatedLogin.LoginProviders;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Web.Core;
using ASC.Web.CRM.Configuration;
using ASC.Web.Mail.Controls;
using ASC.Web.Mail.Masters.ClientScripts;
using ASC.Web.Mail.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Common.HelpCenter;
using ASC.Web.Studio.UserControls.Common.InviteLink;
using ASC.Web.Studio.UserControls.Common.LoaderPage;
using ASC.Web.Studio.UserControls.Common.Support;
using ASC.Web.Studio.UserControls.Common.UserForum;
using ASC.Web.Studio.UserControls.Common.VideoGuides;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json;
using MailBox = ASC.Web.Mail.Controls.MailBox;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Mail
{
    public partial class MailPage : MainPage
    {
        protected List<MailAccountData> Accounts { get; set; }
        protected List<MailFolderData> Folders { get; set; }
        protected List<MailTagData> Tags { get; set; }
        protected List<string> DisplayImagesAddresses { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) // Redirect to home page if user hasn't permissions or not authenticated.
            {
                Response.Redirect("/");
            }

            Accounts = GetAccounts();

            _manageFieldPopup.Options.IsPopup = true;
            _commonPopup.Options.IsPopup = true;

            Page.Title = HeaderStringHelper.GetPageTitle(MailResource.MailTitle);

            ProductEntryPoint.ConfigurePortal();

            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));

            MailSidePanelContainer.Controls.Add(LoadControl(TagBox.Location));

            MailControlContainer.Controls.Add(LoadControl(MailBox.Location));

            var helpCenter = (HelpCenter)LoadControl(HelpCenter.Location);
            helpCenter.IsSideBar = true;
            sideHelpCenter.Controls.Add(helpCenter);

            SupportHolder.Controls.Add(LoadControl(Support.Location));
            VideoGuides.Controls.Add(LoadControl(VideoGuidesControl.Location));
            UserForumHolder.Controls.Add(LoadControl(UserForum.Location));
            InviteUserHolder.Controls.Add(LoadControl(InviteLink.Location));

            if (!Accounts.Any())
                BlankModalPH.Controls.Add(LoadControl(BlankModal.Location));

            if (!IsCrmAvailable())
            {
                crmContactsContainer.Visible = false;
            }

            if (!IsPeopleAvailable())
            {
                tlContactsContainer.Visible = false;
            }

            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute, 
                                         "~/js/third-party/setImmediate.js",
                                         "~/js/third-party/sorttable.js",
                                         "~/js/third-party/moment.min.js",
                                         "~/js/third-party/moment-timezone.min.js",
                                         "~/js/third-party/rrule.js",
                                         "~/js/third-party/nlp.js",
                                         "~/addons/mail/js/third-party/autoresize.jquery.js",
                                         "~/addons/mail/js/third-party/jquery.dotdotdot.min.js",
                                         "~/addons/mail/js/third-party/jquery.textchange.min.js",
                                         "~/addons/mail/js/third-party/linkify.min.js",
                                         "~/addons/mail/js/third-party/linkify-string.min.js",
                                         "~/addons/mail/js/third-party/placeholder.js",
                                         "~/addons/mail/js/containers/set.js",
                                         "~/addons/mail/js/mail.common.js",
                                         "~/addons/mail/js/dropdown.js",
                                         "~/addons/mail/js/mail.crmLinkPopup.js",
                                         "~/addons/mail/js/mail.default.js",
                                         "~/addons/mail/js/mail.servicemanager.js",
                                         "~/addons/mail/js/fromsenderfilter.js",
                                         "~/addons/mail/js/mail.filter.js",
                                         "~/addons/mail/js/mail.folderfilter.js",
                                         "~/addons/mail/js/mail.folderpanel.js",
                                         "~/addons/mail/js/mail.mailbox.js",
                                         "~/addons/mail/js/mail.accounts.js",
                                         "~/addons/mail/js/mail.accountsmodal.js",
                                         "~/addons/mail/js/mail.accountspage.js",
                                         "~/addons/mail/js/wysiwyg.js",
                                         "~/addons/mail/js/mail.cache.js",
                                         "~/addons/mail/js/mail.messagepage.js",
                                         "~/addons/mail/js/mail.printpage.js",
                                         "~/addons/mail/js/mail.navigation.js",
                                         "~/addons/mail/js/mail.settingspanel.js",
                                         "~/addons/mail/js/mail.attachmentmanager.js",
                                         "~/addons/mail/js/actionMenu.js",
                                         "~/addons/mail/js/actionPanel.js",
                                         "~/addons/mail/js/emailAutocomplete.js",
                                         "~/addons/mail/js/hidePanel.js",
                                         "~/addons/mail/js/tags/tags.js",
                                         "~/addons/mail/js/tags/panel.js",
                                         "~/addons/mail/js/tags/colorspopup.js",
                                         "~/addons/mail/js/tags/dropdown.js",
                                         "~/addons/mail/js/tags/page.js",
                                         "~/addons/mail/js/administration/error.js",
                                         "~/addons/mail/js/administration/administration.js",
                                         "~/addons/mail/js/administration/page.js",
                                         "~/addons/mail/js/administration/forms.js",
                                         "~/addons/mail/js/administration/modal/editmailgroup.js",
                                         "~/addons/mail/js/administration/modal/editmailbox.js",
                                         "~/addons/mail/js/administration/modal/createdomain.js",
                                         "~/addons/mail/js/administration/modal/createmailbox.js",
                                         "~/addons/mail/js/administration/modal/createmailgroup.js",
                                         "~/addons/mail/js/tags/modal.js",
                                         "~/addons/mail/js/contacts/filter/crmFilter.js",
                                         "~/addons/mail/js/contacts/page.js",
                                         "~/addons/mail/js/contacts/panel.js",
                                         "~/addons/mail/js/contacts/types.js",
                                         "~/addons/mail/js/contacts/filter/tlFilter.js",
                                         "~/addons/mail/js/contacts/filter/customFilter.js",
                                         "~/addons/mail/js/contacts/tlGroups.js",
                                         "~/addons/mail/js/contacts/contacts.js",
                                         "~/addons/mail/js/contacts/modal/editContact.js",
                                         "~/addons/mail/js/blankpage.js",
                                         "~/addons/mail/js/popup.js",
                                         "~/addons/mail/js/alerts.js",
                                         "~/addons/mail/js/filterCache.js",
                                         "~/addons/mail/js/accountsPanel.js",
                                         "~/addons/mail/js/trustedAddresses.js",
                                         "~/addons/mail/js/init.js",
                                         "~/addons/mail/js/helpcenter/panel.js",
                                         "~/addons/mail/js/helpcenter/page.js",
                                         "~/addons/mail/js/administration/plugin/jquery-mailboxadvansedselector.js",
                                         "~/addons/mail/js/administration/plugin/jquery-domainadvansedselector.js",
                                         "~/addons/mail/js/mail.calendar.js");

            Page.RegisterStyle(ResolveUrl, "~/addons/mail/app_themes/default/less/mail.less");
            Page.RegisterClientLocalizationScript(typeof(ClientLocalizationResources));
            Page.RegisterClientLocalizationScript(typeof(ClientTemplateResources));

            Page.RegisterInlineScript(GetMailInitInlineScript(), true, false);
            Page.RegisterInlineScript(GetMailConstantsAsInlineScript(), true, false);
            Page.RegisterInlineScript(GetMailPresetsAsInlineScript(), true, false);
        }

        #region .Presets

        protected bool IsAdministrator
        {
            get
            {
                return CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID);
            }
        }

        protected bool IsPersonal
        {
            get { return CoreContext.Configuration.Personal; }
        }

        protected bool IsBlank
        {
            get { return Request.QueryString["blankpage"] == "true"; }
        }

        public static int GetMailCheckNewsTimeout()
        {
            return WebConfigurationManager.AppSettings["mail.check-news-timeout"] == null ? 30000 : Convert.ToInt32(WebConfigurationManager.AppSettings["ServiceCheckTimeout"]);
        }

        public static string GetMailFaqUri()
        {
            return !string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink())
                ? CommonLinkUtility.GetHelpLink() + "troubleshooting/mail.aspx"
                : "http://helpcenter.onlyoffice.com/troubleshooting/mail.aspx";
        }

        public static String GetMailSupportUri()
        {
            return WebConfigurationManager.AppSettings["mail.support-url"] ?? "mailto:support@onlyoffice.com";
        }

        public static string GetImportOAuthAccessUrl()
        {
            return WebConfigurationManager.AppSettings["mail.import-oauth-url"] ?? "";
        }

        public static bool IsTurnOnOAuth()
        {
            return !(string.IsNullOrEmpty(GoogleLoginProvider.GoogleOAuth20ClientId)
                     || string.IsNullOrEmpty(GoogleLoginProvider.GoogleOAuth20ClientSecret)
                     || string.IsNullOrEmpty(GoogleLoginProvider.GoogleOAuth20RedirectUrl)
                     || string.IsNullOrEmpty(GetImportOAuthAccessUrl()));
        }

        public static bool IsTurnOnServer()
        {
            return SetupInfo.IsVisibleSettings<AdministrationPage>();
        }

        public static bool IsTurnOnAttachmentsGroupOperations()
        {
            return Convert.ToBoolean(WebConfigurationManager.AppSettings["mail.attachments-group-operations"] ?? "false");
        }

        public static bool IsCrmAvailable()
        {
            return WebItemSecurity.IsAvailableForUser(WebItemManager.CRMProductID.ToString(), SecurityContext.CurrentAccount.ID);
        }

        public static bool IsPeopleAvailable()
        {
            return WebItemSecurity.IsAvailableForUser(WebItemManager.PeopleProductID.ToString(), SecurityContext.CurrentAccount.ID);
        }

        public static bool IsCalndarAvailable()
        {
            return WebItemSecurity.IsAvailableForUser(WebItemManager.CalendarProductID.ToString(), SecurityContext.CurrentAccount.ID);
        }

        public static bool IsMailCommonDomainAvailable()
        {
            return SetupInfo.IsVisibleSettings<AdministrationPage>() && SetupInfo.IsVisibleSettings("MailCommonDomain") && !CoreContext.Configuration.Standalone;
        }

        public static bool IsMailPrintAvailable()
        {
            return SetupInfo.IsVisibleSettings("MailPrint");
        }

        public static string GetMailDownloadHandlerUri()
        {
            return WebConfigurationManager.AppSettings["mail.download-handler-url"] ?? "/addons/mail/httphandlers/download.ashx?attachid={0}";
        }

        public static string GetMailDownloadAllHandlerUri()
        {
            return WebConfigurationManager.AppSettings["mail.download-all-handler-url"] ?? "/addons/mail/httphandlers/downloadall.ashx?messageid={0}";
        }

        public static string GetMailViewDocumentHandlerUri()
        {
            return WebConfigurationManager.AppSettings["mail.view-document-handler-url"] ?? "/addons/mail/httphandlers/viewdocument.ashx?attachid={0}";
        }

        public static string GetMailContactPhotoHandlerUri()
        {
            return WebConfigurationManager.AppSettings["mail.contact-photo-handler-url"] ?? "/addons/mail/httphandlers/contactphoto.ashx?cid={0}&ps={1}";
        }

        public static string GetMailEditDocumentHandlerUri()
        {
            return WebConfigurationManager.AppSettings["mail.edit-document-handler-url"] ?? "/addons/mail/httphandlers/editdocument.ashx?attachid={0}";
        }

        public static string GetMailDaemonEmail()
        {
            return WebConfigurationManager.AppSettings["mail.daemon-email"] ?? "mail-daemon@onlyoffice.com";
        }

        public static string GetProxyHttpUrl()
        {
            return WebConfigurationManager.AppSettings["proxy.http-url"] ?? "/httphandlers/urlProxy.ashx";
        }
        protected MailBoxManager dataManager;

        protected MailBoxManager DataManager
        {
            get { return dataManager ?? (dataManager = new MailBoxManager()); }
        }

        public List<MailAccountData> GetAccounts()
        {
            if (Accounts == null)
                Accounts = new List<MailAccountData>();

            if (!Accounts.Any())
            {
                Accounts =
                    DataManager.GetAccountInfo(TenantProvider.CurrentTenantID,
                        SecurityContext.CurrentAccount.ID.ToString()).
                        ToAddressData();
            }

            return Accounts;
        }

        protected List<MailFolderData> GetFolders()
        {
            if (Folders == null)
                Folders = new List<MailFolderData>();

            if (!Folders.Any())
            {
                Folders =
                    DataManager.GetFolders(TenantProvider.CurrentTenantID,
                        SecurityContext.CurrentAccount.ID.ToString(),
                        true)
                        .Where(f => f.id != MailFolder.Ids.temp)
                        .ToList()
                        .ToFolderData();
            }

            return Folders;
        }

        protected List<MailTagData> GetTags()
        {
            if (Tags == null)
                Tags = new List<MailTagData>();

            if (!Tags.Any())
            {
                Tags =
                    DataManager.GetTags(TenantProvider.CurrentTenantID, 
                    SecurityContext.CurrentAccount.ID.ToString(),
                        false)
                        .ToList()
                        .ToTagData();
            }

            return Tags;
        }

        protected List<string> GetDisplayImagesAddresses()
        {
            if (DisplayImagesAddresses == null)
                DisplayImagesAddresses = new List<string>();

            if (!DisplayImagesAddresses.Any())
            {
                DisplayImagesAddresses =
                    DataManager.GetDisplayImagesAddresses(TenantProvider.CurrentTenantID,
                    SecurityContext.CurrentAccount.ID.ToString())
                        .ToList();
            }

            return DisplayImagesAddresses;
        }

        public static Dictionary<string, int> GetErrorConstants()
        {
            var errorConstants = new Dictionary<string, int>
            {
                {"COR_E_SECURITY", new SecurityException().HResult},
                {"COR_E_ARGUMENT", new ArgumentException().HResult},
                {"COR_E_ARGUMENTOUTOFRANGE", new ArgumentOutOfRangeException().HResult},
                {"COR_E_INVALIDCAST", new InvalidCastException().HResult},
                {"COR_E_ARGUMENTNULL", new ArgumentNullException().HResult},
                {"COR_E_INVALIDDATA", new InvalidDataException().HResult},
                {"COR_E_INVALIDOPERATION", new InvalidOperationException().HResult},
                {"COR_E_DUPLICATENANE", new DuplicateNameException().HResult},
                {"COR_E_AUTHENTICATION", new AuthenticationException().HResult},
                {"COR_E_UNAUTHORIZED_ACCESS", new UnauthorizedAccessException().HResult}
            };

            return errorConstants;
        }

        public static Dictionary<string, int> GetAlerts()
        {
            var type = typeof(MailBoxManager.AlertTypes);
            var types = Enum.GetValues(type).Cast<int>().ToDictionary(e => Enum.GetName(type, e), e => e);
            return types;
        }

        protected string GetMailInitInlineScript()
        {
            var sbScript = new StringBuilder();
            sbScript.AppendLine("if (typeof (ASC) === 'undefined') { ASC = {};}")
                .AppendLine("if (typeof (ASC.Mail) === 'undefined') {ASC.Mail = {};}")
                .AppendLine("if (typeof (ASC.Mail.Presets) === 'undefined') {ASC.Mail.Presets = {};}")
                .AppendLine("if (typeof (ASC.Mail.Constants) === 'undefined') {ASC.Mail.Constants = {};}");

            return sbScript.ToString();
        }

        protected string GetMailConstantsAsInlineScript()
        {
            var sbScript = new StringBuilder();
            sbScript
                .AppendFormat("ASC.Mail.Constants.CHECK_NEWS_TIMEOUT = {0};\r\n",
                    JsonConvert.SerializeObject(GetMailCheckNewsTimeout()))
                .AppendFormat("ASC.Mail.Constants.CRM_AVAILABLE = {0};\r\n",
                    JsonConvert.SerializeObject(IsCrmAvailable()))
                .AppendFormat("ASC.Mail.Constants.PEOPLE_AVAILABLE = {0};\r\n",
                    JsonConvert.SerializeObject(IsPeopleAvailable()))
                .AppendFormat("ASC.Mail.Constants.CALENDAR_AVAILABLE = {0};\r\n",
                    JsonConvert.SerializeObject(IsCalndarAvailable()))
                .AppendFormat("ASC.Mail.Constants.COMMON_DOMAIN_AVAILABLE = {0};\r\n",
                    JsonConvert.SerializeObject(IsMailCommonDomainAvailable()))
                .AppendFormat("ASC.Mail.Constants.PRINT_AVAILABLE = {0};\r\n",
                    JsonConvert.SerializeObject(IsMailPrintAvailable()))
                .AppendFormat("ASC.Mail.Constants.FAQ_URL = {0};\r\n",
                    JsonConvert.SerializeObject(GetMailFaqUri()))
                .AppendFormat("ASC.Mail.Constants.SUPPORT_URL = {0};\r\n",
                    JsonConvert.SerializeObject(GetMailSupportUri()))
                .AppendFormat("ASC.Mail.Constants.DOWNLOAD_HANDLER_URL = {0};\r\n",
                    JsonConvert.SerializeObject(GetMailDownloadHandlerUri()))
                .AppendFormat("ASC.Mail.Constants.DOWNLOAD_ALL_HANDLER_URL = {0};\r\n",
                    JsonConvert.SerializeObject(GetMailDownloadAllHandlerUri()))
                .AppendFormat("ASC.Mail.Constants.VIEW_DOCUMENT_HANDLER_URL = {0};\r\n",
                    JsonConvert.SerializeObject(GetMailViewDocumentHandlerUri()))
                .AppendFormat("ASC.Mail.Constants.EDIT_DOCUMENT_HANDLER_URL = {0};\r\n",
                    JsonConvert.SerializeObject(GetMailEditDocumentHandlerUri()))
                .AppendFormat("ASC.Mail.Constants.CONTACT_PHOTO_HANDLER_URL = {0};\r\n",
                    JsonConvert.SerializeObject(GetMailContactPhotoHandlerUri()))
                .AppendFormat("ASC.Mail.Constants.Errors = {0};\r\n",
                    JsonConvert.SerializeObject(GetErrorConstants()))
                .AppendFormat("ASC.Mail.Constants.Alerts = {0};\r\n",
                    JsonConvert.SerializeObject(GetAlerts()))
                .AppendFormat("ASC.Mail.Constants.MAIL_DAEMON_EMAIL = {0};\r\n",
                    JsonConvert.SerializeObject(GetMailDaemonEmail()))
                .AppendFormat("ASC.Mail.Constants.FiLTER_BY_GROUP_LOCALIZE = {0};\r\n",
                    JsonConvert.SerializeObject(CustomNamingPeople.Substitute<Resources.MailResource>("FilterByGroup")))
                .AppendFormat("ASC.Mail.Constants.NEED_PROXY_HTTP_URL = {0};\r\n",
                    JsonConvert.SerializeObject(SetupInfo.IsVisibleSettings("ProxyHttpContent")))
                .AppendFormat("ASC.Mail.Constants.PROXY_HTTP_URL = {0};\r\n",
                    JsonConvert.SerializeObject(GetProxyHttpUrl()));

            return sbScript.ToString();
        }

        protected string GetMailPresetsAsInlineScript()
        {
            var sbScript = new StringBuilder();
            sbScript
                .AppendFormat("ASC.Mail.Presets.Accounts = {0};\r\n",
                    JsonConvert.SerializeObject(GetAccounts(), new HtmlEncodeStringPropertiesConverter()))
                .AppendFormat("ASC.Mail.Presets.Tags = {0};\r\n",
                    JsonConvert.SerializeObject(GetTags(), new HtmlEncodeStringPropertiesConverter()))
                .AppendFormat("ASC.Mail.Presets.Folders = {0};\r\n",
                    JsonConvert.SerializeObject(GetFolders(), new HtmlEncodeStringPropertiesConverter()))
                .AppendFormat("ASC.Mail.Presets.DisplayImagesAddresses = {0};\r\n",
                    JsonConvert.SerializeObject(GetDisplayImagesAddresses(), new HtmlEncodeStringPropertiesConverter()));

            return sbScript.ToString();
        }

        public class HtmlEncodeStringPropertiesConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(string);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(HttpUtility.HtmlEncode(value.ToString()));
            }
        }

        #endregion
    }
}