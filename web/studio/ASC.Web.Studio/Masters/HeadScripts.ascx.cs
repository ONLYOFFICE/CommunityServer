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
    public class HeadScripts : ResourceScriptBundleControl, IStaticBundle
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SetData(GetStaticJavaScript());
        }

        public ScriptBundleData GetStaticJavaScript()
        {
            return (ScriptBundleData)
                new ScriptBundleData("head", "common")
                    .AddSource(ResolveUrl,
                        "~/js/third-party/jquery/jquery.core.js",
                        "~/js/third-party/jquery/jquery.ui.js",
                        "~/js/third-party/jquery/jquery.tmpl.js",
                        "~/js/third-party/jquery/jquery.json.js",
                        "~/js/third-party/jquery/jquery.cookies.js",
                        "~/js/asc/plugins/jquery.tlcombobox.js",
                        "~/js/asc/plugins/jquery.browser.js",
                        "~/js/third-party/jquery/jquery.blockUI.js",
                        "~/js/third-party/jquery/jquery.mask.js",
                        "~/js/third-party/jquery/jquery.scrollto.js",
                        "~/js/third-party/jquery/jquery.colors.js",
                        "~/js/third-party/jquery/toastr.js",
                        "~/js/third-party/jquery/jquery.caret.1.02.min.js",
                        "~/js/asc/plugins/jquery.helper.js",
                        "~/js/asc/core/asc.anchorcontroller.js",
                        "~/js/third-party/sjcl.js",
                        "~/js/asc/core/common.js",
                        "~/js/asc/plugins/jquery.dropdowntoggle.js",
                        "~/js/asc/core/asc.topstudiopanel.js",
                        "~/js/asc/core/asc.pagenavigator.js",
                        "~/js/asc/core/asc.tabsnavigator.js",
                        "~/js/asc/core/localstorage.js",
                        "~/js/third-party/ajaxpro.core.js",
                        "~/js/asc/plugins/svg4everybody.js");
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return null;
        }
    }
}