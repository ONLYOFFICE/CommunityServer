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
using ASC.Web.Studio.Masters.MasterResources;

namespace ASC.Web.Studio.Masters
{
    public class CommonBodyScripts : ResourceScriptBundleControl, IStaticBundle
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SetData(GetStaticJavaScript());
        }

        public ScriptBundleData GetStaticJavaScript()
        {
            return (ScriptBundleData)
                new ScriptBundleData("studio", "common")
                    .AddSource(ResolveUrl, new MasterTemplateResources())
                    .AddSource(ResolveUrl,
                        "~/js/asc/core/asc.customevents.js",
                        "~/js/asc/api/api.factory.js",
                        "~/js/asc/api/api.helper.js",
                        "~/js/asc/api/asc.teamlab.js",
                        "~/js/asc/plugins/jquery.base64.js",
                        "~/js/asc/plugins/jquery-customcombobox.js",
                        "~/js/asc/plugins/jquery-advansedfilter.js",
                        "~/js/asc/plugins/jquery-advansedselector.js",
                        "~/js/asc/plugins/jquery-useradvansedselector.js",
                        "~/js/asc/plugins/jquery-groupadvansedselector.js",
                        "~/js/asc/plugins/jquery-contactadvansedselector.js",
                        "~/js/asc/plugins/jquery-emailadvansedselector.js",
                        "~/js/asc/plugins/jquery.tlblock.js",
                        "~/js/asc/plugins/popupbox.js",
                        "~/js/asc/core/asc.files.utility.js",
                        "~/js/asc/core/basetemplate.master.init.js",
                        "~/UserControls/Common/HelpCenter/js/help-center.js",
                        "~/js/asc/core/groupselector.js",
                        "~/js/asc/core/asc.mail.utility.js",
                        "~/js/third-party/async.js",
                        "~/js/third-party/clipboard.js",
                        "~/js/asc/core/clipboard.js",
                        "~/js/third-party/email-addresses.js",
                        "~/js/third-party/punycode.js",
                        "~/js/third-party/purify.min.js",
                        "~/js/asc/core/asc.mailreader.js",
                        "~/UserControls/Common/VideoGuides/js/videoguides.js",
                        "~/js/asc/core/asc.feedreader.js",
                        "~/addons/talk/js/talk.navigationitem.js",
                        "~/UserControls/Common/InviteLink/js/invitelink.js",
                        "~/UserControls/Management/InvitePanel/js/invitepanel.js"
                        );
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return null;
        }
    }
}