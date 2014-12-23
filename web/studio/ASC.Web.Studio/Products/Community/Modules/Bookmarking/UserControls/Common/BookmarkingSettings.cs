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

using System;
using System.Drawing.Imaging;
using System.Text;
using ASC.Web.UserControls.Bookmarking.Common.Util;
using System.Drawing;

namespace ASC.Web.UserControls.Bookmarking.Common
{
    public class BookmarkingSettings
    {
        public static Guid ModuleId = new Guid("28B10049-DD20-4f54-B986-873BC14CCFC7");

        public const string BookmarkingSctiptKey = "__boomarking_script_key";

        public const string BookmarkingTagsAutocompleteSctiptKey = "__boomarking_Tags_Autocomplete_script_key";

        public static int BookmarksCountOnPage = 10;
        public static int VisiblePageCount = 3;

        public static ImageFormat CaptureImageFormat = ImageFormat.Jpeg;

        public static string ThumbnailAbsoluteFilePath { get; set; }

        public static string ThumbnailAbsolutePath { get; set; }

        public static string ThumbnailRelativePath = "Products/Community/Modules/Bookmarking/Data/images/";

        public const string ThumbnailVirtualPath = "~/Products/Community/Modules/Bookmarking/Data/images/";

        public static int Timeout = 180;

        public static readonly BookmarkingThumbnailSize ThumbSmallSize = new BookmarkingThumbnailSize(96, 72);

        public static readonly BookmarkingThumbnailSize ThumbMediumSize = new BookmarkingThumbnailSize(192, 152);

        public static readonly Size BrowserSize = new Size(1280, 1020);

        public static Encoding PageTitleEncoding = Encoding.GetEncoding("windows-1251");

        public static int PingTimeout = 3000;

        public const int NameMaxLength = 255;

        public const int DescriptionMaxLength = 65535;

    }
}