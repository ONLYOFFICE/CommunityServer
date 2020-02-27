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


using System.Collections.Generic;
using ASC.Web.Studio.UserControls.Common.ViewSwitcher;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.UserControls.Bookmarking.Resources;

namespace ASC.Web.UserControls.Bookmarking.Common.Util
{
    public class BookmarkingSortUtil
    {
        public SortByEnum? SortBy { get; set; }

        private readonly BookmarkingServiceHelper _serviceHelper = BookmarkingServiceHelper.GetCurrentInstanse();

        #region Sort Items

        private ViewSwitcherLinkItem MostRecentSortItem
        {
            get
            {
                return new ViewSwitcherLinkItem
                    {
                        IsSelected = SortByEnum.MostRecent == SortBy,
                        SortUrl = _serviceHelper.GenerateSortUrl(BookmarkingRequestConstants.MostRecentParam),
                        SortLabel = BookmarkingUCResource.MostRecentLabel
                    };
            }
        }

        private ViewSwitcherLinkItem TopOfTheDaySortItem
        {
            get
            {
                return new ViewSwitcherLinkItem
                    {
                        IsSelected = SortByEnum.TopOfTheDay == SortBy,
                        SortUrl = _serviceHelper.GenerateSortUrl(BookmarkingRequestConstants.TopOfTheDayParam),
                        SortLabel = BookmarkingUCResource.TopOfTheDayLabel
                    };
            }
        }

        private ViewSwitcherLinkItem WeekSortItem
        {
            get
            {
                return new ViewSwitcherLinkItem
                    {
                        IsSelected = SortByEnum.Week == SortBy,
                        SortUrl = _serviceHelper.GenerateSortUrl(BookmarkingRequestConstants.WeekParam),
                        SortLabel = BookmarkingUCResource.WeekLabel
                    };
            }
        }

        private ViewSwitcherLinkItem MonthSortItem
        {
            get
            {
                return new ViewSwitcherLinkItem
                    {
                        IsSelected = SortByEnum.Month == SortBy,
                        SortUrl = _serviceHelper.GenerateSortUrl(BookmarkingRequestConstants.MonthParam),
                        SortLabel = BookmarkingUCResource.MonthLabel
                    };
            }
        }

        private ViewSwitcherLinkItem YearSortItem
        {
            get
            {
                return new ViewSwitcherLinkItem
                    {
                        IsSelected = SortByEnum.Year == SortBy,
                        SortUrl = _serviceHelper.GenerateSortUrl(BookmarkingRequestConstants.YearParam),
                        SortLabel = BookmarkingUCResource.YearLabel,
                    };
            }
        }

        private ViewSwitcherLinkItem PopularitySortItem
        {
            get
            {
                return new ViewSwitcherLinkItem
                    {
                        IsSelected = SortByEnum.Popularity == SortBy,
                        SortUrl = _serviceHelper.GenerateSortUrl(BookmarkingRequestConstants.PopularityParam),
                        SortLabel = BookmarkingUCResource.PopularityLabel,
                    };
            }
        }

        #region Tags Sort Items

        private ViewSwitcherLinkItem SearchBookmarksMostRecentSortItem
        {
            get
            {
                return new ViewSwitcherLinkItem
                    {
                        IsSelected = SortByEnum.MostRecent == SortBy,
                        SortUrl = _serviceHelper.GetSearchMostRecentBookmarksUrl(),
                        SortLabel = BookmarkingUCResource.MostRecentLabel
                    };
            }
        }

        private ViewSwitcherLinkItem SearchBookmarksPopularitySortItem
        {
            get
            {
                return new ViewSwitcherLinkItem
                    {
                        IsSelected = SortByEnum.Popularity == SortBy,
                        SortUrl = _serviceHelper.GetSearchMostPopularBookmarksUrl(),
                        SortLabel = BookmarkingUCResource.PopularityLabel,
                    };
            }
        }

        private ViewSwitcherLinkItem BookmarkCreatedByUserMostRecentSortItem
        {
            get
            {
                return new ViewSwitcherLinkItem
                    {
                        IsSelected = SortByEnum.MostRecent == SortBy,
                        SortUrl = _serviceHelper.GetMostRecentBookmarksCreateByUserUrl(),
                        SortLabel = BookmarkingUCResource.MostRecentLabel
                    };
            }
        }

        private ViewSwitcherLinkItem BookmarkCreatedByUserPopularitySortItem
        {
            get
            {
                return new ViewSwitcherLinkItem
                    {
                        IsSelected = SortByEnum.Popularity == SortBy,
                        SortUrl = _serviceHelper.GetMostPopularBookmarksCreateByUserUrl(),
                        SortLabel = BookmarkingUCResource.PopularityLabel,
                    };
            }
        }

