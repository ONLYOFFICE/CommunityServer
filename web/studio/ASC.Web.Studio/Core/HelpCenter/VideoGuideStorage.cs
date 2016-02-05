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


using ASC.Common.Caching;
using ASC.Data.Storage;
using ASC.Web.Core.Client;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ASC.Web.Studio.Core.HelpCenter
{
    [Serializable]
    [DataContract(Name = "VideoGuideItem", Namespace = "")]
    public class VideoGuideItem
    {
        [DataMember(Name = "Title")]
        public string Title;

        [DataMember(Name = "Id")]
        public string Id;

        [DataMember(Name = "Link")]
        public string Link;

        [DataMember(Name = "Status")]
        public string Status;
    }


    [Serializable]
    [DataContract(Name = "VideoGuideStorageItem", Namespace = "")]
    public class VideoGuideData
    {
        [DataMember(Name = "ListItems")]
        public List<VideoGuideItem> ListItems;

        [DataMember(Name = "ResetCacheKey")]
        public String ResetCacheKey;
    }


    public class VideoGuideStorage
    {
        private const string cacheKey = "videoguide";
        private static readonly ICache cache = AscCache.Memory;
        private static readonly TimeSpan timeout = TimeSpan.FromDays(1);
        private static readonly string filepath = ClientSettings.StorePath.Trim('/') + "/helpcenter/videoguide.html";

        public static Dictionary<string, VideoGuideData> GetVideoGuide()
        {
            Dictionary<string, VideoGuideData> data = null;
            try
            {
                data = FromCache();
                if (data == null && GetStore().IsFile(filepath))
                {
                    using (var stream = GetStore().GetReadStream(filepath))
                    {
                        data = (Dictionary<string, VideoGuideData>)FromStream(stream);
                    }
                    ToCache(data);
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC.Web.HelpCenter").Error("Error GetVideoGuide", e);
            }
            return data ?? new Dictionary<string, VideoGuideData>();
        }

        public static void UpdateVideoGuide(Dictionary<string, VideoGuideData> data)
        {
            try
            {
                using (var stream = ToStream(data))
                {
                    GetStore().Save(filepath, stream);
                }
                ToCache(data);
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC.Web.HelpCenter").Error("Error UpdateVideoGuide", e);
            }
        }

        private static IDataStore GetStore()
        {
            return StorageFactory.GetStorage("-1", "common_static");
        }

        private static MemoryStream ToStream(object obj)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            return stream;
        }

        private static object FromStream(Stream stream)
        {
            var formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);
        }

        private static void ToCache(Dictionary<string, VideoGuideData> obj)
        {
            cache.Insert(cacheKey, obj, DateTime.UtcNow + timeout);
        }

        private static Dictionary<string, VideoGuideData> FromCache()
        {
            return cache.Get<Dictionary<string, VideoGuideData>>(cacheKey);
        }
    }
}