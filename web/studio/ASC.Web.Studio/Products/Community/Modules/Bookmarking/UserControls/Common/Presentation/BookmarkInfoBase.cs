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
using ASC.Bookmarking.Business.Permissions;
using ASC.Bookmarking.Pojo;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.UserControls.Bookmarking.Common.Util;
using ASC.Web.UserControls.Bookmarking.Resources;
using ASC.Bookmarking.Common;
using ASC.Bookmarking;

namespace ASC.Web.UserControls.Bookmarking.Common.Presentation
{
    public abstract class BookmarkInfoBase : System.Web.UI.UserControl
    {
        #region Fields

        protected void Page_Load(object sender, EventArgs e)
        {
            InitUserControl();
        }

        public abstract void InitUserControl();

        protected BookmarkingServiceHelper ServiceHelper = BookmarkingServiceHelper.GetCurrentInstanse();

        #endregion

        #region Bookmark Fields

        public Bookmark Bookmark { get; set; }

        public UserBookmark UserBookmark { get; set; }

        public string Description
        {
            get { return Bookmark.Description.ReplaceSingleQuote(); }
        }

        public string URL
        {
            get { return Bookmark.URL; }
        }

        public string Name
        {
            get { return Bookmark.Name.ReplaceSingleQuote(); }
        }

        public long Raiting
        {
            get { return ServiceHelper.GetUserBookmarksCount(Bookmark); }
        }

        public string TagsString
        {
            get { return BookmarkingServiceHelper.ConvertBookmarkToTagsString(Bookmark).ReplaceSingleQuote(); }
        }

        public DateTime Date
        {
            get { return Bookmark.Date; }
        }

        public long GetBookmarkID()
        {
            return Bookmark == null ? 0 : Bookmark.ID;
        }

        public string UserBookmarkDescription
        {
            get
            {
                return UserBookmark == null
                           ? Description
                           : UserBookmark.Description.ReplaceSingleQuote();
            }
        }

        public bool HasDescription()
        {
            return !string.IsNullOrEmpty(UserBookmarkDescription);
        }

        public string UserBookmarkName
        {
            get
            {
                return UserBookmark == null
                           ? Name
                           : UserBookmark.Name.ReplaceSingleQuote();
            }
        }

        public string UserTagsString { get; set; }

        public DateTime UserDate
        {
            get
            {
                return UserBookmark == null
                           ? Date
                           : UserBookmark.DateAdded;
            }
        }

