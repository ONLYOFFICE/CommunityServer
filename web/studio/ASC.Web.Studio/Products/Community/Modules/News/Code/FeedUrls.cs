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

using System;
using System.Web;

namespace ASC.Web.Community.News.Code
{
    internal static class FeedUrls
    {
        public static string BaseVirtualPath
        {
            get { return "~/products/community/modules/news/"; }
        }

        public static string MainPageUrl
        {
            get { return VirtualPathUtility.ToAbsolute(BaseVirtualPath); }
        }

        public static string EditNewsName
        {
            get { return "editnews.aspx"; }
        }

        public static string EditPollName
        {
            get { return "editpoll.aspx"; }
        }

        public static string EditNewsUrl
        {
            get { return VirtualPathUtility.ToAbsolute(string.Format("{0}{1}", BaseVirtualPath, EditNewsName)); }
        }

        public static string EditOrderUrl
        {
            get { return VirtualPathUtility.ToAbsolute(string.Format("{0}{1}", BaseVirtualPath, EditNewsName)) + "?type=" + FeedType.Order.ToString(); }
        }

        public static string EditAdvertUrl
        {
            get { return VirtualPathUtility.ToAbsolute(string.Format("{0}{1}", BaseVirtualPath, EditNewsName)) + "?type=" + FeedType.Advert.ToString(); }
        }

        public static string EditPollUrl
        {
            get { return VirtualPathUtility.ToAbsolute(string.Format("{0}{1}", BaseVirtualPath, EditPollName)); }
        }

        public static string GetFeedAbsolutePath(long feedId)
        {
            return VirtualPathUtility.ToAbsolute(BaseVirtualPath) + "?docid=" + feedId.ToString();
        }

        public static string GetFeedVirtualPath(long feedId)
        {
            return string.Format("{0}?docid={1}", BaseVirtualPath, feedId);
        }

        public static string GetFeedUrl(long feedId)
        {
            return GetFeedUrl(feedId, Guid.Empty);
        }

        public static string GetFeedUrl(long feedId, Guid userId)
        {
            var url = string.Format("{0}?docid={1}", MainPageUrl, feedId);
            if (userId != Guid.Empty) url += "&uid=" + userId.ToString();
            return url;
        }

        public static string GetFeedListUrl(FeedType feedType)
        {
            return GetFeedListUrl(feedType, Guid.Empty);
        }

        public static string GetFeedListUrl(Guid userId)
        {
            return GetFeedListUrl(FeedType.All, userId);
        }

        public static string GetFeedListUrl(FeedType feedType, Guid userId)
        {
            var url = MainPageUrl;
            string p1 = null;
            string p2 = null;

            if (feedType != FeedType.All) p1 = "type=" + feedType.ToString();
            if (userId != Guid.Empty) p2 = "uid=" + userId.ToString();
            if (p1 == null && p2 == null) return url;

            url += "?";
            if (p1 != null) url += p1 + "&";
            if (p2 != null) url += p2 + "&";
            return url.Substring(0, url.Length - 1);
        }
    }
}