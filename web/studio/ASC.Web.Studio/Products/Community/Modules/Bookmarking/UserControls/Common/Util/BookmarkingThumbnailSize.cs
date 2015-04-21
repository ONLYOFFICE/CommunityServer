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


using System;
using System.Text.RegularExpressions;

namespace ASC.Web.UserControls.Bookmarking.Common.Util
{
	public class BookmarkingThumbnailSize
	{
		public int Width { get; set; }

		public int Height { get; set; }		

		public BookmarkingThumbnailSize()
		{
			Width = BookmarkingSettings.ThumbSmallSize.Width;
			Height = BookmarkingSettings.ThumbSmallSize.Height;
		}

		public BookmarkingThumbnailSize(int width, int height)
		{
			Width = width;
			Height = height;
		}

		private const string SizePrefix = "_size";

		public override string ToString()
		{
			return String.Format("{0}{1}-{2}", SizePrefix, Width, Height);
		}

		public static BookmarkingThumbnailSize ParseThumbnailSize(string fileName)
		{
			if (!fileName.Contains(SizePrefix))
			{
				return new BookmarkingThumbnailSize();
			}
			var m = Regex.Match(fileName, String.Format(".*{0}(?<size>).*", SizePrefix));
			var sizeAsString = m.Groups["size"].Value;
			var dimensions = sizeAsString.Split('-');
			if (dimensions.Length == 2)
			{
				try
				{
					var width = Int32.Parse(dimensions[0]);
					var height = Int32.Parse(dimensions[1]);
					return new BookmarkingThumbnailSize(width, height);
				}
				catch
				{
					return new BookmarkingThumbnailSize();
				}
			}
			return new BookmarkingThumbnailSize();
		}
	}
}
