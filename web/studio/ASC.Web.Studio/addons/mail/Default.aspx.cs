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
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Authentication;
using System.Text;
using System.Web;

using ASC.Core;
using ASC.Core.Users;
using ASC.Mail;
using ASC.Mail.Core;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;
using ASC.Mail.Extensions;
using ASC.Web.Core;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Utility;
using ASC.Web.CRM.Configuration;
using ASC.Web.Mail.Controls;
using ASC.Web.Mail.Masters.ClientScripts;
using ASC.Web.Mail.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Common.HelpCenter;
using ASC.Web.Studio.UserControls.Common.InviteLink;
using ASC.Web.Studio.UserControls.Common.LoaderPage;
using ASC.Web.Studio.UserControls.Common.Support;
using ASC.Web.Studio.UserControls.Common.UserForum;
using ASC.Web.Studio.Utility;

using Newtonsoft.Json;

using Constants = ASC.Core.Users.Constants;
using MailBox = ASC.Web.Mail.Controls.MailBox;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Mail
{
    public partial class MailPage : MainPage, IStaticBundle
    {
        private EngineFactory _engineFactory;

        protected EngineFactory EngineFactory
        {
            get
            {
                return _engineFactory ?? (_engineFactory = new EngineFactory(TenantProvider.CurrentTenantID,
                    SecurityContext.CurrentAccount.ID.ToString()));
            }
        }

        protected List<MailAccountData> Accounts { get; set; }
        protected List<MailFolderData> Folders { get; set; }
        protected List<MailUserFolderData> UserFolders { get; set; }
        protected List<MailTagData> Tags { get; set; }
        protected List<string> DisplayImagesAddresses { get; set; }
        protected List<MailSieveFilterData> Filters { get; set; }

        public const int EntryCountOnPage_def = 25;
        public const int VisiblePageCount_def = 10;

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

            //init Page Navigator
            _phPagerContent.Controls.Add(new PageNavigator
            {
                ID = "mailPageNavigator",
                CurrentPageNumber = 1,
                VisibleOnePage = false,
                EntryCount = 0,
                VisiblePageCount = VisiblePageCount_def,
                EntryCountOnPage = EntryCountOnPage_def
            });

            var helpCenter = (HelpCenter)LoadControl(HelpCenter.Location);
            helpCenter.IsSideBar = true;
            sideHelpCenter.Controls.Add(helpCenter);

            SupportHolder.Controls.Add(LoadControl(Support.Location));
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

            Master
                .AddStaticBodyScripts(GetStaticJavaScript())
                .AddStaticStyles(GetStaticStyleSheet())
                .AddClientScript(
                    new ClientLocalizationResources(),
                    new MasterSettingsResources())
                .RegisterInlineScript(GetMailInitInlineScript(), true, false)
                .RegisterInlineScript(GetMailConstantsAsInlineScript(), true, false)
                .RegisterInlineScript(GetMailPresetsAsInlineScript(), true, false);
        }

        #region .Presets

        protected bool IsAdministrator
        {
            get
            {
                return WebItemSecurity.IsProductAdministrator(WebItemManager.MailProductID, SecurityContext.CurrentAccount.ID);
            }
        }

        protected bool IsFullAdministrator
        {
            get { return CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID); }
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
            return ConfigurationManagerExtension.AppSettings["mail.check-news-timeout"] == null ? 30000 : Convert.ToInt32(ConfigurationManagerExtension.AppSettings["ServiceCheckTimeout"]);
        }

        public static int GetMaximumMessageBodySize()
        {
            return Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.maximum-message-body-size"] ?? "524288");
        }

        private const string MAIL_TROUBLESHOOTING = "troubleshooting/mail.aspx";
        private const string DEFAULT_FAQ_URL = "http://helpcenter.onlyoffice.com/" + MAIL_TROUBLESHOOTING;

        public static string GetMailFaqUri()
        {
            var baseHelpLink = CommonLinkUtility.GetHelpLink();

            if (string.IsNullOrEmpty(baseHelpLink))
                baseHelpLink = ConfigurationManagerExtension.AppSettings["web.faq-url"] ?? string.Empty;

            return baseHelpLink.TrimEnd('/') + "/troubleshooting/mail.aspx";
        }

        public static String GetMailSupportUri()
        {
            return "mailto:" + Core.WhiteLabel.MailWhiteLabelSettings.Instance.SupportEmail;
        }

        public static bool IsTurnOnServer()
        {
            return Configuration.Settings.IsAdministrationPageAvailable();
        }

        public static bool IsTurnOnAttachmentsGroupOperations()
        {
            return Defines.IsAttachmentsGroupOperationsAvailable;
        }

        public static bool IsMSExchangeMigrationLinkAvailable()
        {
            return Defines.IsMSExchangeMigrationLinkAvailable;
        }

        public static bool IsCrmAvailable()
        {
            return WebItemSecurity.IsAvailableForMe(WebItemManager.CRMProductID);
        }

        public static bool IsPeopleAvailable()
        {
            return WebItemSecurity.IsAvailableForMe(WebItemManager.PeopleProductID);
        }

        public static bool IsCalndarAvailable()
        {
            return WebItemSecurity.IsAvailableForMe(WebItemManager.CalendarProductID);
        }

        public static bool IsMailCommonDomainAvailable()
        {
            return Configuration.Settings.IsMailCommonDomainAvailable();
        }

        public static bool IsMailPrintAvailable()
        {
            return SetupInfo.IsVisibleSettings("MailPrint");
        }

        public static string GetMailDownloadHandlerUri()
        {
            return ConfigurationManagerExtension.AppSettings["mail.download-handler-url"] ?? "/addons/mail/httphandlers/download.ashx?attachid={0}";
        }

        public static string GetMailViewDocumentHandlerUri()
        {
            return ConfigurationManagerExtension.AppSettings["mail.view-document-handler-url"] ?? "/addons/mail/httphandlers/viewdocument.ashx?attachid={0}";
        }

        public static string GetMailContactPhotoHandlerUri()
        {
            return ConfigurationManagerExtension.AppSettings["mail.contact-photo-handler-url"] ?? "/addons/mail/httphandlers/contactphoto.ashx?cid={0}&ps={1}";
        }

        public static string GetMailEditDocumentHandlerUri()
        {
            return ConfigurationManagerExtension.AppSettings["mail.edit-document-handler-url"] ?? "/addons/mail/httphandlers/editdocument.ashx?attachid={0}";
        }

        public static string GetMailDaemonEmail()
        {
            return ConfigurationManagerExtension.AppSettings["mail.daemon-email"] ?? "mail-daemon@onlyoffice.com";
        }

        public static string GetProxyHttpUrl()
        {
            return ConfigurationManagerExtension.AppSettings["proxy.http-url"] ?? "/httphandlers/urlProxy.ashx";
        }

        public List<MailAccountData> GetAccounts()
        {
            if (Accounts == null)
                Accounts = new List<MailAccountData>();

            if (Accounts.Any())
                return Accounts;

            Accounts = EngineFactory.AccountEngine.GetAccountInfoList().ToAccountData();

            return Accounts;
        }

        protected List<MailUserFolderData> GetUserFolders()
        {
            if (UserFolders == null)
                UserFolders = new List<MailUserFolderData>();

            if (UserFolders.Any())
                return UserFolders;

            UserFolders = EngineFactory.UserFolderEngine.GetList();

            return UserFolders;
        }

        protected List<MailFolderData> GetFolders()
        {
            if (Folders == null)
                Folders = new List<MailFolderData>();

            if (Folders.Any())
                return Folders;

            Folders = EngineFactory.FolderEngine.GetFolders()
                .Where(f => f.id != FolderType.Sending)
                .ToList()
                .ToFolderData();

            return Folders;
        }

        protected List<MailTagData> GetTags()
        {
            if (Tags == null)
                Tags = new List<MailTagData>();

            if (Tags.Any())
                return Tags;

            Tags = EngineFactory.TagEngine.GetTags().ToTagData();

            return Tags;
        }

        protected List<string> GetDisplayImagesAddresses()
        {
            if (DisplayImagesAddresses == null)
                DisplayImagesAddresses = new List<string>();

            if (DisplayImagesAddresses.Any())
                return DisplayImagesAddresses;

            DisplayImagesAddresses = EngineFactory.DisplayImagesAddressEngine.Get().ToList();

            return DisplayImagesAddresses;
        }

        protected List<MailSieveFilterData> GetFilters()
        {
            if (Filters == null)
                Filters = new List<MailSieveFilterData>();

            if (Filters.Any())
                return Filters;

            Filters = EngineFactory.FilterEngine.GetList();

            return Filters;
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
                {"COR_E_AUTHENTICATION", new AuthenticationException().HResult}
            };

            return errorConstants;
        }

        public static Dictionary<string, int> GetAlerts()
        {
            var type = typeof(MailAlertTypes);
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
                    JsonConvert.SerializeObject(CustomNamingPeople.Substitute<MailResource>("FilterByGroup")))
                .AppendFormat("ASC.Mail.Constants.NEED_PROXY_HTTP_URL = {0};\r\n",
                    JsonConvert.SerializeObject(SetupInfo.IsVisibleSettings("ProxyHttpContent")))
                .AppendFormat("ASC.Mail.Constants.PROXY_HTTP_URL = {0};\r\n",
                    JsonConvert.SerializeObject(GetProxyHttpUrl()))
                .AppendFormat("ASC.Mail.Constants.PASSWORD_SETTINGS = {0};\r\n",
                    JsonConvert.SerializeObject(PasswordSettings.Load()))
                .AppendFormat("ASC.Mail.Constants.MAXIMUM_MESSAGE_BODY_SIZE = {0};\r\n",
                    JsonConvert.SerializeObject(GetMaximumMessageBodySize()))
                .AppendFormat("ASC.Mail.Constants.MS_MIGRATION_LINK_AVAILABLE = {0};\r\n",
                    JsonConvert.SerializeObject(IsMSExchangeMigrationLinkAvailable()));

            return sbScript.ToString();
        }

        protected string GetMailPresetsAsInlineScript()
        {
            var sbScript = new StringBuilder();

            var settings = MailCommonSettings.LoadForCurrentUser();
            settings.CacheUnreadMessagesSetting = false; //TODO: Change cache algoritnm and restore it back

            sbScript
                .AppendFormat("ASC.Mail.Presets.Accounts = {0};\r\n",
                    JsonConvert.SerializeObject(GetAccounts(), new HtmlEncodeStringPropertiesConverter()))
                .AppendFormat("ASC.Mail.Presets.Tags = {0};\r\n",
                    JsonConvert.SerializeObject(GetTags(), new HtmlEncodeStringPropertiesConverter()))
                .AppendFormat("ASC.Mail.Presets.Folders = {0};\r\n",
                    JsonConvert.SerializeObject(GetFolders(), new HtmlEncodeStringPropertiesConverter()))
                .AppendFormat("ASC.Mail.Presets.DisplayImagesAddresses = {0};\r\n",
                    JsonConvert.SerializeObject(GetDisplayImagesAddresses(), new HtmlEncodeStringPropertiesConverter()))
                .AppendFormat("ASC.Mail.Presets.CommonSettings = {0};\r\n",
                    JsonConvert.SerializeObject(settings, new HtmlEncodeStringPropertiesConverter()))
                .AppendFormat("ASC.Mail.Presets.UserFolders = {0};\r\n",
                    JsonConvert.SerializeObject(GetUserFolders(), new HtmlEncodeStringPropertiesConverter()))
                .AppendFormat("ASC.Mail.Presets.Filters = {0};\r\n",
                    JsonConvert.SerializeObject(GetFilters(), new HtmlEncodeStringPropertiesConverter()));

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

        public ScriptBundleData GetStaticJavaScript()
        {
            return (ScriptBundleData)
                new ScriptBundleData("mail", "mail")
                    .AddSource(ResolveUrl, new ClientTemplateResources())
                    .AddSource(VirtualPathUtility.ToAbsolute,
                        "~/js/third-party/ical.js",
                        "~/js/third-party/setimmediate.js",
                        "~/js/third-party/sorttable.js",
                        "~/js/third-party/moment.min.js",
                        "~/js/third-party/moment-timezone.min.js",
                        "~/js/third-party/rrule.js",
                        "~/js/third-party/nlp.js",
                        "~/js/third-party/jquery/jstree.min.js",
                        "~/js/third-party/jquery/jquery.mousewheel.js",
                        "~/js/asc/plugins/progressdialog.js",
                        "~/addons/mail/js/userfolders/plugins/jstree.counters.plugin.js",
                        "~/addons/mail/js/third-party/jquery.dotdotdot.min.js",
                        "~/addons/mail/js/third-party/jquery.textchange.min.js",
                        "~/addons/mail/js/third-party/linkify.min.js",
                        "~/addons/mail/js/third-party/linkify-string.min.js",
                        "~/addons/mail/js/third-party/placeholder.js",
                        "~/addons/mail/js/containers/set.js",
                        "~/addons/mail/js/mail.common.js",
                        "~/addons/mail/js/dropdown.js",
                        "~/addons/mail/js/mail.crmlinkpopup.js",
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
                        "~/addons/mail/js/actionmenu.js",
                        "~/addons/mail/js/actionpanel.js",
                        "~/addons/mail/js/autocomplete/emailautocomplete.js",
                        "~/addons/mail/js/autocomplete/crmautocomplete.js",
                        "~/addons/mail/js/hidepanel.js",
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
                        "~/addons/mail/js/contacts/filter/crmfilter.js",
                        "~/addons/mail/js/contacts/page.js",
                        "~/addons/mail/js/contacts/panel.js",
                        "~/addons/mail/js/contacts/types.js",
                        "~/addons/mail/js/contacts/filter/tlfilter.js",
                        "~/addons/mail/js/contacts/filter/customfilter.js",
                        "~/addons/mail/js/contacts/tlgroups.js",
                        "~/addons/mail/js/contacts/contacts.js",
                        "~/addons/mail/js/contacts/modal/editcontact.js",
                        "~/addons/mail/js/blankpage.js",
                        "~/addons/mail/js/popup.js",
                        "~/addons/mail/js/alerts.js",
                        "~/addons/mail/js/filtercache.js",
                        "~/addons/mail/js/accountspanel.js",
                        "~/addons/mail/js/trustedaddresses.js",
                        "~/addons/mail/js/userfolders/panel.js",
                        "~/addons/mail/js/userfolders/page.js",
                        "~/addons/mail/js/userfolders/dropdown.js",
                        "~/addons/mail/js/userfolders/modal.js",
                        "~/addons/mail/js/userfolders/manager.js",
                        "~/addons/mail/js/init.js",
                        "~/addons/mail/js/helpcenter/panel.js",
                        "~/addons/mail/js/helpcenter/page.js",
                        "~/addons/mail/js/administration/plugin/jquery-mailboxadvansedselector.js",
                        "~/addons/mail/js/administration/plugin/jquery-domainadvansedselector.js",
                        "~/addons/mail/js/mail.calendar.js",
                        "~/addons/mail/js/commonsettings/page.js",
                        "~/addons/mail/js/filters/filter.js",
                        "~/addons/mail/js/filters/page.js",
                        "~/addons/mail/js/filters/edit.js",
                        "~/addons/mail/js/filters/manager.js",
                        "~/addons/mail/js/filters/modal.js",
                        "~/addons/mail/js/templates/plugin/jquery-mailtemplateadvansedselector.js");
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return (StyleBundleData)
                new StyleBundleData("mail", "mail")
                    .AddSource(ResolveUrl,
                        "~/addons/mail/App_Themes/default/less/mail.less");
        }
    }
}