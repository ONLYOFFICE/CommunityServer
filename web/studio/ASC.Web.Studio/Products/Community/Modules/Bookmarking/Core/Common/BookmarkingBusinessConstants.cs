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
using ASC.Notify.Model;
using ASC.Web.Community.Product;
using Action = ASC.Common.Security.Authorizing.Action;

namespace ASC.Bookmarking.Common
{
    public static class BookmarkingBusinessConstants
    {
        public const string BookmarkingDbID = "community";

        public static Guid CommunityProductID = CommunityProduct.ID;

        #region Notify Action String Constants

        public const string BookmarkCreatedID = "new bookmark created";
        public const string BookmarkCommentCreatedID = "new bookmark comment created";

        #endregion

        public const string BookmarkingActionPattern = "ASC.Web.Community.Bookmarking.Core.Patterns.action_pattern.xml";


        public static INotifyAction NotifyActionNewBookmark = new NotifyAction(BookmarkCreatedID, "new-bookmark");
        internal static Guid NotifyActionNewBookmarkID = Guid.NewGuid();

        public static INotifyAction NotifyActionNewComment = new NotifyAction(BookmarkCommentCreatedID, "new-bookmark-comment");
        internal static Guid NotifyActionNewCommentID = Guid.NewGuid();

        public const string SubscriptionRecentBookmarkID = null;


        public static string TagURL = "URL";

        public static string TagUserName = "UserName";
        public static string TagUserURL = "UserURL";
        public static string TagDate = "Date";

        public static string TagPostPreview = "PostPreview";

        public static string TagCommentBody = "CommentBody";
        public static string TagCommentURL = "CommentURL";


        public const int MostPopularBookmarksByTagLimit = 3;

        public const string BookmarkingBasePath = "~/products/community/modules/bookmarking";

        #region Check Permissions

        /// <summary>
        /// base
        /// </summary>
        public static readonly Action BookmarkCreateAction = new Action(
            new Guid("{0D1F72A8-63DA-47ea-AE42-0900E4AC72A9}"),
            "Create bookmark"
            );

        /// <summary>
        /// base
        /// </summary>
        public static readonly Action BookmarkAddToFavouriteAction = new Action(
            new Guid("{FBC37705-A04C-40ad-A68C-CE2F0423F397}"),
            "Add to favorites"
            );

        /// <summary>
        /// base
        /// </summary>
        public static readonly Action BookmarkRemoveFromFavouriteAction = new Action(
            new Guid("{08D66144-E1C9-4065-9AA1-AA4BBA0A7BC8}"),
            "Remove from favorites"
            );

        /// <summary>
        /// base
        /// </summary>
        public static readonly Action BookmarkCreateCommentAction = new Action(
            new Guid("{A362FE79-684E-4d43-A599-65BC1F4E167F}"),
            "Add Comment"
            );

        /// <summary>
        /// base
        /// </summary>
        public static readonly Action BookmarkEditCommentAction = new Action(
            new Guid("{A18480A4-6D18-4c71-84FA-789888791F45}"),
            "Edit comment"
            );

        #endregion
    }
}