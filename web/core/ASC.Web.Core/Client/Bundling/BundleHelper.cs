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
using System.Linq;
using System.Web;
using System.Web.Optimization;

using ASC.Core;
using ASC.Data.Storage;
using ASC.Data.Storage.Configuration;
using ASC.Data.Storage.GoogleCloud;
using ASC.Data.Storage.RackspaceCloud;
using ASC.Data.Storage.S3;
using ASC.Data.Storage.Selectel;

namespace ASC.Web.Core.Client.Bundling
{
    static class BundleHelper
    {
        public const string BUNDLE_VPATH = "/bundle/";
        public const string CLIENT_SCRIPT_VPATH = "/clientscript/";

        public static string AddBundle(BundleData bundleData)
        {
            var path = bundleData.GetBundleVirtualPath(BUNDLE_VPATH, ClientSettings.ResetCacheKey);
            var virtualPath = ToVirtualPath(path);
            var bundle = (ASCBundle)BundleTable.Bundles.GetBundleFor(virtualPath);
            if (bundle == null)
            {
                bundle = bundleData.CreateAscBundle(virtualPath);
                foreach (var script in bundleData.GetSource())
                {
                    bundle.Include(script);
                }
                AddBundle(bundle);
            }

            return bundle.GetLink(path);
        }



        public static void AddBundle(Bundle bundle)
        {
            BundleTable.Bundles.Add(bundle);
            if (DiscTransform.SuccessInitialized || StaticUploader.CanUpload())
            {
                bundle.GenerateBundleResponse(new BundleContext(new HttpContextWrapper(HttpContext.Current), BundleTable.Bundles, bundle.Path));
            }
        }

        public static string GetCssLink(string uri, bool resolve = true)
        {
            return string.Format("<link type='text/css' href='{0}' rel='stylesheet' />", GetUrl(uri, resolve));
        }

        public static string GetJavascriptLink(string uri, bool resolve = true)
        {
            return string.Format("<script type='text/javascript' src='{0}'></script>", GetUrl(uri, resolve));
        }

        private static string GetUrl(string uri, bool resolve)
        {
            var resolved = uri;
            if (resolve)
            {
                resolved = BundleTable.Bundles.ResolveBundleUrl(ToVirtualPath(uri), false);
                if (uri.Contains('?'))
                {
                    resolved += uri.Substring(uri.LastIndexOf('?'));
                }
            }

            var version = "ver=" + ClientSettings.ResetCacheKey;
            if (resolved.Contains("&ver=") || resolved.Contains("?ver="))
            {
                resolved = resolved.Replace("&ver=", "&" + version).Replace("?ver=", "?" + version);
            }
            else
            {
                resolved += "?" + version;
            }
            return resolved;
        }

        private static string ToVirtualPath(string uri)
        {
            if (uri.Contains('?'))
            {
                uri = uri.Substring(0, uri.LastIndexOf('?')).TrimEnd('?');
            }
            if (uri.StartsWith("~"))
            {
                return uri;
            }
            if (Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                uri = "/" + WebPath.GetRelativePath(uri);
            }
            if (Uri.IsWellFormedUriString(uri, UriKind.Relative))
            {
                if (uri.StartsWith("/"))
                {
                    if (HttpContext.Current != null && uri.StartsWith(HttpContext.Current.Request.ApplicationPath))
                    {
                        uri = "/" + uri.Substring(HttpContext.Current.Request.ApplicationPath.Length).TrimStart('/');
                    }
                    return "~" + uri;
                }
                throw new Exception(uri);
            }
            throw new UriFormatException("Resource Bundle Control should not contain absolute uri " + uri);
        }

        internal class ASCBundle : Bundle
        {
            protected virtual string ContentType { get { return ""; } }

            protected ASCBundle(string virtualPath, params IBundleTransform[] transforms)
                : base(virtualPath, transforms)
            {
                Transforms.Add(new CopyrigthTransform());

                if (!BundleTable.Bundles.UseCdn) return;

                bool isCDN = false;

                var section = (StorageConfigurationSection)ConfigurationManagerExtension.GetSection("storage");

                foreach (HandlerConfigurationElement h in section.Handlers)
                {
                    if (String.Compare(h.Name, "cdn", true) != 0) continue;

                    if (h.Type.Equals(typeof(SelectelStorage)))
                    {
                        Transforms.Add(new SelectelStorageTransform());
                    }
                    else if (h.Type.Equals(typeof(S3Storage)))
                    {
                        Transforms.Add(new CloudFrontTransform());
                    }
                    else if (h.Type.Equals(typeof(GoogleCloudStorage)))
                    {
                        Transforms.Add(new GoogleCloudStorageTransform());
                    }
                    else if (h.Type.Equals(typeof(RackspaceCloudStorage)))
                    {
                        Transforms.Add(new RackspaceCloudStorageTransform());
                    }
                    else
                    {
                        throw new Exception("unknown argument");
                    }

                    isCDN = true;

                    break;
                }

                if (!isCDN)
                {
                    if (CoreContext.Configuration.Standalone)
                    {
                        if (DiscTransform.SuccessInitialized)
                        {
                            Transforms.Add(new DiscTransform());
                            CdnPath = DiscTransform.GetUri(Path, ContentType);
                        }
                        else
                        {
                            Transforms.Add(new StorageTransform());
                        }
                    }
                }

            }

            public virtual Bundle Include(string path)
            {
                throw new NotImplementedException();
            }

            internal virtual string GetLink(string uri)
            {
                return uri;
            }
        }

        internal class ASCStyleBundle : ASCBundle
        {
            protected override string ContentType { get { return "text/css"; } }

            public ASCStyleBundle(string virtualPath)
                : base(virtualPath, new CssTransform())
            {
            }

            public override Bundle Include(string path)
            {
                var minify = true;
                if (HttpContext.Current != null)
                {
                    var filePath = HttpContext.Current.Server.MapPath(path);
                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath.Replace(".css", ".min.css").Replace(".less", ".min.css")))
                    {
                        minify = false;
                        path = path.Replace(".css", ".min.css").Replace(".less", ".min.css");
                    }
                }

                return Include(ToVirtualPath(path), new CssTransform(DiscTransform.SuccessInitialized ? CdnPath : Path, minify));
            }

            internal override string GetLink(string uri)
            {
                return GetCssLink(uri);
            }
        }

        internal class ASCJsBundle : ASCBundle
        {
            public bool UseCache { get; set; }
            protected override string ContentType { get { return "text/javascript"; } }
            public override IBundleOrderer Orderer { get { return new NullOrderer(); } }

            public ASCJsBundle(string virtualPath)
                : base(virtualPath, new JsTransform())
            {
                ConcatenationToken = ";" + Environment.NewLine;
                UseCache = true;
            }

            public override Bundle Include(string path)
            {
                return Include(ToVirtualPath(path), new JsTransform());
            }

            public override void UpdateCache(BundleContext context, BundleResponse response)
            {
                if (UseCache)
                {
                    base.UpdateCache(context, response);
                }
            }

            public override BundleResponse CacheLookup(BundleContext context)
            {
                return UseCache ? base.CacheLookup(context) : null;
            }

            internal override string GetLink(string uri)
            {
                return GetJavascriptLink(uri);
            }
        }
    }
}
