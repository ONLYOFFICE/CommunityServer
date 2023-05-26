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
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.WebZones;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.UserControls.Common.Support;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;

using Newtonsoft.Json;

namespace ASC.Web.Studio.Masters
{
    public partial class BaseTemplate : MasterPage
    {
        /// <summary>
        /// Block side panel
        /// </summary>
        public bool DisabledSidePanel { get; set; }

        public bool DisabledTopStudioPanel { get; set; }

        public bool DisabledLayoutMedia { get; set; }

        private bool? _enableWebChat;

        public bool EnabledWebChat
        {
            get { return _enableWebChat.HasValue && _enableWebChat.Value; }
            set { _enableWebChat = value; }
        }

        public string HubUrl { get; set; }

        public bool IsMobile { get; set; }

        public bool IsHealthcheck { get; set; }

        public TopStudioPanel TopStudioPanel;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            TopStudioPanel = (TopStudioPanel)LoadControl(TopStudioPanel.Location);
            MetaKeywords.Content = Resource.MetaKeywords;
            MetaDescription.Content = Resource.MetaDescription.HtmlEncode();
            MetaDescriptionOG.Content = Resource.MetaDescription.HtmlEncode();
            MetaTitleOG.Content = (String.IsNullOrEmpty(Page.Title) ? Resource.MainPageTitle : Page.Title).HtmlEncode();
            CanonicalURLOG.Content = HttpContext.Current.Request.Url.Scheme + "://" + Request.GetUrlRewriter().Host;
            MetaImageOG.Content = WebImageSupplier.GetAbsoluteWebPath("logo/fb_icon_325x325.jpg");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            IsHealthcheck = !string.IsNullOrEmpty(Request["healthcheck"]);

            InitScripts();

            HubUrl = ConfigurationManagerExtension.AppSettings["web.hub"] ?? string.Empty;

            if (!_enableWebChat.HasValue || _enableWebChat.Value)
            {
                EnabledWebChat = Convert.ToBoolean(ConfigurationManager.AppSettings["web.chat"] ?? "false") &&
                                 WebItemManager.Instance.GetItems(WebZoneType.CustomProductList, ItemAvailableState.Normal).Any(id => id.ID == WebItemManager.TalkProductID) &&
                                 !(Request.Browser != null && Request.Browser.Browser == "IE" && Request.Browser.MajorVersion < 11);
            }

            IsMobile = MobileDetector.IsMobile;

            if (!DisabledSidePanel && EnabledWebChat && !IsMobile)
            {
                SmallChatHolder.Controls.Add(LoadControl(UserControls.Common.SmallChat.SmallChat.Location));
            }

            if (!DisabledSidePanel && !CoreContext.Configuration.Personal)
            {
                // InvitePanel popup
                InvitePanelHolder.Controls.Add(LoadControl(InvitePanel.Location));
            }

            if ((!DisabledSidePanel || !DisabledTopStudioPanel) && !TopStudioPanel.DisableSettings && !IsHealthcheck &&
                HubUrl != string.Empty && SecurityContext.IsAuthenticated)
            {
                AddBodyScripts(ResolveUrl, "~/js/third-party/socket.io.js", "~/js/asc/core/asc.socketio.js");
            }

            if (!DisabledTopStudioPanel)
            {
                TopContent.Controls.Add(TopStudioPanel);
            }

            var curUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            if (!DisabledSidePanel)
            {
                if (curUser.ActivationStatus != EmployeeActivationStatus.Activated &&
                    curUser.CreateDate.Date != DateTime.UtcNow.Date &&
                    !CoreContext.Configuration.Personal &&
                    SecurityContext.IsAuthenticated &&
                    EmailActivationSettings.LoadForCurrentUser().Show)
                {
                    activateEmailPanel.Controls.Add(LoadControl(ActivateEmailPanel.Location));
                }
                TariffNotifyHolder.Controls.Add(LoadControl(TariffNotify.Location));
            }

            if (curUser.IsVisitor() && !curUser.IsOutsider())
            {
                var collaboratorPopupSettings = CollaboratorSettings.LoadForCurrentUser();
                if (collaboratorPopupSettings.FirstVisit)
                {
                    AddBodyScripts(ResolveUrl, "~/js/asc/core/collaborators.js");
                }
            }

            if (Request.DesktopApp())
            {
                AddBodyScripts(ResolveUrl, "~/js/asc/core/desktop.polyfills.js");
            }

            var matches = Regex.Match(HttpContext.Current.Request.Url.AbsolutePath, "(products|addons)/(\\w+)/(share\\.aspx|saveas\\.aspx|filechoice\\.aspx|ganttchart\\.aspx|jabberclient\\.aspx|timer\\.aspx|generatedreport\\.aspx).*", RegexOptions.IgnoreCase);

            if (SecurityContext.IsAuthenticated && !matches.Success && AdditionalWhiteLabelSettings.Instance.FeedbackAndSupportEnabled)
            {
                LiveChatHolder.Controls.Add(LoadControl(SupportChat.Location));
            }
        }

