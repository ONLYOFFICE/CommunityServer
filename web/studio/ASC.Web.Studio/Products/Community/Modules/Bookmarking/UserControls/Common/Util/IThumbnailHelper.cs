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
using System.Configuration;
using System.Web;
using ASC.Web.UserControls.Bookmarking.Common.Util;

namespace ASC.Web.UserControls.Bookmarking.Util
{
    public interface IThumbnailHelper
    {
        void MakeThumbnail(string url, bool async, bool notOverride, HttpContext context, int tenantID);
        string GetThumbnailUrl(string Url, BookmarkingThumbnailSize size);
        string GetThumbnailUrlForUpdate(string Url, BookmarkingThumbnailSize size);
        void DeleteThumbnail(string Url);
    }

    public class ThumbnailHelper
    {
        private static IThumbnailHelper _processHelper = new WebSiteThumbnailHelper();
        private static IThumbnailHelper _serviceHelper = new ServiceThumbnailHelper();
        private static IThumbnailHelper _nullHelper = new NullThumbnailHelper();

        public static bool HasService
        {
            get { return ConfigurationManagerExtension.AppSettings["bookmarking.thumbnail-url"] != null; }
        }

        public static string ServiceUrl
        {
            get { return ConfigurationManagerExtension.AppSettings["bookmarking.thumbnail-url"]; }
        }

        public static IThumbnailHelper Instance
        {
            get
            {
                if (HasService)
                {
                    return _serviceHelper;
                }
                if (Environment.OSVersion.Platform == PlatformID.MacOSX ||
                    Environment.OSVersion.Platform == PlatformID.Unix ||
                    Environment.OSVersion.Platform == PlatformID.Xbox)
                {
                    return _nullHelper;
                }
                return _processHelper;
            }
        }
    }

    internal class ServiceThumbnailHelper : IThumbnailHelper
    {
        public void MakeThumbnail(string url, bool async, bool notOverride, HttpContext context, int tenantID)
        {
        }

        public string GetThumbnailUrl(string Url, BookmarkingThumbnailSize size)
        {
            return string.Format("/thumb.ashx?url={0}", Url);
        }

        public string GetThumbnailUrlForUpdate(string Url, BookmarkingThumbnailSize size)
        {
            return GetThumbnailUrl(Url, size);
        }

        public void DeleteThumbnail(string Url)
        {

        }
    }

    internal class NullThumbnailHelper : IThumbnailHelper
    {
        public void MakeThumbnail(string url, bool async, bool notOverride, HttpContext context, int tenantID)
        {            
        }

        public void DeleteThumbnail(string Url)
        {
        }

        public string GetThumbnailUrl(string Url, BookmarkingThumbnailSize size)
        {
            return null;
        }

        public string GetThumbnailUrlForUpdate(string Url, BookmarkingThumbnailSize size)
        {
            return GetThumbnailUrl(Url, size);
        }
    }
}