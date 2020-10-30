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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Imap;
using ASC.Security.Cryptography;
using HtmlAgilityPack;
using MimeKit;
using Newtonsoft.Json;
using File = System.IO.File;

namespace ASC.Mail.Utils
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
            string jsonData;

            if (data == null)
                return null;

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

        public static Ical.Net.CalendarCollection ParseICalendar(string icalFormat)
        {
            using (TextReader sr = new StringReader(icalFormat))
            {
                return Ical.Net.CalendarCollection.Load(sr);
            }
        }

        public static Ical.Net.Calendar ParseValidCalendar(string icalFormat, ILog log = null)
        {
            log = log ?? new NullLog();

            try
            {
                var calendars = ParseICalendar(icalFormat);

                if (!calendars.Any())
                    throw new InvalidDataException("Calendars not found");

                if(calendars.Count > 1)
                    throw new InvalidDataException("Too many calendars");

                var calendar = calendars.First();

                if (calendar.Version != "2.0")
                    throw new InvalidDataException(string.Format("Calendar version is not supported (version == {0})",
                        calendar.Version));

                if (string.IsNullOrEmpty(calendar.Method)
                    || (!calendar.Method.Equals(Defines.ICAL_REQUEST, StringComparison.InvariantCultureIgnoreCase)
                    && !calendar.Method.Equals(Defines.ICAL_REPLY, StringComparison.InvariantCultureIgnoreCase)
                    && !calendar.Method.Equals(Defines.ICAL_CANCEL, StringComparison.InvariantCultureIgnoreCase)))
                {
                    throw new InvalidDataException(string.Format("Calendar method is not supported (method == {0})",
                        calendar.Method));
                }

                if(!calendar.Events.Any())
                    throw new InvalidDataException("Calendar events not found");

                if (calendar.Events.Count > 1)
                    throw new InvalidDataException("Too many calendar events");

                var icalEvent = calendar.Events.First();

                if(string.IsNullOrEmpty(icalEvent.Uid))
                    throw new InvalidDataException("Calendar event uid is empty");

                return calendar;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("ParseValidCalendar() Exception: {0}", ex.ToString());
            }

            return null;
        }

        public static string SerializeCalendar(Ical.Net.Calendar calendar)
        {
            try
            {
                var serializer = new Ical.Net.Serialization.CalendarSerializer(calendar);
                return serializer.SerializeToString(calendar);
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
            return outText.ToString().Trim();
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

        public static bool HasUnsubscribeLink(Stream htmlStream)
        {
            try
            {
                if (htmlStream == null || htmlStream.Length == 0)
                    return false;

                var doc = new HtmlDocument();
                doc.Load(htmlStream);

                var hasUnsubscribe = doc.DocumentNode.SelectNodes("//a[@href]")
                    .Any(
                        link =>
                            link.Attributes["href"].Value
                                .IndexOf("unsubscribe", StringComparison.OrdinalIgnoreCase) != -1);

                return hasUnsubscribe;
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
        public static string CreateMessageId(ILog log = null)
        {
            if (log == null)
                log = new NullLog();

            var domain = "";

            try
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant();

                if (tenant != null)
                    domain = tenant.GetTenantDomain();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CreateMessageId: GetTenantDomain failed: Exception\r\n{0}\r\n", ex.ToString());
            }

            if (string.IsNullOrEmpty(domain))
                domain = System.Net.Dns.GetHostName();

            try
            {
                var indexСolon = domain.IndexOf(":", StringComparison.Ordinal);

                if (indexСolon != -1)
                {
                    domain = domain.Remove(indexСolon, domain.Length - indexСolon);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CreateMessageId: Remove colon failed: Exception\r\n{0}\r\n", ex.ToString());
            }

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

        static readonly Regex UrlReg = new Regex(@"(?:(?:(?:http|ftp|gopher|telnet|news)://)(?:w{3}\.)?(?:[a-zA-Z0-9/;\?&=:\-_\$\+!\*'\(\|\\~\[\]#%\.])+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex EmailReg = new Regex(@"\b[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public static string MakeHtmlFromText(string textBody)
        {
            var listText = textBody.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();

            var builder = new StringBuilder(textBody.Length);

            listText.ForEach(line =>
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var modifiedLine = line;

                    var foundUrls = new List<string>();

                    for (var m = UrlReg.Match(modifiedLine); m.Success; m = m.NextMatch())
                    {
                        var foundUrl = m.Groups[0].Value;

                        foundUrls.Add(string.Format("<a href=\"{0}\" target=\"_blank\">{0}</a>", foundUrl));

                        modifiedLine = modifiedLine.Replace(foundUrl, "{" + foundUrls.Count.ToString(CultureInfo.InvariantCulture) + "}");
                    }

                    for (var m = EmailReg.Match(modifiedLine); m.Success; m = m.NextMatch())
                    {
                        var foundMailAddress = m.Groups[0].Value;

                        foundUrls.Add(string.Format("<a href=\"mailto:{0}\">{1}</a>",
                            HttpUtility.UrlEncode(foundMailAddress),
                            foundMailAddress));

                        modifiedLine = modifiedLine.Replace(foundMailAddress, "{" + foundUrls.Count.ToString(CultureInfo.InvariantCulture) + "}");
                    }

                    modifiedLine = HttpUtility.HtmlEncode(modifiedLine);

                    if (foundUrls.Count > 0)
                    {
                        for (int i = 0; i < foundUrls.Count; i++)
                        {
                            modifiedLine = modifiedLine.Replace("{" + (i + 1).ToString(CultureInfo.InvariantCulture) + "}", foundUrls.ElementAt(i));
                        }
                    }

                    builder.Append(modifiedLine);
                }
                builder.Append("<br/>");

                Thread.Sleep(1);

            });

            return builder.ToString();
        }

        public static void SaveToJson<T>(string path, T obj)
        {
            if (obj == null)
                return;

            if (File.Exists(path))
                File.Delete(path);

            //serialize
            using (var stream = File.Create(path))
            {
                var ser = new DataContractJsonSerializer(typeof(T));

                ser.WriteObject(
                    stream,
                    obj);
            }
        }

        public static T RestoreFromJson<T>(string path)
        {
            //deserialize
            using (var stream = File.Open(path, FileMode.Open))
            {
                var ser = new DataContractJsonSerializer(typeof(T));

                return (T)ser.ReadObject(stream);
            }
        }

        public static string GetIntroduction(string htmlBody)
        {
            var introduction = string.Empty;

            if (string.IsNullOrEmpty(htmlBody))
                return introduction;

            try
            {
                introduction = ExtractTextFromHtml(htmlBody, 200);
            }
            catch (RecursionDepthException)
            {
                throw;
            }
            catch
            {
                introduction = (htmlBody.Length > 200 ? htmlBody.Substring(0, 200) : htmlBody);
            }

            introduction = introduction.Replace("\n", " ").Replace("\r", " ").Trim();

            return introduction;
        }

        private static readonly DateTime DefaultMysqlMinimalDate = new DateTime(1975, 01, 01, 0, 0, 0); // Common decision of TLMail developers 

        public static bool IsDateCorrect(DateTime date)
        {
            return date >= DefaultMysqlMinimalDate && date <= DateTime.Now;
        }

        public static void SkipErrors(Action method)
        {
            try
            {
                method();
            }
            catch
            {
                // Skips
            }
        }

        public static T ExecuteSafe<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch
            {
                // Skip
            }

            return default(T);
        }

        public static string EncryptPassword(string password)
        {
            return InstanceCrypto.Encrypt(password);
        }

        public static string DecryptPassword(string password)
        {
            return InstanceCrypto.Decrypt(password);
        }

        public static bool TryDecryptPassword(string encryptedPassword, out string password)
        {
            password = "";
            try
            {
                if (string.IsNullOrEmpty(encryptedPassword))
                    return false;

                password = DecryptPassword(encryptedPassword);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static readonly string[] ListHeaderNames =
        {
            "List-Subscribe",
            "List-Unsubscribe",
            "List-Help",
            "List-Post",
            "List-Owner",
            "List-Archive",
            "List-ID",
            "Auto-Submitted"
        };

        private static readonly string[] ListFromTextToSkip =
        {
            "noreply",
            "no-reply",
            "mailer-daemon",
            "mail-daemon",
            "notify",
            "postmaster",
            "postman"
        };

        public static bool IsMessageAutoGenerated(MailMessageData message)
        {
            var isMassSending = false;

            if (message.HeaderFieldNames != null &&
                ListHeaderNames.Any(
                    h =>
                        message.HeaderFieldNames.AllKeys.FirstOrDefault(
                            k => k.Equals(h, StringComparison.OrdinalIgnoreCase)) != null))
            {
                isMassSending = true;
            }
            else if (!string.IsNullOrEmpty(message.From) &&
                     ListFromTextToSkip.Any(f =>
                         message.From.IndexOf(f, StringComparison.OrdinalIgnoreCase) != -1))
            {
                isMassSending = true;
            }
            else if (message.HtmlBodyStream != null && message.HtmlBodyStream.Length > 0)
            {
                isMassSending = HasUnsubscribeLink(message.HtmlBodyStream);
            }
            else if (!string.IsNullOrEmpty(message.HtmlBody))
            {
                isMassSending = HasUnsubscribeLink(message.HtmlBody);
            }

            return isMassSending;
        }
    }
}