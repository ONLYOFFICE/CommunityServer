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
using System.Web.Configuration;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Core.HelpCenter;
using ASC.Web.Studio.Masters;

namespace ASC.Web.Studio
{
    public partial class Welcome : MainPage
    {
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            if (CoreContext.Configuration.Personal)
                Context.Response.Redirect(FilesLinkUtility.FilesBaseAbsolutePath);
        }

    //    protected String docsScript { get; set; }
        protected bool showProjects { get; set; }
        protected bool showCRM { get; set; }
        protected string buttonWait { get; set; }
        protected string buttonContinue { get; set; }
        protected string videoTitle { get; set; }

        //protected string culture {
        //    get { return CoreContext.TenantManager.GetCurrentTenant().GetCulture().Name; }
        //}

        protected bool IsVideoPage()
        {
            return (Request["module"] != null);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
          //  Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/firsttime/js/start.js"));
            AjaxPro.Utility.RegisterTypeForAjax(typeof(UserVideoSettings));

            UserHelpTourHelper.IsNewUser = true;

            var studioMaster = (BaseTemplate)Master;
            studioMaster.DisabledSidePanel = true;

            if (studioMaster != null)
            {
                //top panel
                studioMaster.TopStudioPanel.DisableProductNavigation = true;
                studioMaster.TopStudioPanel.DisableUserInfo = true;
                studioMaster.TopStudioPanel.DisableSearch = true;
                studioMaster.TopStudioPanel.DisableSettings = true;
                studioMaster.TopStudioPanel.DisableTariff = true;
            }

            bool isOwner = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsOwner();
            buttonContinue = isOwner ? Resources.Resource.CreatingPortalContinue : Resources.Resource.CreatingPortalContinueUser;
            buttonWait = isOwner ? Resources.Resource.CreatingPortalWaiting : Resources.Resource.CreatingPortalWaitingUser;
            videoTitle = isOwner ? 
                String.Format(Resources.Resource.WizardVideoTitle, "<b>", "</b>", "<span class='gray-text'>", "</span>") :
                String.Format(Resources.Resource.WizardVideoTitleUser, "<b>", "</b>", "<span class='gray-text'>", "</span>");

            var items = WebItemManager.Instance.GetItems(Web.Core.WebZones.WebZoneType.StartProductList);
            var projects = (Product) items.Find(r => r.ProductClassName == "projects");
            var crm = (Product) items.Find(r => r.ProductClassName == "crm");

            if ((!items.Contains(projects) && !items.Contains(crm)))
            {
                if (string.IsNullOrEmpty(Request["module"]))
                {
                    Response.Redirect(Request.RawUrl + "?module=documents");
                }
            }
           
            showProjects = items.Contains(projects);
            showCRM = items.Contains(crm);
     //       docsScript = WebConfigurationManager.AppSettings["files.docservice.url.preloader"];
        }
    }
}