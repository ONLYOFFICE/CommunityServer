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

using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Utility;

namespace ASC.Web.Studio.Masters
{
    public class CommonStyles : ResourceStyleBundleControl, IStaticBundle
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark) 
                SetData(GetStaticDarkStyleSheet());
            else SetData(GetStaticStyleSheet());
        }

        public ScriptBundleData GetStaticJavaScript()
        {
            return null;
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return (StyleBundleData)
                new StyleBundleData("studio", "common")
                    .AddSource(ResolveUrl,
                        "~/skins/default/jquery_style.css",
                        "~/skins/default/main-title-icon.less",
                        "~/skins/default/empty-screen-control.less",
                        "~/skins/default/common_style.less",
                        "~/skins/default/page-tabs-navigators.less",
                        "~/skins/default/main-page-container.less",
                        "~/skins/default/wizard.less",
                        "~/skins/default/helper.less",
                        "~/skins/default/comments-container.less",
                        "~/skins/default/filetype_style.less",
                        "~/skins/default/toastr.less",
                        "~/skins/default/groupselector.less",
                        "~/skins/default/jquery-advansedfilter.css",
                        "~/skins/default/jquery-advansedfilter-fix.less",
                        "~/skins/default/jquery-advansedselector.less",
                        "~/skins/default/jquery-emailadvansedselector.less",
                        "~/skins/default/userselector.less",
                        "~/skins/default/codestyle.less");
        }
        public StyleBundleData GetStaticDarkStyleSheet()
        {
            return (StyleBundleData)
                new StyleBundleData("dark-studio", "common")
                    .AddSource(ResolveUrl,
                        "~/skins/default/jquery_style.css",
                        "~/skins/default/main-title-icon.less",
                        "~/skins/dark/dark-empty-screen-control.less",
                        "~/skins/dark/dark-common_style.less",
                        "~/skins/dark/dark-page-tabs-navigators.less",
                        "~/skins/default/main-page-container.less",
                        "~/skins/default/wizard.less",
                        "~/skins/dark/dark-helper.less",
                        "~/skins/default/comments-container.less",
                        "~/skins/default/filetype_style.less",
                        "~/skins/dark/dark-toastr.less",
                        "~/skins/dark/dark-groupselector.less",
                        "~/skins/default/jquery-advansedfilter.css",
                        "~/skins/dark/dark-jquery-advansedfilter-fix.less",
                        "~/skins/dark/dark-jquery-advansedselector.less",
                        "~/skins/dark/dark-jquery-emailadvansedselector.less",
                        "~/skins/dark/dark-userselector.less",
                        "~/skins/dark/dark-codestyle.less");
        }
    }
}