/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using ASC.Web.Core;
using ASC.Web.Core.WebZones;
using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;

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
                    LargeIconFileName = "product_logolarge.png",
                    DefaultSortOrder = 60,
                };
        }

        public void Shutdown()
        {

        }

        public string StartURL
        {
            get { return BaseVirtualPath + "/default.aspx"; }
        }

        public string HelpURL
        {
            get { return null; }
        }

        public string ProductClassName
        {
            get { return "talk"; }
        }

        public const string BaseVirtualPath = "~/addons/talk";

        public static string GetClientUrl()
        {
            return VirtualPathUtility.ToAbsolute("~/addons/talk/jabberclient.aspx");
        }

        public static string GetTalkClientURL()
        {
            return "javascript:window.ASC.Controls.JabberClient.open('" + VirtualPathUtility.ToAbsolute("~/addons/talk/jabberclient.aspx") + "')";
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