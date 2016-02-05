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


using ASC.Security.Cryptography;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using ASC.Data.Storage.Configuration;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Configuration;
using System.Web.Optimization;

namespace ASC.Web.Core.Client.Bundling
{
    class CdnTransform : IBundleTransform
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Web.Bundle.CdnTransform");
        private static readonly ConcurrentQueue<CdnItem> queue = new ConcurrentQueue<CdnItem>();
        private static readonly Dictionary<string, string> appenders = new Dictionary<string, string>();
        private static readonly string s3publickey;
        private static readonly string s3privatekey;
        private static readonly string s3bucket;
        private static readonly string s3region;
        private static bool successInitialized = false;
        private static int work = 0;


        static CdnTransform()
        {
            try
            {
                var section = (StorageConfigurationSection)WebConfigurationManager.GetSection("storage");
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
                            inputStream.Position = 0;
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
                                        Key = key,
                                        ContentType = AmazonS3Util.MimeTypeFromExtension(Path.GetExtension(key).ToLowerInvariant())
                                    };

                                    request.InputStream = inputStream;
                                    if (ClientSettings.GZipEnabled)
                                    {
                                        request.Headers.ContentEncoding = "gzip";
                                    }

                                    var cache = TimeSpan.FromDays(365);
                                    request.Headers.CacheControl = string.Format("public, maxage={0}", (int)cache.TotalSeconds);
                                    request.Headers.Expires = DateTime.UtcNow.Add(cache);
                                    request.Headers["x-amz-meta-etag"] = checksum;

                                    s3.PutObject(request);
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