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
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;

namespace ASC.Data.Storage
{
    class PathUtils
    {
        public static string Normalize(string path, bool addTailingSeparator = false)
        {
            path = path
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace("\\\\", Path.DirectorySeparatorChar.ToString())
                .Replace("//", Path.DirectorySeparatorChar.ToString())
                .TrimEnd(Path.DirectorySeparatorChar);
            return addTailingSeparator && 0 < path.Length ? path + Path.DirectorySeparatorChar : path;
        }

        public static string ResolveVirtualPath(string module, string domain)
        {
            var url = string.Format("~/storage/{0}/{1}/",
                                 module,
                                 string.IsNullOrEmpty(domain) ? "root" : domain);
            return ResolveVirtualPath(url);
        }

        public static string ResolveVirtualPath(string virtPath, bool addTrailingSlash = true)
        {
            if (virtPath.StartsWith("~") && !Uri.IsWellFormedUriString(virtPath, UriKind.Absolute))
            {
                var rootPath = "/";
                if (!string.IsNullOrEmpty(HostingEnvironment.ApplicationVirtualPath) && 1 < HostingEnvironment.ApplicationVirtualPath.Length)
                {
                    rootPath = HostingEnvironment.ApplicationVirtualPath.Trim('/');
                }
                virtPath = virtPath.Replace("~", rootPath);
            }
            if (addTrailingSlash)
            {
                virtPath += "/";
            }
            else
            {
                virtPath = virtPath.TrimEnd('/');
            }
            return virtPath.Replace("//", "/");
        }

        public static string ResolvePhysicalPath(string physPath, IDictionary<string, string> storageConfig)
        {
            physPath = Normalize(physPath, false).TrimStart('~');

            if (physPath.Contains(Constants.STORAGE_ROOT_PARAM))
            {
                physPath = physPath.Replace(Constants.STORAGE_ROOT_PARAM, storageConfig[Constants.STORAGE_ROOT_PARAM]);
            }

            if (!Path.IsPathRooted(physPath))
            {
                physPath = Path.GetFullPath(Path.Combine(storageConfig[Constants.CONFIG_DIR], physPath.Trim(Path.DirectorySeparatorChar)));
            }
            return physPath;
        }
    }
}
