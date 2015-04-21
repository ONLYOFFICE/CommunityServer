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


using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Core.Client.Bundling
{
    public class ResourceBundleControl : UserControl
    {
        public HashSet<string> Scripts { get; private set; }
        public HashSet<string> Styles { get; private set; }
        public String CategoryName { get; set; }


        public ResourceBundleControl()
        {
            Scripts = new HashSet<string>();
            Styles = new HashSet<string>();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (ClientSettings.BundlingEnabled)
            {
                using (var html = new StringWriter())
                using (var htmlWriter = new HtmlTextWriter(html))
                {
                    Write(htmlWriter);
                    writer.Write(BundleHtml(CategoryName, html.ToString()));
                }
            }
            else
            {
                Write(writer);
            }
        }

        private void Write(HtmlTextWriter writer)
        {
            base.Render(writer);
            foreach (var s in Scripts)
            {
                writer.WriteLine(BundleHelper.HtmlScript(ClientSettings.BundlingEnabled ? s : VirtualPathUtility.ToAbsolute(s), false, true));
            }
            foreach (var s in Styles)
            {
                writer.WriteLine(BundleHelper.HtmlLink(ClientSettings.BundlingEnabled ? s : VirtualPathUtility.ToAbsolute(s), false));
            }
        }

        private string BundleHtml(string category, string html)
        {
            var result = new StringBuilder();

            var hash = HttpServerUtility.UrlTokenEncode(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(html)));
            var path = string.Format("~{0}{1}-{2}", BundleHelper.BUNDLE_VPATH, GetCategory(category), hash);
            var pathcss = path + ".css";
            var pathjs = path + ".js";

            var bundlecss = BundleHelper.GetCssBundle(pathcss);
            var bundlejs = BundleHelper.GetJsBundle(pathjs);

            if (bundlecss == null && bundlejs == null)
            {
                var document = new HtmlDocument();
                document.LoadHtml(html);

                if (bundlecss == null)
                {
                    var styles = document.DocumentNode.SelectNodes("/style | /link[@rel='stylesheet'] | /link[@rel='stylesheet/less']");
                    if (styles != null && 0 < styles.Count)
                    {
                        bundlecss = BundleHelper.CssBundle(pathcss);
                        foreach (var style in styles)
                        {
                            if (style.Name == "style" && !string.IsNullOrEmpty(style.InnerHtml))
                            {
                                throw new NotSupportedException("Embedded styles not supported.");
                            }
                            bundlecss.Include(style.Attributes["href"].Value);
                        }
                        BundleHelper.AddBundle(bundlecss);
                    }
                }

                if (bundlejs == null)
                {
                    var scripts = document.DocumentNode.SelectNodes("/script");
                    if (scripts != null && 0 < scripts.Count)
                    {
                        bundlejs = BundleHelper.JsBundle(pathjs);
                        foreach (var script in scripts)
                        {
                            if (script.Attributes["src"] == null && !string.IsNullOrEmpty(script.InnerHtml))
                            {
                                throw new NotSupportedException("Embedded scripts not supported.");
                            }
                            bundlejs.Include(script.Attributes["src"].Value, script.Attributes["notobfuscate"] == null);
                        }
                        BundleHelper.AddBundle(bundlejs);
                    }
                }
            }

            if (bundlecss != null)
            {
                result.AppendLine(BundleHelper.HtmlLink(pathcss));
            }
            if (bundlejs != null)
            {
                result.AppendLine(BundleHelper.HtmlScript(pathjs));
            }
            return result.ToString();
        }

        private string GetCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                category = "common";
                if (HttpContext.Current.Request.Url != null && !string.IsNullOrEmpty(HttpContext.Current.Request.Url.AbsolutePath))
                {
                    var matches = Regex.Match(HttpContext.Current.Request.Url.AbsolutePath, "(products|addons)/(\\w+)/?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    if (matches.Success && 2 < matches.Groups.Count && matches.Groups[2].Success)
                    {
                        category = matches.Groups[2].Value;
                    }
                }
            }
            return category;
        }
    }
}