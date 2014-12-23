/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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

namespace ASC.Mail.Aggregator{

    public static class HtmlSanitizer
    {
        private static readonly Regex _allowedTags = new Regex("^(a|abbr|acronym|address|applet|area|article|"+
            "aside|audio|b|bdi|bdo|bgsound|blockquote|big|body|blink|br|canvas|caption|center|"+
            "cite|code|col|colgroup|comment|datalist|dd|del|details|dfn|dir|div|dl|dt|em|"+
            "figcaption|figure|font|footer|h1|h2|h3|h4|h5|h6|head|header|hgroup|hr|html|i|"+
            "img|ins|isindex|kbd|label|legend|li|map|marquee|mark|meta|meter|nav|nobr|noembed|"+
            "noframes|noscript|ol|optgroup|option|p|plaintext|pre|q|rp|rt|ruby|s|samp|section|"+
            "small|span|source|strike|strong|style|sub|summary|sup|table|tbody|td|tfoot|th|thead|"+
            "time|title|tr|tt|u|ul|var|video|wbr|xmp)$");

        private static readonly Regex _stylePattern = new Regex("([^\\s^:]+)\\s*:\\s*([^;]+);?");  // color:red;
        private static readonly Regex _styleTagPattern = new Regex("(.*?){(.*?)}");  // color:red;
        private static readonly Regex _urlStylePattern = new Regex("(?i).*\\b\\s*url\\s*\\(([^)]*)\\)");  // url('....')
        private static readonly Regex _embeddedImagesPattern = new Regex("data:([\\w/]+);(\\w+),([^\"^)\\s]+)");  // For embedded images <img src="data:image/gif;base64,R0lGODlhEAAOALMAAOazToeHh0tLS/7LZv/0jvb29t/f3//Ub//ge8WSLf/rhf/3kdbW1mxsbP//mf///yH5BAAAAAAALAAAAAAQAA4AAARe8L1Ekyky67QZ1hLnjM5UUde0ECwLJoExKcppV0aCcGCmTIHEIUEqjgaORCMxIC6e0CcguWw6aFjsVMkkIr7g77ZKPJjPZqIyd7sJAgVGoEGv2xsBxqNgYPj/gAwXEQA7" width="16" height="14" alt="���������� ������ �����"/>
        private static readonly Regex _forbiddenStylePattern = new Regex("(?:(expression|eval|javascript|vbscript))\\s*(\\(|:)");  // expression(....)
        private static bool _imagesAreBlocked;
        private static Dictionary<String, String> _styleClassesNames;


        public static String Sanitize(string html, bool load_images)
        {
            bool images_are_blocked;
            return Sanitize(html, load_images, out images_are_blocked);
        }


