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
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Optimization;

using ASC.Common.Logging;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Data.Storage.Configuration;
using ASC.Data.Storage.DiscStorage;

namespace ASC.Web.Core.Client.Bundling
{
    class DiscTransform : IBundleTransform
    {
        private static readonly ILog log;

        public static readonly bool SuccessInitialized;

        const string BaseVirtualPath = "/discbundle/";
        static string BaseStoragePath;

        static DiscTransform()
        {
            try
            {
                log = LogManager.GetLogger("ASC.Web.Bundle.DiscTransform");

                var section = (StorageConfigurationSection)ConfigurationManagerExtension.GetSection("storage");
                if (section == null)
                {
                    throw new Exception("Storage section not found.");
                }

                foreach (HandlerConfigurationElement h in section.Handlers)
                {
                    if (h.Name == "disc")
                    {
                        BaseStoragePath = Path.Combine(h.HandlerProperties["$STORAGE_ROOT"].Value, "bundle");
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(BaseStoragePath))
                {
                    DiscDataHandler.RegisterVirtualPath(BaseVirtualPath, GetFullPhysicalPath("/"), true);
                    SuccessInitialized = CoreContext.Configuration.Standalone && !StaticUploader.CanUpload();
                }
            }
            catch (Exception fatal)
            {
                log.Fatal(fatal);
            }
        }

        public void Process(BundleContext context, BundleResponse response)
        {
            if (!SuccessInitialized || !BundleTable.Bundles.UseCdn) return;

            try
            {
                var bundle = context.BundleCollection.GetBundleFor(context.BundleVirtualPath);
                if (bundle != null)
                {
                    UploadToDisc(new CdnItem { Bundle = bundle, Response = response });
                }
            }
            catch (Exception fatal)
            {
                log.Fatal(fatal);
                throw;
            }
        }

        private static void UploadToDisc(CdnItem item)
        {
            var filePath = GetFullFileName(item.Bundle.Path, item.Response.ContentType).TrimStart('/');
            var fullFilePath = GetFullPhysicalPath(filePath);

            try
            {
                using (var mutex = new Mutex(true, filePath))
                {
                    var wait = mutex.WaitOne(60000);

                    try
                    {
                        CreateDir(fullFilePath);

                        if (!File.Exists(fullFilePath))
                        {
                            var gzip = ClientSettings.GZipEnabled;

                            using (var fs = File.OpenWrite(fullFilePath))
                            using (var tw = new StreamWriter(fs))
                            {
                                tw.WriteLine(item.Response.Content);
                            }

                            item.Bundle.CdnPath = GetUri(filePath);

                            if (gzip)
                            {
                                using (var fs = File.OpenWrite(fullFilePath + ".gz"))
                                using (var zip = new GZipStream(fs, CompressionMode.Compress, true))
                                using (var tw = new StreamWriter(zip))
                                {
                                    tw.WriteLine(item.Response.Content);
                                }
                            }

                        }
                        else
                        {
                            item.Bundle.CdnPath = GetUri(item.Bundle.Path, item.Response.ContentType);
                        }
                    }
                    finally
                    {
                        if (wait)
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                log.Error(err);
            }
        }

        internal static string GetUri(string path)
        {
            return BaseVirtualPath + path.TrimStart('/');
        }

        internal static string GetUri(string path, string contentType)
        {
            return GetUri(GetFullFileName(path, contentType));
        }

        internal static string GetFullFileName(string path, string contentType)
        {
            var href = Path.GetFileNameWithoutExtension(path);
            var category = GetCategoryFromPath(path);

            var hrefTokenSource = href;

            if (Uri.IsWellFormedUriString(href, UriKind.Relative))
                hrefTokenSource = (SecureHelper.IsSecure() ? "https" : "http") + href;

            var hrefToken = string.Concat(HttpServerUtility.UrlTokenEncode(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(hrefTokenSource))));

            if (string.Compare(contentType, "text/css", StringComparison.OrdinalIgnoreCase) == 0)
                return string.Format("/{0}/css/{1}.css", category, hrefToken);

            return string.Format("/{0}/javascript/{1}.js", category, hrefToken);
        }

        internal static bool IsFile(string path)
        {
            var filePath = GetFullPhysicalPath(path);
            var staticPath = GetFullStaticPhysicalPath(path);
            if (!File.Exists(filePath) && File.Exists(staticPath))
            {
                using (var mutex = new Mutex(true, path))
                {
                    mutex.WaitOne();

                    try
                    {
                        CreateDir(filePath);

                        if (File.Exists(staticPath))
                        {
                            File.Copy(staticPath, filePath, true);
                        }

                        if (File.Exists(staticPath + ".gz"))
                        {
                            File.Copy(staticPath + ".gz", filePath + ".gz", true);
                        }
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }

            return File.Exists(filePath);
        }

        private static string GetCategoryFromPath(string controlPath)
        {
            var result = "common";

            controlPath = controlPath.ToLower();
            var matches = Regex.Match(controlPath, "~/(\\w+)/(\\w+)-(\\w+)/?", RegexOptions.Compiled);

            if (matches.Success && matches.Groups.Count > 3 && matches.Groups[2].Success)
                result = matches.Groups[2].Value;

            return result;
        }

        private static string GetFullPhysicalPath(string path)
        {
            return Path.GetFullPath(Path.Combine(HttpContext.Current.Server.MapPath("~/"), BaseStoragePath, path.TrimStart('/')));
        }

        private static string GetFullStaticPhysicalPath(string path)
        {
            return Path.GetFullPath(Path.Combine(HttpContext.Current.Server.MapPath("~/"), "App_Data/static/bundle/", path.TrimStart('/')));
        }

        private static void CreateDir(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}