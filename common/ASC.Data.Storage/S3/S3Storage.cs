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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

using Amazon;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.Util;

using ASC.Core;
using ASC.Data.Storage.Configuration;

using MimeMapping = ASC.Common.Web.MimeMapping;

namespace ASC.Data.Storage.S3
{
    public class S3Storage : BaseStorage
    {
        private readonly List<string> _domains = new List<string>();
        private readonly Dictionary<string, S3CannedACL> _domainsAcl;
        private readonly S3CannedACL _moduleAcl;
        private string _accessKeyId = "";
        private string _bucket = "";
        private string _recycleDir = "";
        private Uri _bucketRoot;
        private Uri _bucketSSlRoot;
        private string _region = String.Empty;
        private string _serviceurl;
        private bool _forcepathstyle;
        private string _secretAccessKeyId = "";
        private ServerSideEncryptionMethod _sse = ServerSideEncryptionMethod.AES256;
        private bool _useHttp = true;

        private bool _lowerCasing = true;
        private bool _revalidateCloudFront;
        private string _distributionId = string.Empty;
        private String _subDir = String.Empty;

        public S3Storage(string tenant)
        {
            _tenant = tenant;
            _modulename = string.Empty;
            _dataList = null;

            //Make expires
            _domainsExpires = new Dictionary<string, TimeSpan> { { string.Empty, TimeSpan.Zero } };

            _domainsAcl = new Dictionary<string, S3CannedACL>();
            _moduleAcl = S3CannedACL.PublicRead;
        }

        public S3Storage(string tenant, HandlerConfigurationElement handlerConfig, ModuleConfigurationElement moduleConfig)
        {
            _tenant = tenant;
            _modulename = moduleConfig.Name;
            _dataList = new DataList(moduleConfig);
            _domains.AddRange(
                moduleConfig.Domains.Cast<DomainConfigurationElement>().Select(x => string.Format("{0}/", x.Name)));

            //Make expires
            _domainsExpires =
                moduleConfig.Domains.Cast<DomainConfigurationElement>().Where(x => x.Expires != TimeSpan.Zero).
                    ToDictionary(x => x.Name,
                                 y => y.Expires);
            _domainsExpires.Add(string.Empty, moduleConfig.Expires);

            _domainsAcl = moduleConfig.Domains.Cast<DomainConfigurationElement>().ToDictionary(x => x.Name,
                                                                                               y => GetS3Acl(y.Acl));
            _moduleAcl = GetS3Acl(moduleConfig.Acl);
        }

        private S3CannedACL GetDomainACL(string domain)
        {
            if (GetExpire(domain) != TimeSpan.Zero)
            {
                return S3CannedACL.Private;
            }

            if (_domainsAcl.ContainsKey(domain))
            {
                return _domainsAcl[domain];
            }
            return _moduleAcl;
        }

        private S3CannedACL GetS3Acl(ACL acl)
        {
            switch (acl)
            {
                case ACL.Read:
                    return S3CannedACL.PublicRead;
                case ACL.Private:
                    return S3CannedACL.Private;
                default:
                    return S3CannedACL.PublicRead;
            }
        }

        public Uri GetUriInternal(string path)
        {
            return new Uri(SecureHelper.IsSecure() ? _bucketSSlRoot : _bucketRoot, path);
        }

        public Uri GetUriShared(string domain, string path)
        {
            return new Uri(SecureHelper.IsSecure() ? _bucketSSlRoot : _bucketRoot, MakePath(domain, path));
        }

        public override Uri GetInternalUri(string domain, string path, TimeSpan expire, IEnumerable<string> headers)
        {
            if (expire == TimeSpan.Zero || expire == TimeSpan.MinValue || expire == TimeSpan.MaxValue)
            {
                expire = GetExpire(domain);
            }
            if (expire == TimeSpan.Zero || expire == TimeSpan.MinValue || expire == TimeSpan.MaxValue)
            {
                return GetUriShared(domain, path);
            }

            var pUrlRequest = new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Expires = DateTime.UtcNow.Add(expire),
                Key = MakePath(domain, path),
                Protocol = SecureHelper.IsSecure() ? Protocol.HTTPS : Protocol.HTTP,
                Verb = HttpVerb.GET
            };

            if (headers != null && headers.Any())
            {
                var headersOverrides = new ResponseHeaderOverrides();

                foreach (var h in headers)
                {
                    if (h.StartsWith("Content-Disposition")) headersOverrides.ContentDisposition = (h.Substring("Content-Disposition".Length + 1));
                    else if (h.StartsWith("Cache-Control")) headersOverrides.CacheControl = (h.Substring("Cache-Control".Length + 1));
                    else if (h.StartsWith("Content-Encoding")) headersOverrides.ContentEncoding = (h.Substring("Content-Encoding".Length + 1));
                    else if (h.StartsWith("Content-Language")) headersOverrides.ContentLanguage = (h.Substring("Content-Language".Length + 1));
                    else if (h.StartsWith("Content-Type")) headersOverrides.ContentType = (h.Substring("Content-Type".Length + 1));
                    else if (h.StartsWith("Expires")) headersOverrides.Expires = (h.Substring("Expires".Length + 1));
                    else throw new FormatException(string.Format("Invalid header: {0}", h));
                }
                pUrlRequest.ResponseHeaderOverrides = headersOverrides;
            }
            using (var client = GetClient())
            {
                return MakeUri(client.GetPreSignedURL(pUrlRequest));
            }
        }


