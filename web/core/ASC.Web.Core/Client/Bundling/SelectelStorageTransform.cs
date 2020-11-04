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
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Optimization;

using ASC.Common.Logging;
using ASC.Data.Storage.Configuration;
using ASC.Data.Storage.Selectel;

using SelectelSharp;

using MimeMapping = ASC.Common.Web.MimeMapping;

namespace ASC.Web.Core.Client.Bundling
{
    class SelectelStorageTransform : IBundleTransform
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Web.Bundle.SelectelStorageTransform");
        private static bool successInitialized = false;
        private static readonly Dictionary<string, string> appenders = new Dictionary<string, string>();
        private static int work = 0;
        private static readonly ConcurrentQueue<CdnItem> queue = new ConcurrentQueue<CdnItem>();

        private static string _authUser = string.Empty;
        private static string _authPwd = string.Empty;
        private static string _container = string.Empty;

        static SelectelStorageTransform()
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
                    if (h.Type.Equals(typeof(SelectelStorage)) && h.Name == "cdn")
                    {
                        _authPwd = h.HandlerProperties["authPwd"].Value;
                        _authUser = h.HandlerProperties["authUser"].Value;
                        _container = h.HandlerProperties["public_container"].Value;

                        successInitialized = true;

                        break;
                    }
                }


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

                            bool upload = true;

                            var client = new SelectelClient();

                            client.AuthorizeAsync(_authUser, _authPwd).Wait();

                            var etag = SelectelSharp.Common.Helpers.CalculateSHA1(item.Response.Content);

                            var fileInfo = client.GetContainerFilesAsync(_container, 1, null, key, null, null).Result;

                            if (fileInfo != null && fileInfo.Any())
                            {
                                upload = fileInfo.Single().Hash != etag;
                            }

                            if (upload)
                            {
                                var contentType = String.Empty;

                                var mime = string.IsNullOrEmpty(contentType) ? MimeMapping.GetMimeMapping(Path.GetFileName(key))
                             : contentType;

                                var customHeaders = new Dictionary<string, object>();

                                if (ClientSettings.GZipEnabled)
                                {
                                    customHeaders.Add("Content-Encoding", "gzip");
                                }

                                var cache = TimeSpan.FromDays(365);

                                customHeaders.Add("Cache-Control", String.Format("public, maxage={0}", (int)cache.TotalSeconds));
                                customHeaders.Add("Expires", DateTime.UtcNow.Add(cache));

                                client.UploadFileAsync(_container, key, true, true, inputStream, etag, null, mime, null, null, customHeaders).Wait();
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
