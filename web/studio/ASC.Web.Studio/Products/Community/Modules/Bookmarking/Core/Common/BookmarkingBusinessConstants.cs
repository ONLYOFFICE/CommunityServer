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

        public const string BookmarkingBasePath = "~/Products/Community/Modules/Bookmarking";

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