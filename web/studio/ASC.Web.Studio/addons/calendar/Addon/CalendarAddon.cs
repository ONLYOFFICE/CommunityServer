/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Web.Core;
using ASC.Web.Core.WebZones;
using ASC.Web.Calendar.Notification;
using System.Text;
using System.Web.UI;
using System.Web;
using ASC.Data.Storage;

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