        public bool IsTagsIncluded()
        {
            if (Bookmark != null)
            {
                var tags = Bookmark.Tags;
                if (tags != null && tags.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsCurrentUserBookmark()
        {
            return ServiceHelper.IsCurrentUserBookmark(Bookmark);
        }

        public string GetThumbnailUrl()
        {
            return BookmarkingServiceHelper.GetThumbnailUrl(URL);
        }

        public string GetMediumThumbnailUrl()
        {
            return BookmarkingServiceHelper.GetMediumThumbnailUrl(URL);
        }

        public string GetTagsWebPath()
        {
            return WebImageSupplier.GetAbsoluteWebPath(BookmarkingRequestConstants.TagsImageName, BookmarkingSettings.ModuleId);
        }

        public string GetSearchByTagUrl(object bookmarkTagName)
        {
            var name = bookmarkTagName as string;
            return ServiceHelper.GetSearchByTagUrl(name);
        }

        public string GetBookmarkInfoUrlAddedByTab()
        {
            var url = string.Format("{0}&{1}={2}",
                                    BookmarkingServiceHelper.GenerateBookmarkInfoUrl(URL),
                                    BookmarkingRequestConstants.SelectedTab,
                                    BookmarkingRequestConstants.SelectedTabBookmarkAddedBy
                );
            return url;
        }

        public string GetBookmarkInfoUrl()
        {
            return BookmarkingServiceHelper.GenerateBookmarkInfoUrl(URL);
        }

        public string GetBookmarkRaiting()
        {
            return new BookmarkRaitingUserControl().GetBookmarkRaiting(Bookmark, GetUniqueId().ToString(), GetSingleBookmarkID());
        }

        public string GetUserBookmarkRaiting()
        {
            return new BookmarkRaitingUserControl().GetBookmarkRaiting(Bookmark, UserBookmark, GetUniqueId().ToString(), GetSingleBookmarkID());
        }

        public string GetSingleBookmarkID()
        {
            return GetUniqueId() + "SingleBookmark";
        }

        #endregion

        #region User Info

        public bool ShowAddedByUserInfo
        {
            get
            {
                var displayMode = BookmarkingBusinessFactory.GetDisplayMode();

                switch (displayMode)
                {
                    case BookmarkDisplayMode.AllBookmarks:
                        return true;
                    case BookmarkDisplayMode.SelectedBookmark:
                        return true;
                }
                return false;
            }
        }

        public string RenderProfileLink()
        {
            return RenderProfileLink(GetBookmarkCreator());
        }

        public string RenderProfileLink(UserInfo userInfo)
        {
            try
            {
                return userInfo.RenderCustomProfileLink("describe-text", "link gray");
            }
            catch
            {
                return string.Empty;
            }
        }

        public string RenderProfileLink(Object userID)
        {
            var user = GetUserInfoByUserID(userID);
            return RenderProfileLink(user);
        }

        public string GetBookmarkCreatorImageUrl()
        {
            return GetImageUrl(GetBookmarkCreator());
        }

        private static string GetImageUrl(UserInfo userInfo)
        {
            try
            {
                return userInfo.GetMediumPhotoURL();
            }
            catch
            {
                return string.Empty;
            }
        }

        protected UserInfo GetBookmarkCreator()
        {
            return Bookmark == null
                       ? null
                       : CoreContext.UserManager.GetUsers(Bookmark.UserCreatorID);
        }

        public string GetUserImageUrl(object userID)
        {
            var user = GetUserInfoByUserID(userID);
            return GetImageUrl(user);
        }

        public string GetUserImage(object userID)
        {
            var user = GetUserInfoByUserID(userID);
            return BookmarkingServiceHelper.GetHTMLUserAvatar(user.ID);
        }

        private static UserInfo GetUserInfoByUserID(object userID)
        {
            try
            {
                var id = (Guid) userID;
                return CoreContext.UserManager.GetUsers(id);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Uuid

        private Guid _uniqueId;

        public Guid UniqueId
        {
            get
            {
                if (Guid.Empty.Equals(_uniqueId))
                {
                    _uniqueId = Guid.NewGuid();
                }
                return _uniqueId;
            }
            set { _uniqueId = value; }
        }

        /// <summary>
        /// Return unique id, which will be upted for every instance.
        /// Calling this method for the same instance will return the same result.
        /// </summary>
        /// <returns></returns>
        public Guid GetUniqueId()
        {
            return UniqueId;
        }

        #endregion

        #region Added By Info

        public string GetDateAddedAsString(object date)
        {
            try
            {
                var dateAdded = (DateTime) date;
                return BookmarkingConverter.GetDateAsString(dateAdded);
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetDateAddedAsString()
        {
            try
            {
                return Date.ToShortTimeString() + " " + Date.ToShortDateString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public long CommentsCount
        {
            get
            {
                return Bookmark != null
                           ? ServiceHelper.GetCommentsCount(Bookmark)
                           : 0;
            }
        }

        public string CommentString
        {
            get
            {
                var commentsCount = CommentsCount;
                if (commentsCount == 0)
                {
                    return string.Empty;
                }
                var comments = GrammaticalHelper.ChooseNumeralCase((int) commentsCount,
                                                                   BookmarkingUCResource.CommentsNominative,
                                                                   BookmarkingUCResource.CommentsGenitiveSingular,
                                                                   BookmarkingUCResource.CommentsGenitivePlural);

                return String.Format("{0} {1}", commentsCount, comments);
            }
        }

        public string GetUserPageLink(object userID)
        {
            return BookmarkingServiceHelper.GetUserPageLink(new Guid(userID.ToString()));
        }

        #endregion

        #region Display Mode

        public bool IsAllBookmarksMode()
        {
            var displayMode = BookmarkingBusinessFactory.GetDisplayMode();

            return BookmarkDisplayMode.AllBookmarks == displayMode;
        }

        public bool IsBookmarkInfoMode
        {
            get
            {
                var displayMode = BookmarkingBusinessFactory.GetDisplayMode();

                return BookmarkDisplayMode.SelectedBookmark == displayMode;
            }
        }

        public string FavouriteBookmarksMode
        {
            get
            {
                var displayMode = BookmarkingBusinessFactory.GetDisplayMode();

                return (BookmarkDisplayMode.Favourites == displayMode).ToString().ToLower();
            }
        }

        #endregion

        #region Permissions Check

        public static bool PermissionCheckAddToFavourite()
        {
            return BookmarkingPermissionsCheck.PermissionCheckAddToFavourite();
        }

        public bool PermissionCheckRemoveFromFavourite()
        {
            return UserBookmark != null && BookmarkingPermissionsCheck.PermissionCheckRemoveFromFavourite(UserBookmark);
        }

        #endregion

        public string GetUniqueIDFromSingleBookmark(string SingleBookmarkDivID)
        {
            if (!string.IsNullOrEmpty(SingleBookmarkDivID))
            {
                var a = SingleBookmarkDivID.Split(new string[] {"SingleBookmark"}, StringSplitOptions.None);
                if (a.Length == 2)
                {
                    return a[0];
                }
            }
            return Guid.NewGuid().ToString();
        }

        public string GetRandomGuid()
        {
            return Guid.NewGuid().ToString();
        }

        public string GetUserBookmarkDescriptionIfChanged(object o)
        {
            try
            {
                if (o is UserBookmark)
                {
                    var ub = o as UserBookmark;
                    return Bookmark != null
                               ? BookmarkingServiceHelper.GetUserBookmarkDescriptionIfChanged(Bookmark, ub)
                               : BookmarkingServiceHelper.GetUserBookmarkDescriptionIfChanged(ub);
                }
            }
            catch
            {
            }
            return string.Empty;
        }
    }
}