        protected string RenderStatRequest()
        {
            if (string.IsNullOrEmpty(SetupInfo.StatisticTrackURL)) return string.Empty;

            var page = HttpUtility.UrlEncode(Page.AppRelativeVirtualPath.Replace("~", ""));
            return String.Format("<img style=\"display:none;\" src=\"{0}\"/>", SetupInfo.StatisticTrackURL + "&page=" + page);
        }

        protected string ColorThemeClass
        {
            get { return ColorThemesSettings.GetColorThemesSettings(); }
        }

        #region Operations


        private void InitScripts()
        {
            ThemeStyles.AddSource(ResolveUrl, "~/skins/default/layout.less");

            if (!DisabledLayoutMedia)
            {
                ThemeStyles.AddSource(ResolveUrl, Request.DesktopApp()
                    ? "~/skins/default/layout-desktop.less"
                    : "~/skins/default/layout-media.less");
            }

            AddStyles(r => r, "~/skins/<theme_folder>/main.less");

            AddClientScript(
                new MasterResources.MasterSettingsResources(),
                new MasterResources.MasterUserResources(),
                new MasterResources.MasterFileUtilityResources(),
                new MasterResources.MasterCustomResources(),
                new MasterResources.MasterLocalizationResources()
                );

            InitProductSettingsInlineScript();
            InitStudioSettingsInlineScript();
        }

        private void InitStudioSettingsInlineScript()
        {
            var paid = !TenantStatisticsProvider.IsNotPaid();
            var showPromotions = !IsHealthcheck && paid && PromotionsSettings.Load().Show;
            var showTips = !IsHealthcheck && paid && !Request.DesktopApp() && TipsSettings.LoadForCurrentUser().Show;
            var modeThemeSettings = ModeThemeSettings.GetModeThemesSettings();

            var script = new StringBuilder();
            script.AppendFormat("window.ASC.Resources.Master.ShowPromotions={0};", showPromotions.ToString().ToLowerInvariant());
            script.AppendFormat("window.ASC.Resources.Master.ShowTips={0};", showTips.ToString().ToLowerInvariant());
            script.AppendFormat("window.ASC.Resources.Master.ModeThemeSettings={0};", JsonConvert.SerializeObject(modeThemeSettings));

            RegisterInlineScript(script.ToString(), true, false);
        }

        private void InitProductSettingsInlineScript()
        {
            var isAdmin = false;

            if (!CoreContext.Configuration.Personal)
            {
                isAdmin = WebItemSecurity.IsProductAdministrator(CommonLinkUtility.GetProductID(), SecurityContext.CurrentAccount.ID);

                if (!isAdmin)
                {
                    isAdmin = WebItemSecurity.IsProductAdministrator(CommonLinkUtility.GetAddonID(), SecurityContext.CurrentAccount.ID);
                }
            }
            RegisterInlineScript(string.Format("window.ASC.Resources.Master.IsProductAdmin={0};", isAdmin.ToString().ToLowerInvariant()), true, false);
        }

        #region Style

        public BaseTemplate AddStyles(Func<string, string> converter, params string[] src)
        {
            foreach (var s in src)
            {
                if (s.Contains(ColorThemesSettings.ThemeFolderTemplate))
                {
                    if (ThemeStyles == null) continue;

                    ThemeStyles.AddSource(r => ResolveUrl(ColorThemesSettings.GetThemeFolderName(converter(r))), s);
                }
                else
                {
                    if (HeadStyles == null) continue;

                    HeadStyles.AddSource(converter, s);
                }
            }

            return this;
        }


        #endregion

        #region Scripts

        public BaseTemplate AddBodyScripts(Func<string, string> converter, params string[] src)
        {
            if (BodyScripts == null) return this;
            BodyScripts.AddSource(converter, src);

            return this;
        }

        public BaseTemplate AddStaticBodyScripts(ScriptBundleData bundleData)
        {
            StaticScript.SetData(bundleData);

            return this;
        }

        public BaseTemplate AddStaticStyles(StyleBundleData bundleData)
        {
            StaticStyle.SetData(bundleData);

            return this;
        }

        public BaseTemplate RegisterInlineScript(string script, bool beforeBodyScripts = false, bool onReady = true)
        {
            var tuple = new Tuple<string, bool>(script, onReady);
            if (!beforeBodyScripts)
                InlineScript.Scripts.Add(tuple);
            else
                InlineScriptBefore.Scripts.Add(tuple);

            return this;
        }

        #endregion

        #region ClientScript

        public BaseTemplate AddClientScript(ClientScript clientScript)
        {
            var localizationScript = clientScript as ClientScriptLocalization;
            if (localizationScript != null)
            {
                clientLocalizationScript.AddScript(clientScript);
                return this;
            }

            baseTemplateMasterScripts.AddScript(clientScript);
            return this;
        }

        public BaseTemplate AddClientScript(params ClientScript[] clientScript)
        {
            foreach (var script in clientScript)
            {
                AddClientScript(script);
            }

            return this;
        }

        #endregion

        #endregion
    }
}