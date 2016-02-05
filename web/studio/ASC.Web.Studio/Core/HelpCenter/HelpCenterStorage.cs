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
    [DataContract(Name = "HelpCenterItem", Namespace = "")]
    public class HelpCenterItem
    {
        [DataMember(Name = "Title")] public string Title;

        [DataMember(Name = "Content")] public string Content;
    }

    [Serializable]
    [DataContract(Name = "HelpCenterData", Namespace = "")]
    public class HelpCenterData
    {
        [DataMember(Name = "ListItems")] public List<HelpCenterItem> ListItems;

        [DataMember(Name = "ResetCacheKey")] public String ResetCacheKey;
    }

    public class HelpCenterStorage
    {
        private static readonly string Filepath = ClientSettings.StorePath.Trim('/') + "/helpcenter/helpcenter.html";
        private static readonly ICache cache = AscCache.Memory;
        private static readonly TimeSpan ExpirationTimeout = TimeSpan.FromDays(1);
        private const string CacheKey = "helpcenter";


        private static IDataStore GetStore()
        {
            return StorageFactory.GetStorage("-1", "common_static");
        }

        public static Dictionary<string, HelpCenterData> GetHelpCenter()
        {
            Dictionary<string, HelpCenterData> data = null;
            try
            {
                data = FromCache();
                if (data == null && GetStore().IsFile(Filepath))
                {
                    using (var stream = GetStore().GetReadStream(Filepath))
                    {
                        data = (Dictionary<string, HelpCenterData>) FromStream(stream);
                    }
                    ToCache(data);
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC.Web.HelpCenter").Error("Error GetHelpCenter", e);
            }
            return data ?? new Dictionary<string, HelpCenterData>();
        }

        public static void UpdateHelpCenter(Dictionary<string, HelpCenterData> data)
        {
            try
            {
                using (var stream = ToStream(data))
                {
                    GetStore().Save(Filepath, stream);
                }
                ToCache(data);
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC.Web.HelpCenter").Error("Error UpdateHelpCenter", e);
            }
        }

        private static MemoryStream ToStream(object objectType)
        {
            var stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, objectType);
            return stream;
        }

        private static object FromStream(Stream stream)
        {
            return new BinaryFormatter().Deserialize(stream);
        }

        private static void ToCache(Dictionary<string, HelpCenterData> obj)
        {
            cache.Insert(CacheKey, obj, DateTime.UtcNow.Add(ExpirationTimeout));
        }

        private static Dictionary<string, HelpCenterData> FromCache()
        {
            return cache.Get<Dictionary<string, HelpCenterData>>(CacheKey);
        }
    }
}