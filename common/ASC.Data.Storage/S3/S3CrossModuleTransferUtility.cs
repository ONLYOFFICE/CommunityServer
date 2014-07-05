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
using ASC.Data.Storage.Configuration;
using Amazon.S3;
using Amazon.S3.Model;

namespace ASC.Data.Storage.S3
{
    public class S3CrossModuleTransferUtility : ICrossModuleTransferUtility
    {
        private readonly string _srcTenant;
        private readonly string _destTenant;
        private readonly string _srcBucket;
        private readonly string _destBucket;
        private readonly string _awsAccessKeyId;
        private readonly string _awsSecretAccessKey;
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

            using (var s3 = GetS3Client())
            {
                var copyRequest = new CopyObjectRequest{
                     SourceBucket = _srcBucket,
                     SourceKey = srcKey,
                     DestinationBucket = _destBucket,
                     DestinationKey = destKey,
                     CannedACL = GetDestDomainAcl(destDomain),
                     Directive = S3MetadataDirective.REPLACE,
                     ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
               };

                s3.CopyObject(copyRequest);
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

        private AmazonS3 GetS3Client()
        {
            var clientConfig = new AmazonS3Config { CommunicationProtocol = Protocol.HTTP, MaxErrorRetry = 3 };
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
