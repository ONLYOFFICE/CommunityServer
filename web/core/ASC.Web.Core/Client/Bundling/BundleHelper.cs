/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using ASC.Data.Storage;
using System;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace ASC.Web.Core.Client.Bundling
{
    static class BundleHelper
    {
        public const string BUNDLE_VPATH = "/bundle/";
        public const string CLIENT_SCRIPT_VPATH = BUNDLE_VPATH + "clientscript/";


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

        public class ASCStyleBundle : StyleBundle
        {
            public ASCStyleBundle(string virtualPath)
                : base(virtualPath)
            {
                Transforms.Add(new CopyrigthTransform());
                if (BundleTable.Bundles.UseCdn)
                {
                    Transforms.Add(new CdnTransform());
                }
            }

            public Bundle Include(string path)
            {
                return Include(ToVirtualPath(path), new CssTransform(Path));
            }
        }

        public class ASCJsBundle : Bundle
        {
            public bool UseCache
            {
                get;
                set;
            }


            public ASCJsBundle(string virtualPath)
                : base(virtualPath)
            {
                Transforms.Add(new CopyrigthTransform());
                Transforms.Add(new JsTransform());
                if (BundleTable.Bundles.UseCdn && !virtualPath.Contains(CLIENT_SCRIPT_VPATH))
                {
                    Transforms.Add(new CdnTransform());
                }
                Orderer = new NullOrderer();
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