        #endregion

        #region Get Bookmarks By Tag Sort Items

        private ViewSwitcherLinkItem MostRecentBookmarksByTag
        {
            get
            {
                return new ViewSwitcherLinkItem
                    {
                        IsSelected = SortByEnum.MostRecent == SortBy,
                        SortUrl = _serviceHelper.GetSearchMostRecentBookmarksByTagUrl(),
                        SortLabel = BookmarkingUCResource.MostRecentLabel
                    };
            }
        }

        private ViewSwitcherLinkItem MostPopularBookmarksByTagSortItem
        {
            get
            {
                return new ViewSwitcherLinkItem
                    {
                        IsSelected = SortByEnum.Popularity == SortBy,
                        SortUrl = _serviceHelper.GetSearchMostPopularBookmarksByTagUrl(),
                        SortLabel = BookmarkingUCResource.PopularityLabel,
                    };
            }
        }

        #endregion

        #endregion

        #region Generate Sort Items Collections

        public List<ViewSwitcherBaseItem> GetMainPageSortItems(SortByEnum sortBy)
        {
            SortBy = sortBy;
            var sortItems = new List<ViewSwitcherBaseItem>
                {
                    MostRecentSortItem,
                    TopOfTheDaySortItem,
                    WeekSortItem,
                    MonthSortItem,
                    YearSortItem
                };
            return sortItems;
        }

        public List<ViewSwitcherBaseItem> GetFavouriteBookmarksSortItems(SortByEnum sortBy)
        {
            SortBy = sortBy;
            var sortItems = new List<ViewSwitcherBaseItem>
                {
                    MostRecentSortItem,
                    PopularitySortItem
                };
            return sortItems;
        }


        public List<ViewSwitcherBaseItem> GetBookmarksByTagSortItems(SortByEnum sortBy)
        {
            SortBy = sortBy;
            var sortItems = new List<ViewSwitcherBaseItem>
                {
                    MostRecentBookmarksByTag,
                    MostPopularBookmarksByTagSortItem
                };
            return sortItems;
        }

        public List<ViewSwitcherBaseItem> GetBookmarksByTagSortItems(string sortBy)
        {
            return GetBookmarksByTagSortItems(ConvertToSortByEnum(sortBy));
        }

        public List<ViewSwitcherBaseItem> GetSearchBookmarksSortItems(SortByEnum sortBy)
        {
            SortBy = sortBy;
            var sortItems = new List<ViewSwitcherBaseItem>
                {
                    SearchBookmarksMostRecentSortItem,
                    SearchBookmarksPopularitySortItem
                };
            return sortItems;
        }

        public List<ViewSwitcherBaseItem> GetSearchBookmarksSortItems(string sortBy)
        {
            return GetSearchBookmarksSortItems(ConvertToSortByEnum(sortBy));
        }

        public List<ViewSwitcherBaseItem> GetBookmarksCreatedByUserSortItems(string sortBy)
        {
            SortBy = ConvertToSortByEnum(sortBy);
            var sortItems = new List<ViewSwitcherBaseItem>
                {
                    BookmarkCreatedByUserMostRecentSortItem,
                    BookmarkCreatedByUserPopularitySortItem
                };
            return sortItems;
        }

        #endregion

        #region Converter

        public static SortByEnum ConvertToSortByEnum(string param)
        {
            var sortBy = SortByEnum.MostRecent;
            switch (param)
            {
                case BookmarkingRequestConstants.MostRecentParam:
                    sortBy = SortByEnum.MostRecent;
                    return sortBy;
                case BookmarkingRequestConstants.TopOfTheDayParam:
                    sortBy = SortByEnum.TopOfTheDay;
                    return sortBy;
                case BookmarkingRequestConstants.WeekParam:
                    sortBy = SortByEnum.Week;
                    return sortBy;
                case BookmarkingRequestConstants.MonthParam:
                    sortBy = SortByEnum.Month;
                    return sortBy;
                case BookmarkingRequestConstants.YearParam:
                    sortBy = SortByEnum.Year;
                    return sortBy;
                case BookmarkingRequestConstants.PopularityParam:
                    sortBy = SortByEnum.Popularity;
                    return sortBy;
                case BookmarkingRequestConstants.NameParam:
                    sortBy = SortByEnum.Name;
                    return sortBy;
            }
            return sortBy;
        }

        #endregion
    }
}