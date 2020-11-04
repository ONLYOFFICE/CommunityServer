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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Web.Studio.Utility.HtmlUtility;
using ASC.Web.UserControls.Wiki.Resources;
using HtmlAgilityPack;

namespace ASC.Web.UserControls.Wiki
{
    [Flags]
    internal enum XltType
    {
        Normal = 0x0,
        Table = 0x1,
        Pre = 0x2
    }

    internal enum ImageFloat
    {
        None = 0,
        Left,
        Right,
        Center
    }


    public enum ConvertType
    {
        Editable = 0,
        Wysiwyg,
        NotEditable
    }


    internal class TempContainer
    {
        public Guid TempId { get; set; }
        public string Content { get; set; }

        public TempContainer()
        {
            TempId = Guid.NewGuid();
            Content = string.Empty;
        }
    }

    public partial class HtmlWikiUtil
    {
        private static string _imgExtensions = @"gif;jpg;jpeg;png;svg";
        private static string _validProtocols = @"http;https;ftp;mailto";
        private static string _urlChars = @"a-z0-9;/\?:@&=\+\$,\-_\.!~\*'\(\)#%\\";
        private static string _urlEndChars = @"a-z0-9;/\?:@&=\+\$\-_\\\)";
        private static string _convertNoWikitemplate = @"~~{0}~~";

        private List<TempContainer> tableList;
        private List<TempContainer> preList;
        private string strStack = string.Empty;
        private bool blnCodeOpen = false;
        private int intOpenTab = 0;
        private int thumbWidth = 0;

        private int[] arrNr = new int[] { 0, 0, 0, 0, 0, 0, 0 };
        private string[] arrRef = new string[200];
        private string[][] arrTOC = new string[200][];

        private int intRef, intTOC;

        private Regex rexLin, rexI, rexB, rexU, rexLnk, rexDB, rexEmptyTags, rexS, rexSpetial, rexNoWiki, rexLinkContainer,
                      tags, spaces, rexTablesContainer, rexTableCell, rexLinkTabContainer, rexPreContainer, rexSimpleContent;

        private static Regex rexCode = new Regex(@"<code([\s\S]*?)<\/code>");
        private static Regex rexScript = new Regex(@"<\s*script([\s\S]*?)<\s*\/\s*script\s*>", mainOptions);
        private int intMaxTOC;
        private Dictionary<string, string> dctDB;
        private bool shouldCloaseAll = true;

        private string internalMainLink = string.Empty;
        private string fileNameProcessing = string.Empty;
        private List<string> existingPages = new List<string>();
        private string imageHandlerUrlFormat = string.Empty;
        private string selfPageName = string.Empty;

        private bool canBeEdit = true;
        private bool convertCode = true;
        private bool convertTOC = true;

        private static Regex rexSection = new Regex(@"(\n|^)(={1,6})([^=]+)", mainOptions);
        internal static Regex rexNoSection = new Regex(@"<nowiki>[\s\S]*?<\/nowiki>", mainOptions | RegexOptions.Multiline);

        private static string RegExCategorySearch = string.Format(@"\[\[{0}:([^\|^\]]+)(\|[^]]+)*\]\]", Constants.WikiCategoryKeyCaption);

        public static string GetWikiSectionNameBySectionNumber(string wiki, int section)
        {
            Match[] matches = GetWikiSectionMatches(wiki);
            if (section > matches.Length - 1)
            {
                return string.Empty;
            }

            return matches[section].Groups[3].Value.Trim();

        }

        public static string GetWikiSectionBySectionNumber(string wiki, int section)
        {
            Match[] matches = GetWikiSectionMatches(wiki);
            if (section > matches.Length - 1)
            {
                return string.Empty;
            }

            int nextSection = -1;
            int sectionLevel = matches[section].Groups[2].Value.Length;

            for (int i = section + 1; i < matches.Length; i++)
            {
                if (matches[i].Groups[2].Value.Length <= sectionLevel)
                {
                    nextSection = i;
                    break;
                }
            }

            if (nextSection == -1)
            {
                return wiki.Substring(matches[section].Index).Trim();
            }

            return wiki.Substring(matches[section].Index, matches[nextSection].Index - matches[section].Index).Trim();
        }

        public static string SetWikiSectionBySectionNumber(string wiki, int section, string newValue)
        {
            Match[] matches = GetWikiSectionMatches(wiki);

            if (section > matches.Length - 1)
            {
                return wiki;
            }

            int nextSection = -1;
            int sectionLevel = matches[section].Groups[2].Value.Length;

            for (int i = section + 1; i < matches.Length; i++)
            {
                if (matches[i].Groups[2].Value.Length <= sectionLevel)
                {
                    nextSection = i;
                    break;
                }
            }

            string result = wiki.Remove(matches[section].Index) + "\n" + newValue;
            if (nextSection != -1)
            {
                result = string.Format("{0}{1}", result, wiki.Substring(matches[nextSection].Index));
            }

            return result;
        }


        private static Match[] GetWikiSectionMatches(string wiki)
        {
            MatchCollection allSectionMatches = rexSection.Matches(wiki);
            MatchCollection nowikiSectionMatches = rexNoSection.Matches(wiki);

            var selections =
                from noWiki in nowikiSectionMatches.Cast<Match>()
                select
                    from fakeSelection in rexSection.Matches(noWiki.Value).Cast<Match>()
                    select noWiki.Index + fakeSelection.Index;

            List<int> noIndex = new List<int>();

            foreach (var s in selections)
            {
                noIndex.AddRange(s.ToList<int>());
            }

            var allSections = from allSelection in allSectionMatches.Cast<Match>()
                              where !noIndex.Contains(allSelection.Index)
                              select allSelection;

            return allSections.ToArray();
        }

