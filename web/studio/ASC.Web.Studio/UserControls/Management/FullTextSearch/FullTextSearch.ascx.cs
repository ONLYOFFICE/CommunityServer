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
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.MessagingSystem;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using AjaxPro;
using ASC.ElasticSearch;
using ASC.ElasticSearch.Service;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.FullTextSearch, "~/UserControls/Management/FullTextSearch/FullTextSearch.ascx")]
    [AjaxNamespace("FullTextSearch")]
    public partial class FullTextSearch : UserControl
    {
        protected Settings CurrentSettings
        {
            get
            {
                return CoreContext.Configuration.GetSection<Settings>(Tenant.DEFAULT_TENANT) ?? Settings.Default;
            }
        }

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if(!CoreContext.Configuration.Standalone)
                Response.Redirect("~/Management.aspx");

            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts("~/UserControls/Management/FullTextSearch/js/fulltextsearch.js")
                .RegisterStyle("~/UserControls/Management/FullTextSearch/css/fulltextsearch.css");

            HelpLink = CommonLinkUtility.GetHelpLink();
        }

        [AjaxMethod]
        public void Save(Settings settings)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            CoreContext.Configuration.SaveSection(Tenant.DEFAULT_TENANT, settings);

            MessageService.Send(HttpContext.Current.Request, MessageAction.FullTextSearchSetting);
        }

        [AjaxMethod]
        public object Test()
        {
            return FactoryIndexer.CheckState() ?
                new { success = true, message = Resources.Resource.FullTextSearchServiceIsRunning } :
                new { success = false, message = Resources.Resource.FullTextSearchServiceIsNotRunning };
        }
    }
}