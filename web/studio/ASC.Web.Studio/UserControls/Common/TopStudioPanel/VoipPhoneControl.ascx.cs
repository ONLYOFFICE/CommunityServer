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
using System.Web;
using System.Web.UI;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Client.HttpHandlers;

namespace ASC.Web.Studio.UserControls.Common
{
    public partial class VoipPhoneControl : UserControl, IStaticBundle
    {
        public static string Location
        {
            get { return "~/UserControls/Common/TopStudioPanel/VoipPhoneControl.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterStaticScripts(GetStaticJavaScript())
                .RegisterBodyScripts(
                        "~/js/third-party/socket.io.js",
                        "~/js/asc/core/asc.socketio.js")
                .RegisterClientScript(new VoipNumberData());
        }

        public ScriptBundleData GetStaticJavaScript()
        {
            var result = new ScriptBundleData("voip", "studio");
            result.AddSource(ResolveUrl, new ClientTemplateResources());
            result.AddSource(ResolveUrl,
                "~/js/asc/core/voip.countries.js",
                "~/js/asc/core/voip.phone.js");
            return result;
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return null;
        }
    }

    public class ClientTemplateResources : ClientScriptTemplate
    {
        protected override string[] Links
        {
            get
            {
                return new[]
                {
                    "~/Templates/VoipTemplate.html"
                };
            }
        }
    }
}