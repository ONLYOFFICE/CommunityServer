/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using ASC.Core;
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
            if (CoreContext.Configuration.Personal) return string.Empty;

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