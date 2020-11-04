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
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Web.Talk.Addon;
using System.Configuration;

namespace ASC.Web.Talk.UserControls
{
    [AjaxPro.AjaxNamespace("TalkProvider")]
    public partial class TalkNavigationItem : UserControl
    {
        private static String EscapeJsString(String s)
        {
            return s.Replace("'", "\\'");
        }

        public static string Location
        {
            get { return TalkAddon.BaseVirtualPath + "/UserControls/TalkNavigationItem.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
        }

        protected string GetTalkClientURL()
        {
            return TalkAddon.GetTalkClientURL();
        }

        protected string GetMessageStr()
        {
            return TalkAddon.GetMessageStr();
        }

        protected string GetOpenContactHandler()
        {
            return VirtualPathUtility.ToAbsolute("~/addons/talk/opencontact.ashx");
        }

        protected string GetJabberClientPath()
        {
            return TalkAddon.GetClientUrl();
        }

        protected string GetUserName()
        {
            try
            {
                return EscapeJsString(
                    CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).UserName.ToLower());
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        protected string GetUpdateInterval()
        {
            return new TalkConfiguration().UpdateInterval;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            // Migrate to CommonBodyScripts
           // Page.RegisterBodyScripts("~/addons/talk/js/talk.navigationitem.js");
            RegisterScript();
        }

        private void RegisterScript()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("\r\nASC.Controls.JabberClient.init('{0}','{1}','{2}');",
                GetUserName(),
                GetJabberClientPath(),
                GetOpenContactHandler()
            );

            var hubUrl = ConfigurationManagerExtension.AppSettings["web.hub"] ?? string.Empty;
            if (hubUrl == string.Empty)
            {
                sb.AppendFormat("\r\nASC.Controls.TalkNavigationItem.init('{0}');", GetUpdateInterval());
            }
            Page.RegisterInlineScript(sb.ToString());
        }
    }
}