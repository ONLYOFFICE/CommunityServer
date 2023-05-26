/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Web;

using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Controls;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Masters;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.UserControls.Common.LoaderPage;

namespace ASC.Web.Files
{
    public partial class _Default : MainPage, IStaticBundle
    {
        private bool shareDialogV115 = Classes.Global.EnableShareDialogV115;
        private Folder externalFolder;

        protected UserInfo CurrentUser;

        protected AutoCleanUpData CleanUpSettings;

        protected List<FileShare> DefaultSharingAccessRightsSetting;

        protected bool Desktop
        {
            get { return Request.DesktopApp(); }
        }

        protected override void OnPreInit(EventArgs e)
        {
            CheckAuth();

            base.OnPreInit(e);
        }

        private void CheckAuth()
        {
            var shareLinkKey = Request[FilesLinkUtility.FolderShareKey] ?? string.Empty;

            if (string.IsNullOrEmpty(shareLinkKey))
            {
                return;
            }

            if (FileShareLink.CheckSignatureKey(shareLinkKey, out externalFolder) && 
                externalFolder != null && 
                externalFolder.RootFolderType != FolderType.TRASH && 
                !(externalFolder.ProviderEntry && !FilesSettings.EnableThirdParty))
            {
                MayNotAuth = true;
            }
            else
            {
                Response.Redirect(FileShareLink.GetPasswordProtectedFolderLink(shareLinkKey));
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ((BasicTemplate)Master).Master
                                   .AddStaticStyles(ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark ? GetStaticDarkStyleSheet() : GetStaticStyleSheet())
                                   .AddStaticBodyScripts(GetStaticJavaScript());

            CurrentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            CleanUpSettings = FilesSettings.AutomaticallyCleanUp;

            DefaultSharingAccessRightsSetting = FilesSettings.DefaultSharingAccessRights;

            LoadControls();

            if (CoreContext.Configuration.Personal)
            {
                PersonalProcess();
            }
        }

        public ScriptBundleData GetStaticJavaScript()
        {
            var src = shareDialogV115
                ? new List<string> { "Controls/AccessRights/accessrights.js", "Controls/AccessRights/formfilling.js" }
                : new List<string> { "Controls/SharingDialog/sharingdialog.js", "Controls/UnsubscribeDialog/unsubscribedialog.js" };

            src.AddRange(new string[]
                {
                    "Controls/ChunkUploadDialog/chunkuploadmanager.js",
                    "Controls/ConvertFile/convertfile.js",
                    "Controls/ConvertFile/confirmconvert.js",
                    "Controls/CreateMenu/createmenu.js",
                    "Controls/EmptyFolder/emptyfolder.js",
                    "Controls/FileChoisePopup/filechoisepopup.js",
                    "Controls/ThirdParty/thirdparty.js",
                    "Controls/Tree/treebuilder.js",
                    "Controls/Tree/tree.js"
                });

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
                       .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r, src.ToArray());
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            var src = shareDialogV115
                ? new List<string> { "Controls/AccessRights/accessrights.less", "Controls/AccessRights/formfilling.less" }
                : new List<string> { "Controls/SharingDialog/sharingdialog.less" };
            src.AddRange(new string[]
            {
                    "Controls/AppBanner/appbanner.less",
                    "Controls/ChunkUploadDialog/chunkuploaddialog.less",
                    "Controls/ContentList/contentlist.less",
                    "Controls/ConvertFile/convertfile.less",
                    "Controls/ConvertFile/confirmconvert.less",
                    "Controls/EmptyFolder/emptyfolder.less",
                    "Controls/FileChoisePopup/filechoisepopup.less",
                    "Controls/MainContent/maincontent.less",
                    "Controls/MoreFeatures/css/morefeatures.less",
                    "Controls/ThirdParty/thirdparty.less",
                    "Controls/Tree/treebuilder.less",
                    "Controls/Tree/tree.less"
            });

            return (StyleBundleData)
                   new StyleBundleData("files", "files")
                       .AddSource(PathProvider.GetFileStaticRelativePath, "common.less")
                       .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r, src.ToArray());

        }
        public StyleBundleData GetStaticDarkStyleSheet()
        {
            var src = shareDialogV115
                ? new List<string> { "Controls/AccessRights/dark-accessrights.less", "Controls/AccessRights/dark-formfilling.less" }
                : new List<string> { "Controls/SharingDialog/dark-sharingdialog.less" };
            src.AddRange(new string[]
            {
                    "Controls/AppBanner/dark-appbanner.less",
                    "Controls/ChunkUploadDialog/dark-chunkuploaddialog.less",
                    "Controls/ContentList/dark-contentlist.less",
                    "Controls/ConvertFile/dark-convertfile.less",
                    "Controls/ConvertFile/confirmconvert.less",
                    "Controls/EmptyFolder/emptyfolder.less",
                    "Controls/FileChoisePopup/filechoisepopup.less",
                    "Controls/MainContent/dark-maincontent.less",
                    "Controls/MoreFeatures/css/dark-morefeatures.less",
                    "Controls/ThirdParty/dark-thirdparty.less",
                    "Controls/Tree/treebuilder.less",
                    "Controls/Tree/dark-tree.less"
            });

            return (StyleBundleData)
                   new StyleBundleData("dark-files", "files")
                       .AddSource(PathProvider.GetFileStaticRelativePath, "dark-common.less")
                       .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r, src.ToArray());
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
                                   && !Desktop && SecurityContext.IsAuthenticated;

