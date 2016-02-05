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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.WebZones;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.UserControls.Common.Banner;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio.Masters
{
    public partial class BaseTemplate : MasterPage
    {
        /// <summary>
        /// Block side panel
        /// </summary>
        /// 
        protected Tuple<string, string> TariffNotify;

        public bool DisableTariffNotify { get; set; }

        public bool DisabledSidePanel { get; set; }

        public bool DisabledTopStudioPanel { get; set; }

        private bool? _enableWebChat;

        public bool EnabledWebChat
        {
            get { return _enableWebChat.HasValue && _enableWebChat.Value; }
            set { _enableWebChat = value; }
        }

        public string HubUrl { get; set; }

        public bool IsMobile { get; set; }

        public TopStudioPanel TopStudioPanel;

        protected bool DisablePartnerPanel { get; set; }
        private bool? IsAuthorizedPartner { get; set; }
        protected Partner Partner { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            TopStudioPanel = (TopStudioPanel)LoadControl(TopStudioPanel.Location);
            MetaKeywords.Content = Resource.MetaKeywords;
            MetaDescription.Content = Resource.MetaDescription.HtmlEncode();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitScripts();
            HubUrl = ConfigurationManager.AppSettings["web.hub"] ?? string.Empty;

            if (!_enableWebChat.HasValue || _enableWebChat.Value)
            {
                EnabledWebChat = Convert.ToBoolean(ConfigurationManager.AppSettings["web.chat"] ?? "false") &&
                             WebItemManager.Instance.GetItems(WebZoneType.CustomProductList, ItemAvailableState.Normal).
                                            Any(id => id.ID == WebItemManager.TalkProductID) &&
                             !(Request.Browser != null && Request.Browser.Browser == "IE" &&
                               (Request.Browser.MajorVersion == 8 || Request.Browser.MajorVersion == 9 || Request.Browser.MajorVersion == 10));
            }

            IsMobile = MobileDetector.IsMobile;

            if (!DisabledSidePanel && EnabledWebChat && !IsMobile)
            {
                SmallChatHolder.Controls.Add(LoadControl(UserControls.Common.SmallChat.SmallChat.Location));
            }

            if (!DisabledSidePanel)
            {
                /** InvitePanel popup **/
                InvitePanelHolder.Controls.Add(LoadControl(InvitePanel.Location));
            }

            if ((!DisabledSidePanel || !DisabledTopStudioPanel)
                && HubUrl != string.Empty && !Request.Path.Equals("/auth.aspx", StringComparison.InvariantCultureIgnoreCase))
            {
                AddBodyScripts(ResolveUrl("~/js/third-party/jquery/jquery.signalr.js"));
                AddBodyScripts(ResolveUrl("~/js/third-party/jquery/jquery.hubs.js"));
            }

            if (!DisabledTopStudioPanel)
            {
                TopContent.Controls.Add(TopStudioPanel);
            }

            if (!EmailActivated && !CoreContext.Configuration.Personal && SecurityContext.IsAuthenticated)
            {
                activateEmailPanel.Controls.Add(LoadControl(ActivateEmailPanel.Location));
            }

            if (AffiliateHelper.BannerAvailable || CoreContext.Configuration.Personal)
            {
                BannerHolder.Controls.Add(LoadControl(Banner.Location));
            }

            var curUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            if (DisabledSidePanel)
            {
                DisableTariffNotify = true;
                DisablePartnerPanel = true;
            }
            else
            {
                if (!SecurityContext.IsAuthenticated || !TenantExtra.EnableTarrifSettings || CoreContext.Configuration.Personal
                    || curUser.IsVisitor())
                {
                    DisableTariffNotify = true;
                }
                else
                {
                    TariffNotify = GetTariffNotify();
                    if (TariffNotify == null)
                        DisableTariffNotify = true;
                }

                if (CoreContext.Configuration.PartnerHosted)
                {
                    IsAuthorizedPartner = false;
                    var partner = CoreContext.PaymentManager.GetApprovedPartner();
                    if (partner != null)
                    {
                        IsAuthorizedPartner = !string.IsNullOrEmpty(partner.AuthorizedKey);
                        Partner = partner;
                    }
                }
                DisablePartnerPanel = !(IsAuthorizedPartner.HasValue && !IsAuthorizedPartner.Value);
            }

            if (curUser.IsVisitor() && !curUser.IsOutsider())
            {
                var collaboratorPopupSettings = SettingsManager.Instance.LoadSettingsFor<CollaboratorSettings>(curUser.ID);
                if (collaboratorPopupSettings.FirstVisit)
                {
                    AddBodyScripts(ResolveUrl("~/js/asc/core/collaborators.js"));
                }
            }


            #region third-party scripts

            var GoogleTagManagerScriptLocation = "~/UserControls/Common/ThirdPartyScripts/GoogleTagManagerScript.ascx";
            if (File.Exists(HttpContext.Current.Server.MapPath(GoogleTagManagerScriptLocation)) &&
                !CoreContext.Configuration.Standalone && !CoreContext.Configuration.Personal && SetupInfo.CustomScripts.Length != 0)
            {
                GoogleTagManagerPlaceHolder.Controls.Add(LoadControl(GoogleTagManagerScriptLocation));
            } else {
                GoogleTagManagerPlaceHolder.Visible = false;
            }

            var GoogleAnalyticsScriptLocation = "~/UserControls/Common/ThirdPartyScripts/GoogleAnalyticsScript.ascx";
            if (File.Exists(HttpContext.Current.Server.MapPath(GoogleAnalyticsScriptLocation)) &&
                !CoreContext.Configuration.Standalone && !CoreContext.Configuration.Personal && SetupInfo.CustomScripts.Length != 0
                && ASC.Core.SecurityContext.IsAuthenticated)
            {
                GoogleAnalyticsScriptPlaceHolder.Controls.Add(LoadControl(GoogleAnalyticsScriptLocation));
            } else {
                GoogleAnalyticsScriptPlaceHolder.Visible = false;
            }
            

            var YandexMetrikaScriptLocation = "~/UserControls/Common/ThirdPartyScripts/YandexMetrikaScript.ascx";
            if (File.Exists(HttpContext.Current.Server.MapPath(YandexMetrikaScriptLocation)) &&
                !CoreContext.Configuration.Standalone && CoreContext.Configuration.Personal && SetupInfo.CustomScripts.Length != 0)
            {
                YandexMetrikaScriptPlaceHolder.Controls.Add(LoadControl(YandexMetrikaScriptLocation));
            }
            else
            {
                YandexMetrikaScriptPlaceHolder.Visible = false;
            }


            var GoogleConversionPersonScriptLocation = "~/UserControls/Common/ThirdPartyScripts/GoogleConversionPersonScript.ascx";
            if (File.Exists(HttpContext.Current.Server.MapPath(GoogleConversionPersonScriptLocation)) &&
                !CoreContext.Configuration.Standalone && CoreContext.Configuration.Personal && SetupInfo.CustomScripts.Length != 0)
            {
                GoogleConversionPersonScriptPlaceHolder.Controls.Add(LoadControl(GoogleConversionPersonScriptLocation));
            }
            else
            {
                GoogleConversionPersonScriptPlaceHolder.Visible = false;
            }


            #endregion
        }

        private static Tuple<string, string> GetTariffNotify()
        {
            var tariff = TenantExtra.GetCurrentTariff();

            var count = tariff.DueDate.Date.Subtract(DateTime.Today).Days;
            if (tariff.State == TariffState.Trial)
            {
                if (count <= 5)
                {
                    var text = String.Format(CoreContext.Configuration.Standalone ? Resource.TariffLinkStandalone : Resource.TrialPeriodInfoText,
                                             "<a href=\"" + TenantExtra.GetTariffPageLink() + "\">", "</a>");

                    if (count <= 0)
                        return new Tuple<string, string>(Resource.TrialPeriodExpired, text);

                    var end = GetNumeralResourceByCount(count, Resource.Day, Resource.DaysOne, Resource.DaysTwo);
                    return new Tuple<string, string>(string.Format(Resource.TrialPeriod, count, end), text);
                }

                if (CoreContext.Configuration.Standalone)
                {
                    return new Tuple<string, string>(Resource.TrialPeriodInfoTextLicense, string.Empty);
                }
            }

            if (tariff.State == TariffState.Paid)
            {
                if (CoreContext.Configuration.Standalone && count < 10)
                {
                    var text = String.Format(Resource.TariffLinkStandalone,
                                             "<a href=\"" + TenantExtra.GetTariffPageLink() + "\">", "</a>");
                    if (count <= 0)
                        return new Tuple<string, string>(Resource.PaidPeriodExpiredStandalone, text);

                    var end = GetNumeralResourceByCount(count, Resource.Day, Resource.DaysOne, Resource.DaysTwo);
                    return new Tuple<string, string>(string.Format(Resource.PaidPeriodStandalone, count, end), text);
                }

                var quota = TenantExtra.GetTenantQuota();
                long notifySize;
                long.TryParse(ConfigurationManager.AppSettings["web.tariff-notify.storage"] ?? "314572800", out notifySize); //300 MB
                if (notifySize > 0 && quota.MaxTotalSize - TenantStatisticsProvider.GetUsedSize() < notifySize)
                {
                    var head = string.Format(Resource.TariffExceedLimit, FileSizeComment.FilesSizeToString(quota.MaxTotalSize));
                    var text = String.Format(Resource.TariffExceedLimitInfoText, "<a href=\"" + TenantExtra.GetTariffPageLink() + "\">", "</a>");
                    return new Tuple<string, string>(head, text);
                }
            }

            if (tariff.State == TariffState.Delay)
            {
                var text = String.Format(Resource.TariffPaymentDelayText,
                                         "<a href=\"" + TenantExtra.GetTariffPageLink() + "\">", "</a>",
                                         tariff.DelayDueDate.Date.ToLongDateString());
                return new Tuple<string, string>(Resource.TariffPaymentDelay, text);
            }

            return null;
        }

        public static string GetNumeralResourceByCount(int count, string resource, string resourceOne, string resourceTwo)
        {
            var num = count % 100;
            if (num >= 11 && num <= 19)
            {
                return resourceTwo;
            }

            var i = count % 10;
            switch (i)
            {
                case (1):
                    return resource;
                case (2):
                case (3):
                case (4):
                    return resourceOne;
                default:
                    return resourceTwo;
            }
        }

        protected string RenderStatRequest()
        {
            if (string.IsNullOrEmpty(SetupInfo.StatisticTrackURL)) return string.Empty;

            var page = HttpUtility.UrlEncode(Page.AppRelativeVirtualPath.Replace("~", ""));
            return String.Format("<img style=\"display:none;\" src=\"{0}\"/>", SetupInfo.StatisticTrackURL + "&page=" + page);
        }

        protected string RenderCustomScript()
        {
            var sb = new StringBuilder();
            //custom scripts
            foreach (var script in SetupInfo.CustomScripts.Where(script => !String.IsNullOrEmpty(script)))
            {
                sb.AppendFormat("<script language=\"javascript\" src=\"{0}\" type=\"text/javascript\"></script>", script);
            }

            return sb.ToString();
        }

        protected bool EmailActivated
        {
            get { return CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).ActivationStatus == EmployeeActivationStatus.Activated; }
        }

        protected string ColorThemeClass
        {
            get { return ColorThemesSettings.GetColorThemesSettings(); }
        }

        #region Operations

        private void InitScripts()
        {
            AddStyles("~/skins/<theme_folder>/main.less");

            AddClientScript(typeof (MasterResources.MasterSettingsResources));
            AddClientScript(typeof (MasterResources.MasterUserResources));
            AddClientScript(typeof (MasterResources.MasterFileUtilityResources));
            AddClientScript(typeof (MasterResources.MasterCustomResources));

            InitProductSettingsInlineScript();
            InitStudioSettingsInlineScript();

            AddClientLocalizationScript(typeof (MasterResources.MasterLocalizationResources));
            AddClientLocalizationScript(typeof (MasterResources.MasterTemplateResources));
        }

        private void InitStudioSettingsInlineScript()
        {
            var showPromotions = SettingsManager.Instance.LoadSettings<PromotionsSettings>(TenantProvider.CurrentTenantID).Show;
            var showTips = SettingsManager.Instance.LoadSettingsFor<TipsSettings>(SecurityContext.CurrentAccount.ID).Show;

            var script = new StringBuilder();
            script.AppendFormat("window.ASC.Resources.Master.ShowPromotions={0};", showPromotions.ToString().ToLowerInvariant());
            script.AppendFormat("window.ASC.Resources.Master.ShowTips={0};", showTips.ToString().ToLowerInvariant());

            Page.RegisterInlineScript(script.ToString(), true, false);
        }

        private void InitProductSettingsInlineScript()
        {
            var isAdmin = WebItemSecurity.IsProductAdministrator(CommonLinkUtility.GetProductID(), SecurityContext.CurrentAccount.ID);

            var script = new StringBuilder();
            script.AppendFormat("window.ASC.Resources.Master.IsProductAdmin={0};", isAdmin.ToString().ToLowerInvariant());

            Page.RegisterInlineScript(script.ToString(), true, false);
        }

        #region Style

        public void AddStyles(Control control)
        {
            if (HeadStyles == null) return;

            HeadStyles.Controls.Add(control);
        }

        public void AddStyles(string src)
        {
            if (src.Contains(ColorThemesSettings.ThemeFolderTemplate))
            {
                if (ThemeStyles == null) return;

                ThemeStyles.Styles.Add(ResolveUrl(ColorThemesSettings.GetThemeFolderName(src)));
            }
            else
            {
                if (HeadStyles == null) return;

                HeadStyles.Styles.Add(src);
            }
        }

        #endregion

        #region Scripts

        public void AddBodyScripts(string src)
        {
            if (BodyScripts == null) return;
            BodyScripts.Scripts.Add(src);
        }

        public void AddBodyScripts(Control control)
        {
            if (BodyScripts == null) return;
            BodyScripts.Controls.Add(control);
        }


        public void RegisterInlineScript(string script, bool beforeBodyScripts, bool onReady)
        {
            if (!beforeBodyScripts)
                InlineScript.Scripts.Add(new Tuple<string, bool>(script, onReady));
            else
                InlineScriptBefore.Scripts.Add(new Tuple<string, bool>(script, onReady));
        }

        #endregion

        #region Content

        public void AddBodyContent(Control control)
        {
            PageContent.Controls.Add(control);
        }

        public void SetBodyContent(Control control)
        {
            PageContent.Controls.Clear();
            AddBodyContent(control);
        }

        #endregion

        #region ClientScript

        public void AddClientScript(Type type)
        {
            baseTemplateMasterScripts.Includes.Add(type);
        }

        public void AddClientLocalizationScript(Type type)
        {
            clientLocalizationScript.Includes.Add(type);
        }

        #endregion

        #endregion
    }
}