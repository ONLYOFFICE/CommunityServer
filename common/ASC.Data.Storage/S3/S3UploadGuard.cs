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
using System.Linq;
using System.Configuration;
using ASC.Data.Storage.Configuration;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace ASC.Data.Storage.S3
{
    public class S3UploadGuard
    {
        private string accessKey;
        private string secretAccessKey;
        private string bucket;
        private string region;
        private bool configErrors;
        private bool configured;

        public void DeleteExpiredUploads(TimeSpan trustInterval)
        {
            Configure();

            if (configErrors)
            {
                return;
            }

            using (var s3 = GetClient())
            {
                var nextKeyMarker = string.Empty;
                var nextUploadIdMarker = string.Empty;
                bool isTruncated;

                do
                {
                    var request = new ListMultipartUploadsRequest { BucketName = bucket };

                    if (!string.IsNullOrEmpty(nextKeyMarker))
                    {
                        request.KeyMarker = nextKeyMarker;
                    }

                    if (!string.IsNullOrEmpty(nextUploadIdMarker))
                    {
                        request.UploadIdMarker = nextUploadIdMarker;
                    }

                    var response = s3.ListMultipartUploads(request);

                    foreach (var u in response.MultipartUploads.Where(x => x.Initiated + trustInterval <= DateTime.UtcNow))
                    {
                        AbortMultipartUpload(u, s3);
                    }

                    isTruncated = response.IsTruncated;
                    nextKeyMarker = response.NextKeyMarker;
                    nextUploadIdMarker = response.NextUploadIdMarker;
                }
                while (isTruncated);
                
            }
        }

        private void AbortMultipartUpload(MultipartUpload u, AmazonS3Client client)
        {
            var request = new AbortMultipartUploadRequest
            {
                BucketName = bucket,
                Key = u.Key,
                UploadId = u.UploadId,
            };

            client.AbortMultipartUpload(request);
        }

        private AmazonS3Client GetClient()
        {
            var s3Config = new AmazonS3Config {UseHttp = true, MaxErrorRetry = 3, RegionEndpoint = RegionEndpoint.GetBySystemName(region)};
            return new AmazonS3Client(accessKey, secretAccessKey, s3Config);
        }

        private void Configure()
        {
            if (!configured)
            {
                var config = (StorageConfigurationSection)ConfigurationManager.GetSection(Schema.SECTION_NAME);
                var handler = config.Handlers.GetHandler("s3");
                if (handler != null)
                {
                    var props = handler.GetProperties();
                    bucket = props["bucket"];
                    accessKey = props["acesskey"];
                    secretAccessKey = props["secretaccesskey"];
                    region = props["region"];
                }
                configErrors = string.IsNullOrEmpty(ConfigurationManager.AppSettings["core.base-domain"]) //localhost
                                || string.IsNullOrEmpty(accessKey)
                                || string.IsNullOrEmpty(secretAccessKey)
                                || string.IsNullOrEmpty(bucket)
                                || string.IsNullOrEmpty(region);

                configured = true;
            }
        }
    }
}
