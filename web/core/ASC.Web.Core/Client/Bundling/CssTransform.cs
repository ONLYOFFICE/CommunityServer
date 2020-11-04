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


using dotless.Core;
using dotless.Core.configuration;
using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Optimization;
using Microsoft.Ajax.Utilities;

namespace ASC.Web.Core.Client.Bundling
{
    class CssTransform : IItemTransform, IBundleTransform
    {
        private readonly string bundlepath;
        private readonly bool minify;

        public CssTransform(): this("", false) { }

        public CssTransform(string bundlepath, bool minify = true)
        {
            this.bundlepath = bundlepath;
            this.minify = minify;
        }


        public string Process(string includedVirtualPath, string input)
        {
            if (VirtualPathUtility.GetExtension(includedVirtualPath) == ".less")
            {
                var cfg = DotlessConfiguration.GetDefaultWeb();
                cfg.Web = true;
                cfg.MinifyOutput = BundleTable.EnableOptimizations;
                cfg.MapPathsToWeb = true;
                cfg.CacheEnabled = false;

                var importRegex = new Regex(@"@import (.*?);", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
                input = importRegex.Replace(input, m => ResolveImportLessPath(includedVirtualPath, m));
                input = LessWeb.Parse(input, cfg);
            }

            var urlRegex = new Regex(@"url\((.*?)\)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            input = urlRegex.Replace(input, m => ResolveUrlPath(includedVirtualPath, m));
            return minify ?  new Minifier().MinifyStyleSheet(input) : input;
        }

        private string ResolveImportLessPath(string virtualPath, Match m)
        {
            if (m.Success && m.Groups[1].Success)
            {
                var path = m.Groups[1].Value.Trim().Trim('\'', '"');
                var vpath = VirtualPathUtility.Combine(VirtualPathUtility.GetDirectory(virtualPath), path);
                return m.Value.Replace(m.Groups[1].Value, "\"" + vpath + "\"");
            }
            return m.Value;
        }

        private string ResolveUrlPath(string virtualPath, Match m)
        {
            if (m.Success && m.Groups[1].Success)
            {
                var path = m.Groups[1].Value.Trim().Trim('\'', '"');
                if (path.StartsWith("data:", StringComparison.InvariantCultureIgnoreCase))
                {
                    return m.Value;
                }
                if (Uri.IsWellFormedUriString(path, UriKind.Relative))
                {
                    path = VirtualPathUtility.Combine(VirtualPathUtility.GetDirectory(virtualPath), path);
                }
                else if (path.StartsWith(Uri.UriSchemeHttp))
                {
                    path = new Uri(path).PathAndQuery;
                }
                if (HostingEnvironment.ApplicationVirtualPath != "/" && path.StartsWith(HostingEnvironment.ApplicationVirtualPath))
                {
                    path = path.Substring(HostingEnvironment.ApplicationVirtualPath.Length);
                }
                if (path.StartsWith("/"))
                {
                    path = "~" + path;
                }
                path = VirtualPathUtility.MakeRelative(bundlepath, path);
                return m.Value.Replace(m.Groups[1].Value, "\"" + path + "\"");
            }
            return m.Value;
        }

        public void Process(BundleContext context, BundleResponse response)
        {
            response.ContentType = "text/css";
        }
    }
}