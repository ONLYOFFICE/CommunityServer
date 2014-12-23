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