        private Uri MakeUri(string preSignedURL)
        {
            var uri = new Uri(preSignedURL);
            var signedPart = uri.PathAndQuery.TrimStart('/');
            return new UnencodedUri(uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? _bucketSSlRoot : _bucketRoot, signedPart);
        }

        public override Stream GetReadStream(string domain, string path)
        {
            return GetReadStream(domain, path, 0);
        }

        public override Stream GetReadStream(string domain, string path, int offset)
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucket,
                Key = MakePath(domain, path)
            };

            if (0 < offset) request.ByteRange = new ByteRange(offset, int.MaxValue);

            using (var client = GetClient())
            {
                return new ResponseStreamWrapper(client.GetObject(request));
            }
        }

        protected override Uri SaveWithAutoAttachment(string domain, string path, Stream stream, string attachmentFileName)
        {
            var contentDisposition = string.Format("attachment; filename={0};",
                                                   HttpUtility.UrlPathEncode(attachmentFileName));
            if (attachmentFileName.Any(c => (int)c >= 0 && (int)c <= 127))
            {
                contentDisposition = string.Format("attachment; filename*=utf-8''{0};",
                                                   HttpUtility.UrlPathEncode(attachmentFileName));
            }
            return Save(domain, path, stream, null, contentDisposition);
        }

        public override Uri Save(string domain, string path, Stream stream, string contentType,
                        string contentDisposition)
        {
            return Save(domain, path, stream, contentType, contentDisposition, ACL.Auto);
        }

        public Uri Save(string domain, string path, Stream stream, string contentType,
                                 string contentDisposition, ACL acl, string contentEncoding = null, int cacheDays = 5)
        {
            var buffered = stream.GetBuffered();

            if (QuotaController != null)
            {
                QuotaController.QuotaUsedCheck(buffered.Length);
            }

            using (var client = GetClient())
            using (var uploader = new TransferUtility(client))
            {
                var mime = string.IsNullOrEmpty(contentType)
                                  ? MimeMapping.GetMimeMapping(Path.GetFileName(path))
                                  : contentType;

                var request = new TransferUtilityUploadRequest
                {
                    BucketName = _bucket,
                    Key = MakePath(domain, path),
                    ContentType = mime,
                    ServerSideEncryptionMethod = _sse,
                    InputStream = buffered,
                    AutoCloseStream = false,
                    Headers =
                    {
                        CacheControl = string.Format("public, maxage={0}", (int)TimeSpan.FromDays(cacheDays).TotalSeconds),
                        ExpiresUtc = DateTime.UtcNow.Add(TimeSpan.FromDays(cacheDays))
                    }
                };

                if (!WorkContext.IsMono) //  System.Net.Sockets.SocketException: Connection reset by peer
                {
                    switch (acl)
                    {
                        case ACL.Auto:
                            request.CannedACL = GetDomainACL(domain);
                            break;
                        case ACL.Read:
                        case ACL.Private:
                            request.CannedACL = GetS3Acl(acl);
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(contentDisposition))
                {
                    request.Headers.ContentDisposition = contentDisposition;
                }
                else if (mime == "application/octet-stream")
                {
                    request.Headers.ContentDisposition = "attachment";
                }

                if (!string.IsNullOrEmpty(contentEncoding))
                {
                    request.Headers.ContentEncoding = contentEncoding;
                }

                uploader.Upload(request);

                InvalidateCloudFront(MakePath(domain, path));

                QuotaUsedAdd(domain, buffered.Length);

                return GetUri(domain, path);
            }
        }

        private void InvalidateCloudFront(params string[] paths)
        {
            if (!_revalidateCloudFront || string.IsNullOrEmpty(_distributionId)) return;

            using (var cfClient = GetCloudFrontClient())
            {

                var invalidationRequest = new CreateInvalidationRequest
                {
                    DistributionId = _distributionId,
                    InvalidationBatch = new InvalidationBatch
                    {
                        CallerReference = Guid.NewGuid().ToString(),

                        Paths = new Paths
                        {
                            Items = paths.ToList(),
                            Quantity = paths.Count()
                        }
                    }
                };

                cfClient.CreateInvalidation(invalidationRequest);
            }
        }

        public override Uri Save(string domain, string path, Stream stream)
        {
            return Save(domain, path, stream, string.Empty, string.Empty);
        }

        public override Uri Save(string domain, string path, Stream stream, string contentEncoding, int cacheDays)
        {
            return Save(domain, path, stream, string.Empty, string.Empty, ACL.Auto, contentEncoding, cacheDays);
        }

        public override Uri Save(string domain, string path, Stream stream, ACL acl)
        {
            return Save(domain, path, stream, null, null, acl);
        }

        #region chunking

        public override string InitiateChunkedUpload(string domain, string path)
        {
            var request = new InitiateMultipartUploadRequest
            {
                BucketName = _bucket,
                Key = MakePath(domain, path),
                ServerSideEncryptionMethod = _sse
            };

            using (var s3 = GetClient())
            {
                var response = s3.InitiateMultipartUpload(request);
                return response.UploadId;
            }
        }

        public override string UploadChunk(string domain, string path, string uploadId, Stream stream, long defaultChunkSize, int chunkNumber, long chunkLength)
        {
            var request = new UploadPartRequest
            {
                BucketName = _bucket,
                Key = MakePath(domain, path),
                UploadId = uploadId,
                PartNumber = chunkNumber,
                InputStream = stream
            };

            try
            {
                using (var s3 = GetClient())
                {
                    var response = s3.UploadPart(request);
                    return response.ETag;
                }
            }
            catch (AmazonS3Exception error)
            {
                if (error.ErrorCode == "NoSuchUpload")
                {
                    AbortChunkedUpload(domain, path, uploadId);
                }

                throw;
            }
        }

        public override Uri FinalizeChunkedUpload(string domain, string path, string uploadId, Dictionary<int, string> eTags)
        {
            var request = new CompleteMultipartUploadRequest
            {
                BucketName = _bucket,
                Key = MakePath(domain, path),
                UploadId = uploadId,
                PartETags = eTags.Select(x => new PartETag(x.Key, x.Value)).ToList()
            };

            try
            {
                using (var s3 = GetClient())
                {
                    s3.CompleteMultipartUpload(request);
                    InvalidateCloudFront(MakePath(domain, path));
                }

                if (QuotaController != null)
                {
                    var size = GetFileSize(domain, path);
                    QuotaUsedAdd(domain, size);
                }

                return GetUri(domain, path);
            }
            catch (AmazonS3Exception error)
            {
                if (error.ErrorCode == "NoSuchUpload")
                {
                    AbortChunkedUpload(domain, path, uploadId);
                }

                throw;
            }
        }

        public override void AbortChunkedUpload(string domain, string path, string uploadId)
        {
            var key = MakePath(domain, path);

            var request = new AbortMultipartUploadRequest
            {
                BucketName = _bucket,
                Key = key,
                UploadId = uploadId
            };

            using (var s3 = GetClient())
            {
                s3.AbortMultipartUpload(request);
            }
        }

        public override bool IsSupportChunking { get { return true; } }

        #endregion

        public override void Delete(string domain, string path)
        {
            using (var client = GetClient())
            {
                var key = MakePath(domain, path);
                var size = GetFileSize(domain, path);

                Recycle(client, domain, key);

                var request = new DeleteObjectRequest
                {
                    BucketName = _bucket,
                    Key = key
                };

                client.DeleteObject(request);

                QuotaUsedDelete(domain, size);
            }
        }

        public override void DeleteFiles(string domain, List<string> paths)
        {
            if (!paths.Any()) return;

            var keysToDel = new List<string>();

            long quotaUsed = 0;

            foreach (var path in paths)
            {
                try
                {
                    //var obj = GetS3Objects(domain, path).FirstOrDefault();

                    var key = MakePath(domain, path);

                    if (QuotaController != null)
                    {
                        quotaUsed += GetFileSize(domain, path);
                    }

                    keysToDel.Add(key);

                    //objsToDel.Add(obj);
                }
                catch (FileNotFoundException)
                {

                }
            }

            if (!keysToDel.Any())
                return;

            using (var client = GetClient())
            {
                var deleteRequest = new DeleteObjectsRequest
                {
                    BucketName = _bucket,
                    Objects = keysToDel.Select(key => new KeyVersion { Key = key }).ToList()
                };

                client.DeleteObjects(deleteRequest);
            }

            if (quotaUsed > 0)
            {
                QuotaUsedDelete(domain, quotaUsed);
            }
        }

        public override void DeleteFiles(string domain, string path, string pattern, bool recursive)
        {
            var makedPath = MakePath(domain, path) + '/';
            var objToDel = GetS3Objects(domain, path)
                .Where(x =>
                    Wildcard.IsMatch(pattern, Path.GetFileName(x.Key))
                    && (recursive || !x.Key.Remove(0, makedPath.Length).Contains('/'))
                    );

            using (var client = GetClient())
            {
                foreach (var s3Object in objToDel)
                {
                    Recycle(client, domain, s3Object.Key);

                    var deleteRequest = new DeleteObjectRequest
                    {
                        BucketName = _bucket,
                        Key = s3Object.Key
                    };

                    client.DeleteObject(deleteRequest);

                    QuotaUsedDelete(domain, Convert.ToInt64(s3Object.Size));
                }
            }
        }

        public override void DeleteFiles(string domain, string path, DateTime fromDate, DateTime toDate)
        {
            var objToDel = GetS3Objects(domain, path)
                .Where(x => x.LastModified >= fromDate && x.LastModified <= toDate);

            using (var client = GetClient())
            {
                foreach (var s3Object in objToDel)
                {
                    Recycle(client, domain, s3Object.Key);

                    var deleteRequest = new DeleteObjectRequest
                    {
                        BucketName = _bucket,
                        Key = s3Object.Key
                    };

                    client.DeleteObject(deleteRequest);

                    QuotaUsedDelete(domain, Convert.ToInt64(s3Object.Size));
                }
            }
        }

        public override void MoveDirectory(string srcdomain, string srcdir, string newdomain, string newdir)
        {
            var srckey = MakePath(srcdomain, srcdir);
            var dstkey = MakePath(newdomain, newdir);
            //List files from src
            using (var client = GetClient())
            {
                var request = new ListObjectsRequest
                {
                    BucketName = _bucket,
                    Prefix = srckey
                };

                var response = client.ListObjects(request);
                foreach (var s3Object in response.S3Objects)
                {
                    client.CopyObject(new CopyObjectRequest
                    {
                        SourceBucket = _bucket,
                        SourceKey = s3Object.Key,
                        DestinationBucket = _bucket,
                        DestinationKey = s3Object.Key.Replace(srckey, dstkey),
                        CannedACL = GetDomainACL(newdomain),
                        ServerSideEncryptionMethod = _sse
                    });

                    client.DeleteObject(new DeleteObjectRequest
                    {
                        BucketName = _bucket,
                        Key = s3Object.Key
                    });
                }
            }
        }

        public override Uri Move(string srcdomain, string srcpath, string newdomain, string newpath)
        {
            using (var client = GetClient())
            {
                var srcKey = MakePath(srcdomain, srcpath);
                var dstKey = MakePath(newdomain, newpath);
                var size = GetFileSize(srcdomain, srcpath);

                var request = new CopyObjectRequest
                {
                    SourceBucket = _bucket,
                    SourceKey = srcKey,
                    DestinationBucket = _bucket,
                    DestinationKey = dstKey,
                    CannedACL = GetDomainACL(newdomain),
                    MetadataDirective = S3MetadataDirective.REPLACE,
                    ServerSideEncryptionMethod = _sse
                };

                client.CopyObject(request);
                Delete(srcdomain, srcpath);

                QuotaUsedDelete(srcdomain, size);
                QuotaUsedAdd(newdomain, size);

                return GetUri(newdomain, newpath);
            }
        }

        public override Uri SaveTemp(string domain, out string assignedPath, Stream stream)
        {
            assignedPath = Guid.NewGuid().ToString();
            return Save(domain, assignedPath, stream);
        }

        public override string[] ListDirectoriesRelative(string domain, string path, bool recursive)
        {
            return GetS3Objects(domain, path)
                .Select(x => x.Key.Substring((MakePath(domain, path) + "/").Length))
                .ToArray();
        }


        public override string SavePrivate(string domain, string path, Stream stream, DateTime expires)
        {
            using (var client = GetClient())
            {
                var objectKey = MakePath(domain, path);
                var buffered = stream.GetBuffered();
                var request = new TransferUtilityUploadRequest
                {
                    BucketName = _bucket,
                    Key = objectKey,
                    CannedACL = S3CannedACL.BucketOwnerFullControl,
                    ContentType = "application/octet-stream",
                    InputStream = buffered,
                    Headers =
                    {
                        CacheControl = string.Format("public, maxage={0}", (int)TimeSpan.FromDays(5).TotalSeconds),
                        ExpiresUtc = DateTime.UtcNow.Add(TimeSpan.FromDays(5)),
                        ContentDisposition = "attachment",
                    }
                };


                request.Metadata.Add("private-expire", expires.ToFileTimeUtc().ToString(CultureInfo.InvariantCulture));

                new TransferUtility(client).Upload(request);

                //Get presigned url                
                var pUrlRequest = new GetPreSignedUrlRequest
                {
                    BucketName = _bucket,
                    Expires = expires,
                    Key = objectKey,
                    Protocol = Protocol.HTTP,
                    Verb = HttpVerb.GET
                };

                string url = client.GetPreSignedURL(pUrlRequest);
                //TODO: CNAME!
                return url;
            }
        }

        public override void DeleteExpired(string domain, string path, TimeSpan oldThreshold)
        {
            using (var client = GetClient())
            {
                var s3Obj = GetS3Objects(domain, path);
                foreach (var s3Object in s3Obj)
                {
                    var request = new GetObjectMetadataRequest
                    {
                        BucketName = _bucket,
                        Key = s3Object.Key
                    };

                    var metadata = client.GetObjectMetadata(request);
                    var privateExpireKey = metadata.Metadata["private-expire"];
                    if (string.IsNullOrEmpty(privateExpireKey)) continue;

                    long fileTime;
                    if (!long.TryParse(privateExpireKey, out fileTime)) continue;
                    if (DateTime.UtcNow <= DateTime.FromFileTimeUtc(fileTime)) continue;
                    //Delete it
                    var deleteObjectRequest = new DeleteObjectRequest
                    {
                        BucketName = _bucket,
                        Key = s3Object.Key
                    };

                    client.DeleteObject(deleteObjectRequest);
                }
            }
        }

        public override string GetUploadUrl()
        {
            return GetUriInternal(string.Empty).ToString();
        }

        public override string GetPostParams(string domain, string directoryPath, long maxUploadSize, string contentType,
                                             string contentDisposition)
        {
            var key = MakePath(domain, directoryPath) + "/";
            //Generate policy
            string sign;
            var policyBase64 = GetPolicyBase64(key, string.Empty, contentType, contentDisposition, maxUploadSize,
                                                  out sign);
            var postBuilder = new StringBuilder();
            postBuilder.Append("{");
            postBuilder.AppendFormat("\"key\":\"{0}${{filename}}\",", key);
            postBuilder.AppendFormat("\"acl\":\"public-read\",");
            postBuilder.AppendFormat("\"key\":\"{0}\",", key);
            postBuilder.AppendFormat("\"success_action_status\":\"{0}\",", 201);

            if (!string.IsNullOrEmpty(contentType))
                postBuilder.AppendFormat("\"Content-Type\":\"{0}\",", contentType);
            if (!string.IsNullOrEmpty(contentDisposition))
                postBuilder.AppendFormat("\"Content-Disposition\":\"{0}\",", contentDisposition);

            postBuilder.AppendFormat("\"AWSAccessKeyId\":\"{0}\",", _accessKeyId);
            postBuilder.AppendFormat("\"Policy\":\"{0}\",", policyBase64);
            postBuilder.AppendFormat("\"Signature\":\"{0}\"", sign);
            postBuilder.AppendFormat("\"SignatureVersion\":\"{0}\"", 2);
            postBuilder.AppendFormat("\"SignatureMethod\":\"{0}\"", "HmacSHA1");
            postBuilder.Append("}");
            return postBuilder.ToString();
        }

        public override string GetUploadForm(string domain, string directoryPath, string redirectTo, long maxUploadSize,
                                             string contentType, string contentDisposition, string submitLabel)
        {
            var destBucket = GetUploadUrl();
            var key = MakePath(domain, directoryPath) + "/";
            //Generate policy
            string sign;
            var policyBase64 = GetPolicyBase64(key, redirectTo, contentType, contentDisposition, maxUploadSize,
                                                  out sign);

            var formBuilder = new StringBuilder();
            formBuilder.AppendFormat("<form action=\"{0}\" method=\"post\" enctype=\"multipart/form-data\">", destBucket);
            formBuilder.AppendFormat("<input type=\"hidden\" name=\"key\" value=\"{0}${{filename}}\" />", key);
            formBuilder.Append("<input type=\"hidden\" name=\"acl\" value=\"public-read\" />");
            if (!string.IsNullOrEmpty(redirectTo))
                formBuilder.AppendFormat("<input type=\"hidden\" name=\"success_action_redirect\" value=\"{0}\" />",
                                         redirectTo);

            formBuilder.AppendFormat("<input type=\"hidden\" name=\"success_action_status\" value=\"{0}\" />", 201);

            if (!string.IsNullOrEmpty(contentType))
                formBuilder.AppendFormat("<input type=\"hidden\" name=\"Content-Type\" value=\"{0}\" />", contentType);
            if (!string.IsNullOrEmpty(contentDisposition))
                formBuilder.AppendFormat("<input type=\"hidden\" name=\"Content-Disposition\" value=\"{0}\" />",
                                         contentDisposition);
            formBuilder.AppendFormat("<input type=\"hidden\" name=\"AWSAccessKeyId\" value=\"{0}\"/>", _accessKeyId);
            formBuilder.AppendFormat("<input type=\"hidden\" name=\"Policy\" value=\"{0}\" />", policyBase64);
            formBuilder.AppendFormat("<input type=\"hidden\" name=\"Signature\" value=\"{0}\" />", sign);
            formBuilder.AppendFormat("<input type=\"hidden\" name=\"SignatureVersion\" value=\"{0}\" />", 2);
            formBuilder.AppendFormat("<input type=\"hidden\" name=\"SignatureMethod\" value=\"{0}\" />", "HmacSHA1");
            formBuilder.AppendFormat("<input type=\"file\" name=\"file\" />");
            formBuilder.AppendFormat("<input type=\"submit\" name=\"submit\" value=\"{0}\" /></form>", submitLabel);
            return formBuilder.ToString();
        }

        private string GetPolicyBase64(string key, string redirectTo, string contentType, string contentDisposition,
                                       long maxUploadSize, out string sign)
        {
            var policyBuilder = new StringBuilder();
            policyBuilder.AppendFormat("{{\"expiration\": \"{0}\",\"conditions\":[",
                                       DateTime.UtcNow.AddMinutes(15).ToString(AWSSDKUtils.ISO8601DateFormat,
                                                                               CultureInfo.InvariantCulture));
            policyBuilder.AppendFormat("{{\"bucket\": \"{0}\"}},", _bucket);
            policyBuilder.AppendFormat("[\"starts-with\", \"$key\", \"{0}\"],", key);
            policyBuilder.Append("{\"acl\": \"public-read\"},");
            if (!string.IsNullOrEmpty(redirectTo))
            {
                policyBuilder.AppendFormat("{{\"success_action_redirect\": \"{0}\"}},", redirectTo);
            }
            policyBuilder.AppendFormat("{{\"success_action_status\": \"{0}\"}},", 201);
            if (!string.IsNullOrEmpty(contentType))
            {
                policyBuilder.AppendFormat("[\"eq\", \"$Content-Type\", \"{0}\"],", contentType);
            }
            if (!string.IsNullOrEmpty(contentDisposition))
            {
                policyBuilder.AppendFormat("[\"eq\", \"$Content-Disposition\", \"{0}\"],", contentDisposition);
            }
            policyBuilder.AppendFormat("[\"content-length-range\", 0, {0}]", maxUploadSize);
            policyBuilder.Append("]}");

            var policyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(policyBuilder.ToString()));
            //sign = AWSSDKUtils.HMACSign(policyBase64, _secretAccessKeyId, new HMACSHA1());
            var algorithm = new HMACSHA1 { Key = Encoding.UTF8.GetBytes(_secretAccessKeyId) };
            try
            {
                algorithm.Key = Encoding.UTF8.GetBytes(key);
                sign = Convert.ToBase64String(algorithm.ComputeHash(Encoding.UTF8.GetBytes(policyBase64)));
            }
            finally
            {
                algorithm.Clear();
            }

            return policyBase64;
        }

        public override string GetUploadedUrl(string domain, string directoryPath)
        {
            if (HttpContext.Current != null)
            {
                var buket = HttpContext.Current.Request.QueryString["bucket"];
                var key = HttpContext.Current.Request.QueryString["key"];
                var etag = HttpContext.Current.Request.QueryString["etag"];
                var destkey = MakePath(domain, directoryPath) + "/";

                if (!string.IsNullOrEmpty(buket) && !string.IsNullOrEmpty(key) && string.Equals(buket, _bucket) &&
                    key.StartsWith(destkey))
                {
                    var domainpath = key.Substring(MakePath(domain, string.Empty).Length);
                    var skipQuota = false;
                    if (HttpContext.Current.Session != null)
                    {
                        var isCounted = HttpContext.Current.Session[etag];
                        skipQuota = isCounted != null;
                    }
                    //Add to quota controller
                    if (QuotaController != null && !skipQuota)
                    {
                        try
                        {
                            var size = GetFileSize(domain, domainpath);
                            QuotaUsedAdd(domain, size);

                            if (HttpContext.Current.Session != null)
                            {
                                HttpContext.Current.Session.Add(etag, size);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    return GetUriInternal(key).ToString();
                }
            }
            return string.Empty;
        }


        public override string[] ListFilesRelative(string domain, string path, string pattern, bool recursive)
        {
            return GetS3Objects(domain, path)
                .Where(x => Wildcard.IsMatch(pattern, Path.GetFileName(x.Key)))
                .Select(x => x.Key.Substring((MakePath(domain, path) + "/").Length).TrimStart('/'))
                .ToArray();
        }

        private bool CheckKey(string domain, string key)
        {
            return !string.IsNullOrEmpty(domain) ||
                   _domains.All(configuredDomains => !key.StartsWith(MakePath(configuredDomains, "")));
        }

        public override bool IsFile(string domain, string path)
        {
            using (var client = GetClient())
            {
                var request = new ListObjectsRequest { BucketName = _bucket, Prefix = (MakePath(domain, path)) };
                var response = client.ListObjects(request);
                return response.S3Objects.Count > 0;
            }
        }

        public override bool IsDirectory(string domain, string path)
        {
            return IsFile(domain, path);
        }

        public override void DeleteDirectory(string domain, string path)
        {
            DeleteFiles(domain, path, "*.*", true);
        }

        public override long GetFileSize(string domain, string path)
        {
            using (var client = GetClient())
            {
                var request = new ListObjectsRequest { BucketName = _bucket, Prefix = (MakePath(domain, path)) };
                var response = client.ListObjects(request);
                if (response.S3Objects.Count > 0)
                {
                    return response.S3Objects[0].Size;
                }
                throw new FileNotFoundException("file not found", path);
            }
        }

        public override long GetDirectorySize(string domain, string path)
        {
            if (!IsDirectory(domain, path))
                throw new FileNotFoundException("directory not found", path);

            return GetS3Objects(domain, path)
                .Where(x => Wildcard.IsMatch("*.*", Path.GetFileName(x.Key)))
                .Sum(x => x.Size);
        }

        public override long ResetQuota(string domain)
        {
            if (QuotaController != null)
            {
                var objects = GetS3Objects(domain);
                var size = objects.Sum(s3Object => s3Object.Size);
                QuotaController.QuotaUsedSet(_modulename, domain, _dataList.GetData(domain), size);
                return size;
            }
            return 0;
        }

        public override long GetUsedQuota(string domain)
        {
            var objects = GetS3Objects(domain);
            return objects.Sum(s3Object => s3Object.Size);
        }

        public override Uri Copy(string srcdomain, string srcpath, string newdomain, string newpath)
        {
            using (var client = GetClient())
            {
                var srcKey = MakePath(srcdomain, srcpath);
                var dstKey = MakePath(newdomain, newpath);
                var size = GetFileSize(srcdomain, srcpath);

                var request = new CopyObjectRequest
                {
                    SourceBucket = _bucket,
                    SourceKey = srcKey,
                    DestinationBucket = _bucket,
                    DestinationKey = dstKey,
                    CannedACL = GetDomainACL(newdomain),
                    MetadataDirective = S3MetadataDirective.REPLACE,
                    ServerSideEncryptionMethod = _sse
                };

                client.CopyObject(request);

                QuotaUsedAdd(newdomain, size);

                return GetUri(newdomain, newpath);
            }
        }

        public override void CopyDirectory(string srcdomain, string srcdir, string newdomain, string newdir)
        {
            var srckey = MakePath(srcdomain, srcdir);
            var dstkey = MakePath(newdomain, newdir);
            //List files from src
            using (var client = GetClient())
            {
                var request = new ListObjectsRequest { BucketName = _bucket, Prefix = srckey };

                var response = client.ListObjects(request);
                foreach (var s3Object in response.S3Objects)
                {
                    client.CopyObject(new CopyObjectRequest
                    {
                        SourceBucket = _bucket,
                        SourceKey = s3Object.Key,
                        DestinationBucket = _bucket,
                        DestinationKey = s3Object.Key.Replace(srckey, dstkey),
                        CannedACL = GetDomainACL(newdomain),
                        ServerSideEncryptionMethod = _sse
                    });

                    QuotaUsedAdd(newdomain, s3Object.Size);
                }
            }
        }

        private IEnumerable<S3Object> GetS3ObjectsByPath(string domain, string path)
        {
            using (var client = GetClient())
            {
                var request = new ListObjectsRequest
                {
                    BucketName = _bucket,
                    Prefix = path,
                    MaxKeys = (1000)
                };

                var objects = new List<S3Object>();
                ListObjectsResponse response;
                do
                {
                    response = client.ListObjects(request);
                    objects.AddRange(response.S3Objects.Where(entry => CheckKey(domain, entry.Key)));
                    request.Marker = response.NextMarker;
                } while (response.IsTruncated);
                return objects;
            }
        }

        private IEnumerable<S3Object> GetS3Objects(string domain, string path = "", bool recycle = false)
        {
            path = MakePath(domain, path) + '/';
            var obj = GetS3ObjectsByPath(domain, path).ToList();
            if (string.IsNullOrEmpty(_recycleDir) || !recycle) return obj;
            obj.AddRange(GetS3ObjectsByPath(domain, GetRecyclePath(path)));
            return obj;
        }


        public override IDataStore Configure(IDictionary<string, string> props)
        {
            _accessKeyId = props["acesskey"];
            _secretAccessKeyId = props["secretaccesskey"];
            _bucket = props["bucket"];

            if (props.ContainsKey("recycleDir"))
            {
                _recycleDir = props["recycleDir"];
            }

            if (props.ContainsKey("region") && !string.IsNullOrEmpty(props["region"]))
            {
                _region = props["region"];
            }

            if (props.ContainsKey("serviceurl") && !string.IsNullOrEmpty(props["serviceurl"]))
            {
                _serviceurl = props["serviceurl"];
            }

            if (props.ContainsKey("forcepathstyle"))
            {
                bool fps;
                if (bool.TryParse(props["forcepathstyle"], out fps))
                {
                    _forcepathstyle = fps;
                }
            }

            if (props.ContainsKey("usehttp"))
            {
                bool uh;
                if (bool.TryParse(props["usehttp"], out uh))
                {
                    _useHttp = uh;
                }
            }

            if (props.ContainsKey("sse") && !string.IsNullOrEmpty(props["sse"]))
            {
                switch (props["sse"].ToLower())
                {
                    case "none":
                        _sse = ServerSideEncryptionMethod.None;
                        break;
                    case "aes256":
                        _sse = ServerSideEncryptionMethod.AES256;
                        break;
                    case "awskms":
                        _sse = ServerSideEncryptionMethod.AWSKMS;
                        break;
                    default:
                        _sse = ServerSideEncryptionMethod.None;
                        break;
                }
            }

            _bucketRoot = props.ContainsKey("cname") && Uri.IsWellFormedUriString(props["cname"], UriKind.Absolute)
                              ? new Uri(props["cname"], UriKind.Absolute)
                              : new Uri(String.Format("http://s3.{1}.amazonaws.com/{0}/", _bucket, _region), UriKind.Absolute);
            _bucketSSlRoot = props.ContainsKey("cnamessl") &&
                             Uri.IsWellFormedUriString(props["cnamessl"], UriKind.Absolute)
                                 ? new Uri(props["cnamessl"], UriKind.Absolute)
                                 : new Uri(String.Format("https://s3.{1}.amazonaws.com/{0}/", _bucket, _region), UriKind.Absolute);

            if (props.ContainsKey("lower"))
            {
                bool.TryParse(props["lower"], out _lowerCasing);
            }
            if (props.ContainsKey("cloudfront"))
            {
                bool.TryParse(props["cloudfront"], out _revalidateCloudFront);
            }
            if (props.ContainsKey("distribution"))
            {
                _distributionId = props["distribution"];
            }

            if (props.ContainsKey("subdir"))
            {
                _subDir = props["subdir"];
            }

            return this;
        }

        private string MakePath(string domain, string path)
        {
            string result;

            path = path.TrimStart('\\', '/').TrimEnd('/').Replace('\\', '/');

            if (!String.IsNullOrEmpty(_subDir))
            {
                if (_subDir.Length == 1 && (_subDir[0] == '/' || _subDir[0] == '\\'))
                    result = path;
                else
                    result = String.Format("{0}/{1}", _subDir, path); // Ignory all, if _subDir is not null
            }
            else//Key combined from module+domain+filename
                result = string.Format("{0}/{1}/{2}/{3}",
                                                         _tenant,
                                                         _modulename,
                                                         domain,
                                                         path);

            result = result.Replace("//", "/").TrimStart('/').TrimEnd('/');
            if (_lowerCasing)
            {
                result = result.ToLowerInvariant();
            }

            return result;
        }

        private string GetRecyclePath(string path)
        {
            return string.IsNullOrEmpty(_recycleDir) ? "" : string.Format("{0}/{1}", _recycleDir, path.TrimStart('/'));
        }

        private void Recycle(IAmazonS3 client, string domain, string key)
        {
            if (string.IsNullOrEmpty(_recycleDir)) return;

            var copyObjectRequest = new CopyObjectRequest
            {
                SourceBucket = _bucket,
                SourceKey = key,
                DestinationBucket = _bucket,
                DestinationKey = GetRecyclePath(key),
                CannedACL = GetDomainACL(domain),
                MetadataDirective = S3MetadataDirective.REPLACE,
                ServerSideEncryptionMethod = _sse,
                StorageClass = S3StorageClass.Glacier,

            };

            client.CopyObject(copyObjectRequest);
        }

        private IAmazonCloudFront GetCloudFrontClient()
        {
            var cfg = new AmazonCloudFrontConfig { MaxErrorRetry = 3 };
            return new AmazonCloudFrontClient(_accessKeyId, _secretAccessKeyId, cfg);
        }

        private IAmazonS3 GetClient()
        {
            var cfg = new AmazonS3Config { MaxErrorRetry = 3 };

            if (!String.IsNullOrEmpty(_serviceurl))
            {
                cfg.ServiceURL = _serviceurl;

                cfg.ForcePathStyle = _forcepathstyle;
            }
            else
            {
                cfg.RegionEndpoint = RegionEndpoint.GetBySystemName(_region);
            }

            cfg.UseHttp = _useHttp;

            return new AmazonS3Client(_accessKeyId, _secretAccessKeyId, cfg);
        }

        public Stream GetWriteStream(string domain, string path)
        {
            throw new NotSupportedException();
        }

        private class ResponseStreamWrapper : Stream
        {
            private readonly GetObjectResponse _response;


            public ResponseStreamWrapper(GetObjectResponse response)
            {
                if (response == null) throw new ArgumentNullException("response");

                _response = response;
            }


            public override bool CanRead
            {
                get { return _response.ResponseStream.CanRead; }
            }

            public override bool CanSeek
            {
                get { return _response.ResponseStream.CanSeek; }
            }

            public override bool CanWrite
            {
                get { return _response.ResponseStream.CanWrite; }
            }

            public override long Length
            {
                get { return _response.ContentLength; }
            }

            public override long Position
            {
                get { return _response.ResponseStream.Position; }
                set { _response.ResponseStream.Position = value; }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _response.ResponseStream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _response.ResponseStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _response.ResponseStream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _response.ResponseStream.Write(buffer, offset, count);
            }

            public override void Flush()
            {
                _response.ResponseStream.Flush();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing) _response.Dispose();
            }
        }
    }
}