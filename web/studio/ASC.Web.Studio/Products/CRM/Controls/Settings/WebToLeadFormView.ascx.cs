/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using ASC.MessagingSystem;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using AjaxPro;
using ASC.CRM.Core;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;
using ASC.Core;
using System.Text;

namespace ASC.Web.CRM.Controls.Settings
{
    [AjaxNamespace("AjaxPro.WebToLeadFormView")]
    public partial class WebToLeadFormView : BaseUserControl
    {
        #region Members

        protected string _webFormKey;

        #endregion

        #region Properties

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Settings/WebToLeadFormView.ascx"); }
        }

        protected string GetHandlerUrl
        {
            get { return CommonLinkUtility.ServerRootPath + PathProvider.BaseAbsolutePath + "HttpHandlers/WebToLeadFromHandler.ashx".ToLower(); }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            Utility.RegisterTypeForAjax(typeof(WebToLeadFormView));

            _webFormKey = Global.TenantSettings.WebFormKey.ToString();

            Page.RegisterClientScript(typeof(Masters.ClientScripts.WebToLeadFormViewData));
            RegisterScript();


            RegisterClientScriptHelper.DataUserSelectorListView(Page, "_ContactManager", null);

            RegisterClientScriptHelper.DataUserSelectorListView(
                Page, "_Notify",
                new Dictionary<Guid, string>
                    {
                        {
                            SecurityContext.CurrentAccount.ID,
                            SecurityContext.CurrentAccount.Name.HtmlEncode()
                        }
                    });
        }

        [AjaxMethod]
        public string ChangeWebFormKey()
        {
            var tenantSettings = Global.TenantSettings;

            tenantSettings.WebFormKey = Guid.NewGuid();

            SettingsManager.Instance.SaveSettings(tenantSettings, TenantProvider.CurrentTenantID);
            MessageService.Send(HttpContext.Current.Request, MessageAction.WebsiteContactFormUpdatedKey);

            return tenantSettings.WebFormKey.ToString();
        }

        private void RegisterScript()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(@"ASC.CRM.SettingsPage.WebToLeadFormView.init(""{0}"");",
                            CommonLinkUtility.GetFullAbsolutePath("~/products/crm/httphandlers/webtoleadfromhandler.ashx")
                );

            Page.RegisterInlineScript(sb.ToString());
        }
    }
}