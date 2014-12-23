/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
using AjaxPro;
using ASC.Common.Threading.Progress;
using System.Text;
using ASC.Web.CRM.Resources;

namespace ASC.Web.CRM.Controls.Settings
{
    [AjaxNamespace("AjaxPro.CommonSettingsView")]
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
            _sendTestMailContainer.Options.IsPopup = true;
            Utility.RegisterTypeForAjax(typeof(CommonSettingsView));

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

        [AjaxMethod]
        public IProgressItem StartExportData()
        {
            if (!CRMSecurity.IsAdmin) throw new Exception();

            MessageService.Send(HttpContext.Current.Request, MessageAction.CrmAllDataExported);

            return ExportToCSV.Start();
        }

        [AjaxMethod]
        public IProgressItem GetStatus()
        {
            return ExportToCSV.GetStatus();
        }

        [AjaxMethod]
        public IProgressItem Cancel()
        {
            ExportToCSV.Cancel();

            return GetStatus();
        }

        #endregion
    }
}