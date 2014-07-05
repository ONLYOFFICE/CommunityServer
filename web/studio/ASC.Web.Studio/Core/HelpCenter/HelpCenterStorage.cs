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
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ASC.Core.Caching;
using ASC.Data.Storage;
using ASC.Web.Core.Client;

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
        private static readonly string Filepath = ClientSettings.StorePath + "/helpcenter/helpcenter.html";

        private static IDataStore GetStore()
        {
            return StorageFactory.GetStorage("-1", "common_static");
        }

        public static Dictionary<string, HelpCenterData> GetHelpCenter()
        {
            var data = FromCache();
            if (data != null) return data;

            if (!GetStore().IsFile(Filepath)) return null;

            using (var stream = GetStore().GetReadStream(Filepath))
            {
                data = (Dictionary<string, HelpCenterData>)FromStream(stream);
            }
            ToCache(data);
            return data;
        }

        public static void UpdateHelpCenter(Dictionary<string, HelpCenterData> data)
        {
            using (var stream = ToStream(data))
            {
                GetStore().Save(Filepath, stream);
            }
            ToCache(data);
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
            IFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);
        }

        private static readonly ICache Cache = new AspCache();
        private static readonly TimeSpan ExpirationTimeout = TimeSpan.FromDays(1);
        private const string CacheKey = "helpcenter";

        private static void ToCache(Dictionary<string, HelpCenterData> obj)
        {
            Cache.Insert(CacheKey, obj, DateTime.UtcNow.Add(ExpirationTimeout));
        }

        private static Dictionary<string, HelpCenterData> FromCache()
        {
            return Cache.Get(CacheKey) as Dictionary<string, HelpCenterData>;
        }
    }
}