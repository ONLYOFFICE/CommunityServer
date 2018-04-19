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
using System.Linq;
using System.Web;
using System.Web.Optimization;

using ASC.Core;
using ASC.Data.Storage;
using ASC.Data.Storage.Configuration;
using System.Web.Configuration;
using ASC.Data.Storage.Selectel;
using ASC.Data.Storage.S3;
using ASC.Data.Storage.GoogleCloud;
using ASC.Data.Storage.RackspaceCloud;

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
            if (DiscTransform.SuccessInitialized)
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

                var section = (StorageConfigurationSection)WebConfigurationManager.GetSection("storage");

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
                        Transforms.Add(new DiscTransform());
                        CdnPath = DiscTransform.GetUri(Path, ContentType);
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
