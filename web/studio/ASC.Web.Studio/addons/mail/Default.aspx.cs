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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security;
using System.Web;
using System.Web.Configuration;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Mail.Controls;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Common.HelpCenter;
using ASC.Web.Studio.UserControls.Common.Support;
using ASC.Web.Studio.UserControls.Common.VideoGuides;
using ASC.Web.Studio.Utility;
using ASC.Web.Core;
using ASC.Web.CRM.Configuration;
using ASC.Web.Studio.UserControls.Common.LoaderPage;
using Newtonsoft.Json;
using SecurityContext = ASC.Core.SecurityContext;
using ASC.Web.Studio.UserControls.Common.UserForum;

namespace ASC.Web.Mail
{
    public partial class MailPage : MainPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) // Redirect to home page if user hasn't permissions or not authenticated.
            {
                Response.Redirect("/");
            }

            _manageFieldPopup.Options.IsPopup = true;
            _commonPopup.Options.IsPopup = true;

            Page.Title = HeaderStringHelper.GetPageTitle(Resources.MailResource.MailTitle);

            ProductEntryPoint.ConfigurePortal();

            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));

            MailSidePanelContainer.Controls.Add(LoadControl(TagBox.Location));

            MailControlContainer.Controls.Add(LoadControl(MailBox.Location));

            var help_center = (HelpCenter)LoadControl(HelpCenter.Location);
            help_center.IsSideBar = true;
            sideHelpCenter.Controls.Add(help_center);

            SupportHolder.Controls.Add(LoadControl(Support.Location));
            VideoGuides.Controls.Add(LoadControl(VideoGuidesControl.Location));
            UserForumHolder.Controls.Add(LoadControl(UserForum.Location));

            PeopleGroupLocalize.Text = CustomNamingPeople.Substitute<Resources.MailResource>("FilterByGroup");

            // If user doesn't have any mailboxes this will showed.
            var mail_box_manager = new ASC.Mail.Aggregator.MailBoxManager(0);
            if (!mail_box_manager.HasMailboxes(TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID.ToString()))
                BlankModalPH.Controls.Add(LoadControl(BlankModal.Location));

            if (!IsCrmAvailable())
            {
                crmContactsContainer.Visible = false;
            }

            if (!IsPeopleAvailable())
            {
                tlContactsContainer.Visible = false;
            }

            Page.RegisterBodyScripts(LoadControl(VirtualPathUtility.ToAbsolute("~/addons/mail/masters/BodyScripts.ascx")));
            Page.RegisterStyleControl(LoadControl(VirtualPathUtility.ToAbsolute("~/addons/mail/masters/Styles.ascx")));
            Page.RegisterClientLocalizationScript(typeof(Masters.ClientScripts.ClientLocalizationResources));
            Page.RegisterClientLocalizationScript(typeof(Masters.ClientScripts.ClientTemplateResources));

            Master.DisabledHelpTour = true;

            Page.RegisterInlineScript(BuildErrorConstants());
        }

        public String GetServiceCheckTimeout()
        {
            return WebConfigurationManager.AppSettings["ServiceCheckTimeout"] ?? "30";
        }

        public String GetMailServicePath()
        {
            return VirtualPathUtility.ToAbsolute(WebConfigurationManager.AppSettings["ASCMailService"] ?? "~/addons/mail/Service.svc/");
        }

        public String GetApiBaseUrl()
        {
            return SetupInfo.WebApiBaseUrl;
        }

        protected bool IsAdministrator
        {
            get { return CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID); }
        }

        protected bool IsPersonal
        {
            get { return CoreContext.Configuration.Personal; }
        }

        public String GetMailFaqUri()
        {
            return CommonLinkUtility.GetHelpLink() + "troubleshooting/mail.aspx";
        }

        public static String GetMailSupportUri()
        {
            return WebConfigurationManager.AppSettings["mail.support-url"] ?? "mailto:support@onlyoffice.com";
        }

        public static String GetImportOAuthAccessUrl()
        {
            return WebConfigurationManager.AppSettings["mail.import-oauth-url"];
        }

        public static bool IsTurnOnOAuth()
        {
            var config_value = WebConfigurationManager.AppSettings["mail.googleOAuth"];
            return (string.IsNullOrEmpty(config_value) || Convert.ToBoolean(config_value)) // default is true
                && !string.IsNullOrEmpty(GetImportOAuthAccessUrl()); 
        }

        public static bool IsTurnOnServer()
        {
            return SetupInfo.IsVisibleSettings<AdministrationPage>();
        }

        public static bool IsTurnOnAttachmentsGroupOperations()
        {
            var config_value = WebConfigurationManager.AppSettings["mail.attachments-group-operations"];
            return !string.IsNullOrEmpty(config_value) && Convert.ToBoolean(config_value); // default is false
        }

        public bool IsCrmAvailable()
        {
            return WebItemSecurity.IsAvailableForUser(WebItemManager.CRMProductID.ToString(), SecurityContext.CurrentAccount.ID);
        }

        public bool IsPeopleAvailable()
        {
            return WebItemSecurity.IsAvailableForUser(WebItemManager.PeopleProductID.ToString(), SecurityContext.CurrentAccount.ID);
        }

        public String GetMailDownloadHandlerUri()
        {
            return WebConfigurationManager.AppSettings["mail.download-handler-url"] ?? "/addons/mail/httphandlers/download.ashx?attachid={0}";
        }

        public String GetMailDownloadAllHandlerUri()
        {
            return WebConfigurationManager.AppSettings["mail.download-all-handler-url"] ?? "/addons/mail/httphandlers/downloadall.ashx?messageid={0}";
        }

        public String GetMailViewDocumentHandlerUri()
        {
            return WebConfigurationManager.AppSettings["mail.view-document-handler-url"] ?? "/addons/mail/httphandlers/viewdocument.ashx?attachid={0}";
        }

        public String GetMailEditDocumentHandlerUri()
        {
            return WebConfigurationManager.AppSettings["mail.edit-document-handler-url"] ?? "/addons/mail/httphandlers/editdocument.ashx?attachid={0}";
        }

        protected List<ASC.Mail.Aggregator.Common.MailBox> GetMailboxes()
        {
            var mailBoxManager = new ASC.Mail.Aggregator.MailBoxManager(0);
            var mailBoxes = mailBoxManager.GetMailBoxes(TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID.ToString());
            return mailBoxes;
        }

        private string BuildErrorConstants()
        {
            var error_constants = new
                {
                    COR_E_SECURITY = new SecurityException().HResult,
                    COR_E_ARGUMENT = new ArgumentException().HResult,
                    COR_E_ARGUMENTOUTOFRANGE = new ArgumentOutOfRangeException().HResult,
                    COR_E_INVALIDCAST = new InvalidCastException().HResult,
                    COR_E_ARGUMENTNULL = new ArgumentNullException().HResult,
                    COR_E_INVALIDDATA = new InvalidDataException().HResult,
                    COR_E_INVALIDOPERATION = new InvalidOperationException().HResult,
                    COR_E_DUPLICATENANE = new DuplicateNameException().HResult

                };

            var constants = JsonConvert.SerializeObject(error_constants);

            return string.Format("\nASC.Mail.ErrorConstants = {0};", constants);
        }
    }
}