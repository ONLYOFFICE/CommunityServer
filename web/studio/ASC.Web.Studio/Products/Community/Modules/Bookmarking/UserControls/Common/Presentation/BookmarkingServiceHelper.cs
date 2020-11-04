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


using ASC.Bookmarking;
using ASC.Bookmarking.Business;
using ASC.Bookmarking.Common;
using ASC.Bookmarking.Dao;
using ASC.Bookmarking.Pojo;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Users;
using ASC.Notify.Model;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.UserControls.Common.ViewSwitcher;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Bookmarking.Common.Util;
using ASC.Web.UserControls.Bookmarking.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Web.Community.Product;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.UserControls.Bookmarking.Common.Presentation
{
    /// <summary>
    /// This class is used for the interaction between presentation and business layers.
    /// </summary>
    public class BookmarkingServiceHelper : BookmarkingSessionObject<BookmarkingServiceHelper>
    {
        private readonly IBookmarkingService _service = BookmarkingService.GetCurrentInstanse();
        private readonly BookmarkingHibernateDao _dao = BookmarkingHibernateDao.GetCurrentInstanse();
        private ViewSwitcher _sortControl;

        public CommentsList Comments { get; set; }

        private int PageCounter { get; set; }

        public int SelectedTab
        {
            get
            {
                try
                {
                    var tab = CurrentRequest.QueryString[BookmarkingRequestConstants.SelectedTab];
                    switch (tab)
                    {
                        case BookmarkingRequestConstants.SelectedTabBookmarkAddedBy:
                            return 1;
                        default:
                            return 0;
                    }
                }
                catch
                {
                    return 0;
                }
            }
        }

        private HttpRequest CurrentRequest
        {
            get { return HttpContext.Current.Request; }
        }

        public Bookmark BookmarkToAdd { get; private set; }

        private int _maxResults = BookmarkingSettings.BookmarksCountOnPage;

        public int MaxResults
        {
            get { return _maxResults; }
            set { _maxResults = value; }
        }

        public int FirstResult { get; set; }

        public int CurrentPageNumber { get; set; }

        public long ItemsCount
        {
            get { return _dao.ItemsCount; }
        }

        #region Subscription Fields

        public string SubscriptionRecentBookmarkID
        {
            get { return BookmarkingBusinessConstants.SubscriptionRecentBookmarkID; }
        }

        public string SubscriptionBookmarkCommentsID { get; set; }


        #endregion

        #region Init

        public void InitServiceHelper(ViewSwitcher sortControl)
        {
            _sortControl = sortControl;
        }

        #endregion

        #region Add Bookmark

        public Bookmark AddBookmark(string bookmarkUrl, string bookmarkName, string bookmarkDescription, string bookmarkTags)
        {
            bookmarkUrl = GetLimitedTextForDescription(HttpUtility.HtmlEncode(bookmarkUrl));
            bookmarkName = GetLimitedTextForName(EncodeUserData(bookmarkName, false));
            bookmarkDescription = GetLimitedTextForDescription(EncodeUserData(bookmarkDescription, false));

            BookmarkToAdd = GetBookmarkByUrl(bookmarkUrl);


            var tags = ConvertStringToTags(bookmarkTags);
            var bookmark = UserBookmarkInit(bookmarkUrl, bookmarkName, bookmarkDescription, tags);
            if (bookmark != null)
            {
                if (BookmarkToAdd != null && BookmarkToAdd.UserCreatorID.Equals(bookmark.UserID))
                {
                    var b = BookmarkInit(BookmarkToAdd, bookmarkName, bookmarkDescription, tags);
                    BookmarkToAdd = _service.UpdateBookmark(b, tags);
                    BookmarkingServiceHelper.UpdateCurrentInstanse(this);
                    return BookmarkToAdd;
                }

                BookmarkToAdd = _service.UpdateBookmark(bookmark, tags);
                BookmarkingServiceHelper.UpdateCurrentInstanse(this);
                return BookmarkToAdd;
            }

            var newBookmark = BookmarkInit(bookmarkUrl, bookmarkName, bookmarkDescription);
            BookmarkToAdd = _service.AddBookmark(newBookmark, tags);
            BookmarkingServiceHelper.UpdateCurrentInstanse(this);
            return BookmarkToAdd;
        }

        private static string GetLimitedText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text.Length > maxLength ? text.Substring(0, maxLength) : text;
        }

        private static string GetLimitedTextForName(string text)
        {
            const int maxLenght = BookmarkingSettings.NameMaxLength;
            return GetLimitedText(text, maxLenght);
        }

        private static string GetLimitedTextForDescription(string text)
        {
            const int maxLenght = BookmarkingSettings.DescriptionMaxLength;
            return GetLimitedText(text, maxLenght);
        }

        public static string EncodeUserData(string text, bool removeSlashes)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            text = text.Replace("\r", " ");
            text = text.Replace("\t", " ");
            text = text.Trim();

            if (removeSlashes)
            {
                text = text.Replace("\\", string.Empty);
                text = text.Replace("/", string.Empty);
            }
            text = HttpUtility.HtmlDecode(text);
            text = HttpUtility.HtmlDecode(text);
            text = text.Replace("\"", "'");
            text = text.ReplaceSingleQuote();

            text = HttpUtility.HtmlEncode(text);

            text = text.Replace("\n", "<br/>");
            return text;
        }

        public static string EncodeUserData(string text)
        {
            return EncodeUserData(text, true);
        }

        private UserBookmark UserBookmarkInit(string bookmarkUrl, string bookmarkName, string bookmarkDescription, IList<Tag> tags)
        {
            if (BookmarkToAdd == null || !BookmarkToAdd.URL.Equals(bookmarkUrl))
            {
                return null;
            }

            var userBookmark = GetCurrentUserBookmark(BookmarkToAdd) ?? new UserBookmark
                {
                    BookmarkID = BookmarkToAdd.ID,
                    UserID = GetCurrentUserID(),
                    Raiting = 1
                };

            userBookmark.DateAdded = ASC.Core.Tenants.TenantUtil.DateTimeNow();
            userBookmark.Name = bookmarkName;
            userBookmark.Description = bookmarkDescription;

            return userBookmark;
        }

        private Bookmark BookmarkInit(string bookmarkUrl, string bookmarkName, string bookmarkDescription)
        {
            var currentUserID = GetCurrentUserID();

            var date = ASC.Core.Tenants.TenantUtil.DateTimeNow();

            var bookmark = new Bookmark(bookmarkUrl, date, bookmarkName, bookmarkDescription)
                {
                    UserCreatorID = currentUserID
                };

            return bookmark;
        }

        private static Bookmark BookmarkInit(Bookmark b, string bookmarkName, string bookmarkDescription, IList<Tag> tags)
        {
            b.Name = bookmarkName;
            b.Description = bookmarkDescription;
            b.Tags = tags;

            return b;
        }

        public UserBookmark GetCurrentUserBookmark(Bookmark b)
        {
            return _service.GetCurrentUserBookmark(b);
        }

        public UserBookmark GetCurrentUserBookmark(IList<UserBookmark> userBookmarks)
        {
            if (userBookmarks == null || userBookmarks.Count == 0)
            {
                return null;
            }
            var currentUserID = GetCurrentUserID();
            return userBookmarks.FirstOrDefault(userBookmark => currentUserID.Equals(userBookmark.UserID));
        }

        public Bookmark GetBookmarkWithUserBookmarks(string url)
        {
            return GetBookmarkWithUserBookmarks(url, true);
        }

        public Bookmark GetBookmarkWithUserBookmarks(string url, bool decodeUrlFlag)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            if (decodeUrlFlag)
            {
                url = HttpUtility.HtmlDecode(url);
                url = HttpUtility.HtmlEncode(url);
            }
            BookmarkToAdd = _service.GetBookmarkWithUserBookmarks(url);
            if (BookmarkToAdd != null)
            {
                SubscriptionBookmarkCommentsID = BookmarkToAdd.ID.ToString();
            }
            BookmarkingServiceHelper.UpdateCurrentInstanse(this);
            return BookmarkToAdd;
        }

        public Bookmark GetBookmarkWithUserBookmarks()
        {
            var url = GetBookmarkInfoUrl();
            return GetBookmarkWithUserBookmarks(url);
        }

        public Guid GetCurrentUserID()
        {
            return _service.GetCurrentUser().ID;
        }

        #endregion

        #region Get Bookmarks

        public IList<Bookmark> GetBookmarks(int itemsCounter)
        {
            var bookmarks = GetSortedBookmarks(itemsCounter);
            bookmarks = GetFullBookmarksInfo(bookmarks);
            return bookmarks;
        }

        private IList<Bookmark> GetSortedBookmarks(int itemsCounter)
        {
            SetPagination(itemsCounter);

            var displayMode = BookmarkingBusinessFactory.GetDisplayMode();

            switch (displayMode)
            {
                case BookmarkDisplayMode.Favourites:
                    var result = GetFavouriteBookmarksByRequest();
                    return result;
                case BookmarkDisplayMode.SelectedBookmark:
                    var bookmarksList = new List<Bookmark>();
                    var url = GetBookmarkInfoUrl();
                    if (url != null)
                    {
                        bookmarksList.Add(GetBookmarkByUrl(url));
                        return bookmarksList;
                    }
                    if (BookmarkToAdd != null)
                    {
                        bookmarksList.Add(BookmarkToAdd);
                        return bookmarksList;
                    }
                    break;
                case BookmarkDisplayMode.SearchBookmarks:
                    return SearchBookmarks();
                case BookmarkDisplayMode.SearchByTag:
                    return SearchBookmarksByTag();
                case BookmarkDisplayMode.BookmarksCreatedByUser:
                    return GetBookmarksCreatedByUser();
            }
            var bookmarks = GetBookmarksByRequest();
            return bookmarks;
        }

        public IList<Bookmark> GetFullBookmarksInfo(IList<Bookmark> bookmarks)
        {
            return _service.GetFullBookmarksInfo(bookmarks);
        }

        private IList<Bookmark> GetBookmarksCreatedByUser()
        {
            var uid = GetUserIDFromRequest();
            _service.GetBookmarksCountCreatedByUser(uid);

            var sortUtil = new BookmarkingSortUtil();

            try
            {
                var sortBy = CurrentRequest.QueryString[BookmarkingRequestConstants.SortByParam];
                _sortControl.SortItems = sortUtil.GetBookmarksCreatedByUserSortItems(sortBy);
            }
            catch
            {
                sortUtil.SortBy = SortByEnum.MostRecent;
            }

            switch (sortUtil.SortBy)
            {
                case SortByEnum.Popularity:
                    return _service.GetMostPopularBookmarksCreatedByUser(uid, FirstResult, MaxResults);
                default:
                    return _service.GetBookmarksCreatedByUser(uid, FirstResult, MaxResults);
            }
        }

        private IList<Bookmark> GetAllBookmarks()
        {
            return _service.GetAllBookmarks(FirstResult, MaxResults);
        }

        private string GetBookmarkInfoUrl()
        {
            var url = CurrentRequest.QueryString[BookmarkingRequestConstants.UrlGetRequest];
            if (String.IsNullOrEmpty(url))
            {
                return null;
            }
            url = UpdateBookmarkInfoUrl(url);
            return HttpUtility.UrlDecode(url);
        }

        public static string UpdateBookmarkInfoUrl(string url)
        {
            url = UpdateBookmarkInfoUrl(url, "http:/");
            url = UpdateBookmarkInfoUrl(url, "https:/");
            return url;
        }

        private static string UpdateBookmarkInfoUrl(string url, string urlPrefix)
        {
            if (!url.StartsWith(urlPrefix + "/") && url.StartsWith(urlPrefix))
            {
                url = url.Insert(urlPrefix.Length, "/");
            }
            return url;
        }

        private IList<Bookmark> GetFavouriteBookmarksByRequest()
        {
            var sortByParam = CurrentRequest.QueryString[BookmarkingRequestConstants.SortByParam];
            var sortBy = SortByEnum.MostRecent;
            if (string.IsNullOrEmpty(sortByParam))
            {
                InitSortUtil(sortBy);
                return _service.GetFavouriteBookmarksSortedByDate(FirstResult, MaxResults);
            }
            switch (sortByParam)
            {
                case BookmarkingRequestConstants.PopularityParam:
                    sortBy = SortByEnum.Popularity;
                    InitSortUtil(sortBy);
                    return _service.GetFavouriteBookmarksSortedByRaiting(FirstResult, MaxResults);
                default:
                    sortBy = SortByEnum.MostRecent;
                    InitSortUtil(sortBy);
                    return _service.GetFavouriteBookmarksSortedByDate(FirstResult, MaxResults);
            }
        }

        private IList<Bookmark> GetBookmarksByRequest()
        {
            #region Sanity Request check

            if (CurrentRequest == null)
            {
                return GetAllBookmarks();
            }

            var queryString = CurrentRequest.QueryString;
            if (queryString == null || queryString.Count == 0)
            {
                return GetBookmarksBySortEnum(null);
            }

            #endregion

            var sortByParam = CurrentRequest.QueryString[BookmarkingRequestConstants.SortByParam];
            return GetBookmarksBySortEnum(sortByParam);
        }

        public IList<Bookmark> GetMostRecentBookmarks(int bookmarksCount)
        {
            return _service.GetMostRecentBookmarks(0, bookmarksCount);
        }

        public IList<Bookmark> GetMostRecentBookmarksForWidget(int bookmarksCount)
        {
            return _service.GetMostRecentBookmarksWithRaiting(0, bookmarksCount);
        }

        private IList<Bookmark> GetBookmarksBySortEnum(string sortByParam)
        {
            var sortBy = SortByEnum.MostRecent;
            if (string.IsNullOrEmpty(sortByParam))
            {
                InitSortUtil(sortBy);
                return _service.GetMostRecentBookmarks(FirstResult, MaxResults);
            }

            switch (sortByParam)
            {
                case BookmarkingRequestConstants.TopOfTheDayParam:
                    sortBy = SortByEnum.TopOfTheDay;
                    InitSortUtil(sortBy);
                    return _service.GetTopOfTheDay(FirstResult, MaxResults);
                case BookmarkingRequestConstants.WeekParam:
                    sortBy = SortByEnum.Week;
                    InitSortUtil(sortBy);
                    return _service.GetTopOfTheWeek(FirstResult, MaxResults);
                case BookmarkingRequestConstants.MonthParam:
                    sortBy = SortByEnum.Month;
                    InitSortUtil(sortBy);
                    return _service.GetTopOfTheMonth(FirstResult, MaxResults);
                case BookmarkingRequestConstants.YearParam:
                    sortBy = SortByEnum.Year;
                    InitSortUtil(sortBy);
                    return _service.GetTopOfTheYear(FirstResult, MaxResults);
                default:
                    sortBy = SortByEnum.MostRecent;
                    InitSortUtil(sortBy);
                    return _service.GetMostRecentBookmarks(FirstResult, MaxResults);
            }
        }

        private void InitSortUtil(SortByEnum sortBy)
        {
            var sortUtil = new BookmarkingSortUtil();

            var displayMode = BookmarkingBusinessFactory.GetDisplayMode();

            switch (displayMode)
            {
                case BookmarkDisplayMode.Favourites:
                    _sortControl.SortItems = sortUtil.GetFavouriteBookmarksSortItems(sortBy);
                    break;
                default:
                    _sortControl.SortItems = sortUtil.GetMainPageSortItems(sortBy);
                    break;
            }
        }

        public string GetSearchText()
        {
            var searchString = CurrentRequest.QueryString[BookmarkingRequestConstants.Search];
            return !String.IsNullOrEmpty(searchString) ? searchString : null;
        }

        public string GetSearchTag()
        {
            var searchString = CurrentRequest.QueryString[BookmarkingRequestConstants.Tag];
            return !String.IsNullOrEmpty(searchString) ? searchString : null;
        }

        public string GetCurrentURL()
        {
            return CurrentRequest.GetUrlRewriter().ToString();
        }

        private string GetCurrentURLForPagination()
        {
            var currentUrl = CurrentRequest.GetUrlRewriter().ToString();
            var sb = new StringBuilder();
            sb.Append(BookmarkingRequestConstants.AMPERSAND_SYMBOL);
            sb.Append(BookmarkingRequestConstants.Pagination);
            sb.Append(BookmarkingRequestConstants.EqualsSymbol);
            sb.Append("[\\d]*");

            var a = Regex.Split(currentUrl, sb.ToString());

            sb = new StringBuilder();
            foreach (var item in a)
            {
                sb.Append(item);
            }

            currentUrl = sb.ToString();
            if (currentUrl.IndexOf(BookmarkingRequestConstants.Question_Sybmol) == -1)
            {
                currentUrl += BookmarkingRequestConstants.Question_Sybmol;
            }

            return currentUrl;
        }

        private IList<Tag> ConvertStringToTags(string tagsString)
        {
            return ConvertStringToTags(tagsString, ',');
        }

        private static IList<string> ConvertStringToArray(string searchString)
        {
            var separator = ' ';
            searchString = RemoveExtraSpaces(searchString);
            var a = searchString.Split(separator);
            var tagsList = new List<string>();
            foreach (var s in a)
            {
                if (String.IsNullOrEmpty(s.Trim()))
                {
                    continue;
                }
                tagsList.Add(s.Trim());
            }
            return tagsList;
        }

        private IList<Tag> ConvertStringToTags(string searchString, char separator)
        {
            searchString = RemoveExtraSpaces(searchString);
            var a = searchString.Split(separator);
            var tagsList = new List<Tag>();
            foreach (var s in a)
            {
                if (String.IsNullOrEmpty(s.Trim()))
                {
                    continue;
                }
                var t = new Tag { Name = GetLimitedTextForName(s.Trim()) };
                if (tagsList.Contains(t))
                {
                    continue;
                }
                var tag = CreateOrGetTag(t);
                tagsList.Add(tag);
            }
            return tagsList;
        }

        private Tag CreateOrGetTag(Tag t)
        {
            var tags = _service.GetAllTags();

            var tagName = t.Name.ToLower();

            foreach (var tag in tags)
            {
                if (tag.Name.ToLower().Equals(tagName))
                {
                    return tag;
                }
            }
            return t;
        }

        public IList<Tag> GetBookmarkTags(Bookmark b)
        {
            return _service.GetBookmarkTags(b);
        }

        public static string ConvertTagsToString(IList<Tag> tags)
        {
            var tagsString = new StringBuilder();
            var i = 0;
            var count = tags.Count;
            foreach (var tag in tags)
            {
                tagsString.Append(tag.Name);
                i++;
                if (i < count)
                {
                    tagsString.Append(',');
                }
            }
            return tagsString.ToString();
        }

        public static string ConvertBookmarkToTagsString(Bookmark b)
        {
            return ConvertTagsToString(b.Tags);
        }

        private static string RemoveExtraSpaces(string searchString)
        {
            return Regex.Replace(searchString, @"\s+", " ");
        }

        #endregion

        public void RemoveBookmark(long userBookmarkID)
        {
            var bookmark = _service.GetBookmarkByID(userBookmarkID);

            if (!CommunitySecurity.IsAdministrator() && !bookmark.UserCreatorID.Equals(SecurityContext.CurrentAccount.ID)) throw new SecurityException();
            var userBookmarks = _service.GetUserBookmarks(bookmark);

            foreach (var userBookmark in userBookmarks)
            {
                RemoveBookmarkFromFavourite(userBookmark.BookmarkID, userBookmark.UserID);
            }
        }

        #region Current User Info

        public static UserInfo GetUserInfo(Guid userID)
        {
            var userInfo = CoreContext.UserManager.GetUsers(userID);
            return userInfo;
        }

        public string GetUserNameByRequestParam()
        {
            var uid = GetUserIDFromRequest();
            return GetUserInfo(uid).DisplayUserName();
        }

        private Guid GetUserIDFromRequest()
        {
            var uid = CurrentRequest.QueryString[BookmarkingRequestConstants.UidParam];
            if (!string.IsNullOrEmpty(uid))
            {
                var userID = new Guid(uid);
                if (!Guid.Empty.Equals(userID))
                {
                    return userID;
                }
            }
            return Guid.Empty;
        }

        #endregion

        #region Sort Params

        public static string GenerateBookmarkInfoUrl(string url)
        {
            var sb = new StringBuilder();
            sb.Append(BookmarkingRequestConstants.BookmarkInfoPageName);
            sb.Append(BookmarkingRequestConstants.Question_Sybmol);

            sb.Append(BookmarkingRequestConstants.UrlGetRequest);
            sb.Append(BookmarkingRequestConstants.EqualsSymbol);

            sb.Append(HttpUtility.UrlEncode(url));

            return sb.ToString();
        }

        public string GenerateSortUrl(string sortBy)
        {
            var displayMode = BookmarkingBusinessFactory.GetDisplayMode();

            switch (displayMode)
            {
                case BookmarkDisplayMode.Favourites:
                    return GenerateSortUrlWithPageName(BookmarkingRequestConstants.FavouriteBookmarksPageName, sortBy);
                default:
                    return GenerateSortUrlWithPageName(BookmarkingRequestConstants.BookmarkingPageName, sortBy);
            }
        }

        public string GenerateSortUrlWithPageName(string pageName, string sortBy)
        {
            var sb = new StringBuilder();

            sb.Append(pageName);

            sb.Append(BookmarkingRequestConstants.Question_Sybmol);

            sb.Append(BookmarkingRequestConstants.SortByParam);
            sb.Append(BookmarkingRequestConstants.EqualsSymbol);

            sb.Append(sortBy);
            return sb.ToString();
        }

        public string GetSearchByTagUrl(Tag tag)
        {
            if (tag != null && !string.IsNullOrEmpty(tag.Name))
            {
                return GetSearchByTagUrl(tag.Name);

            }
            return string.Empty;
        }

        public string GetSearchByTagUrl(string tagName)
        {
            var sb = new StringBuilder();
            sb.Append(BookmarkingRequestConstants.BookmarkingPageName);
            if (string.IsNullOrEmpty(tagName))
            {
                return sb.ToString();
            }
            sb.Append(BookmarkingRequestConstants.Question_Sybmol);
            sb.Append(BookmarkingRequestConstants.Tag);
            sb.Append(BookmarkingRequestConstants.EqualsSymbol);
            sb.Append(HttpUtility.UrlEncode(tagName));
            return sb.ToString();
        }

        public string GetSearchMostRecentBookmarksByTagUrl()
        {
            var tagName = GetSearchTag();
            var sb = new StringBuilder(GetSearchByTagUrl(tagName));
            sb.Append(BookmarkingRequestConstants.AMPERSAND_SYMBOL);
            sb.Append(BookmarkingRequestConstants.SortByParam);
            sb.Append(BookmarkingRequestConstants.EqualsSymbol);
            sb.Append(BookmarkingRequestConstants.SortByDateParam);
            return sb.ToString();
        }

        public string GetSearchMostPopularBookmarksByTagUrl()
        {
            var tagName = GetSearchTag();
            var sb = new StringBuilder(GetSearchByTagUrl(tagName));
            sb.Append(BookmarkingRequestConstants.AMPERSAND_SYMBOL);
            sb.Append(BookmarkingRequestConstants.SortByParam);
            sb.Append(BookmarkingRequestConstants.EqualsSymbol);
            sb.Append(BookmarkingRequestConstants.PopularityParam);
            return sb.ToString();
        }

        private string GetSearchBookmarksUrl()
        {
            var sb = new StringBuilder();
            sb.Append(BookmarkingRequestConstants.BookmarkingPageName);
            var searchText = GetSearchText();
            if (string.IsNullOrEmpty(searchText))
            {
                return sb.ToString();
            }
            sb.Append(BookmarkingRequestConstants.Question_Sybmol);
            sb.Append(BookmarkingRequestConstants.Search);
            sb.Append(BookmarkingRequestConstants.EqualsSymbol);
            sb.Append(searchText);
            return sb.ToString();
        }

        public string GetSearchMostRecentBookmarksUrl()
        {
            var sb = new StringBuilder(GetSearchBookmarksUrl());
            sb.Append(BookmarkingRequestConstants.AMPERSAND_SYMBOL);
            sb.Append(BookmarkingRequestConstants.SortByParam);
            sb.Append(BookmarkingRequestConstants.EqualsSymbol);
            sb.Append(BookmarkingRequestConstants.MostRecentParam);
            return sb.ToString();
        }

        public string GetSearchMostPopularBookmarksUrl()
        {
            var sb = new StringBuilder(GetSearchBookmarksUrl());
            sb.Append(BookmarkingRequestConstants.AMPERSAND_SYMBOL);
            sb.Append(BookmarkingRequestConstants.SortByParam);
            sb.Append(BookmarkingRequestConstants.EqualsSymbol);
            sb.Append(BookmarkingRequestConstants.PopularityParam);
            return sb.ToString();
        }

        private string GetBookmarksCreateByUserUrl()
        {
            var sb = new StringBuilder();
            sb.Append(BookmarkingRequestConstants.BookmarksCreatedByUserPageName);
            sb.Append(BookmarkingRequestConstants.Question_Sybmol);
            sb.Append(BookmarkingRequestConstants.UidParam);
            sb.Append(BookmarkingRequestConstants.EqualsSymbol);
            try
            {
                var uid = GetUserIDFromRequest();

                sb.Append(uid);

            }
            catch
            {
            }
            return sb.ToString();
        }

        public string GetMostRecentBookmarksCreateByUserUrl()
        {
            var sb = new StringBuilder(GetBookmarksCreateByUserUrl());
            sb.Append(BookmarkingRequestConstants.AMPERSAND_SYMBOL);
            sb.Append(BookmarkingRequestConstants.SortByParam);
            sb.Append(BookmarkingRequestConstants.EqualsSymbol);
            sb.Append(BookmarkingRequestConstants.MostRecentParam);
            return sb.ToString();
        }

        public string GetMostPopularBookmarksCreateByUserUrl()
        {
            var sb = new StringBuilder(GetBookmarksCreateByUserUrl());
            sb.Append(BookmarkingRequestConstants.AMPERSAND_SYMBOL);
            sb.Append(BookmarkingRequestConstants.SortByParam);
            sb.Append(BookmarkingRequestConstants.EqualsSymbol);
            sb.Append(BookmarkingRequestConstants.PopularityParam);
            return sb.ToString();
        }

        public static string GetCreateBookmarkPageUrl()
        {
            return BookmarkingRequestConstants.CreateBookmarkPageName;
        }

        #endregion

        #region Tags

        public IList<Tag> GetAllTags(string startSymbols, int limit)
        {
            return _service.GetAllTags(startSymbols, limit);
        }


        #endregion

        #region Get Thumbnail Url

        public static string GetThumbnailUrl(string Url)
        {
            return GetThumbnailUrl(Url, BookmarkingSettings.ThumbSmallSize);
        }

        public static string GetMediumThumbnailUrl(string Url)
        {
            return GetThumbnailUrl(Url, BookmarkingSettings.ThumbMediumSize);
        }

        public static string GetThumbnailUrl(string Url, BookmarkingThumbnailSize size)
        {
            try
            {
                var uri = new Uri(Url);
                var imageUrl = ThumbnailHelper.Instance.GetThumbnailUrl(HttpUtility.UrlEncode(uri.OriginalString), size);
                return !String.IsNullOrEmpty(imageUrl)
                           ? imageUrl
                           : WebImageSupplier.GetAbsoluteWebPath(BookmarkingRequestConstants.NoImageAvailable, BookmarkingSettings.ModuleId);
            }
            catch (Exception)
            {
                return WebImageSupplier.GetAbsoluteWebPath(BookmarkingRequestConstants.NoImageAvailable, BookmarkingSettings.ModuleId);
            }
        }

        public static string GetThumbnailServiceUrlForUpdate(string Url, string serviceUrl)
        {
            var size = string.Format("{0}x{1}", BookmarkingSettings.ThumbSmallSize.Width, BookmarkingSettings.ThumbSmallSize.Height);
            var imageUrl = string.Format("{0}?url={1}&f=png&s={2}&p={3}", serviceUrl, HttpUtility.UrlEncode(HttpUtility.HtmlDecode(Url)), size, Url.GetHashCode());
            if (!String.IsNullOrEmpty(imageUrl))
            {
                //Make request
                try
                {
                    var req = (HttpWebRequest)WebRequest.Create(imageUrl);
                    using (var responce = (HttpWebResponse)req.GetResponse())
                    {
                        if (responce.StatusCode == HttpStatusCode.OK)
                        {
                            //Means that thumb ready
                            return imageUrl;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            return null;
        }

        public static string GetThumbnailUrlForUpdate(string Url)
        {
            var imageUrl = ThumbnailHelper.Instance.GetThumbnailUrlForUpdate(Url, BookmarkingSettings.ThumbSmallSize);
            return !String.IsNullOrEmpty(imageUrl) ? imageUrl : null;
        }

        public static void UpdateBookmarkThumbnail(int bookmarkID)
        {
            try
            {
                var b = GetCurrentInstanse()._service.GetBookmarkByID(bookmarkID);
                var bookmarks = new List<Bookmark> { b };
                GenerateAllThumbnails(bookmarks, true);
            }
            catch
            {
            }
        }

        public static void GenerateAllThumbnails(bool overrideFlag)
        {
            try
            {
                var bookmarks = GetCurrentInstanse()._service.GetAllBookmarks();
                GenerateAllThumbnails(bookmarks, overrideFlag);
            }
            catch
            {
            }
        }

        private static void GenerateAllThumbnails(IList<Bookmark> bookmarks, bool overrideFlag)
        {
            try
            {

                var p = new List<object>
                    {
                        bookmarks,
                        HttpContext.Current,
                        TenantProvider.CurrentTenantID,
                        overrideFlag
                    };
                var thread = new System.Threading.Thread(MakeThumbnailCallback);
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.Start(p);
            }
            catch
            {
            }
        }

        private static void MakeThumbnailCallback(object p)
        {
            try
            {
                var obj = p as List<Object>;
                var bookmarks = obj[0] as List<Bookmark>;
                var context = obj[1] as HttpContext;
                var tenantID = (int)obj[2];
                var notOverride = !(bool)obj[3];
                foreach (var b in bookmarks)
                {
                    ThumbnailHelper.Instance.MakeThumbnail(b.URL, false, notOverride, context, tenantID);
                }
            }
            catch
            {
            }
        }

        #endregion

        #region Pagination

        public void SetPagination(int ItemsCounter)
        {
            CurrentPageNumber = 0;
            MaxResults = ItemsCounter;
            var pageNumber = CurrentRequest.QueryString[BookmarkingRequestConstants.Pagination];
            try
            {
                CurrentPageNumber = Convert.ToInt32(pageNumber);
            }
            catch
            {
                CurrentPageNumber = 0;
            }
            if (CurrentPageNumber <= 0)
            {
                CurrentPageNumber = 1;
            }
            FirstResult = (CurrentPageNumber - 1)*MaxResults;
            BookmarkingServiceHelper.UpdateCurrentInstanse(this);
        }

        public void InitPageNavigator(PageNavigator pagination, int BookmarkPageCounter)
        {
            InitPageNavigator(pagination, ItemsCount, BookmarkPageCounter);
        }

        public void InitPageNavigator(PageNavigator pagination, long itemsCount, int BookmarkPageCounter)
        {
            var visiblePageCount = (int)itemsCount/BookmarkPageCounter + 1;
            visiblePageCount = visiblePageCount > BookmarkingSettings.VisiblePageCount ? BookmarkingSettings.VisiblePageCount : visiblePageCount;
            PageCounter = BookmarkPageCounter;
            BookmarkingServiceHelper.UpdateCurrentInstanse(this);
            pagination.CurrentPageNumber = CurrentPageNumber;
            pagination.EntryCountOnPage = BookmarkPageCounter;
            pagination.VisiblePageCount = visiblePageCount;
            pagination.EntryCount = (int)itemsCount;
            pagination.PageUrl = GetCurrentURLForPagination();
            pagination.ParamName = "p";
            pagination.VisibleOnePage = false;
        }

        #endregion

        public Bookmark GetBookmarkByUrl(string url)
        {
            BookmarkToAdd = _service.GetBookmarkByUrl(url);
            BookmarkingServiceHelper.UpdateCurrentInstanse(this);
            return BookmarkToAdd;
        }

        public Bookmark GetBookmarkByID(long bookmakrID)
        {
            return _service.GetBookmarkByID(bookmakrID);
        }

        public IList<UserBookmark> GetUserBookmarks(Bookmark b)
        {
            return _service.GetUserBookmarks(b);
        }

        public long GetUserBookmarksCount(Bookmark b)
        {
            return b.UserBookmarks.Count;
        }

        public bool IsCurrentUserBookmark(Bookmark b)
        {
            var currentUserID = GetCurrentUserID();
            var userBookmarks = b.UserBookmarks;
            if (userBookmarks == null || userBookmarks.Count == 0)
            {
                return false;
            }
            foreach (var user in userBookmarks)
            {
                if (currentUserID.Equals(user.UserID))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsCurrentUserBookmark()
        {
            return BookmarkToAdd != null && IsCurrentUserBookmark(BookmarkToAdd);
        }

        public Bookmark RemoveBookmarkFromFavourite(long bookmarkID, Guid? userID = null)
        {
            var b = _service.RemoveBookmarkFromFavourite(bookmarkID, userID);
            if (b == null)
            {
                ThumbnailHelper.Instance.DeleteThumbnail(BookmarkingService.DeletedBookmarkUrl);
                return null;
            }
            return GetBookmarkWithUserBookmarks(b.URL);
        }

        public bool IsSelectedBookmarkDisplayMode()
        {
            var displayMode = BookmarkingBusinessFactory.GetDisplayMode();

            return BookmarkDisplayMode.SelectedBookmark.Equals(displayMode);
        }

        public IList<Bookmark> GetMostPopularBookmarksByTag(Tag t)
        {
            return _service.GetMostPopularBookmarksByTag(t);
        }

        public IList<Bookmark> GetMostPopularBookmarksByTag(IList<Tag> tags)
        {
            return _service.GetMostPopularBookmarksByTag(tags);
        }

        #region Comments

        public Comment GetCommentById(string commentID)
        {
            return _service.GetCommentById(new Guid(commentID));
        }

        public void RemoveComment(string CommentID)
        {
            _service.RemoveComment(new Guid(CommentID));
        }

        public void UpdateComment(String commentID, string text)
        {
            _service.UpdateComment(new Guid(commentID), text);
        }

        public void AddComment(Comment comment)
        {
            _service.AddComment(comment);
        }

        public long GetCommentsCount(Bookmark b)
        {
            return b.Comments.Count(r => !r.Inactive);
        }

        public IList<Comment> GetBookmarkComments(Bookmark b)
        {
            return b.Comments ?? _service.GetBookmarkComments(b);
        }

        #endregion


        public static string GetHTMLUserAvatar(Guid userID)
        {
            string imgPath = UserPhotoManager.GetBigPhotoURL(userID);
            if (imgPath != null)
                return "<img class=\"userMiniPhoto\" alt='' src=\"" + imgPath + "\"/>";

            return string.Empty;
        }

        public static string GetHTMLUserAvatar()
        {
            return GetHTMLUserAvatar(GetCurrentInstanse().GetCurrentUserID());
        }

        public static string GetUserPageLink(Guid userID)
        {
            var userInfo = CoreContext.UserManager.GetUsers(userID);
            var userPageName = CommonLinkUtility.GetUserProfile(userID);
            return string.Format("<a class='linkHeader' href='{0}'>{1}</a>", userPageName, userInfo.DisplayUserName());
        }

        public static string GetUserPageLink()
        {
            var userInfo = CoreContext.UserManager.GetUsers(GetCurrentInstanse().GetCurrentUserID());
            var userPageName = CommonLinkUtility.GetUserProfile(userInfo.ID);
            return string.Format("<a class='linkHeader' href='{0}'>{1}</a>", userPageName, userInfo.DisplayUserName());
        }

        public static string GetUserBookmarkDescriptionIfChanged(UserBookmark ub)
        {
            try
            {
                var userDescription = ub.Description;
                var b = GetCurrentInstanse().GetBookmarkByID(ub.BookmarkID);

                if (b != null && ub != null && b.UserCreatorID.Equals(ub.UserID))
                {
                    return ub.Description;
                }

                var description = b.Description;
                if (!string.IsNullOrEmpty(userDescription) && !userDescription.ToLower().Equals(description.ToLower()))
                {
                    return userDescription;
                }
            }
            catch
            {
            }
            return string.Empty;
        }

        public static string GetUserBookmarkDescriptionIfChanged(Bookmark b, UserBookmark ub)
        {
            try
            {

                if (b != null && ub != null && b.UserCreatorID.Equals(ub.UserID))
                {
                    return ub.Description;
                }

                var userDescription = ub.Description;
                var description = b.Description;
                if (!string.IsNullOrEmpty(userDescription) && !userDescription.ToLower().Equals(description.ToLower()))
                {
                    return userDescription;
                }
            }
            catch
            {
            }
            return string.Empty;
        }

        #region Subcriptions

        public void Subscribe(string objectID, INotifyAction notifyAction)
        {
            _service.Subscribe(objectID, notifyAction);
        }

        public bool IsSubscribed(string objectID, INotifyAction notifyAction)
        {
            return _service.IsSubscribed(objectID, notifyAction);
        }

        public void UnSubscribe(string objectID, INotifyAction notifyAction)
        {
            _service.UnSubscribe(objectID, notifyAction);
        }

        #endregion



        #region Common Search

        public SearchResultItem[] SearchBookmarksBySearchString(string searchString)
        {
            var searchStringList = ConvertStringToArray(searchString);
            var bookmarks = _service.SearchAllBookmarks(searchStringList);

            var searchResultItems = new List<SearchResultItem>();
            if (bookmarks == null)
            {
                return searchResultItems.ToArray();
            }
            foreach (var b in bookmarks)
            {
                var url = VirtualPathUtility.ToAbsolute(BookmarkingRequestConstants.BookmarkingBasePath) + "/" + GenerateBookmarkInfoUrl(b.URL);
                searchResultItems.Add(new SearchResultItem()
                    {
                        Name = b.Name,
                        Description = HtmlUtil.GetText(b.Description, 120),
                        URL = url,
                        Date = b.Date
                    });
            }
            return searchResultItems.ToArray();
        }

        #endregion

        #region Search

        public IList<Bookmark> SearchBookmarks()
        {
            var text = GetSearchText();
            return SearchBookmarks(text);
        }

        private IList<Bookmark> SearchBookmarks(string searchString)
        {
            var searchStringList = ConvertStringToArray(searchString);
            var sortUtil = new BookmarkingSortUtil();

            try
            {
                var sortBy = CurrentRequest.QueryString[BookmarkingRequestConstants.SortByParam];
                _sortControl.SortItems = sortUtil.GetSearchBookmarksSortItems(sortBy);
            }
            catch
            {
                sortUtil.SortBy = SortByEnum.MostRecent;
            }

            switch (sortUtil.SortBy)
            {
                case SortByEnum.Popularity:
                    return _service.SearchBookmarksSortedByRaiting(searchStringList, FirstResult, MaxResults);
                default:
                    return _service.SearchBookmarks(searchStringList, FirstResult, MaxResults);
            }
        }

        private IList<Bookmark> SearchBookmarksByTag()
        {
            var text = GetSearchTag();
            try
            {
                var sortUtil = new BookmarkingSortUtil();
                var sortBy = CurrentRequest.QueryString[BookmarkingRequestConstants.SortByParam];
                _sortControl.SortItems = sortUtil.GetBookmarksByTagSortItems(sortBy);

                switch (sortUtil.SortBy)
                {
                    case SortByEnum.Popularity:
                        return _service.SearchMostPopularBookmarksByTag(text, FirstResult, MaxResults);
                    default:
                        return _service.SearchBookmarksByTag(text, FirstResult, MaxResults);
                }
            }
            catch
            {
                return _service.SearchBookmarksByTag(text, FirstResult, MaxResults);
            }
        }

        #endregion


        public static string RenderUserProfile(Guid userID)
        {
            return CoreContext.UserManager.GetUsers(userID).RenderCustomProfileLink("describe-text", "link gray");
        }

        public void SetBookmarkTags(Bookmark b)
        {
            _service.SetBookmarkTags(b);
        }
    }
}