        private void Init(string pageName, string intMainLink, List<string> existPages, string imgHandlerUrlFormat, int tenant, ConvertType isEditable, int thWidth)
        {
            rexLin = new Regex(@"^(<<|><|>>|<>|!|\{\||\|\}|\|\+|\|\-|\||:{1,}|\*{1,}|\#{1,}|;{1,}|\-{4,}|={1,6})?(.*)", mainOptions);
            rexI = new Regex(@"''(.*?)(''|$)", mainOptions);
            rexB = new Regex(@"'''(.*?)('''|$)", mainOptions);
            rexU = new Regex(@"__(.*?)(__)", mainOptions);
            rexS = new Regex(@"--(.+?)(--)", mainOptions);
            rexSpetial = new Regex(@"[^{]?({(\S+)})[^]]?", mainOptions);
            //rexLnk = new Regex(@"((http|https|ftp)://([" + _urlChars + @"]*[" + _urlEndChars + @"]))|(^(([^<>()[\]\\.,;:\s@\""]+(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$)|\[\[([^[]*)\]\]|\[([" + _urlChars + @"]*[" + _urlEndChars + @"])\]|\[([" + _urlChars + @"]*[" + _urlEndChars + @"])\s*([^\]]*)\]", mainOptions);
            rexLnk = new Regex(@"((http|https|ftp)://([" + _urlChars + @"]*[" + _urlEndChars + @"]))|([a-z0-9\._-]+@[a-z0-9\._-]+[a-z0-9_]+)|\[\[([^[]*)\]\]|\[([" + _urlChars + @"]*[" + _urlEndChars + @"])\]|\[([" + _urlChars + @"]*[" + _urlEndChars + @"])\s*([^\]]*)\]", mainOptions);
            rexDB = new Regex(@"(ISBN[:]? ([0-9A-Z\-]+))|\{\{([^\{]*)\}\}", mainOptions);
            rexEmptyTags = new Regex(@"(<tr>\s*?</tr>)", mainOptions);
            rexNoWiki = new Regex(@"(<nowiki>[\s\S]*?<\/nowiki>)|(<code[\s\S]*?<\/code>)|(<!--[\s\S]*?-->)", mainOptions | RegexOptions.Multiline);
            rexLinkContainer = new Regex(@"\[[\s\S]*?\]", mainOptions);
            rexLinkTabContainer = new Regex(@"\[[\s\S]*?\]\]", mainOptions);
            rexTablesContainer = new Regex(@"({\|)(?:[\s\S](?!{\|))+?(\|})", mainOptions | RegexOptions.Multiline);
            rexPreContainer = new Regex(@"<pre[\s\S]*?>([\s\S]+?)</pre>", mainOptions | RegexOptions.Multiline);
            rexTableCell = new Regex(@"(^|[^{])[\|\!]([^\+\-\}][^\|\!]*)", mainOptions | RegexOptions.Multiline);
            rexSimpleContent = new Regex(@"^[0-9a-zA-Z]+$", mainOptions);
            tags = new Regex(@"</?(.|\n)*?>");
            spaces = new Regex(@"\s+");

            intMaxTOC = 3;

            dctDB = new Dictionary<string, string>();
            dctDB.Add("ISBN", @"http://www.amazon.com/exec/obidos/ASIN/{0}");
            dctDB.Add("ISSN", @"http://pacifix.ddb.de:7000/DB=1.1/SET=2/TTL=1/CMD?ACT=SRCH&IKT=8&TRM={0}");
            dctDB.Add("DWDS", @"http://www.dwds.de/cgi-bin/portalL.pl?search={0}");

            //Init Variables

            intRef = 0;
            intTOC = 0;

            blnCodeOpen = false;

            internalMainLink = intMainLink;
            existingPages = existPages;
            imageHandlerUrlFormat = imgHandlerUrlFormat;
            thumbWidth = thWidth;

            selfPageName = pageName;
            canBeEdit = isEditable == ConvertType.Editable;
            convertTOC = convertCode = isEditable != ConvertType.Wysiwyg;

        }

        public static string CreateImageFromWiki(string pageName, string wiki, string intMainLink, string imageHandlerUrlFormat, int tenant)
        {
            return CreateImageFromWiki(pageName, wiki, intMainLink, imageHandlerUrlFormat, tenant, ConvertType.NotEditable, 180);
        }

        public static string CreateImageFromWiki(string pageName, string wiki, string intMainLink, string imageHandlerUrlFormat, int tenant, ConvertType isEditable, int thumbWidth)
        {
            List<string> existPages = new List<string>();

            HtmlWikiUtil instance = new HtmlWikiUtil();
            instance.Init(pageName, intMainLink, existPages, imageHandlerUrlFormat, tenant, isEditable, thumbWidth);

            return instance.ProcessImage(wiki);
        }

        public static string WikiToHtml(string pageName, string wiki, string intMainLink, List<string> existPages, string imageHandlerUrlFormat, int tenant, ConvertType isEditable)
        {
            return WikiToHtml(pageName, wiki, intMainLink, existPages, imageHandlerUrlFormat, tenant, isEditable, 180);
        }

        public static string WikiToHtml(string pageName, string wiki, string intMainLink, List<string> existPages, string imageHandlerUrlFormat, int tenant, ConvertType isEditable, int thumbWidth)
        {
            if (existPages == null)
                existPages = new List<string>();
            if (intMainLink == null)
                intMainLink = string.Empty;
            if (imageHandlerUrlFormat == null)
                imageHandlerUrlFormat = string.Empty;

            HtmlWikiUtil instance = new HtmlWikiUtil();
            instance.Init(pageName, intMainLink, existPages, imageHandlerUrlFormat, tenant, isEditable, thumbWidth);
            string result = instance.XLT(wiki);
            result = rexScript.Replace(result, string.Empty);
            return result;
        }

        private string MultiString(int n, string s)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < n; i++)
            {
                result.Append(s);
            }

