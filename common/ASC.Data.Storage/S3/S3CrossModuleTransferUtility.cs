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


using System;
using System.Collections.Generic;
using ASC.Data.Storage.Configuration;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using log4net;

namespace ASC.Data.Storage.S3
{
    public class S3CrossModuleTransferUtility : ICrossModuleTransferUtility
    {
        private readonly static ILog log = LogManager.GetLogger(typeof(S3CrossModuleTransferUtility));
        private readonly string _srcTenant;
        private readonly string _destTenant;
        private readonly string _srcBucket;
        private readonly string _destBucket;
        private readonly string _awsAccessKeyId;
        private readonly string _awsSecretAccessKey;
        private readonly string _region;
        private readonly ModuleConfigurationElement _srcModuleConfiguration;
        private readonly ModuleConfigurationElement _destModuleConfiguration;

        public S3CrossModuleTransferUtility(string srcTenant,
                                            ModuleConfigurationElement srcModuleConfig,
                                            IDictionary<string, string> srcStorageConfig,
                                            string destTenant,
                                            ModuleConfigurationElement destModuleConfig,
                                            IDictionary<string, string> destStorageConfig)
        {
            _srcTenant = srcTenant;
            _destTenant = destTenant;
            _srcBucket = srcStorageConfig["bucket"];
            _destBucket = destStorageConfig["bucket"];
            _awsAccessKeyId = srcStorageConfig["acesskey"];
            _awsSecretAccessKey = srcStorageConfig["secretaccesskey"];
            _region = destStorageConfig["region"];
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
                using (var s3 = GetS3Client())
                {
                    var copyRequest = new CopyObjectRequest
                    {
                        SourceBucket = _srcBucket,
                        SourceKey = srcKey,
                        DestinationBucket = _destBucket,
                        DestinationKey = destKey,
                        CannedACL = GetDestDomainAcl(destDomain),
                        MetadataDirective = S3MetadataDirective.REPLACE,
                        ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                    };

                    s3.CopyObject(copyRequest);
                }
            }
            catch (Exception err)
            {
                log.ErrorFormat("sb {0}, db {1}, sk {2}, dk {3}, e {4}, err {5}", _srcBucket, _destBucket, srcKey, destKey, RegionEndpoint.GetBySystemName(_region), err);
                throw;
            }
        }

        private void DeleteSrcFile(string domain, string path)
        {
            var key = GetKey(_srcTenant, _srcModuleConfiguration.Name, domain, path);

            using (var s3 = GetS3Client())
            {
                var deleteRequest = new DeleteObjectRequest
                  {
                      BucketName = _srcBucket,
                      Key = key
                  };

                s3.DeleteObject(deleteRequest);
            }
        }

        private IAmazonS3 GetS3Client()
        {
            var clientConfig = new AmazonS3Config { UseHttp = true, MaxErrorRetry = 3, RegionEndpoint = RegionEndpoint.GetBySystemName(_region) };
            return new AmazonS3Client(_awsAccessKeyId, _awsSecretAccessKey, clientConfig);
        }

        private S3CannedACL GetDestDomainAcl(string domain)
        {
            if (GetDomainExpire(_destModuleConfiguration, domain) != TimeSpan.Zero)
            {
                return S3CannedACL.Private;
            }

            var domainConfiguration = _destModuleConfiguration.Domains.GetDomainElement(domain);
            if (domainConfiguration == null)
            {
                return GetS3Acl(_destModuleConfiguration.Acl);
            }
            return GetS3Acl(domainConfiguration.Acl);
        }

        private static S3CannedACL GetS3Acl(ACL acl)
        {
            switch (acl)
            {
                case ACL.Read:
                    return S3CannedACL.PublicRead;
                default:
                    return S3CannedACL.PublicRead;
            }
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
