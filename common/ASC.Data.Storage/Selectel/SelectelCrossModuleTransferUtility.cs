/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

#region Import

using System;
using System.Collections.Generic;
using ASC.Data.Storage.Configuration;
using SelectelSharp;
using log4net;

#endregion

namespace ASC.Data.Storage.Selectel
{
    public class SelectelCrossModuleTransferUtility : ICrossModuleTransferUtility
    {
        private readonly static ILog log = LogManager.GetLogger(typeof(SelectelCrossModuleTransferUtility));
        private readonly string _srcTenant;
        private readonly string _destTenant;
        private readonly string _srcContainer;
        private readonly string _destContainer;
        private readonly string _authUser;
        private readonly string _authPwd;

        private readonly ModuleConfigurationElement _srcModuleConfiguration;
        private readonly ModuleConfigurationElement _destModuleConfiguration;

        public SelectelCrossModuleTransferUtility(string srcTenant,
                                            ModuleConfigurationElement srcModuleConfig,
                                            IDictionary<string, string> srcStorageConfig,
                                            string destTenant,
                                            ModuleConfigurationElement destModuleConfig,
                                            IDictionary<string, string> destStorageConfig)
        {
            _srcTenant = srcTenant;
            _destTenant = destTenant;
            _srcContainer = srcStorageConfig["container"];
            _destContainer = destStorageConfig["container"];
            _authUser = srcStorageConfig["authUser"];
            _authPwd = srcStorageConfig["authPwd"];
            _srcModuleConfiguration = srcModuleConfig;
            _destModuleConfiguration = destModuleConfig;
        }

        public void MoveFile(string srcDomain, string srcPath, string destDomain, string destPath)
        {
            CopyFile(srcDomain, srcPath, destDomain, destPath);
            DeleteSrcFile(srcDomain, srcPath);
        }

        public void CopyFile(string srcDomain, string srcPath, string destDomain, string destPath)
        {
            var srcKey = GetKey(_srcTenant, _srcModuleConfiguration.Name, srcDomain, srcPath);
            var destKey = GetKey(_destTenant, _destModuleConfiguration.Name, destDomain, destPath);
            try
            {

                var client = GetClient();

                client.CopyFileAsync(_srcContainer, srcKey, _destContainer, destKey).Wait();      
                
            }
            catch (Exception err)
            {
                log.ErrorFormat("sb {0}, db {1}, sk {2}, dk {3}, err {4}", _srcContainer, _destContainer, srcKey, destKey,  err);
                throw;
            }
        }

        private void DeleteSrcFile(string domain, string path)
        {
            var key = GetKey(_srcTenant, _srcModuleConfiguration.Name, domain, path);

            var client = GetClient();

            client.DeleteFileAsync(_srcContainer, key).Wait();           
        }

        private SelectelClient GetClient()
        {
            var client = new SelectelClient();

            client.AuthorizeAsync(_authUser, _authPwd).Wait();

            return client;
        }        

        private static TimeSpan GetDomainExpire(ModuleConfigurationElement moduleConfiguration, string domain)
        {
            var domainConfiguration = moduleConfiguration.Domains.GetDomainElement(domain);
            if (domainConfiguration == null || domainConfiguration.Expires == TimeSpan.Zero)
            {
                return moduleConfiguration.Expires;
            }
            return domainConfiguration.Expires;
        }

        private static string GetKey(string tenantId, string module, string domain, string path)
        {
            path = path.TrimStart('\\').Trim('/').Replace('\\', '/');
            return string.Format("{0}/{1}/{2}/{3}", tenantId, module, domain, path).Replace("//", "/");
        }
    }
}
