/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using ASC.Bookmarking;
using ASC.Bookmarking.Business.Permissions;
using ASC.Bookmarking.Common;
using ASC.Web.Community.Bookmarking.Util;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Bookmarking;
using ASC.Web.UserControls.Bookmarking.Common;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.UserControls.Bookmarking.Resources;
using System.Web;

namespace ASC.Web.Community.Bookmarking
{
    public partial class CreateBookmark : BookmarkingBasePage
    {
        protected override void PageLoad()
        {
            if (!BookmarkingPermissionsCheck.PermissionCheckCreateBookmark())
            {
                Response.Redirect(BookmarkingRequestConstants.BookmarkingPageName);
            }

            BookmarkingBusinessFactory.UpdateDisplayMode(BookmarkDisplayMode.CreateBookmark);

            var c = LoadControl(BookmarkUserControlPath.CreateBookmarkUserControlPath) as CreateBookmarkUserControl;
            c.IsNewBookmark = true;
            BookmarkingPageContent.Controls.Add(c);

            var url = Request.QueryString[BookmarkingRequestConstants.UrlGetRequest];
            var s = string.Empty;
            if (!string.IsNullOrEmpty(url))
            {
                s = string.Format(" getBookmarkUrlInput().val(\"{0}\"); getBookmarkByUrlButtonClick(); ", url.Replace("\"", "\\\""));
            }

            var script = string.Format("showAddBookmarkPanel(); {0}", s);

            Page.RegisterInlineScript(script);

            InitBreadcrumbs(BookmarkingUCResource.AddBookmarkLink);
            Title = HeaderStringHelper.GetPageTitle(BookmarkingUCResource.AddBookmarkLink);
        }

        protected override void InitBreadcrumbs(string pageTitle)
        {
            Title = pageTitle;
        }
    }
}