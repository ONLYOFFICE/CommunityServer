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