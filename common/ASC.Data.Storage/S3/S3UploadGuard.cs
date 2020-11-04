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
using System.Linq;
using System.Configuration;
using System.Threading.Tasks;
using ASC.Data.Storage.Configuration;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using ASC.Core;

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

        public void DeleteExpiredUploadsAsync(TimeSpan trustInterval)
        {
            var task = new Task(() =>
            {
                DeleteExpiredUploads(trustInterval);
            }, TaskCreationOptions.LongRunning);

            task.Start();
        }

        private void DeleteExpiredUploads(TimeSpan trustInterval)
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
                var config = (StorageConfigurationSection)ConfigurationManagerExtension.GetSection(Schema.SECTION_NAME);
                var handler = config.Handlers.GetHandler("s3");
                if (handler != null)
                {
                    var props = handler.GetProperties();
                    bucket = props["bucket"];
                    accessKey = props["acesskey"];
                    secretAccessKey = props["secretaccesskey"];
                    region = props["region"];
                }
                configErrors = string.IsNullOrEmpty(CoreContext.Configuration.BaseDomain) //localhost
                                || string.IsNullOrEmpty(accessKey)
                                || string.IsNullOrEmpty(secretAccessKey)
                                || string.IsNullOrEmpty(bucket)
                                || string.IsNullOrEmpty(region);

                configured = true;
            }
        }
    }
}
