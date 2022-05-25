/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
using System.Web.UI;

using ASC.Web.Core.Files;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Files.Classes;

namespace ASC.Web.Files.Controls
{
    public partial class PrivateRoomOpenFile : UserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileControlPath("PrivateRoomOpenFile/PrivateRoomOpenFile.ascx"); }
        }

        protected string LogoText
        {
            get
            {
                return TenantLogoManager.GetLogoText();
            }
        }

        protected string LogoPath
        {
            get
            {
                var general = !TenantLogoManager.IsRetina(Request);

                if (TenantLogoManager.WhiteLabelEnabled)
                {
                    return TenantLogoManager.GetLogoDark(general);
                }

                return TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.Dark, general);
            }
        }

        protected string EditorUrl {
            get
            {
                var fileId = Request[FilesLinkUtility.FileId];
                var action = Request[FilesLinkUtility.Action];
                var version = Request[FilesLinkUtility.Version];
                var editorUrl = FilesLinkUtility.GetFileCustomProtocolEditorUrl(fileId);

                if (action == "view")
                {
                    editorUrl += "&" + FilesLinkUtility.Action + "=" + action;

                    if (!string.IsNullOrEmpty(version))
                    {
                        editorUrl += "&" + FilesLinkUtility.Version + "=" + version;
                    }
                }

                return editorUrl;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}