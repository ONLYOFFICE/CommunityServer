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


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Optimization;
using ASC.Core.Tenants;
using ASC.Data.Storage;
using log4net;
using Newtonsoft.Json;

namespace ASC.Web.Core.Client.Bundling
{
    internal class DiscTransform : IBundleTransform
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Web.Bundle.DiscTransform");
        private static readonly ConcurrentQueue<CdnItem> queue = new ConcurrentQueue<CdnItem>();
        private static readonly ConcurrentDictionary<String, String> cacheUri = new ConcurrentDictionary<String, String>();
        private static int work;
        private static readonly String pathToCacheFile;
        private static IDataStore StaticDataStorage
        {
            get
            {
                return StorageFactory.GetStorage(Tenant.DEFAULT_TENANT.ToString(CultureInfo.InvariantCulture), "common_static");
            }
        }

        static DiscTransform()
        {
            var hashResetKey = HttpServerUtility.UrlTokenEncode(MD5.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(ClientSettings.ResetCacheKey)))
                .Replace("/", "_");
            var fileName = string.Format("info_{0}.txt", hashResetKey);

            var bundleDirPath = HttpContext.Current.Server.MapPath("~/" + ClientSettings.StorePath.Trim('/') + "/bundle/");
            if (!Directory.Exists(bundleDirPath))
            {
                Directory.CreateDirectory(bundleDirPath);
            }
            pathToCacheFile = Path.Combine(bundleDirPath, fileName);

            if (File.Exists(pathToCacheFile))
            {
                var jsonString = File.ReadAllText(pathToCacheFile);
                cacheUri = new ConcurrentDictionary<string, string>(JsonConvert.DeserializeObject<Dictionary<String, String>>(jsonString));
            }
        }

        public void Process(BundleContext context, BundleResponse response)
        {
            if (BundleTable.Bundles.UseCdn)
            {
                try
                {
                    var bundle = context.BundleCollection.GetBundleFor(context.BundleVirtualPath);
                    if (bundle != null)
                    {
                        queue.Enqueue(new CdnItem { Bundle = bundle, Response = response });
                        Action<HttpContextBase> upload = UploadToDisc;
                        upload.BeginInvoke(context.HttpContext, null, null);
                    }
                }
                catch (Exception fatal)
                {
                    log.Fatal(fatal);
                    throw;
                }
            }
        }

        private static void UploadToDisc(HttpContextBase context)
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

                            var u = context.Request.GetUrlRewriter();
                            var path = GetStorePath(item);

                            if (!BundleExist(path))
                            {
                                SaveStream(path, item.Response.Content);
                            }

                            var uriBuilder = new UriBuilder(u.Scheme, u.Host, u.Port, path);
                            item.Bundle.CdnPath = uriBuilder.ToString();
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
                            Action<HttpContextBase> upload = UploadToDisc;
                            upload.BeginInvoke(context, null, null);
                        }
                    }
                }
            }
            catch (Exception fatal)
            {
                log.Fatal(fatal);
            }
        }

        private static String GetStorePath(CdnItem item)
        {
            var filePath = GetFullFileName(item.Bundle.Path, item.Response.ContentType);

            var cacheKey = item.Bundle.Path;

            if (cacheUri.ContainsKey(cacheKey))
                return cacheUri[cacheKey];

            cacheUri.TryAdd(cacheKey, filePath);

            File.WriteAllText(pathToCacheFile, JsonConvert.SerializeObject(cacheUri));

            return filePath;
        }

        internal static String GetFullFileName(String path, string contentType)
        {
            var href = Path.GetFileNameWithoutExtension(path);
            var category = GetCategoryFromPath(path);

            var hrefTokenSource = href;

            if (Uri.IsWellFormedUriString(href, UriKind.Relative))
                hrefTokenSource = (SecureHelper.IsSecure() ? "https" : "http") + href;

            var hrefToken = String.Concat(HttpServerUtility.UrlTokenEncode(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(hrefTokenSource))));

            if (String.Compare(contentType, "text/css", StringComparison.OrdinalIgnoreCase) == 0)
                return "/" + ClientSettings.StorePath.Trim('/') + String.Format("/bundle/{0}/css/{1}.css", category, hrefToken);

            return "/" + ClientSettings.StorePath.Trim('/') + String.Format("/bundle/{0}/javascript/{1}.js", category, hrefToken);
        }


        private static String GetCategoryFromPath(String controlPath)
        {
            var result = "common";

            controlPath = controlPath.ToLower();
            var matches = Regex.Match(controlPath, "~/(\\w+)/(\\w+)-(\\w+)/?", RegexOptions.Compiled);

            if (matches.Success && matches.Groups.Count > 3 && matches.Groups[2].Success)
                result = matches.Groups[2].Value;

            return result;
        }

        private static void SaveStream(string filePath, string content)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                StaticDataStorage.Save(filePath.TrimStart('/'), stream);
            }
        }

        internal static bool BundleExist(string path)
        {
            return StaticDataStorage.IsFile("", path.TrimStart('/'));
        }
    }
}