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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Optimization;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

using ASC.Common.Logging;
using ASC.Data.Storage.Configuration;

namespace ASC.Web.Core.Client.Bundling
{
    class CloudFrontTransform : IBundleTransform
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Web.Bundle.CloudFrontTransform");
        private static readonly ConcurrentQueue<CdnItem> queue = new ConcurrentQueue<CdnItem>();
        private static readonly Dictionary<string, string> appenders = new Dictionary<string, string>();
        private static readonly string s3publickey;
        private static readonly string s3privatekey;
        private static readonly string s3bucket;
        private static readonly string s3region;
        private static bool successInitialized = false;
        private static int work = 0;


        static CloudFrontTransform()
        {
            try
            {
                var section = (StorageConfigurationSection)ConfigurationManagerExtension.GetSection("storage");
                if (section == null)
                {
                    throw new Exception("Storage section not found.");
                }

                if (section.Appenders.Count == 0)
                {
                    throw new Exception("Appenders not found.");
                }
                foreach (AppenderConfigurationElement a in section.Appenders)
                {
                    var url = string.IsNullOrEmpty(a.AppendSecure) ? a.Append : a.AppendSecure;
                    if (url.StartsWith("~"))
                    {
                        throw new Exception("Only absolute cdn path supported. Can not use " + url);
                    }

                    appenders[a.Extensions + "|"] = url.TrimEnd('/') + "/";
                }

                foreach (HandlerConfigurationElement h in section.Handlers)
                {
                    if (h.Name == "cdn")
                    {
                        s3publickey = h.HandlerProperties["acesskey"].Value;
                        s3privatekey = h.HandlerProperties["secretaccesskey"].Value;
                        s3bucket = h.HandlerProperties["bucket"].Value;
                        s3region = h.HandlerProperties["region"].Value;
                        break;
                    }
                }

                successInitialized = true;
            }
            catch (Exception fatal)
            {
                log.Fatal(fatal);
            }
        }


        public void Process(BundleContext context, BundleResponse response)
        {
            if (successInitialized && BundleTable.Bundles.UseCdn)
            {
                try
                {
                    var bundle = context.BundleCollection.GetBundleFor(context.BundleVirtualPath);
                    if (bundle != null)
                    {
                        queue.Enqueue(new CdnItem { Bundle = bundle, Response = response });
                        Action upload = UploadToCdn;
                        upload.BeginInvoke(null, null);
                    }
                }
                catch (Exception fatal)
                {
                    log.Fatal(fatal);
                    throw;
                }
            }
        }

        private void UploadToCdn()
        {
            try
            {
                // one thread only
                if (Interlocked.CompareExchange(ref work, 1, 0) == 0)
                {
                    var @continue = false;
                    try
                    {
                        CdnItem item;
                        if (queue.TryDequeue(out item))
                        {
                            @continue = true;

                            var cdnpath = GetCdnPath(item.Bundle.Path);
                            var key = new Uri(cdnpath).PathAndQuery.TrimStart('/');
                            var content = Encoding.UTF8.GetBytes(item.Response.Content);
                            var inputStream = new MemoryStream();

                            if (ClientSettings.GZipEnabled)
                            {
                                using (var zip = new GZipStream(inputStream, CompressionMode.Compress, true))
                                {
                                    zip.Write(content, 0, content.Length);
                                    zip.Flush();
                                }
                            }
                            else
                            {
                                inputStream.Write(content, 0, content.Length);
                            }

                            var checksum = AmazonS3Util.GenerateChecksumForContent(item.Response.Content, true);

                            var config = new AmazonS3Config
                            {
                                RegionEndpoint = RegionEndpoint.GetBySystemName(s3region),
                                UseHttp = true
                            };
                            using (var s3 = new AmazonS3Client(s3publickey, s3privatekey, config))
                            {
                                var upload = false;
                                try
                                {
                                    var request = new GetObjectMetadataRequest
                                    {
                                        BucketName = s3bucket,
                                        Key = key,
                                    };
                                    var response = s3.GetObjectMetadata(request);
                                    upload = !string.Equals(checksum, response.Metadata["x-amz-meta-etag"], StringComparison.InvariantCultureIgnoreCase);
                                }
                                catch (AmazonS3Exception ex)
                                {
                                    if (ex.StatusCode == HttpStatusCode.NotFound)
                                    {
                                        upload = true;
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }

                                if (upload)
                                {
                                    var request = new PutObjectRequest
                                    {
                                        BucketName = s3bucket,
                                        CannedACL = S3CannedACL.PublicRead,
                                        AutoCloseStream = true,
                                        AutoResetStreamPosition = true,
                                        Key = key,
                                        ContentType = AmazonS3Util.MimeTypeFromExtension(Path.GetExtension(key).ToLowerInvariant()),
                                        InputStream = inputStream
                                    };

                                    if (ClientSettings.GZipEnabled)
                                    {
                                        request.Headers.ContentEncoding = "gzip";
                                    }

                                    var cache = TimeSpan.FromDays(365);
                                    request.Headers.CacheControl = string.Format("public, maxage={0}", (int)cache.TotalSeconds);
                                    request.Headers.ExpiresUtc = DateTime.UtcNow.Add(cache);
                                    request.Headers["x-amz-meta-etag"] = checksum;

                                    s3.PutObject(request);
                                }
                                else
                                {
                                    inputStream.Close();
                                }

                                item.Bundle.CdnPath = cdnpath;
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        log.Error(err);
                    }
                    finally
                    {
                        work = 0;
                        if (@continue)
                        {
                            Action upload = () => UploadToCdn();
                            upload.BeginInvoke(null, null);
                        }
                    }
                }
            }
            catch (Exception fatal)
            {
                log.Fatal(fatal);
            }
        }

        private static string GetCdnPath(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            var abspath = string.Empty;
            foreach (var a in appenders)
            {
                abspath = a.Value;
                if (a.Key.ToLowerInvariant().Contains(ext + "|"))
                {
                    break;
                }
            }

            return abspath + path.TrimStart('~', '/');
        }
    }

    class CdnItem
    {
        public Bundle Bundle { get; set; }

        public BundleResponse Response { get; set; }
    }
}