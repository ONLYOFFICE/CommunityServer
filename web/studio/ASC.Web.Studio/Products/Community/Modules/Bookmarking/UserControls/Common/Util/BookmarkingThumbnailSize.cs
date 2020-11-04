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
