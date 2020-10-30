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
using ASC.Web.Core.Client.Bundling;

namespace ASC.Web.Studio.Masters
{
    public class CommonStyles : ResourceStyleBundleControl, IStaticBundle
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SetData(GetStaticStyleSheet());
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
                        "~/skins/default/filetype_style.css",
                        "~/skins/default/toastr.less",
                        "~/skins/default/groupselector.css",
                        "~/skins/default/jquery-advansedfilter.css",
                        "~/skins/default/jquery-advansedfilter-fix.less",
                        "~/skins/default/jquery-advansedselector.less",
                        "~/skins/default/jquery-emailadvansedselector.css",
                        "~/skins/default/codestyle.css");
        }
    }
}