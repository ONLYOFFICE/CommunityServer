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
using ASC.Data.Storage;
using ASC.Web.Core.Files;
using ASC.Web.Studio.UserControls.Feed;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio
{
    public partial class Feed : MainPage
    {
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            if (CoreContext.Configuration.Personal)
            {
                Context.Response.Redirect(FilesLinkUtility.FilesBaseAbsolutePath);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Bootstrap();
            LoadControls();
        }

        private void Bootstrap()
        {
            Title = HeaderStringHelper.GetPageTitle(UserControlsCommonResource.FeedTitle);

            var navigation = (NewNavigationPanel)LoadControl(NewNavigationPanel.Location);
            navigationHolder.Controls.Add(navigation);
        }

        private void LoadControls()
        {
            var feedList = (FeedList)LoadControl(FeedList.Location);
            controlsHolder.Controls.Add(feedList);

            var emptyScreenFilter = new Controls.Common.EmptyScreenControl
                {
                    ImgSrc = WebPath.GetPath("usercontrols/feed/images/empty_filter.png"),
                    Header = UserControlsCommonResource.FilterNoNews,
                    Describe = UserControlsCommonResource.FilterNoNewsDescription,
                    ButtonHTML =
                        string.Format("<a href='javascript:void(0)' class='baseLinkAction clearFilterButton'>{0}</a>",
                                      UserControlsCommonResource.ResetFilter)
                };
            controlsHolder.Controls.Add(emptyScreenFilter);

            var emptyScreenControl = new Controls.Common.EmptyScreenControl
                {
                    ImgSrc = WebPath.GetPath("usercontrols/feed/images/empty_screen_feed.png"),
                    Header = UserControlsCommonResource.NewsNotFound,
                    Describe = UserControlsCommonResource.NewsNotFoundDescription
                };
            controlsHolder.Controls.Add(emptyScreenControl);
        }

        #region IRenderCustomNavigation Members

        public static string RenderCustomNavigation(Page page)
        {
            if (CoreContext.Configuration.Personal) return string.Empty;

            page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/js/asc/core/asc.feedreader.js"));

            return
                string.Format(@"<li class=""top-item-box feed"">
                                  <a href=""{0}"" class=""feedActiveBox inner-text {2}"" title=""{1}"" data-feedUrl=""{0}"">
                                      <span class=""inner-label"">{3}</span>
                                  </a>
                                </li>",
                              VirtualPathUtility.ToAbsolute("~/feed.aspx"),
                              UserControlsCommonResource.FeedTitle,
                              string.Empty,
                              0);
        }

        #endregion
    }
}