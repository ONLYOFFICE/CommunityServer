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

using System;
using System.Web;
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
            get { return "~/addons/mail"; }
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

        #region IRenderCustomNavigation Members

        public string RenderCustomNavigation(Page page)
        {
            if (CoreContext.Configuration.Personal) return string.Empty;

            var func = "";

            if (!page.AppRelativeTemplateSourceDirectory.Contains(BaseVirtualPath))
                func = @"setTimeout(function () {
                            Teamlab.getMailFolders({}, new Date(0), {});
                        }, 3000);";

            var script = string.Format(@"
                                        var _inbox_folder_id = 1;
                                        var _setUnreadMailMessagesCount = function(params, folders){{
                                          jq.each(folders, function(index, value) {{
                                            if(_inbox_folder_id==value.id) {{
                                              jq(""#TPUnreadMessagesCount"").text(value.unread>100 ? '>100' : value.unread);
                                              jq(""#TPUnreadMessagesCount"").parent().toggleClass(""has-led"", value.unread != 0);
                                              return false;
                                            }};
                                          }});
                                        }};
                                        Teamlab.bind(Teamlab.events.getMailFolders, _setUnreadMailMessagesCount);
                                        {0}", func);

            page.RegisterInlineScript(script);

            return string.Format(@"<li class=""top-item-box mail"">
                                     <a class=""inner-text"" href=""{0}"" title=""{1}"">
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