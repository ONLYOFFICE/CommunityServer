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
using System.Configuration;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;

using ASC.Web.Core;
using ASC.Web.Core.WebZones;

namespace ASC.Web.Talk.Addon
{
    [WebZoneAttribute(WebZoneType.CustomProductList)]
    public class TalkAddon : IAddon, IRenderCustomNavigation
    {
        public static Guid AddonID
        {
            get { return WebItemManager.TalkProductID; }
        }

        public string Name
        {
            get { return Resources.TalkResource.ProductName; }
        }

        public string Description
        {
            get { return Resources.TalkResource.TalkDescription; }
        }

        public Guid ID
        {
            get { return AddonID; }
        }

        public bool Visible { get { return true; } }
        public AddonContext Context { get; private set; }

        WebItemContext IWebItem.Context
        {
            get { return Context; }
        }


        public void Init()
        {
            Context = new AddonContext
            {
                DisabledIconFileName = "product_logo_disabled.png",
                IconFileName = "product_logo.png",
                LargeIconFileName = "product_logolarge.svg",
                DefaultSortOrder = 60,
                SpaceUsageStatManager = new TalkSpaceUsageStatManager()
            };
        }

        public void Shutdown()
        {

        }

        public string StartURL
        {
            get { return BaseVirtualPath + "/Default.aspx"; }
        }

        public string HelpURL
        {
            get { return null; }
        }

        public string ProductClassName
        {
            get { return "talk"; }
        }

        public string WarmupURL
        {
            get
            {
                return StartURL;
            }
        }

        public const string BaseVirtualPath = "~/addons/talk";

        public static string GetClientUrl()
        {
            return VirtualPathUtility.ToAbsolute("~/addons/talk/JabberClient.aspx");
        }

        public static string GetTalkClientURL()
        {
            return "javascript:window.ASC.Controls.JabberClient.open('" + VirtualPathUtility.ToAbsolute("~/addons/talk/JabberClient.aspx") + "')";
        }

        public static string GetMessageStr()
        {
            return Resources.TalkResource.Chat;
        }

        public Control LoadCustomNavigationControl(Page page)
        {
            return null;
        }

        public string RenderCustomNavigation(Page page)
        {
            var isEnabledTalk = ConfigurationManagerExtension.AppSettings["web.talk"] ?? "false";

            if (isEnabledTalk != "true") return string.Empty;

            var sb = new StringBuilder();
            using (var tw = new StringWriter(sb))
            {
                using (var hw = new HtmlTextWriter(tw))
                {
                    var ctrl = page.LoadControl(UserControls.TalkNavigationItem.Location);
                    ctrl.RenderControl(hw);
                    return sb.ToString();
                }
            }
        }
    }
}