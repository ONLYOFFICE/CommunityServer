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
using System.Net;
using System.Text;
using System.Web;

using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Controls;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Core;

using Global = ASC.Web.Files.Classes.Global;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Masters;

namespace ASC.Web.Files
{
    public partial class OpenPrivate : MainPage
    {
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (!PrivacyRoomSettings.Available || !PrivacyRoomSettings.Enabled || CoreContext.Configuration.Personal)
            {
                Response.Redirect(PathProvider.StartURL, true);
            }

            if (string.IsNullOrEmpty(Request[FilesLinkUtility.FileId]))
            {
                Response.Redirect(PathProvider.StartURL
                                  + "#error/" +
                                  HttpUtility.UrlEncode(FilesCommonResource.ErrorMassage_FileNotFound), true);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Master.Master.DisabledSidePanel = true;
            Master.Master.DisabledTopStudioPanel = true;
            Master.Master
                .AddStaticStyles(GetStaticStyleSheet())
                .AddStaticBodyScripts(GetStaticJavaScript());

            OpenPrivateHolder.Controls.Add(LoadControl(PrivateRoomOpenFile.Location));
        }

        public ScriptBundleData GetStaticJavaScript()
        {
            return (ScriptBundleData)
                   new ScriptBundleData("filesOpenPrivate", "files")
                       .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r,
                                  "Controls/PrivateRoomOpenFile/privateroomopenfile.js"
                       );
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            if(ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
            {
                return (StyleBundleData)
                  new StyleBundleData("dark-filesOpenPrivate", "files")
                      .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r,
                                 "Controls/PrivateRoomOpenFile/dark-privateroomopenfile.less"
                      );
            }
            else
            {
                return (StyleBundleData)
                  new StyleBundleData("filesOpenPrivate", "files")
                      .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r,
                                 "Controls/PrivateRoomOpenFile/privateroomopenfile.less"
                      );
            }
        }

    }
}
