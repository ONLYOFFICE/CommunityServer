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
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Optimization;

using ASC.Common.Logging;
using ASC.Common.Web;
using ASC.Data.Storage.Configuration;

using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;


namespace ASC.Web.Core.Client.Bundling
{

    public class GoogleCloudStorageTransform : IBundleTransform
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Web.Bundle.GoogleCloudStorageTransform");
        private static readonly Dictionary<string, string> appenders = new Dictionary<string, string>();
        private static readonly ConcurrentQueue<CdnItem> queue = new ConcurrentQueue<CdnItem>();
        private static int work = 0;

        private static bool successInitialized = false;

        private static string _bucket = "";
        private static string _json = "";
        private static string _region = "";

        static GoogleCloudStorageTransform()
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
                        _json = h.HandlerProperties["json"].Value;
                        _bucket = h.HandlerProperties["bucket"].Value;
                        _region = h.HandlerProperties["region"].Value;
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

        private StorageClient GetStorage()
        {
            var credential = GoogleCredential.FromJson(_json);

            return StorageClient.Create(credential);
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
                            var storage = GetStorage();

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

                            var upload = false;

                            Google.Apis.Storage.v1.Data.Object objInfo = null;

                            try
                            {
                                objInfo = storage.GetObject(_bucket, key);
                            }
                            catch (GoogleApiException ex)
                            {
                                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                                {
                                    upload = true;
                                }
                                else
                                {
                                    throw;
                                }
                            }

                            if (objInfo != null)
                            {
                                String contentMd5Hash = String.Empty;

                                using (var sha1 = SHA1.Create())
                                {
                                    byte[] bytes = Encoding.UTF8.GetBytes(item.Response.Content);
                                    byte[] hashBytes = sha1.ComputeHash(bytes);
                                    StringBuilder sb = new StringBuilder();
                                    foreach (byte b in hashBytes)
                                        sb.Append(b.ToString("X2"));

                                    contentMd5Hash = sb.ToString().ToLower();
                                }

                                if (String.Compare(objInfo.Md5Hash, contentMd5Hash, StringComparison.InvariantCultureIgnoreCase) != 0)
                                    upload = true;
                            }


                            if (upload)
                            {

                                UploadObjectOptions uploadObjectOptions = new UploadObjectOptions
                                {
                                    PredefinedAcl = PredefinedObjectAcl.PublicRead
                                };

                                inputStream.Position = 0;

                                var uploaded = storage.UploadObject(_bucket, key, MimeMapping.GetMimeMapping(Path.GetFileName(key)), inputStream, uploadObjectOptions, null);

                                inputStream.Close();

                                if (uploaded.Metadata == null)
                                    uploaded.Metadata = new Dictionary<String, String>();


                                if (ClientSettings.GZipEnabled)
                                {
                                    uploaded.ContentEncoding = "gzip";
                                }

                                var cache = TimeSpan.FromDays(365);

                                uploaded.CacheControl = String.Format("public, maxage={0}", (int)cache.TotalSeconds);
                                uploaded.Metadata["Expires"] = DateTime.UtcNow.Add(TimeSpan.FromDays(365)).ToString("R");

                                storage.UpdateObject(uploaded);
                            }
                            else
                            {
                                inputStream.Close();
                            }

                            item.Bundle.CdnPath = cdnpath;
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
}