        public static String Sanitize(string html, bool load_images, out bool images_are_blocked)
        {
            images_are_blocked = false;

            if (String.IsNullOrEmpty(html))
                return string.Empty;

            var doc = new HtmlDocument();

            doc.LoadHtml(html);

            // ReSharper disable UnusedVariable
            var encoding = doc.Encoding;
            // ReSharper restore UnusedVariable

            var base_tag = doc.DocumentNode.SelectSingleNode("//base");

            Uri base_href = null;

            if (base_tag != null && base_tag.HasAttributes)
            {
                var href = base_tag.Attributes
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
                            base_href = url;
                        }
                    }
                    catch { }
                }
            }

            var style_tag = doc.DocumentNode.SelectSingleNode("//style");

            _styleClassesNames = new Dictionary<String, String>();

            if (style_tag != null)
            {
                var classes = _styleTagPattern.Matches(style_tag.OuterHtml);

                var new_value = string.Empty;

                foreach (Match css_class in classes)
                {
                    var val = css_class.Groups[2].Value;

                    if (string.IsNullOrEmpty(val)) // Skip empty values
                        continue;

                    var classes_names = css_class.Groups[1].Value
                                  .Split(new[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries)
                                  .ToList();

                    classes_names
                        .Where(s => s.StartsWith("."))
                        .Select(s => s.Remove(0, 1))
                        .ToList()
                        .ForEach(s =>
                        {
                            if (!_styleClassesNames.ContainsKey(s))
                                _styleClassesNames.Add(s, s.Insert(0, "x_"));
                        });

                    var clean_style = ParseStyles(val, base_href, load_images);

                    if (string.IsNullOrEmpty(new_value))
                        new_value = style_tag.OuterHtml;

                    new_value = new_value.Replace(val, clean_style);

                }

                if (_styleClassesNames.Count > 0)
                {
                    if (string.IsNullOrEmpty(new_value))
                        new_value = style_tag.OuterHtml;
                    // must change css classes 
                    _styleClassesNames
                        .ToList()
                        .ForEach(dict =>
                        {
                            if (new_value.IndexOf("." + dict.Key, StringComparison.Ordinal) > -1)
                                new_value = new_value.Replace("." + dict.Key, "." + dict.Value);
                        });
                }

                if (!string.IsNullOrEmpty(new_value))
                {
                    var new_node = HtmlNode.CreateNode(new_value);
                    style_tag.ParentNode.ReplaceChild(new_node.ParentNode, style_tag);
                }
            }

            var nodes_to_remove = new List<HtmlNode>();

            _imagesAreBlocked = false;

            SanitizeNode(doc.DocumentNode, nodes_to_remove, base_href, load_images);

            nodes_to_remove
                .ForEach(node =>
                    node.Remove());

            images_are_blocked = _imagesAreBlocked;

            return doc.DocumentNode.OuterHtml;
        }

        private static void SanitizeNode(HtmlNode node, List<HtmlNode> nodes_to_remove, Uri base_href, bool load_images)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    nodes_to_remove.Add(node);
                    break;

                case HtmlNodeType.Document:
                    SanitizeDocument(node, nodes_to_remove, base_href, load_images);
                    break;

                case HtmlNodeType.Text:
                    break;

                case HtmlNodeType.Element:
                    if (_allowedTags.Match(node.Name).Success)
                    {
                        if (node.HasAttributes)
                        {
                            var attributes = node.Attributes;

                            var attrebutes_to_delete = new List<HtmlAttribute>();

                            foreach (var attribute in attributes)
                            {
                                SanitizeAttribute(attribute, attrebutes_to_delete, base_href, load_images);
                            }

                            attrebutes_to_delete
                                .ForEach(attr =>
                                         attr.Remove());
                        }

                        if (node.HasChildNodes)
                        {
                            SanitizeDocument(node, nodes_to_remove, base_href, load_images);
                        }
                    }
                    else
                    {
                        nodes_to_remove.Add(node);
                    }
                    break;
            }
        }

        private static void SanitizeAttribute(HtmlAttribute attribute, List<HtmlAttribute> attrebutes_to_delete, 
                                              Uri base_href, bool load_images)
        {
            string new_url;
            switch (attribute.Name)
            {
                case "style":
                    var clean_style = ParseStyles(attribute.Value, base_href, load_images);
                    if (string.IsNullOrEmpty(clean_style)) attrebutes_to_delete.Add(attribute);
                    else attribute.Value = clean_style;
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
                            attrebutes_to_delete.Add(attribute);
                        }
                        else
                        {
                            new_url = FixBaseLink(attribute.Value, base_href);
                            if (!string.IsNullOrEmpty(new_url))
                                attribute.Value = new_url;
                        }
                    }
                    catch
                    {
                        attrebutes_to_delete.Add(attribute);
                    }
                    break;
                case "background":
                case "src":
                    if (!_embeddedImagesPattern.Match(attribute.Value).Success)
                    {
                        try
                        {
                            var val = attribute.Value.StartsWith("//")
                                          ? "http:" + attribute.Value
                                          : attribute.Value;

                            var url = new Uri(val);
                            if (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps)
                            {
                                attrebutes_to_delete.Add(attribute);
                                break;
                            }

                            new_url = FixBaseLink(attribute.Value, base_href);
                            if (!string.IsNullOrEmpty(new_url))
                                attribute.Value = new_url;
                        }
                        catch
                        {
                            attrebutes_to_delete.Add(attribute);
                            break;
                        }
                    }

                    if (!load_images)
                    {
                        attribute.Name = "tl_disabled_" + attribute.Name;
                        _imagesAreBlocked = true;
                    }
                    break;
                case "class":
                    // must change css classes 
                    var splitted_classes =
                        attribute.Value
                                 .Split(new[] { " " },
                                        StringSplitOptions.RemoveEmptyEntries)
                                 .ToList();

                    var new_attr_value = string.Empty;

                    splitted_classes
                        .ForEach(cls =>
                        {
                            var found_new_class_name =
                                _styleClassesNames
                                    .Select(t => t)
                                    .FirstOrDefault(t => t.Key.Equals(cls));

                            new_attr_value += found_new_class_name.Value ?? cls;
                        });

                    attribute.Value = new_attr_value;
                    break;
                default:
                    if (attribute.Name.StartsWith("on"))
                        attrebutes_to_delete.Add(attribute); // skip all javascript events
                    else
                        attribute.Value = EncodeHtml(attribute.Value);
                    // by default encodeHtml all properies

                    break;
            }
        }

        private static void SanitizeDocument(HtmlNode node, List<HtmlNode> nodes_to_remove, Uri base_href, bool load_images)
        {
            foreach (var subnode in node.ChildNodes)
            {
                SanitizeNode(subnode, nodes_to_remove, base_href, load_images);
            }
        }

        private static string FixBaseLink(string founded_url, Uri base_href)
        {
            if (founded_url.StartsWith("//"))
            {
                founded_url = founded_url.Insert(0, "http:");
            }

            if (base_href != null &&
                !_embeddedImagesPattern.Match(founded_url).Success &&
                !Uri.IsWellFormedUriString(founded_url, UriKind.Absolute))
            {
                Uri link;
                if (Uri.TryCreate(base_href, founded_url, out link))
                {
                    return link.ToString();
                }
            }

            return string.Empty;
        }


        private static string ParseStyles(string style_string, Uri base_href, bool load_images)
        {
            var clean_style = string.Empty;
            var need_change_style = false;
            const string embedded_marker = @"http://marker-for-quick-parse.com/without-embedded-image-data"; // hack for right url parse if embedded image exists

            var embedded_image = _embeddedImagesPattern.Match(style_string);
            if (embedded_image.Success)
            {
                style_string = style_string.Replace(embedded_image.Value, embedded_marker);
            }

            var styles = _stylePattern.Matches(style_string);

            foreach (Match style in styles)
            {
                var style_name = style.Groups[1].Value.ToLower();
                var style_value = style.Groups[2].Value;

                // suppress invalid styles values 
                if (_forbiddenStylePattern.Match(style_value).Success)
                {
                    need_change_style = true;
                    continue;
                }


                // check if valid url 
                var url_style_matcher = _urlStylePattern.Match(style_value);
                if (!url_style_matcher.Success) continue;

                try
                {
                    var url_string = url_style_matcher.Groups[1].Value.Replace("'", "").Replace("\"", "");
                    if (!_embeddedImagesPattern.Match(url_string).Success)
                    {
                        var val = url_string.StartsWith("//")
                                      ? "http:" + url_string
                                      : url_string;
                        var uri = new Uri(val);
                        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                        {
                            need_change_style = true;
                            continue;
                        }
                    }

                    var new_url = FixBaseLink(url_string, base_href);
                    if (!string.IsNullOrEmpty(new_url))
                    {
                        style_value = style_value.Replace(url_string, new_url);
                        need_change_style = true;
                    }

                    if ((style_name == "background-image" ||
                         (style_name == "background" &&
                          style_value.IndexOf("url(", StringComparison.Ordinal) != -1)) &&
                        !load_images)
                    {
                        style_name = "tl_disabled_" + style_name;
                        _imagesAreBlocked = true;
                        need_change_style = true;
                    }
                }
                catch
                {
                    need_change_style = true;
                    continue;
                }

                clean_style = clean_style + style_name + ":" + EncodeHtml(style_value) + ";";
            }

            if (clean_style.IndexOf(embedded_marker, StringComparison.Ordinal) != -1)
                clean_style = clean_style.Replace(embedded_marker, embedded_image.Value);

            return need_change_style ? clean_style : style_string;
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

        public static string SanitizeHtmlForEditor(string in_html)
        {
            var res = RemoveHtml.Replace(in_html, "$1");
            res = RemoveHead.Replace(res, "", 1);
            res = RemoveBody.Replace(res, "<div$1</div>");
            return res.Trim();
        }
    }
}