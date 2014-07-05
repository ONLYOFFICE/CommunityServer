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
