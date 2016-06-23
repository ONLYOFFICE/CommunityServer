/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


namespace ASC.Web.UserControls.Bookmarking.Common.Presentation
{
	public static class BookmarkingRequestConstants
	{
		//Page address to be used in a get request
		public const string BookmarkingPageName = "default.aspx";
		public const string Question_Sybmol = "?";
		public const string BookmarkInfoPageName = "bookmarkinfo.aspx";
		public const string FavouriteBookmarksPageName = "favouritebookmarks.aspx";
		public const string CreateBookmarkPageName = "createbookmark.aspx";
		public const string UrlGetRequest = "url";
		public const string SelectedTab = "selectedtab";
		public const string SelectedTabBookmarkCommnets = "bookmarkcommnetstab";
		public const string SelectedTabBookmarkAddedBy = "bookmarkaddedbytab";
		public const string BookmarksCreatedByUserPageName = "userbookmarks.aspx";
		public const string UidParam = "uid";

		//Sorting constants
		public const string SortByParam = "sortby";
		public const string SortByDateParam = "date";
		public const string SortByRaitingParam = "raiting";
		public const string MostRecentParam = "mostrecent";
		public const string TopOfTheDayParam = "topoftheday";
		public const string WeekParam = "week";
		public const string MonthParam = "month";
		public const string YearParam = "year";
		public const string PopularityParam = "popularity";
		public const string NameParam = "name";

		public const string EqualsSymbol = "=";
		public const string AMPERSAND_SYMBOL = "&";

		//Search
		public const string Search = "search";

		//Tags
		public const string Tag = "tag";

		//URL
		public const string URL_Prefix = "http://";
		public const string URL_HTTPS_Prefix = "https://";
		public const string Default_URL = "http://";

		//Tags image
		public const string TagsImageName = "tags.png";

		//Pagination
		public const string Pagination = "p";

		//No thumbnail available image
		public const string NoImageAvailable = "noimageavailable.jpg";

		public const string BookmarkingBasePath = "~/products/community/modules/bookmarking";

		public const string BookmarkingStorageManagerID = "bookmarking";
		
	}
}
