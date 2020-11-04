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
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

namespace ASC.Web.UserControls.Wiki
{
    public partial class HtmlWikiUtil
    {
        private static RegexOptions mainOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

        private static string[] SupportedHtmlTags = { 
                                                        "br", "em", "strong", "b", "i", "u", "strike", "h1", "h2", "h3", "h4", "h5", "h6", "a",
                                                        "div", "table", "tfoot", "thead", "tbody", "th", "tr", "td", "img", "caption",
                                                        "big", "blockquote", "font", "marquee", "nobr", "p", "pre", "small", "s", "sub",
                                                        "sup", "tt", "abbr", "acronym", "cite", "code", "dfm", "kbd", "samp", "var",
                                                        "dd", "dl", "dt", "li", "menu", "ol", "ul", "legend", "span", "label", "hr"
                                                    };


        private HtmlWikiUtil() { }


        public static string Html2Wiki(string html)
        {
            string result;
            Regex spaceReg = new Regex(@"\s+", mainOptions);
            result = string.Format("{0}", spaceReg.Replace(html, " "));
            result = RemoveUnusableTags(result, "doc");
            //Form tag is overlaped it can be trimmed incorrect

            Regex formEnd = new Regex(@"</?form[^>]*>", mainOptions);
            result = formEnd.Replace(result, string.Empty);

            result = ConvertHtml2Wiki(result, null);
            result = DecriptWikiTags(result);
            return result;
        }

        public static string RemoveUnusableTags(string html, string parentTagName)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection nodeCollection = doc.DocumentNode.ChildNodes;
            HtmlNode node;
            if (nodeCollection != null && nodeCollection.Count > 0)
            {
                for (int i = 0; i < nodeCollection.Count; i++)
                {
                    node = nodeCollection[i];
                    if (node is HtmlCommentNode)
                    {
                        doc.DocumentNode.RemoveChild(node);
                        i--;
                        continue;
                    }

                    if (node.HasChildNodes)
                    {
                        node.InnerHtml = RemoveUnusableTags(node.InnerHtml, node.Name);
                    }

                    if (!(node is HtmlTextNode) && !UsableTag(node.Name))
                    {
                        if (!node.HasChildNodes)
                        {
                            doc.DocumentNode.RemoveChild(node);
                            i--;
                        }
                    }
                }
            }

            nodeCollection = doc.DocumentNode.SelectNodes(@"*");
            if (!UsableTag(parentTagName) && (nodeCollection == null || nodeCollection.Count == 0))
            {
                return string.Empty;
            }

            return doc.DocumentNode.InnerHtml;
        }


        private static string ConvertHtml2Wiki(string html, HtmlNode parentNode)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection nodeCollection = doc.DocumentNode.ChildNodes;
            HtmlNode node;
            string result = string.Empty;

