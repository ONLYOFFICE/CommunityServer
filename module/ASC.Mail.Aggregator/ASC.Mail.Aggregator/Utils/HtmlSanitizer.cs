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


/** 
Copyright (c) 2009 Open Lab, http://www.open-lab.com/ 
Permission is hereby granted, free of charge, to any person obtaining 
a copy of this software and associated documentation files (the 
"Software"), to deal in the Software without restriction, including 
without limitation the rights to use, copy, modify, merge, publish, 
distribute, sublicense, and/or sell copies of the Software, and to 
permit persons to whom the Software is furnished to do so, subject to 
the following conditions: 
 
The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software. 
 
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 Original: https://gist.github.com/ntulip/814428 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ASC.Mail.Aggregator.Utils
{
    public static class HtmlSanitizer
    {
        private static readonly Regex AllowedTags = new Regex("^(a|abbr|acronym|address|applet|area|article|"+
            "aside|audio|b|bdi|bdo|bgsound|blockquote|big|body|blink|br|canvas|caption|center|"+
            "cite|code|col|colgroup|comment|datalist|dd|del|details|dfn|dir|div|dl|dt|em|"+
            "figcaption|figure|font|footer|h1|h2|h3|h4|h5|h6|head|header|hgroup|hr|html|i|"+
            "img|ins|isindex|kbd|label|legend|li|map|marquee|mark|meta|meter|nav|nobr|noembed|"+
            "noframes|noscript|ol|optgroup|option|p|plaintext|pre|q|rp|rt|ruby|s|samp|section|"+
            "small|span|source|strike|strong|style|sub|summary|sup|table|tbody|td|tfoot|th|thead|"+
            "time|title|tr|tt|u|ul|var|video|wbr|xmp)$");

        private static readonly Regex StylePattern = new Regex("([^\\s^:]+)\\s*:\\s*([^;]+);?");  // color:red;
        private static readonly Regex StyleTagPattern = new Regex("(.*?){(.*?)}");  // color:red;
        private static readonly Regex UrlStylePattern = new Regex("(?i).*\\b\\s*url\\s*\\(([^)]*)\\)");  // url('....')
        private static readonly Regex EmbeddedImagesPattern = new Regex("data:([\\w/]+);(\\w+),([^\"^)\\s]+)");  // For embedded images <img src="data:image/gif;base64,R0lGODlhEAAOALMAAOazToeHh0tLS/7LZv/0jvb29t/f3//Ub//ge8WSLf/rhf/3kdbW1mxsbP//mf///yH5BAAAAAAALAAAAAAQAA4AAARe8L1Ekyky67QZ1hLnjM5UUde0ECwLJoExKcppV0aCcGCmTIHEIUEqjgaORCMxIC6e0CcguWw6aFjsVMkkIr7g77ZKPJjPZqIyd7sJAgVGoEGv2xsBxqNgYPj/gAwXEQA7" width="16" height="14" alt="���������� ������ �����"/>
        private static readonly Regex ForbiddenStylePattern = new Regex("(?:(expression|eval|javascript|vbscript))\\s*(\\(|:)");  // expression(....)
        private static bool _imagesAreBlocked;
        private static Dictionary<String, String> _styleClassesNames;


        public static String Sanitize(string html, bool loadImages)
        {
            bool imagesAreBlocked;
            return Sanitize(html, loadImages, out imagesAreBlocked);
        }

        public static String Sanitize(string html, bool loadImages, out bool imagesAreBlocked)
        {
            imagesAreBlocked = false;

            if (String.IsNullOrEmpty(html))
                return string.Empty;

            var doc = new HtmlDocument();

            doc.LoadHtml(html);

            // ReSharper disable UnusedVariable
            var encoding = doc.Encoding;
            // ReSharper restore UnusedVariable

            var baseTag = doc.DocumentNode.SelectSingleNode("//base");

            Uri baseHref = null;

            if (baseTag != null && baseTag.HasAttributes)
            {
                var href = baseTag.Attributes
                    .FirstOrDefault(attr =>
                        attr.Name == "href");

                if (href != null)
                {
                    try
                    {
                        var url = new Uri(href.Value);
                        if (url.Scheme == Uri.UriSchemeHttp ||
                            url.Scheme == Uri.UriSchemeHttps ||
                            url.Scheme == Uri.UriSchemeFtp)
                        {
                            baseHref = url;
                        }
                    }
                    catch (Exception)
                    { }
                }
            }

            var styleTag = doc.DocumentNode.SelectSingleNode("//style");

            _styleClassesNames = new Dictionary<String, String>();

            if (styleTag != null)
            {
                var classes = StyleTagPattern.Matches(styleTag.OuterHtml);

                var newValue = string.Empty;

                foreach (Match cssClass in classes)
                {
                    var val = cssClass.Groups[2].Value;

                    if (string.IsNullOrEmpty(val)) // Skip empty values
                        continue;

                    var classesNames = cssClass.Groups[1].Value
                                  .Split(new[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries)
                                  .ToList();

                    classesNames
                        .Where(s => s.StartsWith("."))
                        .Select(s => s.Remove(0, 1))
                        .ToList()
                        .ForEach(s =>
                        {
                            if (!_styleClassesNames.ContainsKey(s))
                                _styleClassesNames.Add(s, s.Insert(0, "x_"));
                        });

                    var cleanStyle = ParseStyles(val, baseHref, loadImages);

                    if (string.IsNullOrEmpty(newValue))
                        newValue = styleTag.OuterHtml;

                    newValue = newValue.Replace(val, cleanStyle);

                }

                if (_styleClassesNames.Count > 0)
                {
                    if (string.IsNullOrEmpty(newValue))
                        newValue = styleTag.OuterHtml;
                    // must change css classes 
                    _styleClassesNames
                        .ToList()
                        .ForEach(dict =>
                        {
                            if (newValue.IndexOf("." + dict.Key, StringComparison.Ordinal) > -1)
                                newValue = newValue.Replace("." + dict.Key, "." + dict.Value);
                        });
                }

                if (!string.IsNullOrEmpty(newValue))
                {
                    var newNode = HtmlNode.CreateNode(newValue);
                    styleTag.ParentNode.ReplaceChild(newNode.ParentNode, styleTag);
                }
            }

            var nodesToRemove = new List<HtmlNode>();

            _imagesAreBlocked = false;

            SanitizeNode(doc.DocumentNode, nodesToRemove, baseHref, loadImages);

            nodesToRemove
                .ForEach(node =>
                    node.Remove());

            imagesAreBlocked = _imagesAreBlocked;

            return doc.DocumentNode.OuterHtml;
        }

        private static void SanitizeNode(HtmlNode node, List<HtmlNode> nodesToRemove, Uri baseHref, bool loadImages)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    nodesToRemove.Add(node);
                    break;

                case HtmlNodeType.Document:
                    SanitizeDocument(node, nodesToRemove, baseHref, loadImages);
                    break;

                case HtmlNodeType.Text:
                    break;

                case HtmlNodeType.Element:
                    if (AllowedTags.Match(node.Name).Success)
                    {
                        if (node.HasAttributes)
                        {
                            var attributes = node.Attributes;

                            var attrebutesToDelete = new List<HtmlAttribute>();

                            foreach (var attribute in attributes)
                            {
                                SanitizeAttribute(attribute, attrebutesToDelete, baseHref, loadImages);
                            }

                            attrebutesToDelete
                                .ForEach(attr =>
                                         attr.Remove());
                        }

                        if (node.HasChildNodes)
                        {
                            SanitizeDocument(node, nodesToRemove, baseHref, loadImages);
                        }
                    }
                    else
                    {
                        nodesToRemove.Add(node);
                    }
                    break;
            }
        }

        private static void SanitizeAttribute(HtmlAttribute attribute, List<HtmlAttribute> attrebutesToDelete, 
                                              Uri baseHref, bool loadImages)
        {
            string newUrl;
            switch (attribute.Name)
            {
                case "style":
                    var cleanStyle = ParseStyles(attribute.Value, baseHref, loadImages);
                    if (string.IsNullOrEmpty(cleanStyle)) attrebutesToDelete.Add(attribute);
                    else attribute.Value = cleanStyle;
                    break;
                case "href":
                    try
                    {
                        var val = attribute.Value.StartsWith("//")
                                      ? "http:" + attribute.Value
                                      : attribute.Value;

                        var url = new Uri(val);
                        if (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps
                            && url.Scheme != Uri.UriSchemeMailto)
                        {
                            attrebutesToDelete.Add(attribute);
                        }
                        else
                        {
                            newUrl = FixBaseLink(attribute.Value, baseHref);
                            if (!string.IsNullOrEmpty(newUrl))
                                attribute.Value = newUrl;
                        }
                    }
                    catch
                    {
                        attrebutesToDelete.Add(attribute);
                    }
                    break;
                case "background":
                case "src":
                    if (!EmbeddedImagesPattern.Match(attribute.Value).Success)
                    {
                        try
                        {
                            var val = attribute.Value.StartsWith("//")
                                          ? "http:" + attribute.Value
                                          : attribute.Value;

                            var url = new Uri(val);
                            if (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps)
                            {
                                attrebutesToDelete.Add(attribute);
                                break;
                            }

                            newUrl = FixBaseLink(attribute.Value, baseHref);
                            if (!string.IsNullOrEmpty(newUrl))
                                attribute.Value = newUrl;
                        }
                        catch
                        {
                            attrebutesToDelete.Add(attribute);
                            break;
                        }
                    }

                    if (!loadImages)
                    {
                        attribute.Name = "tl_disabled_" + attribute.Name;
                        _imagesAreBlocked = true;
                    }
                    break;
                case "class":
                    // must change css classes 
                    var splittedClasses =
                        attribute.Value
                                 .Split(new[] { " " },
                                        StringSplitOptions.RemoveEmptyEntries)
                                 .ToList();

                    var newAttrValue = string.Empty;

                    splittedClasses
                        .ForEach(cls =>
                        {
                            var foundNewClassName =
                                _styleClassesNames
                                    .Select(t => t)
                                    .FirstOrDefault(t => t.Key.Equals(cls));

                            newAttrValue += foundNewClassName.Value ?? cls;
                        });

                    attribute.Value = newAttrValue;
                    break;
                default:
                    if (attribute.Name.StartsWith("on"))
                        attrebutesToDelete.Add(attribute); // skip all javascript events
                    else
                        attribute.Value = EncodeHtml(attribute.Value);
                    // by default encodeHtml all properies

                    break;
            }
        }

        private static void SanitizeDocument(HtmlNode node, List<HtmlNode> nodesToRemove, Uri baseHref, bool loadImages)
        {
            foreach (var subnode in node.ChildNodes)
            {
                SanitizeNode(subnode, nodesToRemove, baseHref, loadImages);
            }
        }

        private static string FixBaseLink(string foundedUrl, Uri baseHref)
        {
            if (foundedUrl.StartsWith("//"))
            {
                foundedUrl = foundedUrl.Insert(0, "http:");
            }

            if (baseHref != null &&
                !EmbeddedImagesPattern.Match(foundedUrl).Success &&
                !Uri.IsWellFormedUriString(foundedUrl, UriKind.Absolute))
            {
                Uri link;
                if (Uri.TryCreate(baseHref, foundedUrl, out link))
                {
                    return link.ToString();
                }
            }

            return string.Empty;
        }


        private static string ParseStyles(string styleString, Uri baseHref, bool loadImages)
        {
            var cleanStyle = string.Empty;
            var needChangeStyle = false;
            const string embedded_marker = @"http://marker-for-quick-parse.com/without-embedded-image-data"; // hack for right url parse if embedded image exists

            var embeddedImage = EmbeddedImagesPattern.Match(styleString);
            if (embeddedImage.Success)
            {
                styleString = styleString.Replace(embeddedImage.Value, embedded_marker);
            }

            var styles = StylePattern.Matches(styleString);

            foreach (Match style in styles)
            {
                var styleName = style.Groups[1].Value.ToLower();
                var styleValue = style.Groups[2].Value;

                // suppress invalid styles values 
                if (ForbiddenStylePattern.Match(styleValue).Success)
                {
                    needChangeStyle = true;
                    continue;
                }


                // check if valid url 
                var urlStyleMatcher = UrlStylePattern.Match(styleValue);
                if (!urlStyleMatcher.Success)
                {
                    cleanStyle = cleanStyle + styleName + ":" + EncodeHtml(styleValue) + ";";
                    continue;
                }

                try
                {
                    var urlString = urlStyleMatcher.Groups[1].Value.Replace("'", "").Replace("\"", "");
                    if (!EmbeddedImagesPattern.Match(urlString).Success)
                    {
                        var val = urlString.StartsWith("//")
                                      ? "http:" + urlString
                                      : urlString;
                        var uri = new Uri(val);
                        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                        {
                            needChangeStyle = true;
                            continue;
                        }
                    }

                    var newUrl = FixBaseLink(urlString, baseHref);
                    if (!string.IsNullOrEmpty(newUrl))
                    {
                        styleValue = styleValue.Replace(urlString, newUrl);
                        needChangeStyle = true;
                    }

                    if ((styleName == "background-image" ||
                         (styleName == "background" &&
                          styleValue.IndexOf("url(", StringComparison.Ordinal) != -1)) &&
                        !loadImages)
                    {
                        styleName = "tl_disabled_" + styleName;
                        _imagesAreBlocked = true;
                        needChangeStyle = true;
                    }
                }
                catch
                {
                    needChangeStyle = true;
                    continue;
                }

                cleanStyle = cleanStyle + styleName + ":" + EncodeHtml(styleValue) + ";";
            }

            if (cleanStyle.IndexOf(embedded_marker, StringComparison.Ordinal) != -1)
                cleanStyle = cleanStyle.Replace(embedded_marker, embeddedImage.Value);

            return needChangeStyle ? cleanStyle : styleString;
        }

        private static String EncodeHtml(String text)
        {
            return ConvertLineFeedToBr(HtmlEncodeApexesAndTags(text ?? ""));
        }

        private static String HtmlEncodeApexesAndTags(String text)
        {
            return HtmlEncodeTag(HtmlEncodeApexes(text));
        }

        private static String HtmlEncodeApexes(String text)
        {
            return text != null ? ReplaceAllNoRegex(text, new[] {"\"", "'"}, new[] {"&quot;", "&#39;"}) : null;
        }

        private static String HtmlEncodeTag(String text)
        {
            return text != null ? ReplaceAllNoRegex(text, new[] {"<", ">"}, new[] {"&lt;", "&gt;"}) : null;
        }


        private static String ConvertLineFeedToBr(String text)
        {
            return text != null ? ReplaceAllNoRegex(text, new[] { "\n", "\f", "\r" }, new[] { "<br>", "<br>", " " }) : null;
        }

        private static String ReplaceAllNoRegex(String source, String[] searches, String[] replaces)
        {
            int k;
            var tmp = source;
            for (k = 0; k < searches.Length; k++)
            {
                tmp = source.Replace(searches[k], replaces[k]);
            }
            return tmp;
        }

        private static readonly Regex RemoveHtml = new Regex("<html>(.*)</html>", RegexOptions.Singleline);
        private static readonly Regex RemoveHead = new Regex("<head>.*?</head>", RegexOptions.Singleline);
        private static readonly Regex RemoveBody = new Regex("<body(.*)</body>", RegexOptions.Singleline);

        public static string SanitizeHtmlForEditor(string inHtml)
        {
            var res = RemoveHtml.Replace(inHtml, "$1");
            res = RemoveHead.Replace(res, "", 1);
            res = RemoveBody.Replace(res, "<div$1</div>");
            return res.Trim();
        }
    }
}