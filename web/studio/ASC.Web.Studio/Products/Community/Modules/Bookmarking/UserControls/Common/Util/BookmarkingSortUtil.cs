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