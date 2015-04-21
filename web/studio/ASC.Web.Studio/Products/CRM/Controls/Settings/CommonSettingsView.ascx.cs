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
using System.Web;
using System.Collections.Generic;
using System.Linq;
using ASC.CRM.Core;
using ASC.MessagingSystem;
using ASC.Web.Core.Mobile;
using ASC.Web.CRM.Classes;
using ASC.Web.Studio.Utility;
using ASC.Common.Threading.Progress;
using System.Text;
using ASC.Web.CRM.Resources;

namespace ASC.Web.CRM.Controls.Settings
{
    public partial class CommonSettingsView : BaseUserControl
    {
        #region Property

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Settings/CommonSettingsView.ascx"); }
        }

        protected List<CurrencyInfo> BasicCurrencyRates { get; set; }
        protected List<CurrencyInfo> OtherCurrencyRates { get; set; }

        protected bool MobileVer = false;

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            MobileVer = MobileDetector.IsMobile;

            BasicCurrencyRates = CurrencyProvider.GetBasic().Where(n => n.IsConvertable).ToList();
            OtherCurrencyRates = CurrencyProvider.GetOther().Where(n => n.IsConvertable).ToList();

            var settings = Global.TenantSettings.SMTPServerSetting;
            Page.JsonPublisher(settings, "SMTPSettings");
            RegisterScript();
        }

        private void RegisterScript()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(@"ASC.CRM.SettingsPage.initSMTPSettings('{0}');",
                            String.Format(CRMSettingResource.pattern_TestMailSMTPMainBody,
                                          ASC.Core.CoreContext.TenantManager.GetTenant(TenantProvider.CurrentTenantID).TenantDomain).HtmlEncode().ReplaceSingleQuote()
                );

            Page.RegisterInlineScript(sb.ToString());
        }

        public bool IsSelectedBidCurrency(String abbreviation)
        {
            return string.Compare(abbreviation, Global.TenantSettings.DefaultCurrency.Abbreviation, StringComparison.OrdinalIgnoreCase) == 0;
        }

        #endregion
    }
}