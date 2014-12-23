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
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Core.WebZones;
using ASC.Web.Calendar.Notification;
using System.Text;
using System.Web.UI;
using System.Web;

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
                               LargeIconFileName = "biglogo.png",
                               SubscriptionManager = new SubscriptionManager(),
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
        public string ProductClassName
        {
            get { return "calendar"; }
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
                                      <span class=""inner-label""></span>
                                  </a>
                              </li>", VirtualPathUtility.ToAbsolute(StartURL), Name);

            return sb.ToString();
        }

        #endregion
    }
}