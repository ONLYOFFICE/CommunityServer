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


using dotless.Core;
using dotless.Core.configuration;
using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Optimization;

namespace ASC.Web.Core.Client.Bundling
{
    class CssTransform : IItemTransform
    {
        private readonly string bundlepath;


        public CssTransform(string bundlepath)
        {
            this.bundlepath = bundlepath;
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
            return input;
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
                path = VirtualPathUtility.MakeRelative(bundlepath, path).ToLowerInvariant();
                return m.Value.Replace(m.Groups[1].Value, "\"" + path + "\"");
            }
            return m.Value;
        }
    }
}