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
using System.IO;

using ASC.Common.Logging;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;


namespace ASC.Data.Backup.Storage
{
    internal class S3BackupStorage : IBackupStorage
    {
        private readonly string accessKeyId;
        private readonly string secretAccessKey;
        private readonly string bucket;
        private readonly string region;

        private readonly ILog log = LogManager.GetLogger("ASC.Backup.Service");

        public S3BackupStorage(string accessKeyId, string secretAccessKey, string bucket, string region)
        {
            this.accessKeyId = accessKeyId;
            this.secretAccessKey = secretAccessKey;
            this.bucket = bucket;
            this.region = region;
        }

        public string Upload(string storageBasePath, string localPath, Guid userId)
        {
            String key = String.Empty;

            if (String.IsNullOrEmpty(storageBasePath))
                key = "backup/" + Path.GetFileName(localPath);
            else                        
                key = String.Concat(storageBasePath.Trim(new char[] {' ', '/', '\\'}), "/", Path.GetFileName(localPath));

            using (var fileTransferUtility = new TransferUtility(accessKeyId, secretAccessKey, RegionEndpoint.GetBySystemName(region)))
            {
                fileTransferUtility.Upload(
                    new TransferUtilityUploadRequest
                    {
                        BucketName = bucket,
                        FilePath = localPath,
                        StorageClass = S3StorageClass.StandardInfrequentAccess,
                        PartSize = 6291456, // 6 MB.
                        Key = key
                    });
            }


            return key;
        }

        public void Download(string storagePath, string targetLocalPath)
        {
            var request = new GetObjectRequest
                {
                    BucketName = bucket,
                    Key = GetKey(storagePath),
                };

            using (var s3 = GetClient())
            using (var response = s3.GetObject(request))
            {
                response.WriteResponseStreamToFile(targetLocalPath);
            }
        }

        public void Delete(string storagePath)
        {
            using (var s3 = GetClient())
            {
                s3.DeleteObject(new DeleteObjectRequest
                    {
                        BucketName = bucket,
                        Key = GetKey(storagePath)
                    });
            }
        }

        public bool IsExists(string storagePath)
        {
            using (var s3 = GetClient())
            {
                try
                {
                    var request = new ListObjectsRequest { BucketName = bucket, Prefix = GetKey(storagePath) };
                    var response = s3.ListObjects(request);
                    return response.S3Objects.Count > 0;
                }
                catch (AmazonS3Exception ex)
                {
                    log.Warn(ex);

                    return false;
                }
            }
        }

        public string GetPublicLink(string storagePath)
        {
            using (var s3 = GetClient())
            {
                return s3.GetPreSignedURL(
                    new GetPreSignedUrlRequest
                        {
                            BucketName = bucket,
                            Key = GetKey(storagePath),
                            Expires = DateTime.UtcNow.AddDays(1),
                            Verb = HttpVerb.GET
                        });
            }
        }

        private string GetKey(string fileName)
        {
           // return "backup/" + Path.GetFileName(fileName);
            return fileName;
        }

        private AmazonS3Client GetClient()
        {
            return new AmazonS3Client(accessKeyId, secretAccessKey, new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(region) });
        }
    }
}
