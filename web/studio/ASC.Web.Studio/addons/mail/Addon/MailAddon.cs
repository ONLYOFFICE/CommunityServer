/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Core.WebZones;
using ASC.Web.Mail.Resources;

namespace ASC.Web.Mail
{
    [WebZoneAttribute(WebZoneType.CustomProductList)]
    public class MailAddon : IAddon, IRenderCustomNavigation
    {
        public static Guid AddonID
        {
            get { return WebItemManager.MailProductID; }
        }

        public static string BaseVirtualPath
        {
            get { return "~/addons/mail/"; }
        }

        private AddonContext _context;

        public AddonContext Context
        {
            get { return _context; }
        }

        WebItemContext IWebItem.Context
        {
            get { return _context; }
        }

        public string Description
        {
            get { return MailResource.MailDescription; }
        }

        public Guid ID
        {
            get { return AddonID; }
        }

        public void Init()
        {
            _context = new AddonContext
                {
                    DisabledIconFileName = "mail_disabled.png",
                    IconFileName = "mail.png",
                    SpaceUsageStatManager = new Configuration.MailSpaceUsageStatManager()
                };
        }

        public string Name
        {
            get { return MailResource.ProductName; }
        }

        public void Shutdown()
        {

        }

        public string StartURL
        {
            get { return BaseVirtualPath; }
        }

        public string ProductClassName
        {
            get { return "mail"; }
        }

        public int MailCountGetFirstInMillisecond
        {
            get { return Convert.ToInt32(WebConfigurationManager.AppSettings["mail.count-get-first-query"] ?? "3000"); }
        }

        public int MailCountGetIntervalInMillisecond
        {
            get { return Convert.ToInt32(WebConfigurationManager.AppSettings["mail.count-get-query-interval"] ?? "30000"); }
        }

         public bool MailCountGetEnableInterval
        {
            get { return Convert.ToBoolean(WebConfigurationManager.AppSettings["mail.count-get-query-enable-interval"] ?? "false"); }
        }

        

        #region IRenderCustomNavigation Members

        public string RenderCustomNavigation(Page page)
        {
            var func = "";

            if (!page.AppRelativeTemplateSourceDirectory.Contains(BaseVirtualPath))
                func = string.Format(@"

            setTimeout(function () {{ Teamlab.getMailFolders(); }}, {0}); 
            {1}", MailCountGetFirstInMillisecond,
                                     MailCountGetEnableInterval
                                         ? string.Format(
                                             "setInterval(function () {{ Teamlab.getMailFolders(); }}, {0});",
                                             MailCountGetIntervalInMillisecond)
                                         : "");

            var script = string.Format(@"

            function _setUnreadMailMessagesCount(params, folders) {{
                if (undefined == folders.length)
                    return;

                var inbox = folders[0];
                if(jq(""#TPUnreadMessagesCount"") && inbox && inbox.unread) {{
                    jq(""#TPUnreadMessagesCount"").text(inbox.unread>100 ? '>100' : inbox.unread);
                    jq(""#TPUnreadMessagesCount"").parent().toggleClass(""has-led"", inbox.unread != 0);
                }}
            }}

            function _onUpdateFolders(params, folders) {{
                if (undefined == folders.length)
                    return;

                if (Modernizr && Modernizr.localstorage) {{
                    var stored_count = window.localStorage.getItem(""TPUnreadMessagesCount"");
                    if(stored_count != folders[0].unread)
                    {{
                        window.localStorage.setItem(""TPUnreadMessagesCount"", folders[0].unread);
                    }}
                    else
                    {{
                        _setUnreadMailMessagesCount(params, [ folders[0] ]);
                    }}
                }}
                else {{
                    _setUnreadMailMessagesCount(params, folders);
                }}
            }}

            function _onLocalStorageChanged(e) {{
                if(e && e.key && e.key == ""TPUnreadMessagesCount"") {{
                    _setUnreadMailMessagesCount(e, [ {{ unread: e.newValue }} ]);
                }}
            }}

            Teamlab.bind(Teamlab.events.getMailFolders, _onUpdateFolders);

            if (Modernizr && Modernizr.localstorage) {{
                // window.localStorage is available!
                window.addEvent(window, 'storage', _onLocalStorageChanged);
                var stored_count = window.localStorage.getItem(""TPUnreadMessagesCount"");
                if(stored_count)
                    _setUnreadMailMessagesCount({{}}, [ {{ unread: stored_count }} ]);
            }}

            {0}", func);

            page.RegisterInlineScript(script);

            return string.Format(@"<li class=""top-item-box mail"">
                                     <a class=""inner-text mailActiveBox"" href=""{0}"" title=""{1}"">
                                       <span id=""TPUnreadMessagesCount"" class=""inner-label""></span>
                                     </a>
                                   </li>",
                                 VirtualPathUtility.ToAbsolute(BaseVirtualPath + "/"),
                                 MailResource.MailTitle);
        }

        public Control LoadCustomNavigationControl(Page page)
        {
            return null;
        }

        #endregion
    }
}