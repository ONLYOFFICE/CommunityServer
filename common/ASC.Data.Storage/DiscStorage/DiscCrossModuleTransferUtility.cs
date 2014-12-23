/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using ASC.Data.Storage.Configuration;

namespace ASC.Data.Storage.DiscStorage
{
    public class DiscCrossModuleTransferUtility : ICrossModuleTransferUtility
    {
        private readonly Dictionary<string, MappedPath> _srcMappedPaths;
        private readonly Dictionary<string, MappedPath> _destMappedPaths;

        public DiscCrossModuleTransferUtility(string srcTenant, ModuleConfigurationElement srcModuleConfig, IDictionary<string, string> srcStorageConfig, string destTenant, ModuleConfigurationElement destModuleConfig, IDictionary<string, string> destStorageConfig)
        {
            _srcMappedPaths = new Dictionary<string, MappedPath>(GetMappedPaths(srcTenant, srcModuleConfig));
            _destMappedPaths = new Dictionary<string, MappedPath>(GetMappedPaths(destTenant, destModuleConfig));
        }

        public void MoveFile(string srcDomain, string srcPath, string destDomain, string destPath)
        {
            var srcTarget = GetTarget(_srcMappedPaths, srcDomain, srcPath);
            var destTarget = GetTarget(_destMappedPaths, destDomain, destPath);

            if (!File.Exists(srcTarget))
            {
                throw new FileNotFoundException("File not found", Path.GetFullPath(srcTarget));
            }
            if (!Directory.Exists(Path.GetDirectoryName(destTarget)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destTarget));
            }
            File.Delete(destTarget);
            File.Move(srcTarget, destTarget);
        }

        public void CopyFile(string srcDomain, string srcPath, string destDomain, string destPath)
        {
            var srcTarget = GetTarget(_srcMappedPaths, srcDomain, srcPath);
            var destTarget = GetTarget(_destMappedPaths, destDomain, destPath);

            if (!File.Exists(srcTarget))
            {
                throw new FileNotFoundException("File not found", Path.GetFullPath(srcTarget));
            }
            if (!Directory.Exists(Path.GetDirectoryName(destTarget)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destTarget));
            }
            File.Copy(srcTarget, destTarget, true);
        }

        private string GetTarget(IDictionary<string, MappedPath> mappedPaths, string domain, string path)
        {
            var pathMap = domain != null && mappedPaths.ContainsKey(domain) ?
                mappedPaths[domain] :
                mappedPaths[string.Empty].AppendDomain(domain);
            return Path.Combine(pathMap.Path, PathUtils.Normalize(path));
        }


        private static IDictionary<string, MappedPath> GetMappedPaths(string tenant, ModuleConfigurationElement moduleConfig)
        {
            var mappedPaths = new Dictionary<string, MappedPath>();
            foreach (DomainConfigurationElement domain in moduleConfig.Domains)
            {
                mappedPaths.Add(
                    domain.Name,
                    new MappedPath(HttpContext.Current, tenant, moduleConfig.AppendTenant, domain.Path, domain.VirtualPath, SelectDirectory(moduleConfig), domain.Quota, domain.Overwrite));
            }

            mappedPaths.Add(
                string.Empty,
                new MappedPath(HttpContext.Current, tenant, moduleConfig.AppendTenant, PathUtils.Normalize(moduleConfig.Path), moduleConfig.VirtualPath, SelectDirectory(moduleConfig), moduleConfig.Quota, moduleConfig.Overwrite));

            return mappedPaths;
        }

        private static string SelectDirectory(ModuleConfigurationElement moduleConfig)
        {
            if (string.IsNullOrEmpty(moduleConfig.BaseDir))
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Server.MapPath(VirtualPathUtility.ToAbsolute("~/"));
                }
                if (AppDomain.CurrentDomain.GetData(Constants.CustomDataDirectory) != null)
                {
                    return (string)AppDomain.CurrentDomain.GetData(Constants.CustomDataDirectory);
                }
                return AppDomain.CurrentDomain.BaseDirectory;
            }
            return moduleConfig.BaseDir;
        }
    }
}
