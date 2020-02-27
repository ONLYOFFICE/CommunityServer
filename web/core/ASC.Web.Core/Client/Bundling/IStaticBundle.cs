/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
