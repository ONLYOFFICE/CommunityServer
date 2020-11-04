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

using ASC.Data.Storage;
using ASC.Web.Calendar.Notification;
using ASC.Web.Core;
using ASC.Web.Core.WebZones;

namespace ASC.Web.Calendar
{
    [WebZoneAttribute(WebZoneType.CustomProductList)]
    public class CalendarAddon : IAddon, IRenderCustomNavigation
    {
        public static Guid AddonID
        {
            get { return WebItemManager.CalendarProductID; }
        }

        public static string BaseVirtualPath
        {
            get { return "~/addons/calendar/"; }
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
            get { return Resources.CalendarAddonResource.AddonDescription; }
        }

        public Guid ID
        {
            get { return AddonID; }
        }

        public void Init()
        {
            _context = new AddonContext
            {
                DefaultSortOrder = 80,
                DisabledIconFileName = "disabledlogo.png",
                IconFileName = "logo.png",
                LargeIconFileName = "product_logolarge.svg",
                SubscriptionManager = new CalendarSubscriptionManager(),
            };
        }

        public string Name
        {
            get { return Resources.CalendarAddonResource.AddonName; }
        }

        public void Shutdown()
        {

        }

        public string StartURL
        {
            get { return "~/addons/calendar/"; }
        }

        public string HelpURL
        {
            get { return null; }
        }

        public string ProductClassName
        {
            get { return "calendar"; }
        }

        public string WarmupURL
        {
            get
            {
                return string.Concat(BaseVirtualPath, "Default.aspx");
            }
        }

        #region IRenderCustomNavigation Members

        public Control LoadCustomNavigationControl(Page page)
        {
            return null;
        }

        public string RenderCustomNavigation(Page page)
        {
            var sb = new StringBuilder();
            //sb.AppendFormat(@"<style type=""text/css"">
            //                .studioTopNavigationPanel .systemSection .calendar a{{background:url(""{0}"") left 1px no-repeat;}}
            //                </style>", WebImageSupplier.GetAbsoluteWebPath("minilogo.png", AddonID));

            //sb.AppendFormat(@"<li class=""itemBox calendar"" style=""float: right;"">
            //        <a href=""{0}""><span>{1}</span>
            //        </a></li>", VirtualPathUtility.ToAbsolute(this.StartURL), this.Name);

            sb.AppendFormat(@"<li class=""top-item-box calendar"">
                                  <a class=""inner-text"" href=""{0}"" title=""{1}"">
                                      <svg><use base=""{2}""  href=""/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenuCalendar""></use></svg>
                                      <span class=""inner-label""></span>
                                  </a>
                              </li>",
                              VirtualPathUtility.ToAbsolute(StartURL),
                              Name,
                              WebPath.GetPath("/"));

            return sb.ToString();
        }

        #endregion
    }
}