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