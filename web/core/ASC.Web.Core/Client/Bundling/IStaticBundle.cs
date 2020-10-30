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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Web.Core.Client.HttpHandlers;

namespace ASC.Web.Core.Client.Bundling
{
    public interface IStaticBundle
    {
        ScriptBundleData GetStaticJavaScript();
        StyleBundleData GetStaticStyleSheet();
    }

    public class BundleData
    {
        private readonly HashSet<string> source;
        internal string CategoryName { get; private set; }
        internal string FileName { get; set; }
        internal readonly Func<string, bool, string> GetLink;

        private readonly string sourceType;
        private readonly string ext;

        protected BundleData(string fileName, string categoryName)
        {
            source = new HashSet<string>();
            CategoryName = GetCategory(categoryName);
            FileName = fileName;
        }

        protected BundleData(string sourceType, string ext, Func<string, bool, string> getLink, string fileName, string categoryName) : this(fileName, categoryName)
        {
            this.sourceType = sourceType;
            this.ext = ext;
            GetLink = getLink;
        }

        public BundleData AddSource(Func<string, string> converter, params string[] src)
        {
            foreach (var s in src)
            {
                source.Add(converter(s));
            }

            return this;
        }

        public IEnumerable<string> GetSource()
        {
            return source;
        }

        public string GetStorageVirtualPath(string resetCacheKey)
        {
            return string.Format("{0}/{1}/{2}-{3}{4}", CategoryName, sourceType, FileName, GetHash(resetCacheKey), ext);
        }

        public string GetBundleVirtualPath(string bundleVPath, string resetCacheKey)
        {
            return string.Format("~{0}{1}-{2}{3}", bundleVPath, CategoryName, GetHash(resetCacheKey), ext);
        }

        internal virtual BundleHelper.ASCBundle CreateAscBundle(string virtualPath)
        {
            return null;
        }

        private static string GetCategory(string category)
        {
            if (!string.IsNullOrEmpty(category)) return category;

            category = "common";
            if (HttpContext.Current != null && HttpContext.Current.Request.Url != null && !string.IsNullOrEmpty(HttpContext.Current.Request.Url.AbsolutePath))
            {
                var matches = Regex.Match(HttpContext.Current.Request.Url.AbsolutePath, "(products|addons)/(\\w+)/?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (matches.Success && 2 < matches.Groups.Count && matches.Groups[2].Success)
                {
                    category = matches.Groups[2].Value;
                }
            }
            return category;
        }

        private string GetHash(string resetCacheKey)
        {
            return HttpServerUtility.UrlTokenEncode(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(string.Join(",", source) + resetCacheKey)));
        }
    }

    public class ScriptBundleData : BundleData
    {
        public ScriptBundleData(string fileName, string categoryName) : base("javascript", ".js", BundleHelper.GetJavascriptLink, fileName, categoryName) { }

        internal override BundleHelper.ASCBundle CreateAscBundle(string virtualPath)
        {
            return new BundleHelper.ASCJsBundle(virtualPath);
        }

        public ScriptBundleData AddSource(Func<string, string> converter, ClientScriptTemplate clientScriptTemplate)
        {
            AddSource(converter, clientScriptTemplate.GetLinks());
            return this;
        }
    }

    public class StyleBundleData : BundleData
    {
        public StyleBundleData(string fileName, string categoryName) : base("css", ".css", BundleHelper.GetCssLink, fileName, categoryName) { }

        internal override BundleHelper.ASCBundle CreateAscBundle(string virtualPath)
        {
            return new BundleHelper.ASCStyleBundle(virtualPath);
        }
    }
}
