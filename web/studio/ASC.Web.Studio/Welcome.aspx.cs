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
using System.Web;
using System.Web.Configuration;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Core.HelpCenter;
using ASC.Web.Studio.Masters;
using System.IO;
using ASC.Web.Studio.Core;

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
          //  Page.RegisterBodyScripts("~/usercontrols/firsttime/js/start.js");
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


            #region third-party scripts

            var WelcomeScriptLocation = "~/UserControls/Common/ThirdPartyScripts/WelcomeScript.ascx";
            if (File.Exists(HttpContext.Current.Server.MapPath(WelcomeScriptLocation)) &&
                !CoreContext.Configuration.Standalone && !CoreContext.Configuration.Personal && SetupInfo.CustomScripts.Length != 0)
            {
                WelcomeScriptPlaceHolder.Controls.Add(LoadControl(WelcomeScriptLocation));
            }
            else
            {
                WelcomeScriptPlaceHolder.Visible = false;
            }

            #endregion

        }
    }
}