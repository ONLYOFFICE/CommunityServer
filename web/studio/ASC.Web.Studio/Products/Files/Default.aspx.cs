/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.IO;
using System.Web;
using ASC.Core;
using ASC.Core.Common.Notify.Push;
using ASC.Core.Users;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Files;
using ASC.Web.Core.Mobile;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Controls;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Masters;
using ASC.Web.Files.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.UserControls.Common.LoaderPage;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Files
{
    public partial class _Default : MainPage, IStaticBundle
    {
        private bool AddCustomScript;

        protected bool DisplayAppsBanner;

        protected bool Desktop
        {
            get { return Request.DesktopApp(); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ((BasicTemplate)Master).Master
                                   .AddStaticStyles(GetStaticStyleSheet())
                                   .AddStaticBodyScripts(GetStaticJavaScript());

            LoadControls();

            var mobileAppRegistrator = new CachedMobileAppInstallRegistrator(new MobileAppInstallRegistrator());
            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            DisplayAppsBanner =
                SetupInfo.DisplayMobappBanner("files")
                && !CoreContext.Configuration.Standalone
                && !TenantExtra.GetTenantQuota().Trial
                && !mobileAppRegistrator.IsInstallRegistered(currentUser.Email, MobileAppType.IosDocuments);

            if (CoreContext.Configuration.Personal)
            {
                PersonalProcess();
            }


            #region third-party scripts

            if (AddCustomScript)
            {
                using (var streamReader = new StreamReader(HttpContext.Current.Server.MapPath(PathProvider.GetFileControlPath("AnalyticsPersonalFirstVisit.js"))))
                {
                    var yaScriptText = streamReader.ReadToEnd();
                    Page.RegisterInlineScript(yaScriptText);
                }
            }

            #endregion
        }

        public ScriptBundleData GetStaticJavaScript()
        {
            return (ScriptBundleData)
                   new ScriptBundleData("files", "files")
                       .AddSource(PathProvider.GetFileStaticRelativePath,
                                  "auth.js",
                                  "common.js",
                                  "filter.js",
                                  "templatemanager.js",
                                  "servicemanager.js",
                                  "ui.js",
                                  "mousemanager.js",
                                  "markernew.js",
                                  "actionmanager.js",
                                  "anchormanager.js",
                                  "foldermanager.js",
                                  "eventhandler.js",
                                  "socketmanager.js"
                       )
                       .AddSource(ResolveUrl,
                                  "~/js/third-party/jquery/jquery.mousewheel.js",
                                  "~/js/third-party/jquery/jquery.uri.js",
                                  "~/js/uploader/jquery.fileupload.js")
                       .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r,
                                  "controls/accessrights/accessrights.js",
                                  "controls/chunkuploaddialog/chunkuploadmanager.js",
                                  "controls/convertfile/convertfile.js",
                                  "controls/createmenu/createmenu.js",
                                  "controls/emptyfolder/emptyfolder.js",
                                  "controls/thirdparty/thirdparty.js",
                                  "controls/tree/treebuilder.js",
                                  "controls/tree/tree.js"
                       );
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return (StyleBundleData)
                   new StyleBundleData("files", "files")
                       .AddSource(PathProvider.GetFileStaticRelativePath, "common.css")
                       .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r,
                                  "controls/accessrights/accessrights.css",
                                  "controls/chunkuploaddialog/chunkuploaddialog.css",
                                  "controls/contentlist/contentlist.css",
                                  "controls/convertfile/convertfile.css",
                                  "controls/emptyfolder/emptyfolder.css",
                                  "controls/maincontent/maincontent.css",
                                  "controls/morefeatures/css/morefeatures.css",
                                  "controls/thirdparty/thirdparty.css",
                                  "controls/tree/treebuilder.css",
                                  "controls/tree/tree.css");
        }

        private void LoadControls()
        {
            if (Desktop)
            {
                Master.Master.DisabledTopStudioPanel = true;
                Master.Master.EnabledWebChat = false;
            }

            var enableThirdParty = ThirdpartyConfiguration.SupportInclusion
                                   && !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()
                                   && (Classes.Global.IsAdministrator
                                       || FilesSettings.EnableThirdParty
                                       || CoreContext.Configuration.Personal)
                                   && !Desktop;

            var mainMenu = (MainMenu)LoadControl(MainMenu.Location);
            mainMenu.EnableThirdParty = enableThirdParty;
            mainMenu.Desktop = Desktop;
            CommonSideHolder.Controls.Add(mainMenu);

            if (Request.SailfishApp())
            {
                CommonContainerHolder.Controls.Add(LoadControl(Files.Controls.Sailfish.Location));
            }

            var mainContent = (MainContent)LoadControl(MainContent.Location);
            mainContent.TitlePage = FilesCommonResource.TitlePage;
            CommonContainerHolder.Controls.Add(mainContent);

            if (CoreContext.Configuration.Personal
                && !Desktop)
                CommonContainerHolder.Controls.Add(LoadControl(MoreFeatures.Location));

            CommonContainerHolder.Controls.Add(LoadControl(AccessRights.Location));

            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));

            if (enableThirdParty)
            {
                SettingPanelHolder.Controls.Add(LoadControl(ThirdParty.Location));
            }

            if (Desktop)
            {
                CommonContainerHolder.Controls.Add(LoadControl(Files.Controls.Desktop.Location));
            }
        }

        private void PersonalProcess()
        {
            if (PersonalSettings.IsNewUser)
            {
                PersonalSettings.IsNewUser = false;
                AddCustomScript = SetupInfo.CustomScripts.Length != 0 && !SetupInfo.IsSecretEmail(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).Email);

                Classes.Global.Logger.Info("New personal user " + SecurityContext.CurrentAccount.ID);
            }

            if (PersonalSettings.IsNotActivated)
            {
                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

                if (user.ActivationStatus != EmployeeActivationStatus.NotActivated) return;

                try
                {
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                    user.ActivationStatus = EmployeeActivationStatus.Activated;
                    CoreContext.UserManager.SaveUserInfo(user);
                }
                finally
                {
                    SecurityContext.Logout();
                }

                SecurityContext.AuthenticateMe(user.ID);

                PersonalSettings.IsNotActivated = false;
                Classes.Global.Logger.InfoFormat("User {0} ActivationStatus - Activated", user.ID);

                StudioNotifyService.Instance.UserHasJoin();
                StudioNotifyService.Instance.SendUserWelcomePersonal(user);
            }
        }
    }
}