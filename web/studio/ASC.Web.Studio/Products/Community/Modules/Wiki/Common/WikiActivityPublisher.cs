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
using System.Globalization;
using ASC.Core.Tenants;
using ASC.Web.Community.Wiki.Resources;
using ASC.Web.Core.Users.Activity;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Wiki;
using ASC.Web.UserControls.Wiki.Data;

namespace ASC.Web.Community.Wiki.Common
{
    public class WikiActivityPublisher : BaseUserActivityPublisher
    {
        internal static void PublishInternal(UserActivity activity)
        {
            UserActivityPublisher.Publish<WikiActivityPublisher>(activity);
        }

        internal static string GetContentID(object page)
        {
            string result = string.Empty;
            if (page is Page)
            {
                result = string.Format(CultureInfo.CurrentCulture, "wikiPage#{0}", (page as Page).PageName);
            }
            else if(page is File)
            {
                result = string.Format(CultureInfo.CurrentCulture, "wikiFile#{0}", (page as File).FileName);
            }

            return result;
        }

        internal static string GetTitle(object page)
        {
            string result = string.Empty;
            if (page is Page)
            {
                result = (page as Page).PageName;
                if(string.IsNullOrEmpty(result))
                {
                    result = WikiResource.MainWikiCaption;
                }
            }
            else if (page is File)
            {
                result = (page as File).FileName;
            }

            return result;
        }

        internal static string GetUrl(object page)
        {
            string result = string.Empty;
            if (page is Page)
            {
                result = ActionHelper.GetViewPagePath(WikiManager.ViewVirtualPath, (page as Page).PageName); 
            }
            else if (page is File)
            {
                result = ActionHelper.GetViewFilePath(WikiManager.ViewVirtualPath, (page as File).FileName);
            }

            return result;
        }

        internal static UserActivity ComposeActivityByPage(object page)
        {
            UserActivity ua = new UserActivity();
            ua.TenantID = TenantProvider.CurrentTenantID;
            ua.ContentID = GetContentID(page);
            ua.Date = TenantUtil.DateTimeNow();
            ua.ModuleID = WikiManager.ModuleId;
            ua.ProductID = Product.CommunityProduct.ID;
            ua.Title = GetTitle(page);
            ua.URL = GetUrl(page);

            return ua;
        }

        internal static UserActivity ApplyCustomeActivityParams(UserActivity ua, string actionText, Guid userID, int actionType, int businessValue)
        {
            ua.ImageOptions = new ImageOptions();
            ua.ImageOptions.PartID = WikiManager.ModuleId;
            ua.ImageOptions.ImageFileName = string.Empty;
            ua.ActionText = actionText;
            ua.UserID = userID;
            ua.ActionType = actionType;
            ua.BusinessValue = businessValue;
            return ua;
        }


        public static void AddPage(Page page)
        {
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(page),
                            WikiResource.wikiAction_PageAdded,
                            page.UserID,
                            UserActivityConstants.ContentActionType,
                            UserActivityConstants.NormalContent
                        );

                PublishInternal(ua);
        }

        public static bool EditPage(Page page)
        {
            if(page.Version == 1) //New Page Saved!!!
            {
                AddPage(page);
                return false;
            }
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(page),
                            WikiResource.wikiAction_PageEdited,
                            page.UserID,
                            UserActivityConstants.ActivityActionType,
                            UserActivityConstants.ImportantActivity
                        );

            PublishInternal(ua);
            return true;
        }

        public static void RevertPage(Page page)
        {
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(page),
                            WikiResource.wikiAction_VersionRevert,
                            page.UserID,
                            UserActivityConstants.ActivityActionType,
                            UserActivityConstants.SmallActivity
                        );

            PublishInternal(ua);
        }

        public static void AddFile(File file)
        {
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(file),
                            WikiResource.wikiAction_FileAdded,
                            file.UserID,
                            UserActivityConstants.ActivityActionType,
                            UserActivityConstants.ImportantActivity
                        );

            PublishInternal(ua);
        }

        public static void DeleteFile(File file)
        {
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(file),
                            WikiResource.wikiAction_FileDeleted,
                            file.UserID,
                            UserActivityConstants.ActivityActionType,
                            UserActivityConstants.SmallActivity
                        );

            PublishInternal(ua);
        }

        public static void AddPageComment(Page page, Comment newComment)
        {
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(page),
                            WikiResource.wikiAction_CommentAdded,
                            newComment.UserId,
                            UserActivityConstants.ActivityActionType,
                            UserActivityConstants.NormalActivity
                        );

            PublishInternal(ua);
        }

        public static void EditPageComment(Page page, Comment newComment)
        {
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(page),
                            WikiResource.wikiAction_CommentEdited,
                            newComment.UserId,
                            UserActivityConstants.ActivityActionType,
                            UserActivityConstants.SmallActivity
                        );

            PublishInternal(ua);
        }

        public static void DeletePageComment(Page page, Comment newComment)
        {
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(page),
                            WikiResource.wikiAction_CommentDeleted,
                            newComment.UserId,
                            UserActivityConstants.ActivityActionType,
                            UserActivityConstants.SmallActivity
                        );

            PublishInternal(ua);
        }
    }
}
