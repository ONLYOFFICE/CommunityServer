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
using System.Linq;
using ASC.Data.Storage;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Files.Classes;

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
    }
}