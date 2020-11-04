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


using ASC.Data.Storage;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Files.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Web.Files.Api
{
    public static class FilesIntegration
    {
        private static readonly IDictionary<string, IFileSecurityProvider> providers = new Dictionary<string, IFileSecurityProvider>();


        public static object RegisterBunch(string module, string bunch, string data)
        {
            using (var folderDao = GetFolderDao())
            {
                return folderDao.GetFolderID(module, bunch, data, true);
            }
        }

        public static IEnumerable<object> RegisterBunchFolders(string module, string bunch, IEnumerable<string> data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            data = data.ToList();
            if (!data.Any())
                return new List<object>();

            using (var folderDao = GetFolderDao())
            {
                return folderDao.GetFolderIDs(module, bunch, data, true);
            }
        }

        public static bool IsRegisteredFileSecurityProvider(string module, string bunch)
        {
            lock (providers)
            {
                return providers.ContainsKey(module + bunch);
            }

        }

        public static void RegisterFileSecurityProvider(string module, string bunch, IFileSecurityProvider securityProvider)
        {
            lock (providers)
            {
                providers[module + bunch] = securityProvider;
            }
        }

        public static IFileDao GetFileDao()
        {
            return Global.DaoFactory.GetFileDao();
        }

        public static IFolderDao GetFolderDao()
        {
            return Global.DaoFactory.GetFolderDao();
        }

        public static ITagDao GetTagDao()
        {
            return Global.DaoFactory.GetTagDao();
        }

        public static FileSecurity GetFileSecurity()
        {
            return Global.GetFilesSecurity();
        }

        public static IDataStore GetStore()
        {
            return Global.GetStore();
        }


        internal static IFileSecurity GetFileSecurity(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            var parts = path.Split('/');
            if (parts.Length < 3) return null;

            IFileSecurityProvider provider;
            lock (providers)
            {
                providers.TryGetValue(parts[0] + parts[1], out provider);
            }
            return provider != null ? provider.GetFileSecurity(parts[2]) : null;
        }

        internal static Dictionary<object, IFileSecurity> GetFileSecurity(Dictionary<string, string> paths)
        {
            var result = new Dictionary<object, IFileSecurity>();
            var gropped = paths.GroupBy(r =>
            {
                var parts = r.Value.Split('/');
                if (parts.Length < 3) return "";

                return parts[0] + parts[1];
            }, v =>
            {
                var parts = v.Value.Split('/');
                if (parts.Length < 3) return new KeyValuePair<string, string>(v.Key, "");

                return new KeyValuePair<string, string>(v.Key, parts[2]);
            });

            foreach (var grouping in gropped)
            {
                IFileSecurityProvider provider;
                lock (providers)
                {
                    providers.TryGetValue(grouping.Key, out provider);
                }
                if (provider == null) continue;

                var data = provider.GetFileSecurity(grouping.ToDictionary(r => r.Key, r => r.Value));
                data.ToList().ForEach(x => result.Add(x.Key, x.Value));
            }

            return result;
        }
    }
}