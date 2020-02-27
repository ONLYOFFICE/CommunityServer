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
using System.IO;
using HtmlAgilityPack;

namespace ASC.Mail.Autoreply.Utility.Html
{
    public class Html2TextConverter
    {
        private static readonly List<string> MaliciousTags = new List<string> { "script", "style" };
        private static readonly List<string> LineBreakers = new List<string> {"p", "div", "blockquote", "br"}; 

        public static String Convert(String html)
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

                    if (LineBreakers.Contains(name))
                        outText.Write("\r\n");

                    if (node.HasChildNodes)
                        ProcessContent(node, outText);
                        
                    break;

                case HtmlNodeType.Text:
                    var text = ((HtmlTextNode)node).Text;

                    if (HtmlNode.IsOverlappedClosingElement(text))
                        break;

                    if (text.Trim().Length > 0)
                        outText.Write(text);

                    break;
            }
        }
    }
}
