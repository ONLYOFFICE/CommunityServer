/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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