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
using System.IO;
using System.Web;

using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Controls;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Masters;
using ASC.Web.Files.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.UserControls.Common.LoaderPage;

namespace ASC.Web.Files
{
    public partial class _Default : MainPage, IStaticBundle
    {
        private bool AddCustomScript;

        protected UserInfo CurrentUser;

        protected bool Desktop
        {
            get { return Request.DesktopApp(); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ((BasicTemplate)Master).Master
                                   .AddStaticStyles(GetStaticStyleSheet())
                                   .AddStaticBodyScripts(GetStaticJavaScript());

            CurrentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            LoadControls();

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
                                  "Controls/AccessRights/accessrights.js",
                                  "Controls/AppBanner/appbanner.js",
                                  "Controls/ChunkUploadDialog/chunkuploadmanager.js",
                                  "Controls/ConvertFile/convertfile.js",
                                  "Controls/ConvertFile/confirmconvert.js",
                                  "Controls/CreateMenu/createmenu.js",
                                  "Controls/EmptyFolder/emptyfolder.js",
                                  "Controls/ThirdParty/thirdparty.js",
                                  "Controls/Tree/treebuilder.js",
                                  "Controls/Tree/tree.js"
                       );
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return (StyleBundleData)
                   new StyleBundleData("files", "files")
                       .AddSource(PathProvider.GetFileStaticRelativePath, "common.css")
                       .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r,
                                  "Controls/AccessRights/accessrights.css",
                                  "Controls/AppBanner/appbanner.css",
                                  "Controls/ChunkUploadDialog/chunkuploaddialog.css",
                                  "Controls/ContentList/contentlist.css",
                                  "Controls/ConvertFile/convertfile.css",
                                  "Controls/ConvertFile/confirmconvert.css",
                                  "Controls/EmptyFolder/emptyfolder.css",
                                  "Controls/MainContent/maincontent.css",
                                  "Controls/MoreFeatures/css/morefeatures.css",
                                  "Controls/ThirdParty/thirdparty.css",
                                  "Controls/Tree/treebuilder.css",
                                  "Controls/Tree/tree.css");
        }

        private void LoadControls()
        {
            if (Desktop)
            {
                Master.Master.DisabledTopStudioPanel = true;
                Master.Master.EnabledWebChat = false;
            }

            var enableThirdParty = ThirdpartyConfiguration.SupportInclusion
                                   && !CurrentUser.IsVisitor()
                                   && (Classes.Global.IsAdministrator
                                       || FilesSettings.EnableThirdParty
                                       || CoreContext.Configuration.Personal)
                                   && !Desktop;

            CreateButtonHolder.Controls.Add(LoadControl(MainButton.Location));

            var mainMenu = (MainMenu)LoadControl(MainMenu.Location);
            mainMenu.EnableThirdParty = enableThirdParty;
            mainMenu.Desktop = Desktop;
            CommonSideHolder.Controls.Add(mainMenu);

            if (Request.SailfishApp())
            {
                CommonContainerHolder.Controls.Add(LoadControl(Sailfish.Location));
            }

            FilterHolder.Controls.Add(LoadControl(MainContentFilter.Location));

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
                SettingPanelHolder.Controls.Add(LoadControl(Files.Controls.ThirdParty.Location));
            }

            if (Desktop)
            {
                CommonContainerHolder.Controls.Add(LoadControl(Files.Controls.Desktop.Location));
            }

            if (!Desktop
                && SetupInfo.DisplayMobappBanner("files"))
            {
                AppBannerHolder.Controls.Add(LoadControl(AppBanner.Location));
            }
        }

        private void PersonalProcess()
        {
            if (PersonalSettings.IsNewUser)
            {
                PersonalSettings.IsNewUser = false;
                AddCustomScript = SetupInfo.CustomScripts.Length != 0 && !SetupInfo.IsSecretEmail(CurrentUser.Email);

                Classes.Global.Logger.Info("New personal user " + SecurityContext.CurrentAccount.ID);
            }

            if (PersonalSettings.IsNotActivated)
            {
                if (CurrentUser.ActivationStatus != EmployeeActivationStatus.NotActivated) return;

                try
                {
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                    CurrentUser.ActivationStatus = EmployeeActivationStatus.Activated;
                    CoreContext.UserManager.SaveUserInfo(CurrentUser);
                }
                finally
                {
                    SecurityContext.Logout();
                }

                SecurityContext.AuthenticateMe(CurrentUser.ID);

                PersonalSettings.IsNotActivated = false;
                Classes.Global.Logger.InfoFormat("User {0} ActivationStatus - Activated", CurrentUser.ID);

                StudioNotifyService.Instance.UserHasJoin();
                StudioNotifyService.Instance.SendUserWelcomePersonal(CurrentUser);
            }
        }
    }
}