/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

#region Usings

using System;
using System.Collections.Generic;
using ASC.Bookmarking.Pojo;
using System.Linq;

#endregion

namespace ASC.Bookmarking.Common.Util
{
	public static class Converters
	{
		public static Bookmark ToBookmark(object[] row)
		{
			try
			{
			    return new Bookmark
			               {
			                   ID = Convert.ToInt64(row[0]),
			                   URL = Convert.ToString(row[1]),
			                   Date = Core.Tenants.TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[2])),
			                   Name = Convert.ToString(row[3]),
			                   Description = Convert.ToString(row[4]),
			                   UserCreatorID = new Guid(Convert.ToString(row[5]))
			               };
			}
			catch { return null; }
		}

		public static Bookmark ToBookmarkWithRaiting(object[] row)
		{
			try
			{
				var b = ToBookmark(row);
				var raiting = Convert.ToInt64(row[6]);
				for (var i = 0; i < raiting; i++)
				{
					b.UserBookmarks.Add(new UserBookmark());
				}
				return b;
			}
			catch {return null; }
		}

		public static UserBookmark ToUserBookmark(object[] row)
		{
			try
			{
			    return new UserBookmark
			               {
			                   UserBookmarkID = Convert.ToInt64(row[0]),
			                   UserID = new Guid(Convert.ToString(row[1])),
			                   DateAdded = Core.Tenants.TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[2])),
			                   Name = Convert.ToString(row[3]),
			                   Description = Convert.ToString(row[4]),
			                   BookmarkID = Convert.ToInt64(row[5]),
			                   Raiting = Convert.ToInt32(row[6])
			               };
			}
			catch { return null; }
		}

		public static long ToLong(object[] row)
		{
			try
			{
				return Convert.ToInt64(row[0]);
			}
			catch { return 0; }
		}

		public static Tag ToTag(object[] row)
		{
			try
			{
			    return new Tag
			               {
			                   TagID = Convert.ToInt64(row[0]),
			                   Name = Convert.ToString(row[1])
			               };
			}
			catch { return null; }
		}

		public static Tag ToTagWithRaiting(object[] row)
		{
			try
			{
			    return new Tag
			               {
			                   TagID = Convert.ToInt64(row[0]),
			                   Name = Convert.ToString(row[1]),
			                   Populatiry = Convert.ToInt64(row[2])
			               };
			}
			catch { return null; }
		}

		public static Tag ToTagWithBookmarkID(object[] row)
		{
			try
			{
			    return new Tag
			               {
			                   BookmarkID = Convert.ToInt64(row[0]),
			                   TagID = Convert.ToInt64(row[1]),
			                   Name = Convert.ToString(row[2])
			               };
			}
			catch { return null; }
		}

		public static Comment ToComment(object[] row)
		{
			try
			{
				return new Comment
				{
					ID = new Guid(Convert.ToString(row[0])),
					UserID = new Guid(Convert.ToString(row[1])),
					Content = Convert.ToString(row[2]),
					Datetime = Core.Tenants.TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[3])),
					Parent = Convert.ToString(row[4]),
					BookmarkID = Convert.ToInt64(row[5]),
					Inactive = Convert.ToBoolean(row[6])
				};
			}
			catch { return null; }
		}

        public static Comment ToComment(object[] row, bool withBookmark)
        {
            if (!withBookmark) return ToComment(row);
            try
            {
                var bookmark = new Bookmark
                                   {
                                       ID = Convert.ToInt64(row[8]),
                                       URL = Convert.ToString(row[9]),
                                       Date = Core.Tenants.TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[10])),
                                       Name = Convert.ToString(row[11]),
                                       Description = Convert.ToString(row[12]),
                                       UserCreatorID = new Guid(Convert.ToString(row[13]))
                                   };

                return new Comment(bookmark)
                {
                    ID = new Guid(Convert.ToString(row[0])),
                    UserID = new Guid(Convert.ToString(row[1])),
                    Content = Convert.ToString(row[2]),
                    Datetime = Core.Tenants.TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[3])),
                    Parent = Convert.ToString(row[4]),
                    BookmarkID = Convert.ToInt64(row[5]),
                    Inactive = Convert.ToBoolean(row[7])
                };
            }
            catch { return null; }
        }

		public static Bookmark ToBookmarkWithUserBookmarks(object[] row)
		{
			try
			{
				var b = new object[10];
				int i;
				for (i = 0; i < 6; i++)
				{
					b[i] = row[i];
				}
				//skip tenant
				//i++;

				var bookmark = ToBookmark(b);

				for (var j = 0; i < 13; i++, j++)
				{
					b[j] = row[i];
				}
				var userBookmark = ToUserBookmark(b);
				if (userBookmark != null)
				{
					bookmark.UserBookmarks.Add(userBookmark);
				}

				//skip tenant
				//i++;
				for (var j = 0; i < row.Length; i++, j++)
				{
					b[j] = row[i];
				}
				var comment = ToComment(b);
				if (comment != null)
				{
					bookmark.Comments.Add(comment);
				}

				return bookmark;
			}
			catch { return null; }

		}

		public static Bookmark ToBookmarkWithUserBookmarks(IList<Bookmark> bookmarks)
		{
			try
			{
				if (bookmarks != null && bookmarks.Count > 0)
				{
					var result = bookmarks[0];
					foreach (var b in bookmarks)
					{
						if (b.UserBookmarks.Count != 0)
						{
							var ub = b.UserBookmarks[0];
							if (!result.UserBookmarks.Contains(ub))
							{
								result.UserBookmarks.Add(ub);
							}
						}

						if (b.Comments.Count != 0)
						{
							var c = b.Comments[0];
							if (!result.Comments.Contains(c))
							{
								result.Comments.Add(c);
							}
						}
					}
					if (result != null && result.Comments != null)
					{
					    result.Comments = (from r in result.Comments
					                       orderby r.Datetime ascending
					                       select r).ToList<Comment>();
					}
					return result;
				}
				return null;
			}
			catch { return null; }
		}



		public static IList<Bookmark> ToBookmarkWithUserBookmarks(IList<Bookmark> bookmarks, IList<long> bookmarkIds)
		{
		    try
			{
				if (bookmarks != null && bookmarks.Count > 0)
				{
					var resultList = new List<Bookmark>();
					foreach (var id in bookmarkIds)
					{
						var bookmarksById = GetBookmarksById(id, bookmarks);
						if (bookmarksById.Count == 0)
						{
							continue;
						}
						var result = bookmarksById[0];
						foreach (var b in bookmarksById)
						{
							if (b.UserBookmarks.Count != 0)
							{
								var ub = b.UserBookmarks[0];
								if (!result.UserBookmarks.Contains(ub))
								{
									result.UserBookmarks.Add(ub);
								}
							}

							if (b.Comments.Count != 0)
							{
								var c = b.Comments[0];
								if (!result.Comments.Contains(c))
								{
									result.Comments.Add(c);
								}
							}
						}
						resultList.Add(result);
					}
					return resultList;
				}
                return null;
			}
            catch { return null; }
		}

	    private static IList<Bookmark> GetBookmarksById(long id, IEnumerable<Bookmark> bookmarks)
		{
			try
			{
			    var result = (from b in bookmarks
			                  where b.ID == id
			                  select b)
			        .ToList<Bookmark>();
				return result;
			}
			catch { return null; }
		}

		public static void SetBookmarksTags(IList<Bookmark> bookmarks, List<Tag> tags)
		{
			try
			{
				if (tags == null || tags.Count == 0)
				{
					return;
				}

				foreach (var b in bookmarks)
				{
					b.Tags.Clear();

					var bookmarkTags = (from t in tags
										where b.ID == t.BookmarkID
										select t)
										.Distinct<Tag>()
										.ToList<Tag>();

					(b.Tags as List<Tag>).AddRange(bookmarkTags);
				}
			}
			catch {  }
		}
	}
}
