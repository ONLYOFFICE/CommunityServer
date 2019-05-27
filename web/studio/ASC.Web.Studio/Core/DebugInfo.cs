/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Web.Core.Files;

namespace ASC.Web.Studio.Core
{
    public class DebugInfo
    {
        private static readonly string ChangeLogPatternFilePath = string.Concat(HttpContext.Current.Server.MapPath("~/"), "change.log");
        private static readonly string ChangeLogFilePath = string.Concat(HttpContext.Current.Server.MapPath("~/"), "changelog.xml");
        private static readonly string debugString;
        public static bool ShowDebugInfo;

        static DebugInfo()
        {
            try
            {
                var basePath = HttpContext.Current.Server.MapPath("~/");
                ChangeLogPatternFilePath = Path.Combine(basePath, "change.log");
                ChangeLogFilePath = Path.Combine(basePath, "changelog.xml");
#if DEBUGINFO
                ShowDebugInfo = File.Exists(ChangeLogPatternFilePath) && File.Exists(ChangeLogFilePath);
                debugString = GetStaticDebugString();
#else
                ShowDebugInfo = false;
                debugString = "";
#endif

            }
            catch (Exception)
            {
                
            }
        }

        private static string GetStaticDebugString()
        {
            var xmlLog = new XmlDocument();
            xmlLog.Load(ChangeLogFilePath);

            var logs = xmlLog.SelectNodes("//changeSet//item");
            if (logs == null) return "";
            var nodes = logs.Cast<XmlNode>().ToList();

            try
            {
                var fileContent = File.ReadAllText(ChangeLogPatternFilePath, Encoding.Default);
                fileContent = fileContent.Replace("{BuildVersion}", xmlLog.GetElementsByTagName("number")[0].InnerText);
                fileContent = fileContent.Replace("{BuildDate}", new DateTime(1970, 1, 1).AddMilliseconds(Convert.ToInt64(xmlLog.GetElementsByTagName("timestamp")[0].InnerText)).ToString("yyyy-MM-dd hh:mm"));
                fileContent = fileContent.Replace("{DocServiceApi}", FilesLinkUtility.DocServiceApiUrl.HtmlEncode());
                fileContent = fileContent.Replace("{DocServiceCommand}", FilesLinkUtility.DocServiceCommandUrl.HtmlEncode());
                fileContent = fileContent.Replace("{DocServiceConverter}", FilesLinkUtility.DocServiceConverterUrl.HtmlEncode());
                fileContent = fileContent.Replace("{DocServiceDocbuilder}", FilesLinkUtility.DocServiceDocbuilderUrl.HtmlEncode());

                var firstCommitIDNode = nodes.FirstOrDefault();
                if (firstCommitIDNode != null)
                {
                    var firstCommitID = firstCommitIDNode.SelectSingleNode("commitId");
                    if (firstCommitID != null)
                    {
                        fileContent = fileContent.Replace("{RevisionFirst}", firstCommitID.InnerText);
                    }
                }
                else
                {
                    fileContent = fileContent.Replace("{RevisionFirst}", "");
                }
                var lastCommitIDNode = nodes.LastOrDefault();
                if (lastCommitIDNode != null)
                {
                    var lastCommitID = lastCommitIDNode.SelectSingleNode("commitId");
                    if (lastCommitID != null)
                    {
                        fileContent = fileContent.Replace("{RevisionLast}", lastCommitID.InnerText);
                    }
                }
                else
                {
                    fileContent = fileContent.Replace("{RevisionLast}", "");
                }

                fileContent += GetChangeLogData(nodes).HtmlEncode();

                return fileContent;
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC").Error("DebugInfo", e);
            }

            return "";
        }

        private static string GetChangeLogData(IEnumerable<XmlNode> logs)
        {
            var hashTable = new Dictionary<string, HashSet<string>>();

            foreach (var log in logs)
            {
                var comment = log.SelectSingleNode("comment");
                if (comment == null || IsServiceLogItem(comment.InnerText)) continue;

                var author = log.SelectSingleNode("author//fullName");
                if (author == null) continue;

                var commentText = comment.InnerText.Replace("\n", " ") + "\n";
                if (!hashTable.ContainsKey(author.InnerText))
                    hashTable.Add(author.InnerText, new HashSet<string> { "author: " + author.InnerText + Environment.NewLine, commentText });
                else
                    hashTable[author.InnerText].Add(commentText);
            }

            return string.Join(Environment.NewLine, hashTable.Select(r => string.Join("", r.Value)).OrderBy(r => r));
        }

        private static bool IsServiceLogItem(string log)
        {
            return Regex.IsMatch(log, "(^Merged)|(^Sql)", RegexOptions.IgnoreCase);
        }

        public static string DebugString
        {
            get
            {
                try
                {
                    return debugString.Replace("{User}", SecurityContext.CurrentAccount.ToString().HtmlEncode())
                    .Replace("{UserAgent}", HttpContext.Current.Request.UserAgent)
                    .Replace("{Url}", HttpContext.Current.Request.Url.ToString().HtmlEncode())
                    .Replace("{RewritenUrl}", HttpContext.Current.Request.GetUrlRewriter().ToString().HtmlEncode());
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("ASC").Error("DebugInfo", e);
                }

                return "";
            }
        }
    }
}
