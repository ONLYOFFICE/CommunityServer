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


using System.IO;
using System.Text;
using System.Web.UI;
using ASC.Bookmarking.Pojo;
using ASC.Web.Community.Product;
using ASC.Web.UserControls.Bookmarking.Common;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;

namespace ASC.Web.UserControls.Bookmarking
{
	public partial class BookmarkRaitingUserControl : BookmarkInfoBase
	{

		private long _simpleModeRaiting;

		public long SimpleModeRaiting
		{
			get
			{
				return Bookmark == null ? _simpleModeRaiting : Raiting;
			}
			set
			{
				_simpleModeRaiting = value;
			}
		}

		/// <summary>
		/// ID of the div which contains raiting.
		/// This field is used to the copying raiting to the Add bookmark to Favourites panel.
		/// </summary>
		public string DivId { get; set; }

		public bool SimpleMode { get; set; }

		private string _singleBookmarkDivID;

		public string SingleBookmarkDivID
		{
			get
			{
				return _singleBookmarkDivID == null ? string.Empty : _singleBookmarkDivID;
			}
			set
			{
				_singleBookmarkDivID = value;
			}
		}

		public string GetUniqueIDFromSingleBookmark()
		{
			return GetUniqueIDFromSingleBookmark(SingleBookmarkDivID);
		}

		public bool FavouriteMode
		{
			get
			{
				return ASC.Web.UserControls.Bookmarking.Common.Presentation.BookmarkingServiceHelper.BookmarkDisplayMode.Favourites.Equals(ServiceHelper.DisplayMode);
			}
		}

		public string GetBookmarkRaiting(Bookmark b)
		{
            if (CommunitySecurity.IsOutsider()) return string.Empty;

            StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb))
			{
				using (HtmlTextWriter textWriter = new HtmlTextWriter(sw))
				{
					using (var c = LoadControl(BookmarkUserControlPath.BookmarkRaitingUserControlPath) as BookmarkRaitingUserControl)
					{
						c.SimpleModeRaiting = ServiceHelper.GetUserBookmarksCount(b);
						c.SimpleMode = true;
						c.Bookmark = b;
						c.RenderControl(textWriter);
					}
				}
			}
			return sb.ToString();
		}

		public string GetBookmarkRaiting(Bookmark b, string divID)
		{
            if (CommunitySecurity.IsOutsider()) return string.Empty;

            StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb))
			{
				using (HtmlTextWriter textWriter = new HtmlTextWriter(sw))
				{
					using (var c = LoadControl(BookmarkUserControlPath.BookmarkRaitingUserControlPath) as BookmarkRaitingUserControl)
					{
						c.Bookmark = b;
						c.DivId = divID;
						c.RenderControl(textWriter);
					}
				}
			}
			return sb.ToString();
		}

		public string GetBookmarkRaiting(Bookmark b, string divID, string singleBookmarkDivID)
		{
			return GetBookmarkRaiting(b, null, divID, singleBookmarkDivID);
		}

		public string GetBookmarkRaiting(Bookmark b, UserBookmark ub, string divID, string singleBookmarkDivID)
		{
            if (CommunitySecurity.IsOutsider()) return string.Empty;

            StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb))
			{
				using (HtmlTextWriter textWriter = new HtmlTextWriter(sw))
				{
					using (var c = LoadControl(BookmarkUserControlPath.BookmarkRaitingUserControlPath) as BookmarkRaitingUserControl)
					{
						c.Bookmark = b;
						c.UserBookmark = ub;
						c.DivId = divID;
						c.SingleBookmarkDivID = singleBookmarkDivID;
						c.RenderControl(textWriter);
					}
				}
			}
			return sb.ToString();
		}



		public override void InitUserControl() {	}		
	}
}