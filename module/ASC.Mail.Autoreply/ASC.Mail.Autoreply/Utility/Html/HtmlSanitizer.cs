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
using System.IO;
using HtmlAgilityPack;

namespace ASC.Mail.Autoreply.Utility.Html
{
    public class HtmlSanitizer
    {
        private static readonly List<string> MaliciousTags = new List<string> { "script", "style" };
        private static readonly List<string> LineBreakers = new List<string> { "p", "div", "blockquote", "br" };  
        private static readonly List<string> WhiteList = new List<string> {"b", "strong", "it", "em", "dfn", "sub", "sup", "strike", "s", "del", "code", "kbd", "samp", "ins", "h1", "h2", "h3", "h4", "h5", "h6"};

        public static String Sanitize(String html)
        {
            if (String.IsNullOrEmpty(html))
                return html;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            var sw = new StringWriter();
            ProcessNode(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        private static void ProcessContent(HtmlNode node, TextWriter outText)
        {
            foreach (var child in node.ChildNodes)
            {
                ProcessNode(child, outText);
            }
        }

        private static void ProcessNode(HtmlNode node, TextWriter outText)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    break;

                case HtmlNodeType.Document:
                    ProcessContent(node, outText);
                    break;

                case HtmlNodeType.Element:
                    var name = node.Name.ToLowerInvariant();

                    if (MaliciousTags.Contains(name))
                        break;

                    if (WhiteList.Contains(name) && node.HasChildNodes && node.Closed)
                    {
                        outText.Write("<{0}>", name);
                        ProcessContent(node, outText);
                        outText.Write("</{0}>", name);
                        break;
                    }

                    if (name.Equals("img") && node.HasAttributes && node.Attributes["src"] != null)
                    {
                        outText.Write("<img src=\"{0}\"/>", node.Attributes["src"].Value);
                    }
                    else if (LineBreakers.Contains(name))
                    {
                        outText.Write("<br>");
                    }

                    if (node.HasChildNodes)
                        ProcessContent(node, outText);
                    
                    break;

                case HtmlNodeType.Text:
                    var text = ((HtmlTextNode) node).Text;

                    if (HtmlNode.IsOverlappedClosingElement(text))
                        break;

                    if (text.Trim().Length > 0)
                        outText.Write(text);

                    break;
            }
        }
    }
}
