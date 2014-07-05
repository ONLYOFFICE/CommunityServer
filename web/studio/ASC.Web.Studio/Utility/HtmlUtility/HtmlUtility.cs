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
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Users;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace ASC.Web.Studio.Utility.HtmlUtility
{
    public class HtmlUtility : HtmlUtil
    {
        private const RegexOptions MainOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
        private static readonly Regex Worder = new Regex(@"\S+", MainOptions);

        public static string GetPreview(string html, string replacmentHtml, Guid productID)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(string.Format("<html>{0}</html>", htmlTags.Replace(html, string.Empty)));
            var nodes = doc.DocumentNode.SelectNodes("//div[translate(@class,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='asccut']");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var newNode = doc.CreateElement("div");
                    var styleAttr = doc.CreateAttribute("style");
                    styleAttr.Value = "display:inline;";
                    newNode.Attributes.Append(styleAttr);
                    newNode.InnerHtml = replacmentHtml ?? string.Empty;
                    node.ParentNode.ReplaceChild(newNode, node);
                }
            }
            ProcessCustomTags(doc, productID);
            return htmlTags.Replace(doc.DocumentNode.InnerHtml, string.Empty);
        }

        public static string GetFull(string html, bool removeAsccut = true)
        {
            return GetFull(html, Guid.Empty, removeAsccut);
        }

        public static string GetFull(string html, Guid productID, bool removeAsccut = true)
        {
            html = html ?? string.Empty;
            var doc = new HtmlDocument();
            doc.LoadHtml(string.Format("<html>{0}</html>", htmlTags.Replace(html, string.Empty)));
            if (removeAsccut)
            {
                var nodes = doc.DocumentNode.SelectNodes("//div[translate(@class,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='asccut']");
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        node.Attributes.Remove("class");
                        var styleAttr = doc.CreateAttribute("style");
                        styleAttr.Value = "display:inline;";
                        node.Attributes.Append(styleAttr);
                    }
                }
            }

            ProcessCustomTags(doc, productID);
            return htmlTags.Replace(doc.DocumentNode.InnerHtml, string.Empty);
        }

        #region SearchTextHighlight

        /// <summary>
        /// The function highlight all words in htmlText by searchText.
        /// </summary>
        /// <param name="searchText">the space separated string</param>
        /// <param name="htmlText">html for highlight</param>
        /// <returns>highlighted html</returns>
        public static string SearchTextHighlight(string searchText, string htmlText)
        {
            return SearchTextHighlight(searchText, htmlText, Guid.Empty);
        }

        /// <summary>
        /// The function highlight all words in htmlText by searchText.
        /// </summary>
        /// <param name="searchText">the space separated string</param>
        /// <param name="htmlText">html for highlight</param>
        /// <param name="productId">current ProfuctId</param>
        /// <returns>highlighted html</returns>
        public static string SearchTextHighlight(string searchText, string htmlText, Guid productId)
        {
            return SearchTextHighlight(searchText, htmlText, productId, true);
        }

        /// <summary>
        /// The function highlight all words in htmlText by searchText.
        /// </summary>
        /// <param name="searchText">the space separated string</param>
        /// <param name="htmlText">html for highlight</param>
        /// <param name="prepareHtml">an input html to be prepare (GetFull)</param>
        /// <returns>highlighted html</returns>
        public static string SearchTextHighlight(string searchText, string htmlText, bool prepareHtml)
        {
            return SearchTextHighlight(searchText, htmlText, Guid.Empty, prepareHtml);
        }

        /// <summary>
        /// The function highlight all words in htmlText by searchText.
        /// </summary>
        /// <param name="searchText">the space separated string</param>
        /// <param name="htmlText">html for highlight</param>
        /// <param name="productId">current ProfuctId</param>
        /// <param name="className">custom css class name</param>
        /// <returns>highlighted html</returns>
        public static string SearchTextHighlight(string searchText, string htmlText, Guid productId, string className)
        {
            return SearchTextHighlight(searchText, htmlText, productId, className, true);
        }

        /// <summary>
        /// The function highlight all words in htmlText by searchText.
        /// </summary>
        /// <param name="searchText">the space separated string</param>
        /// <param name="htmlText">html for highlight</param>
        /// <param name="productId">current ProfuctId</param>
        /// <param name="prepareHtml">an input html to be prepare (GetFull)</param>
        /// <returns>highlighted html</returns>
        public static string SearchTextHighlight(string searchText, string htmlText, Guid productId, bool prepareHtml)
        {
            return SearchTextHighlight(searchText, htmlText, productId, "searchTextHighlight", prepareHtml);
        }

        /// <summary>
        /// The function highlight all words in htmlText by searchText.
        /// </summary>
        /// <param name="search">the space separated string</param>
        /// <param name="html">html for highlight</param>
        /// <param name="productId">current ProfuctId</param>
        /// <param name="className">custom css class name</param>
        /// <param name="prepare">an input html to be prepare (GetFull)</param>
        /// <returns>highlighted html</returns>
        public static string SearchTextHighlight(string search, string html, Guid productId, string className, bool prepare)
        {
            if (string.IsNullOrEmpty(search) || string.IsNullOrEmpty(html)) return html;
            if (prepare) html = GetFull(html, productId);

            var regexpstr = Worder.Matches(search).Cast<Match>().Select(m => m.Value).Distinct().Aggregate((r, n) => r + "|" + n);
            var wordsFinder = new Regex(Regex.Escape(regexpstr), MainOptions | RegexOptions.Multiline);
            return wordsFinder.Replace(html, m => string.Format("<span class='{0}'>{1}</span>", className, m.Value));
        }

        #endregion

        private static string GetLanguageAttrValue(HtmlNode node)
        {
            var attr = node.Attributes["lang"];
            if (attr != null && !string.IsNullOrEmpty(attr.Value))
            {
                return attr.Value;
            }
            return string.Empty;
        }

        private static LangType GetLanguage(HtmlNode node)
        {
            var result = LangType.Unknown;

            switch (GetLanguageAttrValue(node).ToLower())
            {
                case "c":
                    result = LangType.C;
                    break;
                case "cpp":
                case "c++":
                    result = LangType.CPP;
                    break;
                case "csharp":
                case "c#":
                case "cs":
                    result = LangType.CS;
                    break;
                case "asp":
                    result = LangType.Asp;
                    break;
                case "html":
                case "htm":
                    result = LangType.Html;
                    break;
                case "xml":
                    result = LangType.Xml;
                    break;
                case "js":
                case "jsript":
                case "javascript":
                    result = LangType.JS;
                    break;
                case "sql":
                case "tsql":
                    result = LangType.TSql;
                    break;
                case "vb":
                case "vbnet":
                    result = LangType.VB;
                    break;
            }

            return result;
        }

        private static void ProcessCustomTags(HtmlDocument doc, Guid productID)
        {
            ProcessAscUserTag(doc, productID);
            ProcessCodeTags(doc);
            ProcessExternalLinks(doc);
            ProcessScriptTag(doc);
            ProcessMaliciousAttributes(doc);
            ProcessZoomImages(doc);
        }

        private static readonly List<string> BlockedAttrs = new List<string>
            {
                "onload",
                "onunload",
                "onclick",
                "ondblclick",
                "onmousedown",
                "onmouseup",
                "onmouseover",
                "onmousemove",
                "onmouseout",
                "onfocus",
                "onblur",
                "onkeypress",
                "onkeydown",
                "onkeyup",
                "onsubmit",
                "onreset",
                "onselect",
                "onchange"
            };

        private static void ProcessMaliciousAttributes(HtmlDocument doc)
        {
            foreach (var node in doc.DocumentNode.SelectNodes("//*"))
            {
                var toRemove = node.Attributes
                                   .Where(htmlAttribute => BlockedAttrs.Contains(htmlAttribute.Name.ToLowerInvariant())
                                                           ||
                                                           htmlAttribute.Value.StartsWith("javascript", StringComparison.OrdinalIgnoreCase)
                                                           ||
                                                           htmlAttribute.Value.StartsWith("data", StringComparison.OrdinalIgnoreCase)
                                                           ||
                                                           htmlAttribute.Value.StartsWith("vbscript", StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                foreach (var htmlAttribute in toRemove)
                {
                    node.Attributes.Remove(htmlAttribute);
                }
            }
        }

        private static readonly Regex RxNumeric = new Regex(@"^[0-9]+$", MainOptions);

        private static void ProcessZoomImages(HtmlDocument doc)
        {
            var nodes = doc.DocumentNode.SelectNodes("//img[@_zoom]");

            if (nodes == null) return;
            foreach (var node in nodes)
            {
                if (node.ParentNode != null && (node.ParentNode.Name ?? "").ToLower() == "a") continue;

                var srcAttribute = node.Attributes["src"];
                if (srcAttribute == null || string.IsNullOrEmpty(srcAttribute.Value)) continue;

                var zoomAttribute = node.Attributes["_zoom"];
                if (zoomAttribute == null || string.IsNullOrEmpty(zoomAttribute.Value)) continue;

                var borderAttribute = node.Attributes["border"];
                if (borderAttribute == null)
                {
                    borderAttribute = doc.CreateAttribute("border");
                    node.Attributes.Append(borderAttribute);
                }
                borderAttribute.Value = "0";

                var imgSrc = srcAttribute.Value;

                if (!RxNumeric.IsMatch(zoomAttribute.Value))
                {
                    imgSrc = zoomAttribute.Value;
                }

                if (node.ParentNode != null)
                {
                    var hrefNode = doc.CreateElement("a");

                    var hrefAttribute = doc.CreateAttribute("href");
                    hrefAttribute.Value = imgSrc;
                    hrefNode.Attributes.Append(hrefAttribute);

                    hrefAttribute = doc.CreateAttribute("class");
                    hrefAttribute.Value = "screenzoom";
                    hrefNode.Attributes.Append(hrefAttribute);

                    string title = null;
                    var titleAttribute = node.Attributes["title"];
                    if (titleAttribute != null)
                    {
                        title = titleAttribute.Value;
                    }
                    else
                    {
                        var altAttribute = node.Attributes["alt"];
                        if (altAttribute != null)
                        {
                            title = altAttribute.Value;
                        }
                    }
                    if (!string.IsNullOrEmpty(title))
                    {
                        hrefAttribute = doc.CreateAttribute("title");
                        hrefAttribute.Value = title;
                        hrefNode.Attributes.Append(hrefAttribute);
                    }

                    node.ParentNode.ReplaceChild(hrefNode, node);
                    hrefNode.AppendChild(node);
                }
            }
        }

        private static void ProcessScriptTag(HtmlDocument doc)
        {
            var nodes = doc.DocumentNode.SelectNodes("//script");

            if (nodes == null || nodes.Count == 0)
                return;

            foreach (var node in nodes.Where(node => node.ParentNode != null))
            {
                node.ParentNode.RemoveChild(node);
            }
        }

        private static void ProcessAscUserTag(HtmlDocument doc, Guid productID)
        {
            var nodes = doc.DocumentNode.SelectNodes("//div[@__ascuser]");
            if (nodes == null || nodes.Count == 0) return;

            foreach (var node in nodes)
            {
                var userId = new Guid(node.Attributes["__ascuser"].Value);
                node.Attributes.RemoveAll();
                var styleAttr = doc.CreateAttribute("style");
                styleAttr.Value = "display:inline;";
                node.Attributes.Append(styleAttr);
                node.InnerHtml = CoreContext.UserManager.GetUsers(userId).RenderProfileLinkBase(productID);
            }
        }

        private static void ProcessCodeTags(HtmlDocument doc)
        {
            var scripts = doc.DocumentNode.SelectNodes("//code");
            if (scripts != null)
            {
                foreach (var node in scripts)
                {
                    var textNode = doc.CreateTextNode(Highlight.HighlightToHTML(node.InnerHtml, GetLanguage(node), true).Replace(@"class=""codestyle""", string.Format(@"class=""codestyle"" _wikiCodeStyle=""{0}""", GetLanguageAttrValue(node))));
                    node.ParentNode.ReplaceChild(textNode, node);
                }
            }
        }

        private static void ProcessExternalLinks(HtmlDocument doc)
        {
            var links = doc.DocumentNode.SelectNodes("//a");
            if (links == null) return;

            var con = HttpContext.Current;
            var internalHost = con.Request.GetUrlRewriter().Host;
            if ((con.Request.GetUrlRewriter().Port != 80 && con.Request.GetUrlRewriter().Scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase))
                || (con.Request.GetUrlRewriter().Port != 443 && con.Request.GetUrlRewriter().Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase)))
            {
                internalHost = string.Format(@"^{2}:\/\/{0}:{1}", internalHost, con.Request.GetUrlRewriter().Port, con.Request.GetUrlRewriter().Scheme);
            }
            else
            {
                internalHost = string.Format(@"^{2}:\/\/{0}(:{1})?", internalHost, con.Request.GetUrlRewriter().Port, con.Request.GetUrlRewriter().Scheme);
            }

            var rxInternalHost = new Regex(internalHost, RegexOptions.Compiled | RegexOptions.CultureInvariant);

            foreach (var node in links)
            {
                ProcessExternalLink(node, rxInternalHost);
            }
        }

        private static void ProcessExternalLink(HtmlNode node, Regex rxIntLink)
        {
            var attrHref = node.Attributes["href"];
            if (attrHref == null) return;

            if (!rxIntLink.IsMatch(attrHref.Value))
            {
                var attrTarg = node.Attributes["target"];
                if (attrTarg == null)
                {
                    attrTarg = node.OwnerDocument.CreateAttribute("target");
                    node.Attributes.Append(attrTarg);
                }

                attrTarg.Value = "_blank";
            }
        }
    }
}