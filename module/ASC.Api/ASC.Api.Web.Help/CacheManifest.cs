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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ASC.Api.Web.Help
{
    public class CacheManifest
    {
        private const string CacheHeader = "CACHE MANIFEST";
        private const string CacheSection = "CACHE:";
        private const string NetworkSection = "NETWORK:";
        private const string FallbackSection = "FALLBACK:";

        private readonly Dictionary<string, HashSet<string>> _mainfestSections = new Dictionary<string, HashSet<string>>();

        private DateTime _lastChange;

        public class StringComparer:IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return string.Equals(x, y, StringComparison.Ordinal);
            }

            public int GetHashCode(string obj)
            {
                return obj.GetHashCode();
            }
        }

        public CacheManifest()
        {
            _lastChange = DateTime.UtcNow;
            _mainfestSections.Add(CacheSection, new HashSet<string>(new StringComparer()));
            _mainfestSections.Add(NetworkSection, new HashSet<string>(new StringComparer()) { "*" });
            _mainfestSections.Add(FallbackSection, new HashSet<string>(new StringComparer()));
        }

        public IEnumerable<IGrouping<string, HashSet<string>>> GetRegisteredCacheSections()
        {
            return _mainfestSections.GroupBy(key=>key.Key,value=>value.Value);
        }

        public void AddFallback(Uri uriOnline, Uri uriOffline)
        {
            AddToManifest(FallbackSection, uriOnline, uriOffline);
        }

        public void AddOnline(Uri uri)
        {
            AddToManifest(NetworkSection, uri);
        }

        public void AddCached(Uri uri)
        {
            AddToManifest(CacheSection,uri);
        }

        public void AddServerFile(HttpContextBase context,string virtualPathToFile)
        {
            var physicalPath = context.Server.MapPath(virtualPathToFile);
            var appPhysicalRoot = context.Server.MapPath("~/");
            var url = VirtualPathUtility.ToAbsolute("~/" +
                                              physicalPath.Substring(appPhysicalRoot.Length).Replace("\\", "/").TrimStart('/'));
            AddCached(new Uri(url,UriKind.Relative));
        }

        public void AddServerFolder(HttpContextBase context,string virtualPath,string mask)
        {
            var physicalPath = context.Server.MapPath(virtualPath);
            var appPhysicalRoot = context.Server.MapPath("~/");
            var files = Directory.GetFiles(physicalPath, mask, SearchOption.TopDirectoryOnly).Select(x=>VirtualPathUtility.ToAbsolute("~/"+x.Substring(appPhysicalRoot.Length).Replace("\\","/").TrimStart('/')));
            foreach (var file in files)
            {
                AddCached(new Uri(file,UriKind.Relative));
            }
        }


        protected void AddToManifest(string section,params Uri[] uri)
        {
            var urls = string.Join(" ", uri.Select(x => x.ToString()).ToArray());
            var changed = _mainfestSections[section].Add(urls);
            if (changed)
            {
                _lastChange = DateTime.UtcNow;
            }
        }

        public void Write(TextWriter output)
        {
            output.WriteLine(CacheHeader);
            output.WriteLine("#{0}", _lastChange.ToString("R"));
            output.WriteLine();
            foreach (var mainfestSection in _mainfestSections)
            {
                if (mainfestSection.Value.Any())
                {
                    output.WriteLine(mainfestSection.Key);
                    foreach (var url in mainfestSection.Value)
                    {
                        output.WriteLine(url);
                    }
                    output.WriteLine();
                }
            }
        }
    }
}