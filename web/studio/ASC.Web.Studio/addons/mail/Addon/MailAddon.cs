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
using System.Linq;
using System.Web;
using System.Web.UI;

using ASC.Data.Storage;
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

        public bool Visible { get { return true; } }

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
                LargeIconFileName = "product_logolarge.svg",
                SpaceUsageStatManager = new Configuration.MailSpaceUsageStatManager(),
                AdminOpportunities = () => MailResource.AddonAdminOpportunities.Split('|').ToList(),
                UserOpportunities = () => MailResource.AddonUserOpportunities.Split('|').ToList(),
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

        public string HelpURL
        {
            get { return BaseVirtualPath; }
        }

        public string ProductClassName
        {
            get { return "mail"; }
        }

        public string HubUrl
        {
            get { return ConfigurationManagerExtension.AppSettings["web.hub"] ?? string.Empty; }
        }

        public string WarmupURL
        {
            get
            {
                return string.Concat(BaseVirtualPath, "Default.aspx");
            }
        }

        #region IRenderCustomNavigation Members

        public string RenderCustomNavigation(Page page)
        {
            var updateMailCounters = string.Empty;

            if (!page.AppRelativeTemplateSourceDirectory.Contains(BaseVirtualPath) && HubUrl == string.Empty)
            {
                updateMailCounters = string.Format("\r\nStudioManager.addPendingRequest(Teamlab.getMailFolders);");
            }

            // Migrate to CommonBodyScripts.ascx.cs
            // page.RegisterBodyScripts("~/js/asc/core/asc.mailreader.js");

            if (!string.IsNullOrEmpty(updateMailCounters))
            {
                page.RegisterInlineScript(updateMailCounters);
            }

            return string.Format(@"<li class=""top-item-box mail"">
                                     <a class=""inner-text mailActiveBox"" href=""{0}"" title=""{1}"">
                                       <svg><use base=""{2}"" href=""/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenumail""></use></svg>
                                       <span id=""TPUnreadMessagesCount"" class=""inner-label""></span>
                                     </a>
                                   </li>",
                                 VirtualPathUtility.ToAbsolute(BaseVirtualPath + "/"),
                                 MailResource.MailTitle,
                                 WebPath.GetPath("/"));
        }

        public Control LoadCustomNavigationControl(Page page)
        {
            return null;
        }

        #endregion
    }
}