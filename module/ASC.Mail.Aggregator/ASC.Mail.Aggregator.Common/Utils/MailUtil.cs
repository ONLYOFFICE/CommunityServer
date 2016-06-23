/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ASC.Core;
using ASC.Mail.Aggregator.Common.Imap;
using ASC.Mail.Aggregator.Common.Logging;
using DDay.iCal;
using HtmlAgilityPack;
using MimeKit;
using Newtonsoft.Json;

namespace ASC.Mail.Aggregator.Common.Utils
{
    public static class MailUtil
    {
        private static readonly Regex RegexSubjectPrefix =
            new Regex(
                "([\\[\\(]\\s*)?((?<=(\\A)|\\s|\\^|\\:|\\(|\\[)(RE?S?|FYI|RIF|I|FS|VB|RV|ENC|ODP|PD|YNT|ILT|SV|VS|VL|BLS|TRS?|AW|WG|ΑΠ|ΣΧΕΤ|ΠΡΘ|ANTW|DOORST|НА|תגובה|הועבר|主题|转发|FWD?))(\\[\\d+\\])* *([-:;)\\]][ :;\\])-]*|$)|\\]+ *$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RegexSubjectAbbreviationsSplit = new Regex("\\[.*\\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RegexSubjectAbbreviations = new Regex("([A-Z]+[\\s\\/]*[A-Z]+)+",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RegexSurrogateCodePoint = new Regex(@"\&#\d+;",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        private static readonly string RegexSearchInvalidChars = new string(Path.GetInvalidFileNameChars()) +
                                                                 new string(Path.GetInvalidPathChars());

        private static readonly Regex RegexInvalidCharsInPath =
            new Regex(string.Format("[{0}]", Regex.Escape(RegexSearchInvalidChars)),
                RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public static List<int> GetLabelsFromString(string stringLabel)
        {
            var list = new List<int>();
            if (!string.IsNullOrEmpty(stringLabel))
            {
                var labels = stringLabel.Split(',');
                foreach (var label in labels)
                {
                    int labelIn;
                    if (int.TryParse(label, out labelIn))
                    {
                        list.Add(labelIn);
                    }
                }
            }
            return list;
        }

        public static string GetStringFromLabels(List<int> labels)
        {
            if (labels != null)
            {
                return string.Join(",", labels.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray());
            }
            return string.Empty;
        }

        public static string GetJsonString(object data)
        {
            string jsonData = null;
            if (data != null)
            {
                using (var stream = new MemoryStream())
                {
                    var serializer = new DataContractJsonSerializer(data.GetType());
                    serializer.WriteObject(stream, data);
                    stream.Position = 0;
                    using (var reader = new StreamReader(stream))
                    {
                        jsonData = reader.ReadToEnd();
                    }
                }
            }
            return jsonData;
        }

        public static double BytesToMegabytes(long bytes)
        {
            return Math.Round(bytes/1024d/1024d, 1);
        }

        public static Dictionary<string, ImapFolderUids> ParseImapIntervals(string json)
        {
            var text = json;

            if (json.StartsWith("["))
            {
                text =
                    text.Replace("\"Key\":", "")
                        .Replace(",\"Value\"", "")
                        .Replace("\"__type\":\"ImapFolderUids:#ASC.Mail.Aggregator.Common.Imap\",", "")
                        .Replace("},{", ",");
                text = text
                    .Substring(1, text.Length - 2);
            }

            return
                (Dictionary<string, ImapFolderUids>)
                    JsonConvert.DeserializeObject(text, typeof(Dictionary<string, ImapFolderUids>));
        }

        public static string NormalizeSubject(string subject)
        {
            subject = RegexSubjectPrefix.Replace(subject, "");

            var matches = RegexSubjectAbbreviationsSplit.Matches(subject);

            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i].Value.Trim('[', ']');

                if (RegexSubjectAbbreviations.IsMatch(match))
                {
                    subject = subject.Replace(matches[i].Value, "");
                }
            }

            return subject.Trim();
        }

        /// <summary>
        /// Removes control characters and other non-UTF-8 characters
        /// </summary>
        /// <param name="inString">The string to process</param>
        /// <return>A string with no control characters or entities above 0x00FD</return>
        public static string NormalizeStringForMySql(string inString)
        {
            if (string.IsNullOrEmpty(inString))
                return inString;

            var newString = new StringBuilder(inString.Length);

            foreach (var ch in inString.Where(XmlConvert.IsXmlChar))
                newString.Append(ch);

            return newString.ToString();
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static IICalendarCollection ParseICalendar(string icalFormat)
        {
            using (TextReader sr = new StringReader(icalFormat))
            {
                return iCalendar.LoadFromStream(sr);
            }
        }

        public static IICalendar ParseValidCalendar(string icalFormat)
        {
            try
            {
                var calendars = ParseICalendar(icalFormat);

                if (calendars.Count == 1 &&
                    calendars[0].Version == "2.0" &&
                    (calendars[0].Method == "REQUEST" ||
                     calendars[0].Method == "REPLY" ||
                     calendars[0].Method == "CANCEL") &&
                    calendars[0].Events.Count == 1 &&
                    !string.IsNullOrEmpty(calendars[0].Events[0].UID))
                {
                    return calendars[0];
                }

            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        public static string SerializeCalendar(IICalendar calendar)
        {
            try
            {
                var context = new DDay.iCal.Serialization.SerializationContext();
                var factory = new DDay.iCal.Serialization.iCalendar.SerializerFactory();
                var serializer = factory.Build(calendar.GetType(), context) as DDay.iCal.Serialization.IStringSerializer;
                return serializer != null ? serializer.SerializeToString(calendar) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string CreateFullEmail(string displayName, string email)
        {
            return !string.IsNullOrEmpty(displayName) ? string.Format("{0} <{1}>", Quote(displayName), email) : email;
        }

        /// <summary>
        /// Quotes the specified text.
        /// </summary>
        /// <remarks>
        /// Quotes the specified text, enclosing it in double-quotes and escaping
        /// any backslashes and double-quotes within.
        /// </remarks>
        /// <returns>The quoted text.</returns>
        /// <param name="text">The text to quote.</param>
        /// <param name="skipFirstAndLastQuotes">skip addition of first and last quotes</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="text"/> is <c>null</c>.
        /// </exception>
        public static string Quote(string text, bool skipFirstAndLastQuotes = false)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            var quoted = new StringBuilder(text.Length + 2, (text.Length*2) + 2);

            if (!skipFirstAndLastQuotes)
                quoted.Append("\"");
            foreach (var t in text)
            {
                if (t == '\\' || t == '"')
                    quoted.Append('\\');
                quoted.Append(t);
            }
            if (!skipFirstAndLastQuotes)
                quoted.Append("\"");

            return quoted.ToString();
        }

        /// <summary>
        /// Unquotes the specified text.
        /// </summary>
        /// <remarks>
        /// Unquotes the specified text, removing any escaped backslashes within.
        /// </remarks>
        /// <returns>The unquoted text.</returns>
        /// <param name="text">The text to unquote.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="text"/> is <c>null</c>.
        /// </exception>
        public static string Unquote(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            var index = text.IndexOfAny(new[] {'\r', '\n', '\t', '\\', '"'});

            if (index == -1)
                return text;

            var builder = new StringBuilder();
            var escaped = false;
            var quoted = false;

            foreach (var t in text)
            {
                switch (t)
                {
                    case '\r':
                    case '\n':
                        escaped = false;
                        break;
                    case '\t':
                        builder.Append(' ');
                        escaped = false;
                        break;
                    case '\\':
                        if (escaped)
                            builder.Append('\\');
                        escaped = !escaped;
                        break;
                    case '"':
                        if (escaped)
                        {
                            builder.Append('"');
                            escaped = false;
                        }
                        else
                        {
                            quoted = !quoted;
                        }
                        break;
                    default:
                        builder.Append(t);
                        escaped = false;
                        break;
                }
            }

            return builder.ToString();
        }

        //if limit_length < 1 then text will be unlimited
        /// <summary>
        /// Extract Text from Html string
        /// </summary>
        /// <param name="html">Html string</param>
        /// <param name="text">[out] Text without html tags</param>
        /// <param name="maxLength" optional="true">max length of text</param>
        /// <returns>true if success</returns>
        public static bool TryExtractTextFromHtml(string html, out string text, int maxLength = 0)
        {
            text = "";
            try
            {
                text = ExtractTextFromHtml(html, maxLength);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //if limit_length < 1 then text will be unlimited
        /// <summary>
        /// Extract Text from Html string
        /// </summary>
        /// <param name="html">Html string</param>
        /// <param name="maxLength" optional="true">max length of text</param>
        /// <returns>Text without html tags</returns>
        /// <exception cref="RecursionDepthException">Exception happens when in html contains too many recursion of unclosed tags.</exception>
        public static string ExtractTextFromHtml(string html, int maxLength = 0)
        {
            if (string.IsNullOrEmpty(html))
                return "";

            var limit = maxLength < 1 ? html.Length : maxLength;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var outText = new StringBuilder(limit);
            ConvertTo(doc.DocumentNode, outText, limit);
            return outText.ToString();
        }

        private static void ConvertTo(HtmlNode node, StringBuilder outText, int limitLength)
        {
            if (outText.Length >= limitLength) return;

            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // Comments will not ouput.
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText, limitLength);
                    break;

                case HtmlNodeType.Text:
                    // Scripts and styles will not ouput.
                    var parentName = node.ParentNode.Name.ToLowerInvariant();
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    var html = ((HtmlTextNode) node).Text;

                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    html = html.Replace("&nbsp;", "");

                    if (html.Trim().Length > 0)
                    {
                        html = RegexSurrogateCodePoint.Replace(html, "");
                        var text = HtmlEntity.DeEntitize(html);

                        if (parentName == "a" &&
                            node.ParentNode.HasAttributes &&
                            node.ParentNode.Attributes["href"] != null &&
                            !text.Equals(node.ParentNode.Attributes["href"].Value,
                                StringComparison.InvariantCultureIgnoreCase))
                        {
                            text = string.Format("{0} ({1})", text, node.ParentNode.Attributes["href"].Value);
                        }

                        if (parentName == "p")
                            text += "\r\n";

                        var newLength = outText.Length + text.Length + 2;
                        if (limitLength > 0 && newLength >= limitLength)
                        {
                            text += "\r\n";
                            if (newLength != limitLength)
                                text = text.Substring(0, text.Length - (newLength - limitLength));
                            outText.Append(text);
                            return;
                        }

                        outText.AppendLine(text);
                    }
                    break;
                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "br":
                        case "p":
                            outText.Append("\r\n");
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText, limitLength);
                    }
                    break;
            }
        }

        private static void ConvertContentTo(HtmlNode node, StringBuilder outText, int limitLength)
        {
            foreach (var subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText, limitLength);
            }
        }

        public static bool HasUnsubscribeLink(string html)
        {
            if (string.IsNullOrEmpty(html))
                return false;

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var hasUnsubscribe = doc.DocumentNode.SelectNodes("//a[@href]")
                    .Any(
                        link =>
                            link.Attributes["href"].Value
                                .IndexOf("unsubscribe", StringComparison.OrdinalIgnoreCase) != -1);

                return hasUnsubscribe || html.IndexOf("unsubscribe", StringComparison.OrdinalIgnoreCase) != -1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Return encoding based on encoding name
        /// Tries to resolve some wrong-formatted encoding names
        /// </summary>
        /// <param name="encodingName"></param>
        /// <returns></returns>
        public static Encoding GetEncoding(string encodingName)
        {
            var encoding = Encoding.UTF8;

            if (string.IsNullOrEmpty(encodingName))
                return encoding;

            try
            {
                encoding = Encoding.GetEncoding(encodingName);
            }
            catch
            {
                encoding = EncodingTools.GetEncodingByCodepageName(encodingName);

                if (encoding != null)
                    return encoding;

                try
                {
                    if (encodingName.ToUpper() == "UTF8")
                        encodingName = "UTF-8";
                    else if (encodingName.StartsWith("ISO") && char.IsDigit(encodingName, 3))
                        encodingName = encodingName.Insert(3, "-");
                    encodingName = encodingName.Replace("_", "-").ToUpper();
                    encoding = Encoding.GetEncoding(encodingName);
                }
                catch
                {
                    encoding = Encoding.UTF8;
                }
            }

            return encoding;
        }

        public static string CreateStreamId()
        {
            var streamId = Guid.NewGuid().ToString("N").ToLower();
            return streamId;
        }

        /// <summary>
        /// Creates Rfc 2822 3.6.4 message-id. Syntax: id-left '@' id-right
        /// </summary>
        public static string CreateMessageId(ILogger log = null)
        {
            if (log == null)
                log = new NullLogger();

            var domain = "";

            try
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant();

                if (tenant != null)
                    domain = tenant.GetTenantDomain();
            }
            catch (Exception ex)
            {
                log.Error("CreateMessageId failed: Exception\r\n{0}\r\n", ex.ToString());
            }

            if (string.IsNullOrEmpty(domain))
                domain = System.Net.Dns.GetHostName();

            return string.Format("AU{0}@{1}", GetUniqueString(), domain);
        }

        public static string GetUniqueString()
        {
            return string.Format("{0}{1}{2}{3}", System.Diagnostics.Process.GetCurrentProcess().Id,
                DateTime.Now.ToString("yyMMddhhmmss"), DateTime.Now.Millisecond, new Random().GetHashCode());
        }

        public static string ReplaceInvalidFilePathCharacters(string filename, string replacement = "_")
        {
            var newFilename = RegexInvalidCharsInPath.Replace(filename, replacement);
            return newFilename;
        }

        public static string ImproveFilename(string filename, ContentType contentType)
        {
            try
            {
                var validName = ReplaceInvalidFilePathCharacters(filename);

                if (string.IsNullOrEmpty(validName))
                    validName = "attachment";

                var ext = Path.GetExtension(validName);

                if (validName.Length > 255)
                {
                    var name = Path.GetFileNameWithoutExtension(validName);
                    name = name.Substring(0, 255 - (!string.IsNullOrEmpty(ext) ? ext.Length : 0));
                    validName = name + ext;
                }

                if (!string.IsNullOrEmpty(ext) || contentType == null)
                    return validName;

                // If the file extension is not specified, there will be issues with saving on s3

                var newExt = MimeTypeMap.GetExtension(contentType.MimeType);

                validName = Path.ChangeExtension(validName, newExt);

                return validName;
            }
            catch (Exception)
            {
                // skip
            }

            return string.IsNullOrEmpty(filename) ? "attachment.ext" : filename;
        }
    }
}