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

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using Microsoft.Security.Application;

namespace ASC.Common.Utils
{
    public class HtmlUtil
    {
        private static readonly Regex HtmlTagReplacer = new Regex(@"</?(.|\n)*?>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static readonly Regex HtmlCommentsReplacer = new Regex("<!--(?s).*?-->", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static readonly Regex XssTagReplacer = new Regex(@"<\s*(style|script)[^>]*>(.*?)<\s*/\s*(style|script)>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex Worder = new Regex(@"\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);


        public static string GetText(string html)
        {
            return GetText(html, 0);
        }

        public static string GetText(string html, bool preserveSpaces)
        {
            return GetText(html, 0, "...", true, preserveSpaces);
        }

        public static string GetText(string html, int maxLength)
        {
            return GetText(html, maxLength, "...");
        }

        public static string GetText(string html, int maxLength, bool preserveSpaces)
        {
            return GetText(html, maxLength, "...", true, preserveSpaces);
        }

        public static string GetText(string html, int maxLength, string endBlockTemplate)
        {
            return GetText(html, maxLength, endBlockTemplate, true);
        }

        public static string GetText(string html, int maxLength, string endBlockTemplate, bool calcEndBlockTemplate)
        {
            return GetText(html, maxLength, endBlockTemplate, calcEndBlockTemplate, false);
        }

        public static string GetText(string html, int maxLength, string endBlockTemplate, bool calcEndBlockTemplate, bool preserveSpaces)
        {
            string unformatedText = string.Empty;
            if (!string.IsNullOrEmpty(html))
            {
                html = XssTagReplacer.Replace(html,string.Empty);//Clean malicious tags. <script> <style> etc
                if (string.IsNullOrEmpty(html)) return html;//return this if it's empty

                try
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    unformatedText = doc.DocumentNode.InnerText;//Parsing is done by HtmlDocument, also it preserves sapces and line breaks
                }
                catch (Exception)
                {
                    //Try simple replace
                    unformatedText = HtmlTagReplacer.Replace(XssTagReplacer.Replace(html,""), "");
                }

                if (!string.IsNullOrEmpty(unformatedText))
                {
                    //Kill comments
                    unformatedText = HtmlCommentsReplacer.Replace(unformatedText, string.Empty);
                    unformatedText=unformatedText.Trim('\r', '\n', ' ');//Trim spaces and line breaks

                    if (!string.IsNullOrEmpty(unformatedText))
                    {
                        if (maxLength == 0 || unformatedText.Length < maxLength)
                            return HttpUtility.HtmlDecode(unformatedText);
                        //Set maximum length with end block
                        maxLength = Math.Max(0,calcEndBlockTemplate ? maxLength - endBlockTemplate.Length : maxLength);
                        var startIndex = Math.Max(0, Math.Min(unformatedText.Length - 1, maxLength));
                        var countToScan = Math.Max(0, startIndex-1);

                        var lastSpaceIndex = unformatedText.LastIndexOf(' ',startIndex, countToScan);

                        unformatedText = lastSpaceIndex > 0 && lastSpaceIndex < unformatedText.Length
                                             ? unformatedText.Remove(lastSpaceIndex)
                                             : unformatedText.Substring(0, maxLength);
                        if (!string.IsNullOrEmpty(endBlockTemplate))
                        {
                            unformatedText += endBlockTemplate;
                        }
                    }
                }
            }
            return HttpUtility.HtmlDecode(unformatedText);//TODO:!!!
        }

        public static string ToPlainText(string html)
        {
            return GetText(html);
        }

        public static string SanitizeFragment(string htmlText)
        {
            return Sanitizer.GetSafeHtmlFragment(htmlText);
        }

        public static string SanitizeHtml(string htmlText)
        {
            return Sanitizer.GetSafeHtml(htmlText);
        }


        /// <summary>
        /// The function highlight all words in htmlText by searchText.
        /// </summary>
        /// <param name="searchText">the space separated string</param>
        /// <param name="htmlText">html for highlight</param>
        /// <returns>highlighted html</returns>
        public static string SearchTextHighlight(string searchText, string htmlText)
        {
            if (string.IsNullOrEmpty(searchText) || string.IsNullOrEmpty(htmlText)) return htmlText;

            var regexpstr = Worder.Matches(searchText).Cast<Match>().Select(m => m.Value).Distinct().Aggregate((r, n) => r + "|" + n);
            var wordsFinder = new Regex(Regex.Escape(regexpstr), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);
            return wordsFinder.Replace(htmlText, m => string.Format("<span class='searchTextHighlight'>{0}</span>", m.Value));
        }
    }
}