            if (nodeCollection != null && nodeCollection.Count > 0)
            {
                for (int i = 0; i < nodeCollection.Count; i++)
                {
                    node = nodeCollection[i];
                    if (node is HtmlTextNode)
                    {
                        result += EscapeWikiTags(node.InnerHtml);
                        continue;
                    }

                    if (node.Name.Equals("br", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result += "\r\n";
                        continue;
                    }

                    if (node.Name.Equals("hr", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result += "----";
                        continue;
                    }

                    if (node.Name.Equals("h1", StringComparison.InvariantCultureIgnoreCase) ||
                        node.Name.Equals("h2", StringComparison.InvariantCultureIgnoreCase) ||
                        node.Name.Equals("h3", StringComparison.InvariantCultureIgnoreCase) ||
                        node.Name.Equals("h4", StringComparison.InvariantCultureIgnoreCase) ||
                        node.Name.Equals("h5", StringComparison.InvariantCultureIgnoreCase) ||
                        node.Name.Equals("h6", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result += ParceHtmlTagToWiki(node.InnerText, node);
                        continue;
                    }

                    if (node.Name.Equals("img", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result += ParceHtmlTagToWiki(string.Empty, node);
                        continue;
                    }

                    if (node.HasChildNodes)
                    {
                        result += ConvertHtml2Wiki(node.InnerHtml, node);
                    }
                }
            }

            if (parentNode == null)
            {
                return result;
            }

            return ParceHtmlTagToWiki(result, parentNode);
        }

        private static string EscapeWikiTags(string text)
        {
            string result = text.Replace("[", "&#91;").Replace("]", "&#93;");
            return HttpUtility.HtmlEncode(result);
        }

        private static string DecriptWikiTags(string text)
        {
            return HttpUtility.HtmlDecode(text);
        }

        private static string ParceHtmlTagToWiki(string parcedText, HtmlNode node)
        {
            string result = string.Empty;
            string style = string.Empty;
            style = GetAttrValue(node, "style");

            switch (node.Name.ToLower())
            {
                case "code":
                    result = string.Format("{{{{{0}}}}}", parcedText);
                    break;
                case "b":
                case "strong":
                    result = string.Format(@"'''{0}'''", parcedText);
                    break;
                case "i":
                case "em":
                    result = string.Format(@"''{0}''", parcedText);
                    break;
                case "u":
                    result = string.Format(@"__{0}__", parcedText);
                    break;
                case "strike":
                    result = string.Format(@"--{0}--", parcedText);
                    break;
                case "h1":
                    result = string.Format("\n={0}=\n", parcedText);
                    break;
                case "h2":
                    result = string.Format("\n=={0}==\n", parcedText);
                    break;
                case "h3":
                    result = string.Format("\n==={0}===\n", parcedText);
                    break;
                case "h4":
                    result = string.Format("\n===={0}====\n", parcedText);
                    break;
                case "h5":
                    result = string.Format("\n====={0}=====\n", parcedText);
                    break;
                case "h6":
                    result = string.Format("\n======{0}======\n", parcedText);
                    break;
                case "dd":
                case "dl":
                case "dt":
                case "li":
                case "menu":
                case "ol":
                case "ul":
                    result = ConvertHtmlListTags(parcedText, node);
                    break;
                case "a":
                    result = ConvertHtmlATag(parcedText, node);
                    break;
                case "img":
                    result = ConvertHtmlImgTag(node);
                    break;
                case "p":
                    result = string.Format(@"{0}<br />", parcedText);
                    break;
                case "table":
                    result = string.Format("\n{{| {1} {0} \n|}}\n", parcedText, string.IsNullOrEmpty(style) ? string.Empty : string.Format("{0}\n", style));
                    break;
                case "caption":
                    result = string.Format("\n|+ {1} {0}", parcedText, string.IsNullOrEmpty(style) ? string.Empty : string.Format("{0}\n", style));
                    break;
                case "tr":
                    result = string.Format("\n|- {1} {0}", parcedText, string.IsNullOrEmpty(style) ? string.Empty : string.Format("{0}\n", style));
                    break;
                case "th":
                    result = string.Format("\n! {0}", parcedText);
                    break;
                case "td":
                    result = string.Format("\n| {0}", parcedText);
                    break;
                case "div":
                case "span":
                case "sub":
                case "sup":
                    result = string.Format("<{1}{2}>{0}</{1}>", parcedText, node.Name, GetStyleNClassAttributes(node));
                    break;
                default:
                    result = parcedText;
                    break;
            }

            return result;
        }

        private static string GetStyleNClassAttributes(HtmlNode node)
        {
            string style = GetAttrValue(node, "style");
            string _class = GetAttrValue(node, "class");
            string result = string.Empty;

            if (!string.IsNullOrEmpty(_class))
            {
                result += string.Format(" class=\"{0}\"", _class);
            }

            if (!string.IsNullOrEmpty(style))
            {
                result += string.Format(" style=\"{0}\"", style);
            }

            return result;

        }

        private static string ConvertHtmlListTags(string text, HtmlNode node)
        {
            if (node.Name.Equals("li", StringComparison.InvariantCultureIgnoreCase))
            {
                if (node.ParentNode != null && node.ParentNode.Name.Equals("ol", StringComparison.InvariantCultureIgnoreCase))
                {
                    return string.Format("\n# {0}", text);
                }
                return string.Format("\n* {0}", text);
            }

            return text;
        }

        private static string ConvertHtmlATag(string text, HtmlNode node)
        {
            string href = GetAttrValue(node, "href");
            string result = string.Empty;
            Regex imageFindLink = new Regex(@".*\[\[.*\]\].*");
            if (imageFindLink.Match(text).Success)
            {
                return text;
            }

            if (string.IsNullOrEmpty(href))
            {
                return string.Empty;
            }

            result = GetInternalLink(href, text);
            if (!string.IsNullOrEmpty(result))
            {
                string article = GetArticleLink(href, text);
                if (!string.IsNullOrEmpty(article))
                {
                    result = article;
                }
            }
            else
            {
                result = GetExternalLink(href, text);
            }

            return result;
        }

        private static string GetInternalLink(string href, string text)
        {
            return string.Empty;
        }

        private static string GetArticleLink(string href, string text)
        {
            return string.Empty;
        }

        private static string GetExternalLink(string href, string text)
        {
            Regex httpReg = new Regex(@"(http://).+", mainOptions);
            if (!httpReg.Match(href).Success)
            {
                href = string.Format("http://{0}", href);
            }
            return string.Format("[{0} {1}]", href.TrimEnd('/'), text);
        }

        private static string ConvertHtmlImgTag(HtmlNode node)
        {
            Regex imageNameReg = new Regex(@".*/([^/])\??", mainOptions);
            string helpAttr = GetHeplAttr(node);
            string src = imageNameReg.Replace(GetAttrValue(node, "src"), "$1");



            return string.Format("[[Image:{0} |{1}]]", src, helpAttr);
        }

        private static string GetHeplAttr(HtmlNode node)
        {
            string result = string.Empty;
            result = GetAttrValue(node, "alt");
            if (string.IsNullOrEmpty(result))
            {
                result = GetAttrValue(node, "title");
            }

            return result;
        }

        private static string GetAttrValue(HtmlNode node, string attrName)
        {
            string result = string.Empty;
            attrName = attrName.ToLower();
            if (node.Attributes[attrName] != null && !string.IsNullOrEmpty(node.Attributes[attrName].Value))
            {
                result = node.Attributes[attrName].Value;
            }

            return result;
        }

        private static bool UsableTag(string tagName)
        {
            foreach (string s in SupportedHtmlTags)
            {
                if (s.Equals(tagName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}