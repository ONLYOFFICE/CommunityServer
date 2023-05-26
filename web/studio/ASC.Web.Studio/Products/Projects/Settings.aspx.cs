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
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

using ASC.Web.Core.Client;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Utility;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects
{
    public partial class Settings : BasePage
    {
        protected override bool CheckSecurity { get { return ProjectSecurity.IsProjectsEnabled(); } }

        protected override void PageLoad()
        {
            Title = HeaderStringHelper.GetPageTitle(ProjectsCommonResource.CommonSettings);
            if(ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
            {
                Page.RegisterStyle("~/Products/Projects/App_Themes/dark/dark-settings.less",
                "~/Products/Files/Controls/FileSelector/fileselector.less",
                "~/Products/Files/Controls/ThirdParty/dark-thirdparty.less",
                "~/Products/Files/Controls/ContentList/dark-contentlist.less",
                "~/Products/Files/Controls/EmptyFolder/emptyfolder.less",
                "~/Products/Files/Controls/Tree/dark-tree.less");
            }
            else
            {
                Page.RegisterStyle(PathProvider.GetFileStaticRelativePath("settings.less"))
                    .RegisterStyle("~/Products/Files/Controls/FileSelector/fileselector.less",
                "~/Products/Files/Controls/ThirdParty/thirdparty.less",
                "~/Products/Files/Controls/ContentList/contentlist.less",
                "~/Products/Files/Controls/EmptyFolder/emptyfolder.less",
                "~/Products/Files/Controls/Tree/tree.less"); 
            }
            Page.RegisterBodyScripts(
                PathProvider.GetFileStaticRelativePath("settings.js"),
                    "~/Products/Files/Controls/Tree/tree.js",
                    "~/Products/Files/Controls/EmptyFolder/emptyfolder.js",
                    "~/Products/Files/Controls/FileSelector/fileselector.js",
                    "~/Products/Files/js/common.js",
                    "~/Products/Files/js/templatemanager.js",
                    "~/Products/Files/js/servicemanager.js",
                    "~/Products/Files/js/ui.js",
                    "~/Products/Files/js/eventhandler.js");

            FolderSelectorHolder.Controls.Add(LoadControl(CommonLinkUtility.ToAbsolute("~/Products/Files/Controls/FileSelector/FileSelector.ascx")));

            Page.RegisterClientScript(new ClientSettingsResources());
        }
    }

    public class ClientSettingsResources : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Projects.Master.Settings"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var icons = new[]
                        {
                            "/skins/default/images/svg/projects/timetrack.svg",
                            "/skins/default/images/svg/projects/inbox.svg",
                            "/skins/default/images/svg/projects/pause.svg",
                            "/skins/default/images/svg/projects/milstones.svg",
                            "/skins/default/images/svg/projects/check_tick.svg",
                            "/skins/default/images/svg/projects/documents.svg",
                            "/skins/default/images/svg/projects/bookmark.svg",
                            "/skins/default/images/svg/projects/discussions.svg",
                            "/skins/default/images/svg/projects/projects.svg",
                            "/skins/default/images/svg/calendar/up.svg"
                        }
            .Select(r => HttpContext.Current.Server.MapPath(r))
            .Select(r => File.ReadAllText(r))
            .Select(r => Convert.ToBase64String(Encoding.UTF8.GetBytes(r)))
            .ToList();

            return new List<KeyValuePair<string, object>>(1)
            {
                RegisterObject(
                    new
                    {
                        icons,
                        colors = new[]
                        {
                            "#f48454",
                            "#ffb45e",
                            "#ffd267",
                            "#b7d269",
                            "#6bbd72",
                            "#77cf9a",
                            "#6ac6dd",
                            "#4682b6",
                            "#6a9ad2",
                            "#8a98d8",
                            "#7e6eb2",
                            "#b58fd6",
                            "#d28cc8",
                            "#e795c1",
                            "#f2a9be",
                            "#df7895"
                        }
                    })
            };
        }

        protected override string GetCacheHash()
        {
            return ClientSettings.ResetCacheKey;
        }
    }
}