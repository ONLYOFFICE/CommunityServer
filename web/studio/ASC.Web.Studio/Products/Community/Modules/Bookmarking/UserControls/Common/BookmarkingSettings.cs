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