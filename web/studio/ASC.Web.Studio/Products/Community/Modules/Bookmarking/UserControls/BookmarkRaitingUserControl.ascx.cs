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


using System.IO;
using System.Text;
using System.Web.UI;
using ASC.Bookmarking.Pojo;
using ASC.Web.Community.Product;
using ASC.Web.UserControls.Bookmarking.Common;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Bookmarking.Common;
using ASC.Bookmarking;
using System;

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
                var displayMode = BookmarkingBusinessFactory.GetDisplayMode();

				return BookmarkDisplayMode.Favourites.Equals(displayMode);
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