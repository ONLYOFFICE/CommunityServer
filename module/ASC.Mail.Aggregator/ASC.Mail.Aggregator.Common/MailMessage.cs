/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using ActiveUp.Net.Mail;
using ASC.Mail.Aggregator.Common.Collection;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Specific;
using HtmlAgilityPack;

namespace ASC.Mail.Aggregator.Common
{
    [DataContract(Namespace = "")]
    public class MailMessage
    {
        static readonly Regex UrlReg = new Regex(@"(?:(?:(?:http|ftp|gopher|telnet|news)://)(?:w{3}\.)?(?:[a-zA-Z0-9/;\?&=:\-_\$\+!\*'\(\|\\~\[\]#%\.])+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex EmailReg = new Regex(@"\b[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        static readonly Regex SurrogateCodePointReg = new Regex(@"\&#\d+;", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        private static readonly DateTime DefaultMysqlMinimalDate = new DateTime(1975, 01, 01, 0, 0, 0); // Common decision of TLMail developers 

        public MailMessage()
        {
        }

        private bool IsDateCorrect(DateTime date)
        {
            return date >= DefaultMysqlMinimalDate && date <= DateTime.Now;
        }

        public MailMessage(Message message)
        {
            if (message == null) throw new ArgumentNullException("message");

            Date = IsDateCorrect(message.Date)
                       ? message.Date
                       : (IsDateCorrect(message.ReceivedDate) ? message.ReceivedDate : DateTime.Now);
            MimeMessageId = message.MessageId;
            MimeReplyToId = message.InReplyTo;
            ChainId = message.MessageId;
            ReplyTo = message.ReplyTo.ToString();
            From = message.From.ToString();
            FromEmail = message.From.Email;
            To = string.Join(", ", message.To.Select(s => s.ToString()));
            Cc = string.Join(", ", message.Cc.Select(s => s.ToString()));
            Bcc = string.Join(", ", message.Bcc.Select(s => s.ToString()));
            Subject = message.Subject ?? "";
            bool isMessageHasHighFlag = String.Compare(message.Flag, "high", StringComparison.OrdinalIgnoreCase) == 0;
            Important = isMessageHasHighFlag || message.Priority == MessagePriority.High;
            TextBodyOnly = false;
            SetHtmlBodyAndIntroduction(message);
            Size = HtmlBody.Length;

            LoadCalendarAttachments(message);
            LoadAttachments(message.Attachments.Cast<MimePart>(), false);
            LoadAttachments(message.EmbeddedObjects.Cast<MimePart>(), true);
            LoadAttachments(message.UnknownDispositionMimeParts.Cast<MimePart>(), true);
        }

        private void LoadCalendarAttachments(Message message)
        {
            if (string.IsNullOrEmpty(message.BodyCalendar.Text)) return;

            try
            {
                var calendarExists =
                    message.Attachments.Cast<MimePart>()
                        .Any(
                            attach =>
                                attach.ContentType.SubType == "ics" || attach.ContentType.SubType == "ical" ||
                                attach.ContentType.SubType == "ifb" || attach.ContentType.SubType == "icalendar");

                if (calendarExists) return;

                var part =
                    new MimePart(
                        Encoding.GetEncoding(message.BodyCalendar.Charset).GetBytes(message.BodyCalendar.Text),
                        "calendar.ics", message.BodyCalendar.Charset);

                LoadAttachments(new List<MimePart> { part }, false);
            }
            catch
            {
                // Ignore
            }
        }

        private void SetHtmlBodyAndIntroduction(Message message)
        {
            HtmlBody = "<br/>";
            Introduction = "";

            var htmlBodyBuilder = new StringBuilder().Append(message.BodyHtml.Text);

            if (message.ContentType.MimeType.Equals("multipart/mixed", StringComparison.InvariantCultureIgnoreCase)
                && !string.IsNullOrEmpty(message.BodyHtml.Text)
                && !string.IsNullOrEmpty(message.BodyText.Text))
            {
                htmlBodyBuilder.AppendFormat("<p>{0}</p>", MakeHtmlFromText(message.BodyText.Text));
            }

            if (htmlBodyBuilder.Length != 0)
            {
                HtmlBody = htmlBodyBuilder.ToString();
                Introduction = GetIntroduction(HtmlBody);
                return;
            }

            if (!string.IsNullOrEmpty(message.BodyText.Text))
            {
                message.BodyText.Text = MakeHtmlFromText(message.BodyText.Text);
                HtmlBody = string.Format("<body><pre>{0}</pre></body>", message.BodyText.Text);
                Introduction = GetIntroduction(message.BodyText.Text);
                TextBodyOnly = true;
                return;
            }

            if (message.SubMessages.Count > 0)
            {
                BuildBodyFromSubmessages(message, htmlBodyBuilder);
                HtmlBody = htmlBodyBuilder.ToString();
                Introduction = GetIntroduction(message.SubMessages[0].BodyHtml.Text);
            }
        }

        private static void BuildBodyFromSubmessages(Message message, StringBuilder htmlBodyBuilder)
        {
            foreach (Message subMessage in message.SubMessages)
            {
                var toString = string.Join(", ", subMessage.To.Select(s => s.ToString()));
                htmlBodyBuilder.Append("<hr /><br/>")
                                 .Append("<div style=\"padding-left:15px;\">")
                                 .AppendFormat("<span><b>Subject:</b> {0}</span><br/>", subMessage.Subject)
                                 .AppendFormat("<span><b>From:</b> {0}</span><br/>", subMessage.From)
                                 .AppendFormat("<span><b>Date:</b> {0}</span><br/>", subMessage.ReceivedDate)
                                 .AppendFormat("<span><b>To:</b> {0}</span><br/></div>", toString)
                                 .AppendFormat("<br/>{0}", subMessage.BodyHtml.Text);
            }
        }

        public static string GetIntroduction(string htmlBody)
        {
            var introduction = string.Empty;
            
            if (!string.IsNullOrEmpty(htmlBody))
            {
                try
                {
                    introduction = ExtractTextFromHtml(htmlBody, 200);
                }
                catch (RecursionDepthException ex)
                {
                    throw new Exception(ex.Message);
                }
                catch
                {
                    introduction = (htmlBody.Length > 200 ? htmlBody.Substring(0, 200) : htmlBody);
                }

                introduction = introduction.Replace("\n", " ").Replace("\r", " ");
            }

            return introduction;
        }

        //if limit_length < 1 then text will be unlimited
        private static string ExtractTextFromHtml(string html, int limitLength)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var outText = limitLength < 1 ? new StringBuilder() : new StringBuilder(limitLength);
            ConvertTo(doc.DocumentNode, outText, limitLength);
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
                    var parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    var html = ((HtmlTextNode)node).Text;

                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    html = html.Replace("&nbsp;", "");

                    if (html.Trim().Length > 0)
                    {
                        html = SurrogateCodePointReg.Replace(html, "");
                        var text = HtmlEntity.DeEntitize(html);
                        var newLength = (outText + text).Length;
                        if (limitLength > 0 && newLength >= limitLength)
                        {
                            if (newLength > limitLength)
                            {
                                text = text.Substring(0, limitLength - outText.Length);
                                outText.Append(text);
                            }
                            return;
                        }
                        outText.Append(text);
                    }
                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
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

        private static string MakeHtmlFromText(string textBody)
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

        public void LoadAttachments(IEnumerable<MimePart> mimeParts, bool skipText)
        {
            if (Attachments == null)
            {
                Attachments = new List<MailAttachment>();
            }

            foreach (var mimePart in mimeParts)
            {
                if (skipText && mimePart.ContentType.Type.Contains("text"))
                    continue;

                if(mimePart.BinaryContent == null || mimePart.BinaryContent.Length == 0)
                    continue;

                var ext = Path.GetExtension(mimePart.Filename);

                if (string.IsNullOrEmpty(ext))
                {
                    // If the file extension is not specified, there will be issues with saving on s3
                    var newExt = ".ext";

                    if (mimePart.ContentType.Type.ToLower().IndexOf("image", StringComparison.Ordinal) != -1)
                    {
                        var subType = mimePart.ContentType.SubType;

                        newExt = ".png";

                        if (!string.IsNullOrEmpty(subType))
                        {
                            // List was get from http://en.wikipedia.org/wiki/Internet_media_type
                            var knownImageTypes = new List<string> { "gif", "jpeg", "pjpeg", "png", "svg", "tiff", "ico", "bmp" };

                            var foundExt = knownImageTypes
                                .Find(s => subType.IndexOf(s, StringComparison.Ordinal) != -1);

                            if (!string.IsNullOrEmpty(foundExt))
                                newExt = "." + foundExt;
                        }
                    }
                    mimePart.Filename = Path.ChangeExtension(mimePart.Filename, newExt);
                }

                Attachments.Add(new MailAttachment
                    {
                        contentId = mimePart.EmbeddedObjectContentId,
                        size = mimePart.Size,
                        fileName = mimePart.Filename,
                        contentType = mimePart.ContentType.MimeType,
                        data = mimePart.BinaryContent,
                        contentLocation = mimePart.ContentLocation
                    });
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public List<MailAttachment> Attachments { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Introduction { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string HtmlBody { get; set; }

        [DataMember]
        public bool ContentIsBlocked { get; set; }

        [DataMember]
        public bool Important { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public bool HasAttachments { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Bcc { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Cc { get; set; }

        [DataMember]
        public string To { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public string From { get; set; }

        public string FromEmail { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ReplyTo { get; set; }

        [DataMember]
        public long Id { get; set; }

        /*[DataMember(EmitDefaultValue = false)]
        public string MessageId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string InReplyTo { get; set; }*/

        [DataMember(EmitDefaultValue = false)]
        public string ChainId { get; set; }

        public DateTime ChainDate { get; set; }

        [DataMember(Name = "ChainDate")]
        public string ChainDateString
        {
            get { return ChainDate.ToString("G", CultureInfo.InvariantCulture); }
            set
            {
                DateTime date;
                if (DateTime.TryParseExact(value, "g", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date))
                {
                    ChainDate = date;
                }
            }
        }

        public DateTime Date { get; set; }

        [DataMember(Name = "Date")]
        public string DateString
        {
            get { return Date.ToString("G", CultureInfo.InvariantCulture);}
            set { 
                DateTime date;
                if (DateTime.TryParseExact(value,"g",CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal,out date))
                {
                    Date = date;
                }
            }
        }

        [DataMember(Name = "DateDisplay")]
        public string DateDisplay
        {
            get { return Date.ToVerbString(); }
        }

        [DataMember(EmitDefaultValue = false)]
        public ItemList<int> TagIds { get; set; }

        public string LabelsString
        {
            set
            {
                TagIds = new ItemList<int>(MailUtil.GetLabelsFromString(value));
            }
        }

        [DataMember(Name = "LabelsInString")]
        public string LabelsInString
        {
            get { return MailUtil.GetStringFromLabels(TagIds); }
        }


        [DataMember]
        public bool IsNew { get; set; }

        [DataMember]
        public bool IsAnswered { get; set; }

        [DataMember]
        public bool IsForwarded { get; set; }

        [DataMember]
        public bool IsFromCRM { get; set; }

        [DataMember]
        public bool IsFromTL { get; set; }

        [DataMember]
        public bool TextBodyOnly { get; set; }

        [DataMember]
        public long Size { get; set; }

        [DataMember]
        public string EMLLink { get; set; }

        [DataMember]
        public string StreamId { get; set; }

        [DataMember]
        public int RestoreFolderId { get; set; }

        [DataMember]
        public int Folder { get; set; }

        [DataMember]
        public int ChainLength { get; set; }

        [DataMember]
        public bool WasNew { get; set; }

        [DataMember]
        public bool IsToday { get; set; }

        [DataMember]
        public bool IsYesterday { get; set; }

        [DataMember]
        public ApiDateTime ReceivedDate
        {
            get { return (ApiDateTime)Date; }
        }

        public int MailboxId { get; set; }

        public List<CrmContactEntity> LinkedCrmEntityIds { get; set; }

        public List<int> ParticipantsCrmContactsId { get; set; }

        [DataMember]
        public bool IsBodyCorrupted { get; set; }

        [DataMember]
        public bool HasParseError { get; set; }

        [DataMember]
        public string MimeMessageId { get; set; }

        [DataMember]
        public string MimeReplyToId { get; set; }
    }
}