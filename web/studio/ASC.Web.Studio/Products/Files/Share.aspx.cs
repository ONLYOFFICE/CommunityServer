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
using System.Net;
using System.Text;
using System.Web;

using ASC.Files.Core;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Controls;
using ASC.Web.Files.Resources;
using ASC.Web.Studio;

using Newtonsoft.Json;

using Global = ASC.Web.Files.Classes.Global;

namespace ASC.Web.Files
{
    public partial class Share : MainPage, IStaticBundle
    {
        private bool shareDialogV115 = Global.EnableShareDialogV115;

        public static string Location
        {
            get { return FilesLinkUtility.FilesBaseAbsolutePath + "Share.aspx"; }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (string.IsNullOrEmpty(Request[FilesLinkUtility.FileId]))
            {
                Response.Redirect(PathProvider.StartURL
                                  + "#error/" +
                                  HttpUtility.UrlEncode(FilesCommonResource.ErrorMassage_FileNotFound));
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Master.Master.DisabledSidePanel = true;
            Master.Master.DisabledTopStudioPanel = true;
            Master.Master
                  .AddStaticStyles(ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark ? GetStaticDarkStyleSheet() : GetStaticStyleSheet())
                  .AddStaticBodyScripts(GetStaticJavaScript());

            if (shareDialogV115) {
                var accessRights = (AccessRights)LoadControl(AccessRights.Location);
                accessRights.IsPopup = false;
                CommonContainerHolder.Controls.Add(accessRights);
            } else {
                var sharingDialog = (SharingDialog)LoadControl(SharingDialog.Location);
                sharingDialog.IsPopup = false;
                CommonContainerHolder.Controls.Add(sharingDialog);
            }

            InitScript();
        }

        private void InitScript()
        {
            var fileId = Request[FilesLinkUtility.FileId];
            File file;
            try
            {
                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    file = fileDao.GetFile(fileId);
                }
            }
            catch (Exception ex)
            {
                Global.Logger.Error("ShareLink", ex);

                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            if (file == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }
            if (!Global.GetFilesSecurity().CanRead(file))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            var originForPost = "*";
            if (!string.IsNullOrEmpty(FilesLinkUtility.DocServiceApiUrl) && !FilesLinkUtility.DocServiceApiUrl.StartsWith("/"))
            {
                var origin = new Uri(FilesLinkUtility.DocServiceApiUrl);
                originForPost = origin.Scheme + "://" + origin.Host + ":" + origin.Port;
            }

            var script = new StringBuilder();
            script.AppendFormat("ASC.Files.Share.getSharedInfo(\"file_{0}\", \"{1}\", true, {2}, \"{3}\", {4}, {5}, {6}, {7});",
                                file.ID,
                                file.Title,
                                (file.RootFolderType == FolderType.COMMON).ToString().ToLower(),
                                originForPost,
                                file.DenyDownload.ToString().ToLower(),
                                file.DenySharing.ToString().ToLower(),
                                file.ProviderEntry.ToString().ToLower(),
                                JsonConvert.SerializeObject(FilesSettings.DefaultSharingAccessRights));

            //todo: change hardcode url
            script.AppendFormat("\r\nASC.Controls.JabberClient.pathWebTalk = \"{0}\";",
                                VirtualPathUtility.ToAbsolute("~/addons/talk/JabberClient.aspx"));
            Page.RegisterInlineScript(script.ToString());
        }


        public ScriptBundleData GetStaticJavaScript()
        {
            var src = shareDialogV115
                ? new List<string> { "Controls/AccessRights/accessrights.js", "Controls/AccessRights/formfilling.js" }
                : new List<string> { "Controls/SharingDialog/sharingdialog.js" };

            return (ScriptBundleData)
                   new ScriptBundleData("filesshare", "files")
                       .AddSource(PathProvider.GetFileStaticRelativePath,
                                  "common.js",
                                  "templatemanager.js",
                                  "servicemanager.js",
                                  "ui.js"
                       )
                       .AddSource(ResolveUrl,
                                  "~/js/third-party/clipboard.js",
                                  "~/Products/Files/Controls/Desktop/desktop.js"
                       )
                       .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r, src.ToArray());
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            var src = shareDialogV115
                ? new List<string> { "Controls/AccessRights/accessrights.less", "Controls/AccessRights/formfilling.less" }
                : new List<string> { "Controls/SharingDialog/sharingdialog.less" };

            return (StyleBundleData)
                   new StyleBundleData("filesshare", "files")
                       .AddSource(PathProvider.GetFileStaticRelativePath, "common.less")
                       .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r, src.ToArray());
        }
        public StyleBundleData GetStaticDarkStyleSheet()
        {
            var src = shareDialogV115
                    ? new List<string> { "Controls/AccessRights/dark-accessrights.less", "Controls/AccessRights/dark-formfilling.less" }
                    : new List<string> { "Controls/SharingDialog/dark-sharingdialog.less" };

            return (StyleBundleData)
                   new StyleBundleData("dark-filesshare", "files")
                       .AddSource(PathProvider.GetFileStaticRelativePath, "dark-common.less")
                       .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r, src.ToArray());
        }
    }
}