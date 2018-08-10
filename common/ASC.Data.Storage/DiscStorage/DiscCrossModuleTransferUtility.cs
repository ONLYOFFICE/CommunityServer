/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using ASC.Data.Storage.Configuration;
using System.Collections.Generic;
using System.IO;

namespace ASC.Data.Storage.DiscStorage
{
    public class DiscCrossModuleTransferUtility : ICrossModuleTransferUtility
    {
        private readonly Dictionary<string, MappedPath> _srcMappedPaths;
        private readonly Dictionary<string, MappedPath> _destMappedPaths;


        public DiscCrossModuleTransferUtility(string srcTenant, ModuleConfigurationElement srcModuleConfig, IDictionary<string, string> srcStorageConfig, string destTenant, ModuleConfigurationElement destModuleConfig, IDictionary<string, string> destStorageConfig)
        {
            _srcMappedPaths = new Dictionary<string, MappedPath>(GetMappedPaths(srcTenant, srcModuleConfig, srcStorageConfig));
            _destMappedPaths = new Dictionary<string, MappedPath>(GetMappedPaths(destTenant, destModuleConfig, destStorageConfig));
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
            return Path.Combine(pathMap.PhysicalPath, PathUtils.Normalize(path));
        }


        private IDictionary<string, MappedPath> GetMappedPaths(string tenant, ModuleConfigurationElement moduleConfig, IDictionary<string, string> storageConfig)
        {
            var mappedPaths = new Dictionary<string, MappedPath>();
            foreach (DomainConfigurationElement domain in moduleConfig.Domains)
            {
                mappedPaths.Add(
                    domain.Name,
                    new MappedPath(tenant, moduleConfig.AppendTenant, domain.Path, storageConfig));
            }

            mappedPaths.Add(
                string.Empty,
                new MappedPath(tenant, moduleConfig.AppendTenant, PathUtils.Normalize(moduleConfig.Path), storageConfig));

            return mappedPaths;
        }
    }
}
