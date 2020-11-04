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
using System.Text;
using System.Web;
using ASC.Core;
using ASC.Web.CRM.Classes;
using ASC.Web.Studio.Utility;

namespace ASC.Web.CRM.Controls.Settings
{
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
            _webFormKey = Global.TenantSettings.WebFormKey.ToString();

            Page.RegisterClientScript(new Masters.ClientScripts.WebToLeadFormViewData());
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

        private void RegisterScript()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(@"ASC.CRM.SettingsPage.WebToLeadFormView.init(""{0}"");",
                            CommonLinkUtility.GetFullAbsolutePath("~/Products/CRM/HttpHandlers/webtoleadfromhandler.ashx")
                );

            Page.RegisterInlineScript(sb.ToString());
        }
    }
}