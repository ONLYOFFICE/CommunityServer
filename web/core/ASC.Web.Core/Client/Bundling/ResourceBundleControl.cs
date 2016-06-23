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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Core.Client.Bundling
{
    public class ResourceBundleControl : UserControl
    {
        public List<string> Scripts { get; private set; }
        public List<string> Styles { get; private set; }
        public String CategoryName { get; set; }


        public ResourceBundleControl()
        {
            Scripts = new List<string>();
            Styles = new List<string>();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (ClientSettings.BundlingEnabled)
            {
                writer.Write(BundleHtml());
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

        private string BundleHtml()
        {
            var result = new StringBuilder();
            var hash = "";

            if (Scripts.Any())
            {
                hash = GetHash(Scripts);
            } else if (Styles.Any())
            {
                hash = GetHash(Styles);
            }

            var path = string.Format("~{0}{1}-{2}", BundleHelper.BUNDLE_VPATH, GetCategory(CategoryName), hash);
            var pathcss = path + ".css";
            var pathjs = path + ".js";

            var bundlecss = BundleHelper.GetCssBundle(pathcss);
            var bundlejs = BundleHelper.GetJsBundle(pathjs);

            if (bundlecss == null && bundlejs == null)
            {
                if (Styles.Any())
                {
                    bundlecss = BundleHelper.CssBundle(pathcss);
                    foreach (var style in Styles)
                    {
                        bundlecss.Include(style);
                    }
                    BundleHelper.AddBundle(bundlecss);
                }

                if (Scripts.Any())
                {
                    bundlejs = BundleHelper.JsBundle(pathjs);
                    foreach (var script in Scripts)
                    {
                        bundlejs.Include(script, true);
                    }
                    BundleHelper.AddBundle(bundlejs);
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

        private string GetHash(IEnumerable<string> hashSet)
        {
            return HttpServerUtility.UrlTokenEncode(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(string.Join(",", hashSet))));
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