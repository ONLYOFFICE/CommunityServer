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