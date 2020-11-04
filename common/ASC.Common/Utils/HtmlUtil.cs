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
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ASC.Common.Utils
{
    public static class HtmlUtil
    {
        private static readonly Regex tagReplacer = new Regex("<[^>]*>", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex commentsReplacer = new Regex("<!--(?s).*?-->", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex xssReplacer = new Regex(@"<\s*(style|script)[^>]*>(.*?)<\s*/\s*(style|script)>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex Worder = new Regex(@"\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);


        public static string GetText(string html, int maxLength = 0, string endBlockTemplate = "...")
        {
            var unformatedText = string.Empty;
            if (!string.IsNullOrEmpty(html))
            {
                html = xssReplacer.Replace(html, string.Empty); //Clean malicious tags. <script> <style>

                if (string.IsNullOrEmpty(html))
                {
                    return html;
                }

                unformatedText = tagReplacer.Replace(html, string.Empty);

                if (!string.IsNullOrEmpty(unformatedText))
                {
                    // kill comments
                    unformatedText = commentsReplacer.Replace(unformatedText, string.Empty);
                    unformatedText = unformatedText.Trim();

                    if (!string.IsNullOrEmpty(unformatedText))
                    {
                        if (maxLength == 0 || unformatedText.Length < maxLength)
                        {
                            return HttpUtility.HtmlDecode(unformatedText);
                        }

                        //Set maximum length with end block
                        maxLength = Math.Max(0, maxLength - endBlockTemplate.Length);
                        var startIndex = Math.Max(0, Math.Min(unformatedText.Length - 1, maxLength));
                        var countToScan = Math.Max(0, startIndex - 1);

                        var lastSpaceIndex = unformatedText.LastIndexOf(' ', startIndex, countToScan);

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

        /// <summary>
        /// The function highlight all words in htmlText by searchText.
        /// </summary>
        /// <param name="searchText">the space separated string</param>
        /// <param name="htmlText">html for highlight</param>
        /// <param name="withoutLink"></param>
        /// <returns>highlighted html</returns>
        public static string SearchTextHighlight(string searchText, string htmlText, bool withoutLink = false)
        {
            if (string.IsNullOrEmpty(searchText) || string.IsNullOrEmpty(htmlText)) return htmlText;

            var regexpstr = Worder.Matches(searchText).Cast<Match>().Select(m => m.Value).Distinct().Aggregate((r, n) => r + "|" + n);
            var wordsFinder = new Regex(Regex.Escape(regexpstr), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);
            return wordsFinder.Replace(htmlText, m => string.Format("<span class='searchTextHighlight{1}'>{0}</span>", m.Value, withoutLink ? " bold" : string.Empty));
        }
    }
}