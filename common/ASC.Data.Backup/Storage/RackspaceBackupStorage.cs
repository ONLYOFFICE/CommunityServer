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


using System;
using System.IO;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Net;
using ASC.Data.Backup.Logging;
using Amazon.S3.Transfer;

namespace ASC.Data.Backup.Storage
{
    internal class RackspaceBackupStorage : IBackupStorage
    {
        private readonly string accessKeyId;
        private readonly string secretAccessKey;
        private readonly string bucket;
        private readonly string region;

        private readonly ILog log = LogFactory.Create("ASC.Backup.Service");

        public RackspaceBackupStorage(string accessKeyId, string secretAccessKey, string bucket, string region)
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
