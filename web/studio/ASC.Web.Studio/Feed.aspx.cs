/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
                ImgSrc = WebPath.GetPath("usercontrols/feed/images/empty_screen_feed.png"),
                Header = UserControlsCommonResource.NewsNotFound,
                Describe = UserControlsCommonResource.NewsNotFoundDescription
            };
            emptyScreensHolder.Controls.Add(emptyScreen);

            var emptyFilterScreen = new EmptyScreenControl
            {
                ID = "emptyFeedFilterScr",
                ImgSrc = WebPath.GetPath("usercontrols/feed/images/empty_filter.png"),
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
                    Describe = UserControlsCommonResource.FeedEmptyCommunityDescription,
                    ButtonHTML = isVisitor
                                     ? string.Empty
                                     : string.Format("<a class='link underline plus' href='{0}'>{1}</a>" +
                                                     "<br/><a class='link underline plus' href='{2}'>{3}</a>" +
                                                     "<br/><a class='link underline plus' href='{4}'>{5}</a>",
                                                     VirtualPathUtility.ToAbsolute("~/products/community/modules/blogs/addblog.aspx"),
                                                     UserControlsCommonResource.FeedBlogsModuleLink,
                                                     VirtualPathUtility.ToAbsolute("~/products/community/modules/news/editnews.aspx"),
                                                     UserControlsCommonResource.FeedEventsModuleLink,
                                                     VirtualPathUtility.ToAbsolute("~/products/community/modules/bookmarking/createbookmark.aspx"),
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
                                               VirtualPathUtility.ToAbsolute("~/products/crm/default.aspx"),
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
                    Describe = UserControlsCommonResource.FeedEmptyListProjDescribe,
                    ID = "emptyListProjects",
                    ButtonHTML = canCreateProjects
                                     ? string.Format("<a href='{0}' class='link underline addFirstElement'>{1}<a>",
                                                     VirtualPathUtility.ToAbsolute("~/products/projects/projects.aspx?action=add"),
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
                                               VirtualPathUtility.ToAbsolute("~/products/files/"),
                                               UserControlsCommonResource.FeedButtonGotoMy)
                });
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