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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

using ASC.Core;
using ASC.Data.Storage;

namespace ASC.Web.Core.Client.Bundling
{
    static class BundleHelper
    {
        public const string BUNDLE_VPATH = "/bundle/";
        public const string CLIENT_SCRIPT_VPATH = "/clientscript/";


        public static ASCStyleBundle GetCssBundle(string path)
        {
            return (ASCStyleBundle)BundleTable.Bundles.GetBundleFor(ToVirtualPath(path));
        }

        public static ASCJsBundle GetJsBundle(string path)
        {
            return (ASCJsBundle)BundleTable.Bundles.GetBundleFor(ToVirtualPath(path));
        }

        public static void AddBundle(Bundle bundle)
        {
            BundleTable.Bundles.Add(bundle);
            if (((ASCBundle)bundle).UseDisc)
            {
                bundle.GenerateBundleResponse(new BundleContext(new HttpContextWrapper(HttpContext.Current), BundleTable.Bundles, bundle.Path));
            }
        }

        public static ASCStyleBundle CssBundle(string virtualPath)
        {
            return new ASCStyleBundle(ToVirtualPath(virtualPath));
        }

        public static ASCJsBundle JsBundle(string virtualPath)
        {
            return new ASCJsBundle(ToVirtualPath(virtualPath));
        }

        public static string HtmlLink(string uri)
        {
            return HtmlLink(uri, true);
        }

        public static string HtmlScript(string uri)
        {
            return HtmlScript(uri, true, false);
        }

        public static string HtmlLink(string uri, bool resolve)
        {
            return string.Format("<link type='text/css' href='{0}' rel='stylesheet' />", GetUrl(uri, resolve));
        }

        public static string HtmlScript(string uri, bool resolve, bool notobfuscate)
        {
            return string.Format("<script type='text/javascript' src='{0}'{1}></script>", GetUrl(uri, resolve), notobfuscate ? " notobfuscate='true'" : string.Empty);
        }

        public static string GetUrl(string path, bool resolve)
        {
            var resolved = path;
            if (resolve)
            {
                resolved = BundleTable.Bundles.ResolveBundleUrl(ToVirtualPath(path), false);
                if (path.Contains('?'))
                {
                    resolved += path.Substring(path.LastIndexOf('?'));
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

        public static string ToVirtualPath(string uri)
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
            public bool UseDisc;

            protected ASCBundle(string virtualPath, params IBundleTransform[] transforms)
                : base(virtualPath, transforms)
            {
                Transforms.Add(new CopyrigthTransform());

                if (!BundleTable.Bundles.UseCdn) return;

                if (CoreContext.Configuration.Standalone)
                {
                    UseDisc = true;
                    Transforms.Add(new DiscTransform());
                    CdnPath = DiscTransform.GetUri(Path, ContentType);
                }
                else
                {
                    Transforms.Add(new CdnTransform());
                }
            }
        }

        internal class ASCStyleBundle : ASCBundle
        {
            protected override string ContentType { get { return "text/css"; } }

            public ASCStyleBundle(string virtualPath)
                : base(virtualPath, new CssMinify())
            {
            }

            public Bundle Include(string path)
            {
                return Include(ToVirtualPath(path), new CssTransform(UseDisc ? CdnPath : Path));
            }
        }

        internal class ASCJsBundle : ASCBundle
        {
            public bool UseCache {  get; set; }
            protected override string ContentType { get { return "text/javascript"; } }
            public override IBundleOrderer Orderer { get { return new NullOrderer(); } }

            public ASCJsBundle(string virtualPath)
                : base(virtualPath, new JsTransform())
            {
                ConcatenationToken = ";" + Environment.NewLine;
                UseCache = true;
            }

            public ASCJsBundle Include(string path, bool obfuscate)
            {
                return (ASCJsBundle)Include(ToVirtualPath(path), new JsTransform(obfuscate));
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
        }
    }
}
