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
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.UserControls.EmptyScreens;
using ASC.Web.Studio.UserControls.Feed;
using ASC.Web.Studio.Utility;
using Resources;
using ASC.Web.Studio.UserControls.Common.LoaderPage;

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
            if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsOutsider())
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
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
            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));
            controlsHolder.Controls.Add(feedList);

            var emptyScreen = new EmptyScreenControl
            {
                ID = "emptyFeedScr",
                ImgSrc = WebPath.GetPath("UserControls/Feed/images/empty_screen_feed.png"),
                Header = UserControlsCommonResource.NewsNotFound,
                Describe = UserControlsCommonResource.NewsNotFoundDescription
            };
            emptyScreensHolder.Controls.Add(emptyScreen);

            var emptyFilterScreen = new EmptyScreenControl
            {
                ID = "emptyFeedFilterScr",
                ImgSrc = WebPath.GetPath("UserControls/Feed/images/empty_filter.png"),
                Header = UserControlsCommonResource.FilterNoNews,
                Describe = UserControlsCommonResource.FilterNoNewsDescription,
                ButtonHTML =
                    string.Format("<a href='javascript:void(0)' class='baseLinkAction clearFilterButton'>{0}</a>",
                                  UserControlsCommonResource.ResetFilter)
            };
            emptyScreensHolder.Controls.Add(emptyFilterScreen);

            var managerEmptyScreen = (ManagerDashboardEmptyScreen)Page.LoadControl(ManagerDashboardEmptyScreen.Location);
            emptyScreensHolder.Controls.Add(managerEmptyScreen);

            var userId = SecurityContext.CurrentAccount.ID;
            var isVisitor = CoreContext.UserManager.GetUsers(userId).IsVisitor();

            emptyScreensHolder.Controls.Add(new EmptyScreenControl
                {
                    ID = "emptyListCommunity",
                    ImgSrc = WebImageSupplier.GetAbsoluteWebPath("community150.png"),
                    Header = UserControlsCommonResource.FeedEmptyCommunityHeader,
                    Describe = isVisitor ? string.Empty : UserControlsCommonResource.FeedEmptyCommunityDescription,
                    ButtonHTML = isVisitor
                                     ? string.Empty
                                     : string.Format("<a class='link underline plus' href='{0}'>{1}</a>" +
                                                     "<br/><a class='link underline plus' href='{2}'>{3}</a>" +
                                                     "<br/><a class='link underline plus' href='{4}'>{5}</a>",
                                                     VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Blogs/AddBlog.aspx"),
                                                     UserControlsCommonResource.FeedBlogsModuleLink,
                                                     VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/EditNews.aspx"),
                                                     UserControlsCommonResource.FeedEventsModuleLink,
                                                     VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Bookmarking/CreateBookmark.aspx"),
                                                     UserControlsCommonResource.FeedBookmarkModuleLink)
                });

            emptyScreensHolder.Controls.Add(new EmptyScreenControl
                {
                    ID = "emptyListCrm",
                    ImgSrc = WebImageSupplier.GetAbsoluteWebPath("empty_screen_persons.png", WebItemManager.CRMProductID),
                    Header = UserControlsCommonResource.FeedEmptyContactListHeader,
                    Describe = UserControlsCommonResource.FeedEmptyContactListDescription,
                    ButtonHTML = string.Format("<a class='link underline plus' href='{0}?action=manage'>{1}</a><br/>" +
                                               "<a class='link underline plus' href='{0}?action=manage&type=people'>{2}</a>",
                                               VirtualPathUtility.ToAbsolute("~/Products/CRM/Default.aspx"),
                                               UserControlsCommonResource.FeedCreateFirstCompany,
                                               UserControlsCommonResource.FeedCreateFirstPerson)
                });

            var canCreateProjects =
                !isVisitor
                && (CoreContext.UserManager.IsUserInGroup(userId, Constants.GroupAdmin.ID)
                    || WebItemSecurity.IsProductAdministrator(WebItemManager.ProjectsProductID, userId));
            emptyScreensHolder.Controls.Add(new EmptyScreenControl
                {
                    Header = UserControlsCommonResource.FeedEmptyListProjHeader,
                    ImgSrc = WebImageSupplier.GetAbsoluteWebPath("projects_logo.png", WebItemManager.ProjectsProductID),
                    Describe = canCreateProjects ? UserControlsCommonResource.FeedEmptyListProjDescribe : string.Empty,
                    ID = "emptyListProjects",
                    ButtonHTML = canCreateProjects
                                     ? string.Format("<a href='{0}' class='link underline addFirstElement'>{1}<a>",
                                                     VirtualPathUtility.ToAbsolute("~/Products/Projects/Projects.aspx?action=add"),
                                                     UserControlsCommonResource.FeedCreateFirstProject)
                                     : string.Empty
                });

            emptyScreensHolder.Controls.Add(new EmptyScreenControl
                {
                    ID = "emptyListDocuments",
                    ImgSrc = WebImageSupplier.GetAbsoluteWebPath("documents150.png"),
                    Header = UserControlsCommonResource.FeedCorporateFiles,
                    Describe = UserControlsCommonResource.FeedEmptyScreenDescrCorporate,
                    ButtonHTML = string.Format("<a href=\"{0}\" class=\"link underline up\">{1}</a>",
                                               VirtualPathUtility.ToAbsolute("~/Products/Files/"),
                                               UserControlsCommonResource.FeedButtonGotoDocuments)
                });
        }

        #region IRenderCustomNavigation Members

        public static string RenderCustomNavigation(Page page)
        {
            if (CoreContext.Configuration.Personal) return string.Empty;

            // Migrate to CommonBodyScript
            //page.RegisterBodyScripts("~/js/asc/core/asc.feedreader.js");

            return
                string.Format(@"<li class=""top-item-box feed"">
                                  <a href=""{0}"" class=""feedActiveBox inner-text {2}"" title=""{1}"" data-feedUrl=""{0}"">
                                      <svg><use base=""{4}"" href=""/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenufeed""></use></svg>
                                      <span class=""inner-label"">{3}</span>
                                  </a>
                                </li>",
                              VirtualPathUtility.ToAbsolute("~/Feed.aspx"),
                              UserControlsCommonResource.FeedTitle,
                              string.Empty,
                              0,
                              WebPath.GetPath(@"/"));
        }

        #endregion
    }
}