/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Data.Storage;
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
            Page.RegisterBodyScripts("~/addons/talk/js/talk.navigationitem.js");
            RegisterScript();
        }

        private void RegisterScript()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(@"
                    ASC.Controls.JabberClient.init('{0}','{1}','{2}');",
                GetUserName(),
                GetJabberClientPath(),
                GetOpenContactHandler()
            );

            var hubUrl = ConfigurationManager.AppSettings["web.hub"] ?? string.Empty;
            if (hubUrl == string.Empty)
            {
                sb.AppendFormat(@"ASC.Controls.TalkNavigationItem.init('{0}');", GetUpdateInterval());
            }
            Page.RegisterInlineScript(sb.ToString());
        }
    }
}