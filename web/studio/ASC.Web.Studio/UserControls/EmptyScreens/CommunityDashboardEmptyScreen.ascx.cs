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
using System;
using System.Web;
using System.Web.UI;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.EmptyScreens
{
    public partial class CommunityDashboardEmptyScreen : UserControl
    {
        public static string Location
        {
            get { return VirtualPathUtility.ToAbsolute("~/UserControls/EmptyScreens/CommunityDashboardEmptyScreen.ascx"); }
        }

        public bool UseStaticPosition { get; set; }

        public string ProductStartUrl { get; set; }

        public CommunityDashboardEmptyScreen()
        {
            UseStaticPosition = false;
        }

        protected string HelpLink { get; set; }

        protected bool IsBlogsAvailable { get; set; }
        protected bool IsEventsAvailable { get; set; }
        protected bool IsForumsAvailable { get; set; }
        protected bool IsBookmarksAvailable { get; set; }
        protected bool IsWikiAvailable { get; set; }
        protected bool IsBirthdaysAvailable { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            HelpLink = CommonLinkUtility.GetHelpLink();
            InitPermission();
        }


        private void InitPermission()
        {
            foreach (var module in WebItemManager.Instance.GetSubItems(WebItemManager.CommunityProductID))
            {
                switch (module.GetSysName())
                {
                    case "community-blogs":
                        IsBlogsAvailable = true;
                        break;
                    case "community-news":
                        IsEventsAvailable = true;
                        break;
                    case "community-forum":
                        IsForumsAvailable = true;
                        break;
                    case "community-bookmarking":
                        IsBookmarksAvailable = true;
                        break;
                    case "community-wiki":
                        IsWikiAvailable = true;
                        break;
                    case "community-birthdays":
                        IsBirthdaysAvailable = true;
                        break;
                }
            }
        }

    }
}