            var mainButton = (MainButton)LoadControl(MainButton.Location);
            mainButton.ExternalFolderFullAccess = externalFolder != null && externalFolder.Access == FileShare.ReadWrite;
            CreateButtonHolder.Controls.Add(mainButton);

            var mainMenu = (MainMenu)LoadControl(MainMenu.Location);
            mainMenu.EnableThirdParty = enableThirdParty;
            mainMenu.Desktop = Desktop;
            mainMenu.ExternalFolder = externalFolder;

            CommonSideHolder.Controls.Add(mainMenu);

            if (Request.SailfishApp())
            {
                CommonContainerHolder.Controls.Add(LoadControl(Sailfish.Location));
            }

            FilterHolder.Controls.Add(LoadControl(MainContentFilter.Location));

            var mainContent = (MainContent)LoadControl(MainContent.Location);
            mainContent.TitlePage = FilesCommonResource.TitlePage;

            if (externalFolder != null)
            {
                mainContent.ExternalFolderIDCurrentRoot = externalFolder.ID;
            }

            CommonContainerHolder.Controls.Add(mainContent);

            if (CoreContext.Configuration.Personal
                && !Desktop)
                CommonContainerHolder.Controls.Add(LoadControl(MoreFeatures.Location));

            if (shareDialogV115)
            {
                CommonContainerHolder.Controls.Add(LoadControl(AccessRights.Location));
            }
            else
            {
                CommonContainerHolder.Controls.Add(LoadControl(SharingDialog.Location));
                if (!CoreContext.Configuration.Personal)
                {
                    CommonContainerHolder.Controls.Add(LoadControl(UnsubscribeDialog.Location));
                }
            }

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
                && SetupInfo.DisplayMobappBanner("files") && SecurityContext.IsAuthenticated)
            {
                AppBannerHolder.Controls.Add(LoadControl(AppBanner.Location));
            }
        }

        private void PersonalProcess()
        {
            if (PersonalSettings.IsNewUser)
            {
                PersonalSettings.IsNewUser = false;

                Classes.Global.Logger.Info("New personal user " + SecurityContext.CurrentAccount.ID);
            }

            if (PersonalSettings.IsNotActivated)
            {
                if (CurrentUser.ActivationStatus != EmployeeActivationStatus.NotActivated) return;

                try
                {
                    SecurityContext.CurrentAccount = ASC.Core.Configuration.Constants.CoreSystem;
                    CurrentUser.ActivationStatus = EmployeeActivationStatus.Activated;
                    CoreContext.UserManager.SaveUserInfo(CurrentUser);
                }
                finally
                {
                    SecurityContext.Logout();
                }

                SecurityContext.CurrentUser = CurrentUser.ID;

                PersonalSettings.IsNotActivated = false;
                Classes.Global.Logger.InfoFormat("User {0} ActivationStatus - Activated", CurrentUser.ID);

                StudioNotifyService.Instance.UserHasJoin();
                StudioNotifyService.Instance.SendUserWelcomePersonal(CurrentUser);
            }
        }
    }
}