            return result.ToString();
        }

        private string OpenBlk(char sBlk, int n)
        {
            string result = string.Empty;

            switch (sBlk)
            {
                case 'B':
                    result = MultiString(n, "<blockquote>\n");
                    break;
                case 'O':
                    result = MultiString(n, "<ol>\n");
                    break;
                case 'U':
                    result = MultiString(n, "<ul>\n");
                    break;
                case 'D':
                    result = MultiString(n, "<dl>\n");
                    break;
                case 'L':
                    result = MultiString(n, "<dd>\n");
                    break;
                case 'T':
                    result = MultiString(n, "<table>\n");
                    break;
                case 'P':
                    result = MultiString(n, "<p>\n");
                    break;
            }

            strStack += new String(sBlk, n);

            return result;
        }

        private string CloseBlk(int n)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < n; i++)
            {
                switch (strStack[strStack.Length - 1])
                {
                    case 'B':
                        result.Append("</blockquote>\n");
                        break;
                    case 'O':
                        result.Append("</ol>\n");
                        break;
                    case 'U':
                        result.Append("</ul>\n");
                        break;
                    case 'D':
                        result.Append("</dl>\n");
                        break;
                    case 'L':
                        result.Append("</dd>\n");
                        break;
                    case 'T':
                        result.Append("</table>\n)");
                        break;
                    case 'P':
                        result.Append("</p>\n");
                        break;
                }
                strStack = strStack.Remove(strStack.Length - 1);
            }

            if (blnCodeOpen)
            {
                result.Append("</pre>\n");
                blnCodeOpen = false;
            }
            return result.ToString();
        }

        private string CloseAllBlkExP()
        {
            if (strStack.Length == 0 || strStack[strStack.Length - 1] == 'P')
                return string.Empty;

            for (int i = strStack.Length - 1; i >= 0; i--)
            {
                if (strStack[i] == 'P')
                {
                    return CloseAllBlk(strStack.Length - 1 - i);
                }
            }

            return CloseAllBlk();
        }

        private string CloseAllBlk()
        {
            return CloseAllBlk(0);
        }

        private string CloseAllBlk(int count)
        {
            if (!shouldCloaseAll)
                return string.Empty;

            if (count == 0)
            {
                count = strStack.Length;
            }

            StringBuilder result = new StringBuilder(CloseBlk(count));
            return result.ToString();
        }


        private string StartBlk(int n, char sBlk)
        {
            StringBuilder result = new StringBuilder();
            bool exit = false;
            if (sBlk == 'D')
            {

                while (GetLastBlk() == 'L')
                {
                    if (!exit)
                        exit = true;

                    result.Append(CloseBlk(1));
                }
            }

            if (exit)
                return result.ToString();

            if (n == strStack.Length)
            {
                if (strStack[strStack.Length - 1] != sBlk || sBlk == 'P')
                {
                    result.Append(CloseBlk(1) + OpenBlk(sBlk, 1));
                }
            }
            else if (n < strStack.Length)
            {
                result = new StringBuilder(CloseBlk(strStack.Length - n));
                if (strStack[strStack.Length - 1] != sBlk)
                {
                    result.Append(CloseBlk(1) + OpenBlk(sBlk, 1));
                }
            }
            else
            {
                result = new StringBuilder(OpenBlk(sBlk, n - strStack.Length));
            }

            return result.ToString();
        }

        private char GetLastBlk()
        {
            char result = '\0';
            if (strStack.Length > 0)
            {
                result = strStack[strStack.Length - 1];
            }

            return result;
        }

        private string Autonumber(int n)
        {
            StringBuilder result = new StringBuilder();
            int i;
            arrNr[n - 1]++;
            for (i = n; i < 6; i++)
                arrNr[i] = 0;

            for (i = 0; i < n; i++)
            {
                if (arrNr[i] == 0)
                    continue;

                result.Append(string.Format("{0}.", arrNr[i]));
            }

            result.Append(" ");

            return result.ToString();
        }

        private bool IsPageExists(string pageName)
        {
            return fileNameProcessing.Equals(pageName, StringComparison.InvariantCultureIgnoreCase);
        }

        private string ProcessInternalLinks(string pageName, bool isFile)
        {
            string ancor = string.Empty;
            string spetial = string.Empty;
            bool _isLinkOnly = false;

            if (pageName.Contains(":"))
            {
                spetial = GetSpecial(pageName);
                if (!string.IsNullOrEmpty(spetial) && pageName[0] == ':')
                {
                    _isLinkOnly = true;
                }
                pageName = pageName.Remove(0, pageName.IndexOf(spetial) + spetial.Length).TrimStart(':');
            }
            else if (pageName.Contains("#"))
            {
                ancor = pageName.Split('#')[1];
                pageName = pageName.Split('#')[0];

            }

            string result;
            if (isFile)
            {
                result = string.Format(@"<a href=""{0}{1}""", ActionHelper.GetViewFilePath(internalMainLink, pageName),
                                       string.IsNullOrEmpty(ancor) ? "" : "#" + ancor);
            }
            else
            {
                result = string.Format(@"<a href=""{0}{1}""{2}", ActionHelper.GetViewPagePath(internalMainLink, pageName, PageNameUtil.NormalizeNameCase(spetial)),
                                       string.IsNullOrEmpty(ancor) ? "" : "#" + ancor,
                                       _isLinkOnly ? " _wikiCategoryLink=\"\" " : string.Empty
                    );
            }

            fileNameProcessing = pageName.Replace("_", " ");

            if (existingPages.Exists(IsPageExists) || IsSpetialExists(spetial))
            {
                result = string.Format(@"{0} class=""internal""", result);
            }
            else
            {
                result = string.Format(@"{0} class=""internalNotExists""", result);
            }

            return result;
        }

        public static bool IsSpetialExists(string spetial)
        {
            if (string.IsNullOrEmpty(spetial))
                return false;

            if (spetial.Equals(Constants.WikiCategoryKeyCaption, StringComparison.InvariantCultureIgnoreCase))
                return true;
            if (spetial.Equals(Constants.WikiInternalKeyCaption, StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
        }

        private string ProcessImage(string pattern)
        {
            string result = string.Empty;
            string[] param = pattern.Split('|');
            string fileName = string.Empty, alt = string.Empty, title = string.Empty;
            bool isFrame = false, isThumb = false;
            ImageFloat imgFloat = ImageFloat.None;
            string maxSize = string.Empty;
            Regex regMax = new Regex(@"([0-9]+)px", mainOptions);
            int iSize = 0;
            string str;
            foreach (string p in param)
            {
                str = p.Trim();
                if (str.ToLower().Contains("image:")) //GetFileName
                {
                    fileName = str.Split(':')[1].Trim();
                }
                else if (str.ToLower().Contains("alt="))
                {
                    alt = str.Split('=')[1].Trim();
                }
                else if (str.Equals("center", StringComparison.InvariantCultureIgnoreCase))
                {
                    imgFloat = ImageFloat.Center;
                }
                else if (str.Equals("right", StringComparison.InvariantCultureIgnoreCase))
                {
                    imgFloat = ImageFloat.Right;
                }
                else if (str.Equals("left", StringComparison.InvariantCultureIgnoreCase))
                {
                    imgFloat = ImageFloat.Left;
                }
                else if (str.Equals("frame", StringComparison.InvariantCultureIgnoreCase))
                {
                    isFrame = true;
                    if (imgFloat.Equals(ImageFloat.None))
                    {
                        imgFloat = ImageFloat.Right;
                    }
                }
                else if (str.Equals("thumb", StringComparison.InvariantCultureIgnoreCase))
                {
                    isThumb = true;
                    if (imgFloat.Equals(ImageFloat.None))
                    {
                        imgFloat = ImageFloat.Right;
                    }
                }
                else if (regMax.Match(str.ToLower()).Success)
                {
                    maxSize = str.ToLower();
                    iSize = Convert.ToInt32(regMax.Replace(str, "$1"));
                }
                else
                {
                    title = str;
                }

            }
            //0       :1  :2       :3      :4      :5    :6
            //fileName:alt:imgFloat:isThumb:isFrame:iSize:title
            string wysiwygInfo = string.Format(@"_wikiInfo=""{0}:{1}:{2}:{3}:{4}:{5}:{6}""", PageNameUtil.Encode(fileName), PageNameUtil.Encode(alt), imgFloat.Equals(ImageFloat.None) ? string.Empty : imgFloat.ToString().ToLower(),
                                               isThumb ? 1 : 0, isFrame ? 1 : 0, iSize, PageNameUtil.Encode(title));

            if (isThumb || isFrame)
            {
                result = string.Format(@"<div {2} class=""thumb {1}""><div style=""width:{0}px;"" class=""thumbinner"">", iSize == 0 ? thumbWidth : iSize, (imgFloat.Equals(ImageFloat.Left) ? "tleft" : imgFloat.Equals(ImageFloat.Right) ? "tright" : "tcenter"), wysiwygInfo);
                result = string.Format(@"{0}<a _wikiIgnore="""" class=""image"" href=""{1}""><img _wikiIgnore="""" width=""{3}"" class=""thumbimage"" src=""{2}"" alt=""{4}""/></a><div _wikiIgnore="""" class=""thumbcaption"">",
                                       result,
                                       ActionHelper.GetViewFilePath(internalMainLink, fileName),
                                       string.Format(imageHandlerUrlFormat, fileName),
                                       iSize - 2 <= 0 ? (thumbWidth - 2 > 0 ? thumbWidth - 2 : thumbWidth) : iSize - 2,
                                       alt);
                if (isThumb)
                {
                    result = string.Format(@"{0}<div _wikiIgnore="""" class=""magnify"">", result);
                    result = string.Format(@"{0}<a _wikiIgnore="""" title=""{1}"" class=""enlarge"" href=""{2}""></a></div>",
                                           result,
                                           WikiUCResource.Enlarge,
                                           ActionHelper.GetViewFilePath(internalMainLink, fileName));

                }
                result = string.Format(@"{0}{1}</div></div></div>", result, title);
            }
            else
            {
                result = string.Format(@"<div {1} {0}>", imgFloat.Equals(ImageFloat.None) ? "class='floatleft'" :
                                                             string.Format(@"class=""{0}""", (imgFloat.Equals(ImageFloat.Left) ? "floatleft" : imgFloat.Equals(ImageFloat.Right) ? "floatright" : "floatcenter")), wysiwygInfo);
                result = string.Format(@"{0}<a _wikiIgnore="""" class=""image"" href=""{1}""><img _wikiIgnore="""" {3} src=""{2}"" alt=""{4}"" title=""{5}""/></a>",
                                       result,
                                       ActionHelper.GetViewFilePath(internalMainLink, fileName),
                                       string.Format(imageHandlerUrlFormat, fileName),
                                       (string.IsNullOrEmpty(maxSize) ? string.Empty : string.Format(@"width=""{0}""", maxSize)),
                                       alt,
                                       title);
                result = string.Format(@"{0}</div>", result);
                if (imgFloat.Equals(ImageFloat.None))
                {
                    result = string.Format("<div class='clearFix'>{0}</div>", result);
                }
            }

            return result;
        }

        private string Ref(string sUrl, string sTxt, string sTgt, string all)
        {
            string result = string.Empty;
            int i;
            string strProt = string.Empty;
            string strExt = string.Empty;

            sUrl = sUrl.Trim();
            strExt = sUrl.Split('.')[sUrl.Split('.').Length - 1].ToLower();
            string sLowUrl = sUrl.ToLower();
            if (sLowUrl.Contains("image:") || sLowUrl.Contains("file:"))
            {
                sUrl = sUrl.Split(':')[1];

                if (sLowUrl.Contains("image:") && _imgExtensions.Contains(strExt))
                {
                    return ProcessImage(all);
                }
                else
                {
                    result = ProcessInternalLinks(sUrl, true);
                    if (string.IsNullOrEmpty(sTxt))
                        sTxt = sUrl;
                    result = string.Format(@"{0} _wikiFile="""" title=""{2}"">{1}</a>", result, sTxt, sUrl);
                }
            }
            else //link
            {
                if (string.Empty.Equals(sTxt))
                {
                    if (intRef > 0)
                    {
                        for (i = 0; i < intRef; i++)
                        {
                            if (arrRef[i].Equals(sUrl, StringComparison.InvariantCultureIgnoreCase))
                            {
                                sTxt = string.Format("[{0}]", i + 1);
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(sTxt))
                    {
                        if (sUrl.Contains("/"))
                        {
                            sTxt = string.Format("[{0}]", intRef + 1);
                            arrRef[intRef] = sUrl;
                            intRef++;
                        }
                        else
                        {
                            sTxt = sUrl;
                        }
                    }
                }

                result = string.Format(@"<a href=""{0}""", sUrl);
                if (sUrl.Contains(":"))
                {
                    strProt = sUrl.Split(':')[0].ToLower();
                }
                switch (strProt)
                {
                    case "https":
                        result = string.Format(@"{0} class=""secure""", result);
                        break;
                    case "http":
                        result = string.Format(@"{0} class=""external""", result);
                        break;
                    case "ftp":
                        result = string.Format(@"{0} class=""ftp""", result);
                        break;
                    case "mailto":
                        result = string.Format(@"{0} class=""mail""", result);
                        break;
                    default:
                        result = ProcessInternalLinks(sUrl, false);
                        sTgt = string.Empty;
                        break;
                }

                if (sTxt.Contains(":") && IsSpetialExists(GetSpecial(sTxt)))
                {
                    sTxt = sTxt.TrimStart(':');
                }

                if (sUrl.Contains(":") && IsSpetialExists(GetSpecial(sUrl)))
                {
                    sUrl = sUrl.TrimStart(':');
                }

                if (!sUrl.Equals(sTxt.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    result = string.Format(@"{0} title=""{1}""", result, HttpUtility.HtmlDecode(sTxt).HtmlEncode());
                }
                if (!string.IsNullOrEmpty(sTgt))
                {
                    result = string.Format(@"{0} target=""{1}""", result, sTgt);
                }

                result = string.Format(@"{0}>{1}</a>", result, sTxt);
            }

            return result;
        }

        private string GetSpecial(string input)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(input))
            {
                if (input.LastIndexOf(':') > 0)
                {
                    result = input.TrimStart(':').Split(':')[0];
                }
            }

            return result;
        }

        private string Cell(string sTxt, int iColSpan, int iRowSpan, string sHAl, string sVAl, bool bNoWrap, bool bHdr)
        {
            string result = string.Empty;

            if (bHdr)
            {
                result = "<th ";
            }
            else
            {
                result = "<td ";
            }

            if (iColSpan > 1)
            {
                result = string.Format(@"{0} colspan=""{1}""", result, iColSpan);
            }

            if (iRowSpan > 1)
            {
                result = string.Format(@"{0} rowspan=""{1}""", result, iRowSpan);
            }

            if (!string.IsNullOrEmpty(sHAl))
            {
                result = string.Format(@"{0} align=""{1}""", result, sHAl);
            }

            if (!string.IsNullOrEmpty(sVAl))
            {
                result = string.Format(@"{0} valign=""{1}""", result, sVAl);
            }

            if (bNoWrap)
            {
                result = string.Format(@"{0} nowrap=""nowrap""", result);
            }

            result = string.Format(@"{0}>{1}</{2}>", result, sTxt, bHdr ? "th" : "td");

            return result;
        }

        private string TOC()
        {
            string result = string.Empty;
            int i;
            string[] arr;
            if (!convertTOC)
            {
                result = string.Format(@"<div _wikiTOC="""" class=""wikiTOC"">{0}</div>", WikiUCResource.wikiTOCFakeCaption);
                return result;
            }

            if (intTOC > 1)
            {
                if (convertCode)
                    result = @"<div _wikiTOC="""" class=""TOC"">";
                for (i = 0; i < intTOC; i++)
                {
                    string[] arrTOC_i = arrTOC[i];
                    arr = new string[arrTOC_i.Length];
                    for (int arr_i = 0; arr_i < arrTOC_i.Length; arr_i++)
                    {
                        arr[arr_i] = arrTOC_i[arr_i];
                    }
                    if (Convert.ToInt32(arr[0]) <= intMaxTOC)
                    {
                        result = string.Format(@"{0}<div _wikiIgnore="""" class=""TOC{1}"" >", result, arr[0]);
                        result = string.Format(@"{0}<span _wikiIgnore="""" class=""TOCNr"">{1}</span>", result, arr[1]);
                        result = string.Format(@"{0}<a _wikiIgnore="""" href=""{1}#{2}"">{2}</a>", result, ActionHelper.GetViewPagePath(internalMainLink, selfPageName), arr[2]);
                        result = string.Format(@"{0}</div>", result);
                    }
                }
                result = string.Format(@"{0}</div>", result);
            }

            return result;
        }

        private string LinkTOC()
        {
            string result = string.Empty;
            int i;
            string strProt = string.Empty;

            if (intRef > 1)
            {
                result = @"<div _wikiLinkTOC="""" class=""LinkTOC"">";
                for (i = 0; i < intRef - 1; i++)
                {
                    if (arrRef[i].Contains(":"))
                    {
                        strProt = arrRef[i].Split(':')[0].ToLower();
                    }
                    if (!string.IsNullOrEmpty(strProt) && _validProtocols.Contains(strProt))
                    {
                        strProt = @"  target=""_blank""";
                    }
                    else
                    {
                        strProt = string.Empty;
                    }

                    result = string.Format(@"{0}<span _wikiIgnore="""" class=""LinkTOCNr"">[{1}]</span>", result, i);
                    result = string.Format(@"{0}<a _wikiIgnore="""" href=""{1}{2}"">{2}</a>", result, arrRef[i], strProt);
                }

                result = string.Format(@"{0}</div>", result);
            }

            return result;
        }


        public void SetExternalDBUrl(string name, string sVal)
        {
            dctDB.Add(name, sVal);
        }

        public void SetMaxTocLevel(int iVal)
        {
            if (iVal > 0)
            {
                intMaxTOC = iVal;
            }
        }

        #region Replace

        private string Replace(string expression, string find, string replacewith)
        {
            return expression.Replace(find, replacewith);
        }

        private string Replace(string expression, string find, string replacewith, int start)
        {
            return Replace(expression, find, replacewith, start, -1);
        }

        private string Replace(string expression, string find, string replacewith, int start, int count)
        {
            var result = new StringBuilder(expression);
            count = expression != null && count != -1 ? expression.IndexOf(find) + find.Length : result.Length;
            return result.Replace(find, replacewith, 0, count).ToString();
        }

        #endregion;

        private string ProcessSavePreCollection(string s)
        {
            preList = new List<TempContainer>();
            MatchCollection matches = rexPreContainer.Matches(s);
            TempContainer container;

            StringBuilder sb = new StringBuilder(s);
            foreach (Match m in matches)
            {
                var oldValue = m.Groups[1].Value.Trim();

                container = new TempContainer() { Content = string.IsNullOrEmpty(oldValue) ? m.Value : m.Value.Replace(oldValue, XLT(oldValue, XltType.Pre)) };
                sb.Replace(m.Value, container.TempId.ToString());
                preList.Add(container);
            }

            return sb.ToString();
        }

        private string ProcessSaveTableCollection(string s)
        {
            tableList = new List<TempContainer>();
            MatchCollection matches = rexTablesContainer.Matches(s);
            while (matches.Count > 0)
            {
                s = SaveTableItems(s);
                matches = rexTablesContainer.Matches(s);
            }

            //To Back ORDERED!!!!
            tableList.Reverse();
            return s;
        }

        private string SaveTableItems(string s)
        {
            MatchCollection matches = rexTablesContainer.Matches(s);
            TempContainer container;
            foreach (Match m in matches)
            {
                container = new TempContainer() { Content = m.Value.Trim() };
                tableList.Add(container);
            }

            StringBuilder sb = new StringBuilder(s);
            foreach (TempContainer cont in tableList)
            {
                sb.Replace(cont.Content, cont.TempId.ToString());
            }

            return sb.ToString();
        }

        private StringBuilder LoadPreItems(StringBuilder sb)
        {
            foreach (TempContainer cont in preList)
            {
                sb.Replace(cont.TempId.ToString(), cont.Content);
            }

            return sb;
        }

        private StringBuilder LoadTableItems(StringBuilder sb)
        {
            //Back ORDERED!!!!
            foreach (TempContainer cont in tableList)
            {
                sb.Replace(cont.TempId.ToString(), ProcessTables(cont.Content));
            }

            return sb;
        }

        private bool CellEntriesCanBeAdded(List<TempContainer> cellEntries, string value)
        {
            if (rexSimpleContent.IsMatch(value.Trim()))
            {
                return false;
            }

            if ((from cell in cellEntries where cell.Content.Equals(value) select cell).Count() > 0)
            {
                return false;
            }

            return true;
        }

        private string ProcessTables(string s)
        {
            List<TempContainer> linkList = new List<TempContainer>();
            s = SaveLinkTabItems(linkList, s);

            MatchCollection matches = rexTableCell.Matches(s);
            TempContainer container;
            string value = string.Empty;
            List<TempContainer> cellEntries = new List<TempContainer>();
            foreach (Match m in matches)
            {
                value = m.Groups[2].Value;
                if (!string.IsNullOrEmpty(value.Trim()) && CellEntriesCanBeAdded(cellEntries, value))
                {
                    container = new TempContainer() { Content = value };
                    cellEntries.Add(container);
                }
            }

            StringBuilder sb = new StringBuilder(s);
            foreach (TempContainer cont in cellEntries)
            {
                sb.Replace(cont.Content, cont.TempId.ToString() + "\n");
                cont.Content = cont.Content.Trim();
            }
            s = sb.ToString();

            /*s = XLT(s);*/
            //ProcessTableOnly!!!

            //s = LoadLinkTabItems(linkList, s);
            sb = new StringBuilder(XLT(s, XltType.Table));
            foreach (TempContainer cont in cellEntries)
            {
                sb.Replace(cont.TempId.ToString(), XLT(LoadLinkTabItems(linkList, cont.Content)));
            }

            return sb.ToString();
        }

        private string SaveLinkTabItems(List<TempContainer> linkContainerList, string s)
        {
            MatchCollection matches = rexLinkTabContainer.Matches(s);
            TempContainer container;
            foreach (Match m in matches)
            {
                container = new TempContainer() { Content = m.Value.Trim() };
                linkContainerList.Add(container);
            }

            StringBuilder sb = new StringBuilder(s);
            foreach (TempContainer cont in linkContainerList)
            {
                sb.Replace(cont.Content, cont.TempId.ToString());
            }

            return sb.ToString();
        }

        private string LoadLinkTabItems(List<TempContainer> linkContainerList, string s)
        {
            StringBuilder sb = new StringBuilder(s);
            foreach (TempContainer cont in linkContainerList)
            {
                sb.Replace(cont.TempId.ToString(), cont.Content);
            }

            return sb.ToString();
        }

        private string SaveLinkItems(List<TempContainer> linkContainerList, string s)
        {
            MatchCollection matches = rexLinkContainer.Matches(s);
            TempContainer container;
            foreach (Match m in matches)
            {
                container = new TempContainer() { Content = m.Value.Trim() };
                linkContainerList.Add(container);
            }

            StringBuilder sb = new StringBuilder(s);
            foreach (TempContainer cont in linkContainerList)
            {
                sb.Replace(cont.Content, cont.TempId.ToString());
            }

            return sb.ToString();
        }

        private string LoadLinkItems(List<TempContainer> linkContainerList, string s)
        {
            StringBuilder sb = new StringBuilder(s);
            foreach (TempContainer cont in linkContainerList)
            {
                sb.Replace(cont.TempId.ToString(), cont.Content);
            }

            return sb.ToString();
        }

        private string SaveNoWikiPart(List<TempContainer> noWikiList, string s)
        {
            MatchCollection matches = rexNoWiki.Matches(s);
            TempContainer container;
            foreach (Match m in matches)
            {
                container = new TempContainer() { Content = m.Value.Trim() };
                noWikiList.Add(container);
            }

            StringBuilder sb = new StringBuilder(s);
            foreach (TempContainer cont in noWikiList)
            {
                sb.Replace(cont.Content, string.Format(_convertNoWikitemplate, cont.TempId));
            }

            return sb.ToString();
        }


        private StringBuilder LoadNoWikiPart(List<TempContainer> noWikiList, StringBuilder sb)
        {
            foreach (TempContainer cont in noWikiList)
            {
                sb.Replace(string.Format(_convertNoWikitemplate, cont.TempId), cont.Content);
            }

            return sb;
        }


        private string ProcessSpecCategoryLinks(string source)
        {
            string result = source;

            Regex catReg = new Regex(RegExCategorySearch, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            List<string> categories = new List<string>();
            string cat, hCat;
            foreach (Match m in catReg.Matches(source))
            {
                cat = m.Value;
                result = result.Replace(cat, string.Empty);

                if (!categories.Exists(ct => ct.Equals(cat, StringComparison.InvariantCultureIgnoreCase)))
                {
                    cat = cat.Trim("[]".ToCharArray());
                    cat = cat.Split('|')[0];
                    cat = string.Format("[[{0}]]", cat);
                    hCat = cat;
                    if (hCat.IndexOf(':') > 0)
                    {
                        hCat = hCat.Trim("[]".ToCharArray());
                        hCat = hCat.Substring(hCat.IndexOf(':'));
                        hCat = hCat.TrimStart(":".ToCharArray());
                        hCat = hCat.Trim();
                        if (string.IsNullOrEmpty(hCat))
                            continue; //Unnamed categroy

                        cat = cat.Replace(hCat, hCat + "|" + hCat);
                    }
                    categories.Add(cat);
                }
            }
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(result);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(@"//div[@_wikiCatArea]");
            if (nodes != null)
            {
                foreach (HtmlNode node in nodes)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }

            if (categories.Count > 0)
            {
                result = string.Format("{0}\n\n<div _wikiCatArea=\"\" class=\"categoriesArea\">{2}: {1}</div>", result, string.Join(", ", categories.ToArray()),
                                       string.Format("[[{0}:{1} | {2}]]", Constants.WikiInternalKeyCaption, Constants.WikiInternalCategoriesKey, WikiUCResource.wikiInternalCategoriesKeyCaption));
            }

            return result;
        }

        private string XLT(string sSrc)
        {
            return XLT(sSrc, XltType.Normal);
        }

        private string XLT(string sSrc, XltType xltType)
        {
            List<TempContainer> linkContainerList = new List<TempContainer>();
            List<TempContainer> noWikiList = new List<TempContainer>();
            StringBuilder result = new StringBuilder();
            bool isParag = false, shouldParag = false;

            string s, ss, strNr, strHAl, strVAl;
            string[] arrLin, arr, arr2;
            int intLin, intDep, i, intColSpan, intRowSpan = 1;
            MatchCollection Matches;

            bool blnNoWrap, blnHdr, blnAddBR = false, blnAddTOC;

            if (string.IsNullOrEmpty(sSrc))
                return result.ToString();

            s = sSrc.TrimEnd(" \n\r".ToCharArray());
            if ((xltType & XltType.Table) == XltType.Normal)
            {
                s = SaveNoWikiPart(noWikiList, s);

                s = ProcessSpecCategoryLinks(s);

                s = ProcessSaveTableCollection(s);

                if (string.IsNullOrEmpty(s))
                    return result.ToString();

                //Init();
            }

            if ((xltType & XltType.Pre) == XltType.Normal)
            {
                s = ProcessSavePreCollection(s);
            }

            blnAddTOC = intMaxTOC > 0 && s.Contains("@@TOC@@");
            arrLin = s.Split('\n');
            s = string.Empty;

            for (intLin = 0; intLin < arrLin.Length; intLin++)
            {
                s = string.Format("{0}{1}", s, arrLin[intLin]).TrimEnd(" ".ToCharArray()).Replace("\r", "");

                //if (!string.IsNullOrEmpty(s) && s[0] == ' ')
                //{
                //    if (!blnCodeOpen)
                //    {
                //        s = s.Trim();
                //    }
                //}

                if ((xltType & XltType.Table) == XltType.Normal)
                {
                    if (shouldParag)
                    {
                        shouldParag = false;
                        if (!rexLin.IsMatch(s) || (rexLin.Matches(s).Count == 1 && string.IsNullOrEmpty(rexLin.Matches(s)[0].Groups[1].Value)))
                        {
                            isParag = true;
                        }
                    }
                }
                if (string.IsNullOrEmpty(s))
                {
                    if (intLin > 0) // Not First
                    {
                        if (!string.Empty.Equals(arrLin[intLin - 1].Trim()) && (intOpenTab == 0 || (blnCodeOpen || ((xltType & XltType.Pre) == XltType.Pre))))
                        {
                            //result = string.Format("{0}{1}<br />\n", result, CloseAllBlk());
                            shouldParag = true;
                            isParag = false;
                        }
                    }
                }
                else if (s[s.Length - 1] == '\\')
                {
                    s = s.Remove(s.Length - 1);
                }
                else if (s[0] == ' ' && !s.Trim().Equals(string.Empty))
                {
                    if (!blnCodeOpen && ((xltType & XltType.Pre) == XltType.Normal))
                    {
                        result = new StringBuilder(string.Format("{0}{1}<pre>", result, CloseAllBlk()));
                        blnCodeOpen = true;
                    }
                    result = new StringBuilder(string.Format("{0}{1}<br/>", result, s.Substring(1).Replace(" ", "&nbsp;")));
                    s = string.Empty;
                }
                else //Parse the line
                {
                    if ((xltType & XltType.Table) == XltType.Normal)
                    {
                        if (isParag)
                        {
                            isParag = false;
                            if (!(rexLin.IsMatch(s) && IsStringInternalWiki(s)))
                            {
                                s = string.Format("{0}{1}", StartBlk(1, 'P'), s);
                                if (blnCodeOpen)
                                {
                                    s = "</pre>\n" + s;
                                    blnCodeOpen = false;
                                }
                                //s = StartBlk(1, 'P') + s;
                            }
                            else if (blnCodeOpen && (rexLin.IsMatch(s) && IsStringInternalWiki(s)))
                            {
                                result.Append("</pre>\n");
                                blnCodeOpen = false;
                            }
                        }
                        else
                        {
                            if (blnCodeOpen && (rexLin.IsMatch(s) && IsStringInternalWiki(s)))
                            {
                                result.Append("</pre>\n");
                                blnCodeOpen = false;
                            }
                        }

                        s = SaveLinkItems(linkContainerList, s);

                        blnAddBR = false;
                        s = rexB.Replace(s, "<b>$1</b>");
                        s = rexI.Replace(s, "<i>$1</i>");
                        s = rexU.Replace(s, "<u>$1</u>");
                        s = rexS.Replace(s, "<strike>$1</strike>");

                        s = LoadLinkItems(linkContainerList, s);

                        Matches = rexLnk.Matches(s);

                        foreach (Match match in Matches)
                        {

                            if (!string.IsNullOrEmpty(match.Groups[7].Value))
                            {
                                s = Replace(s, match.Groups[0].Value, Ref(match.Groups[7].Value, match.Groups[8].Value, "_blank", match.Groups[7].Value), 0, 1);
                            }
                            else if (!string.IsNullOrEmpty(match.Groups[1].Value))
                            {
                                s = Replace(s, match.Groups[1].Value, Ref(match.Groups[1].Value, match.Groups[1].Value, string.Empty, match.Groups[1].Value), 0, 1);
                            }
                            else if (!string.IsNullOrEmpty(match.Groups[4].Value))
                            {
                                s = Replace(s, match.Groups[4].Value, Ref("mailto:" + match.Groups[4].Value, match.Groups[4].Value, match.Groups[4].Value, match.Groups[4].Value), 0, 1);
                            }
                                //else if (!string.IsNullOrEmpty(match.Groups[5].Value))
                                //{
                                //    arr = match.Groups[5].Value.Split('|');

                                //    if (!string.IsNullOrEmpty(dctDB["WIKI"]))
                                //    {
                                //        s = Replace(s, match.Groups[5].Value, Ref(dctDB["WIKI"].Replace("[1]", arr[5]), arr[arr.Length - 1], string.Empty, match.Groups[5].Value), 0, 1);
                                //    }
                                //    else
                                //    {
                                //        s = Replace(s, match.Groups[5].Value, arr[arr.Length - 1], 0, 1);
                                //    }
                                //}
                            else if (!string.IsNullOrEmpty(match.Groups[5].Value))
                            {
                                s = Replace(s, match.Groups[0].Value, Ref(match.Groups[5].Value.Split('|')[0], match.Groups[5].Value.Split('|').Length > 1 ? match.Groups[5].Value.Remove(0, match.Groups[5].Value.Split('|')[0].Length).TrimStart('|') : match.Groups[5].Value, "_blank", match.Groups[5].Value), 0, 1);
                            }
                            else if (!string.IsNullOrEmpty(match.Groups[0].Value))
                            {
                                s = Replace(s, match.Groups[0].Value, Ref(match.Groups[6].Value, match.Groups[2].Value, "_blank", match.Groups[0].Value), 0, 1);
                            }
                            else if (!string.IsNullOrEmpty(match.Groups[6].Value))
                            {
                                s = Replace(s, match.Groups[6].Value, Ref(match.Groups[6].Value, match.Groups[7].Value, "_blank", match.Groups[6].Value), 0, 1);
                            }
                        }

                        Matches = rexDB.Matches(s);
                        foreach (Match match in Matches)
                        {
                            if (!string.IsNullOrEmpty(match.Groups[0].Value)) //ISBN
                            {
                                ss = dctDB["ISBN"].Replace("[1]", match.Groups[1].Value.Replace("-", ""));
                                s = Replace(s, match.Groups[0].Value, Ref(ss, match.Groups[0].Value, "_blank", string.Empty), 0, 1);
                            }
                            else if (!string.IsNullOrEmpty(match.Groups[2].Value)) //{{...}}
                            {
                                arr = match.Groups[2].Value.Split('|');
                                if (arr.Length == 1) //Text
                                {
                                    s = Replace(s, match.Groups[2].Value, arr[0], 0, 1);
                                }
                                else if (arr.Length > 1) //{{DB|expression}}
                                {
                                    if (!string.IsNullOrEmpty(dctDB[arr[0]])) // DB is definied
                                    {
                                        ss = dctDB[arr[0]].Replace("[1]", arr[1]);

                                        if (arr.Length > 2)
                                        {
                                            s = Replace(s, match.Groups[2].Value, Ref(ss, arr[2], "_blank", string.Empty), 0, 1);
                                        }
                                        else
                                        {
                                            s = Replace(s, match.Groups[2].Value, Ref(ss, arr[0] + " " + arr[1], "_blank", string.Empty), 0, 1);
                                        }
                                    }
                                    else
                                    {
                                        if (arr.Length > 2)
                                        {
                                            s = Replace(s, match.Groups[2].Value, arr[2], 0, 1);
                                        }
                                        else
                                        {
                                            s = Replace(s, match.Groups[2].Value, arr[0] + " " + arr[1], 0, 1);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Matches = rexLin.Matches(s);

                    foreach (Match match in Matches)
                    {
                        intDep = match.Groups[1].Length;

                        if (string.IsNullOrEmpty(match.Groups[0].Value))
                        {
                            s = CloseAllBlk() + match.Groups[1].Value.Trim();
                            blnAddBR = true;
                        }
                        else if (intDep > 0)
                        {
                            string first2;

                            if (intDep > 1)
                            {
                                first2 = match.Groups[1].Value.Substring(0, 2);
                            }
                            else
                            {
                                first2 = match.Groups[1].Value.Substring(0, 1);
                            }

                            if (first2.Equals("<<"))
                            {
                                s = CloseAllBlk() + @"<div align=""left"">" + match.Groups[2].Value + "</div>";
                            }
                            else if (first2.Equals("><"))
                            {
                                s = CloseAllBlk() + @"<div align=""center"">" + match.Groups[2].Value + "</div>";
                            }
                            else if (first2.Equals(">>"))
                            {
                                s = CloseAllBlk() + @"<div align=""right"">" + match.Groups[2].Value + "</div>";
                            }
                            else if (first2.Equals("<>"))
                            {
                                s = CloseAllBlk() + @"<div align=""justify"">" + match.Groups[2].Value + "</div>";
                            }
                            else if (first2[0] == '!' && intOpenTab == 0) // alternative table syntax
                            {
                                s = StartBlk(1, 'T') + "  <tr>\n";
                                arr = match.Groups[2].Value.Trim().Split('!');
                                intColSpan = 1;
                                for (i = 0; i < arr.Length; i++)
                                {
                                    if (string.IsNullOrEmpty(arr[i].Trim()))
                                    {
                                        intColSpan++;
                                    }
                                    else
                                    {
                                        strHAl = string.Empty;
                                        strVAl = "top";
                                        blnNoWrap = false;
                                        blnHdr = false;


                                        if (arr[i].Substring(0, 1).Equals("+"))
                                        {
                                            blnHdr = true;
                                            arr[i] = arr[i].Substring(1).Trim();
                                        }

                                        if (arr[i].Substring(0, 2).Equals("<<"))
                                        {
                                            strHAl = "left";
                                            arr[i] = arr[i].Substring(2).Trim();
                                        }

                                        if (arr[i].Substring(0, 2).Equals(">>"))
                                        {
                                            strHAl = "right";
                                            arr[i] = arr[i].Substring(2).Trim();
                                        }

                                        if (arr[i].Substring(0, 2).Equals("><"))
                                        {
                                            strHAl = "center";
                                            arr[i] = arr[i].Substring(2).Trim();
                                        }

                                        if (arr[i].Substring(0, 2).Equals("<>"))
                                        {
                                            strHAl = "justify";
                                            arr[i] = arr[i].Substring(2).Trim();
                                        }

                                        if (arr[i].Substring(0, 1).Equals("^"))
                                        {
                                            blnNoWrap = true;
                                            arr[i] = arr[i].Substring(1).Trim();
                                        }

                                        s = string.Format("{0}{1}", s, Cell(arr[i], intColSpan, intRowSpan, strHAl, strVAl, blnNoWrap, blnHdr));
                                        intColSpan = 1;
                                        intRowSpan = 1;
                                    }
                                    s = string.Format("{0} </tr>", s);
                                }
                            }
                            else if (first2.Equals("{|")) //Table Strart MediaWiki format.
                            {
                                intOpenTab++;
                                s = string.Format("<table {0}><tr>\n", match.Groups[2].Value);
                                if (intOpenTab > 1)
                                {
                                    s = string.Format("<td>\n{0}", s);
                                }
                            }
                            else if (first2.Equals("|}")) //table end
                            {
                                intOpenTab--;
                                s = "</tr></table>";
                                if (intOpenTab > 0)
                                {
                                    s += "</td>";
                                }
                            }
                            else if (first2.Equals("|+")) //table caption
                            {
                                s = string.Format("<caption>{0}</caption>", match.Groups[2].Value.Trim());
                            }
                            else if (first2.Equals("|-")) //table new row
                            {
                                s = "</tr><tr>";
                            }
                            else if (first2[0] == '|' || first2[0] == '!') //table cell
                            {
                                var node = first2[0] == '|' ? "td" : "th";
                                arr = match.Groups[2].Value.Split(new string[] { "|!" }, StringSplitOptions.RemoveEmptyEntries);
                                s = string.Empty;
                                if (arr.Length == 0)
                                {
                                    s = string.Format("{0}<{1}>&nbsp;</{1}>", s, node);
                                }
                                else
                                {
                                    for (i = 0; i < arr.Length; i++)
                                    {
                                        arr2 = arr[i].Split('|');
                                        //shouldCloaseAll = false;
                                        if (arr2.Length == 1)
                                        {
                                            s = string.Format("{0}<{2}>{1}</{2}>", s, arr2[0].Trim() /*XLT(arr2[0].Trim())*/, node);
                                        }
                                        else if (arr2.Length == 2)
                                        {
                                            s = string.Format("{0}<{3} {1}>{2}</{3}>", s, arr2[0].Trim(), arr2[1].Trim() /*XLT(arr2[1].Trim())*/, node);
                                        }
                                        //shouldCloaseAll = true;
                                    }
                                }
                            }
                            else if (first2[0] == ':')
                            {
                                if (string.IsNullOrEmpty(match.Groups[2].Value.Trim())) //ident
                                {
                                    s = "<br />\n<br />\n";
                                }
                                else if (GetLastBlk() == 'D')
                                {
                                    s = StartBlk(intDep + 1, 'L') + match.Groups[2].Value.Trim();
                                }
                                else
                                {
                                    s = StartBlk(intDep, 'B') + match.Groups[2].Value.Trim();
                                }
                            }
                            else if (first2[0] == '*') //unordered
                            {
                                s = string.Format("{0}<li>{1}</li>", StartBlk(intDep, 'U'), match.Groups[2].Value.Trim());
                            }
                            else if (first2[0] == '#') //ordered
                            {
                                s = string.Format("{0}<li>{1}</li>", StartBlk(intDep, 'O'), match.Groups[2].Value.Trim());
                            }
                            else if (first2[0] == ';') //definition
                            {
                                s = StartBlk(intDep, 'D');
                                arr = match.Groups[2].Value.Trim().Split(':');
                                s = string.Format("{0}<dt>{1}</dt>", s, arr[0]);
                                if (arr.Length > 1)
                                {
                                    s = string.Format("{0}<dd>{1}</dd>", s, arr[1]);
                                }
                            }
                            else if (first2[0] == '=') //Headers
                            {
                                ss = match.Groups[2].Value.Trim();
                                ss = ss.Replace(new String('=', intDep), "");

                                if (string.IsNullOrEmpty(ss))
                                {
                                    ss = "&nbsp;";
                                }

                                s = CloseAllBlk();
                                if (blnAddTOC)
                                {
                                    s = string.Format(@"{0}<a _wikiIgnore="""" class=""hAncor"" name=""{1}""></a>", s, ss.Trim('#').Trim());
                                }
                                s = string.Format("{0}<h{1}>", s, intDep);

                                strNr = Autonumber(intDep);

                                if (ss[0] == '#')
                                {
                                    s += strNr;
                                    ss = ss.Substring(0).Trim();
                                }

                                //save sup and sub
                                ss = ss.Replace("<sup>", "&lt;sup&gt;").Replace("</sup>", "&lt;/sup&gt;");
                                ss = ss.Replace("<sub>", "&lt;sub&gt;").Replace("</sub>", "&lt;/sub&gt;");

                                //Trim tags
                                ss = tags.Replace(ss, string.Empty);

                                //save sup and sub
                                ss = ss.Replace("&lt;sup&gt;", "<sup>").Replace("&lt;/sup&gt;", "</sup>");
                                ss = ss.Replace("&lt;sub&gt;", "<sub>").Replace("&lt;/sub&gt;", "</sub>");

                                //Trim Spaces
                                ss = spaces.Replace(ss, " ").Trim();

                                arrTOC[intTOC] = new string[] { intDep.ToString(), strNr.Trim(), HttpUtility.HtmlEncode(ss) };

                                s = string.Format(@"{0}{3}{1}</h{2}>", s, ss, intDep,
                                                  canBeEdit ? string.Format(@"<span class=""editsection"">{0}</span>",
                                                                            string.Format(@"<a href=""{0}"" title=""{2}"">{1}</a>",
                                                                                          string.Format(@"{0}&section={1}", ActionHelper.GetEditPagePath(internalMainLink, selfPageName), intTOC),
                                                                                          WikiUCResource.EditSectionCaption,
                                                                                          string.Format(WikiUCResource.EditSectionTitleFormat, ss)))
                                                      : string.Empty);
                                intTOC++;


                            }
                            else if (first2[0] == '-') //horizontal ruler
                            {
                                s = CloseAllBlk() + "<hr/>";
                            }
                        }
                        else
                        {
                            s = CloseAllBlkExP() + s;
                        }
                    }

                    if ((xltType & XltType.Table) == XltType.Normal)
                    {
                        Matches = rexSpetial.Matches(s);

                        foreach (Match match in Matches)
                        {
                            var replaceWidth = string.Empty;
                            switch (match.Groups[2].Value.ToLower())
                            {
                                case "br":
                                    replaceWidth = "<br />";
                                    break;
                            }

                            s = s.Replace(match.Groups[1].Value, replaceWidth);
                        }
                    }
                    result.Append(s);
                    if (intLin < arrLin.Length - 1)
                    {
                        if (blnAddBR)
                        {
                            result.Append("<br />");
                        }

                        result.Append("\n");
                    }
                    s = string.Empty;
                }
            }

            result.Append(CloseAllBlk());

            if ((xltType & XltType.Table) == XltType.Normal)
            {
                string sResult = result.ToString();
                if (intMaxTOC > 0 && sResult.IndexOf("@@TOC@@") >= 0) //insert TOC
                {
                    result = result.Replace("@@TOC@@", TOC());
                }

                if (sResult.IndexOf("@@LinkTOC@@") >= 0)
                {
                    result = result.Replace("@@LinkTOC@@", LinkTOC());
                }

                result = LoadTableItems(result);
                result = new StringBuilder(rexEmptyTags.Replace(result.ToString(), string.Empty));
                result = LoadNoWikiPart(noWikiList, result);
                if (convertCode)
                {
                    result = new StringBuilder(HighLightCode(result.ToString()));
                }
            }

            if ((xltType & XltType.Pre) == XltType.Normal)
            {
                result = LoadPreItems(result);
            }

            return result.ToString();
        }

        private static string EscapeHtml(string src)
        {
            src = src.Replace("&", "&amp;");
            src = src.Replace("<", "&lt;");
            src = src.Replace(">", "&gt;");

            return src;
        }

        private static string HighLightCode(string source)
        {
            //Escape HTML Symbols in code Tags

            var mcCode = rexCode.Matches(source);
            foreach (Match m in mcCode)
            {
                try
                {
                    var sCode = m.Groups[1].Value.Substring(m.Groups[1].Value.IndexOf(">", StringComparison.Ordinal) + 1);
                    source = source.Replace(sCode, EscapeHtml(sCode));
                }
                catch (Exception)
                {
                }
            }

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(source);
                var scripts = doc.DocumentNode.SelectNodes("//code");
                if (scripts != null)
                {
                    foreach (var node in scripts)
                    {
                        var textNode = doc.CreateTextNode(Highlight.HighlightToHTML(node.InnerHtml, GetLanguage(node), true).Replace(@"class=""codestyle""", string.Format(@"class=""codestyle"" _wikiCodeStyle=""{0}""", GetLanguageAttrValue(node))));
                        node.ParentNode.ReplaceChild(textNode, node);
                    }
                }
                return doc.DocumentNode.InnerHtml;
            }
            catch (Exception)
            {
                return source;
            }
        }

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

        private const string WikiLineSymbols = @"<< >> <> >< ! {| |} |+ |- | ^ * # ; = - ~~";

        private static bool IsStringInternalWiki(string str)
        {
            var ss = str.Trim();
            return WikiLineSymbols.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Any(ss.StartsWith);
        }
    }
}