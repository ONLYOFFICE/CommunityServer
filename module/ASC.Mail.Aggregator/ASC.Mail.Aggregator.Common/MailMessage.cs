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
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using ASC.Mail.Aggregator.Common.Collection;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Specific;
using DDay.iCal;
using HtmlAgilityPack;
using MimeKit;

namespace ASC.Mail.Aggregator.Common
{
    [DataContract(Namespace = "")]
    public class MailMessage
    {
        static readonly Regex UrlReg = new Regex(@"(?:(?:(?:http|ftp|gopher|telnet|news)://)(?:w{3}\.)?(?:[a-zA-Z0-9/;\?&=:\-_\$\+!\*'\(\|\\~\[\]#%\.])+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex EmailReg = new Regex(@"\b[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        private static readonly DateTime DefaultMysqlMinimalDate = new DateTime(1975, 01, 01, 0, 0, 0); // Common decision of TLMail developers 

        public MailMessage() {}

        private bool IsDateCorrect(DateTime date)
        {
            return date >= DefaultMysqlMinimalDate && date <= DateTime.Now;
        }

        public MailMessage(MimeMessage message, int folder = 1, bool unread = false, string chainId = "",
            string streamId = "")
        {
            if (message == null)
                throw new ArgumentNullException("message");

            Date = IsDateCorrect(message.Date.UtcDateTime) ? message.Date.UtcDateTime : DateTime.UtcNow;
            MimeMessageId = (string.IsNullOrEmpty(message.MessageId) ? MailUtil.CreateMessageId() : message.MessageId)
                .Trim(new[] {'<', '>'});
            ChainId = string.IsNullOrEmpty(chainId) ? MimeMessageId : chainId;
            MimeReplyToId = ChainId.Equals(MimeMessageId) ? null : message.InReplyTo.Trim(new[] {'<', '>'});
            ReplyTo = message.ReplyTo.ToString();
            From = message.From.ToString();
            FromEmail = message.From != null && message.From.Mailboxes != null && message.From.Mailboxes.Any()
                ? message.From.Mailboxes.First().Address
                : "";
            ToList = message.To.Mailboxes.Select(s => new MailAddress(s.Address, s.Name)).ToList();
            To = string.Join(", ", message.To.Mailboxes.Select(s => s.ToString()));
            CcList = message.Cc.Mailboxes.Select(s => new MailAddress(s.Address, s.Name)).ToList();
            Cc = string.Join(", ", message.Cc.Mailboxes.Select(s => s.ToString()));
            Bcc = string.Join(", ", message.Bcc.Mailboxes.Select(s => s.ToString()));
            Subject = message.Subject ?? string.Empty;
            Important = message.Importance == MessageImportance.High || message.Priority == MessagePriority.Urgent;
            TextBodyOnly = false;

            if (message.HtmlBody == null && message.TextBody == null)
            {
                HtmlBody = "";
                Introduction = "";
                IsBodyCorrupted = true;

                var messagePart = new MessagePart
                {
                    Message = message,
                    ContentDisposition = new ContentDisposition
                    {
                        FileName = "message.eml"
                    }
                };

                LoadAttachments(new List<MimeEntity> { messagePart });
            }
            else
            {
                SetHtmlBodyAndIntroduction(message);
            }
            
            Size = HtmlBody.Length;
            HeaderFieldNames = new NameValueCollection();

            if (message.Headers.Any())
            {
                foreach (var header in message.Headers)
                {
                    HeaderFieldNames.Add(header.Field, header.Value);
                }
            }

            Folder = folder;
            IsNew = unread;
            StreamId = string.IsNullOrEmpty(streamId) ? MailUtil.CreateStreamId() : streamId;

            LoadAttachments(message.Attachments);
            LoadEmbeddedAttachments(message);
            LoadCalendarInfo(message);
        }

        private void LoadCalendarInfo(MimeMessage message)
        {
            if (!message.BodyParts.Any())
                return;

            try
            {
                var calendarParts = message.BodyParts.Where(p => p.ContentType.IsMimeType("text", "calendar")).ToList();

                if(!calendarParts.Any())
                    return;

                foreach (var calendarPart in calendarParts)
                {
                    var p = calendarPart as TextPart;
                    if (p == null)
                        continue;

                    var ics = p.Text;

                    var calendar = MailUtil.ParseValidCalendar(ics);

                    if (calendar == null)
                        return;

                    if (calendar.Events[0].Organizer == null &&
                        calendar.Method.Equals("REPLY", StringComparison.OrdinalIgnoreCase))
                    {
                        // Fix reply organizer (Outlook style of Reply)
                        var toAddress = message.To.Mailboxes.FirstOrDefault();
                        if (toAddress != null)
                        {
                            calendar.Events[0].Organizer = new Organizer("mailto:" + toAddress.Address)
                            {
                                CommonName = string.IsNullOrEmpty(toAddress.Name)
                                    ? toAddress.Address
                                    : toAddress.Name
                            };

                            ics = MailUtil.SerializeCalendar(calendar);
                        }
                    }

                    CalendarUid = calendar.Events[0].UID;
                    CalendarId = -1;
                    CalendarEventIcs = ics;
                    CalendarEventCharset = string.IsNullOrEmpty(p.ContentType.Charset) ? Encoding.UTF8.HeaderName : p.ContentType.Charset;
                    CalendarEventMimeType = p.ContentType.MimeType;

                    var calendarExists =
                        message.Attachments
                            .Any(
                                attach =>
                                {
                                    var subType = attach.ContentType.MediaSubtype.ToLower().Trim();

                                    if (string.IsNullOrEmpty(subType) ||
                                        (!subType.Equals("ics") &&
                                         !subType.Equals("ical") &&
                                         !subType.Equals("ifb") &&
                                         !subType.Equals("icalendar") &&
                                         !subType.Equals("calendar")))
                                    {
                                        return false;
                                    }

                                    var icsTextPart = attach as TextPart;
                                    string icsAttach;

                                    if (icsTextPart != null)
                                    {
                                        icsAttach = icsTextPart.Text;
                                    }
                                    else
                                    {
                                        using (var stream = new MemoryStream())
                                        {
                                            p.ContentObject.DecodeTo(stream);
                                            var bytes = stream.ToArray();
                                            var encoding = MailUtil.GetEncoding(p.ContentType.Charset);
                                            icsAttach = encoding.GetString(bytes);
                                        }
                                    }

                                    var cal = MailUtil.ParseValidCalendar(icsAttach);

                                    if (cal == null)
                                        return false;

                                    return CalendarUid == cal.Events[0].UID;
                                });

                    if (calendarExists)
                        continue;

                    if (calendarPart.ContentDisposition == null)
                    {
                        calendarPart.ContentDisposition = new ContentDisposition();
                    }

                    if (string.IsNullOrEmpty(calendarPart.ContentDisposition.FileName))
                    {
                        calendarPart.ContentDisposition.FileName = calendar.Method == "REQUEST"
                            ? "invite.ics"
                            : calendar.Method == "REPLY" ? "reply.ics" : "cancel.ics";
                    }

                    LoadAttachments(new List<MimeEntity> {calendarPart});
                }
            }
            catch
            {
                // Ignore
            }
        }

        private void SetHtmlBodyAndIntroduction(MimeMessage message)
        {
            HtmlBody = "<br/>";
            Introduction = "";

            var htmlBodyBuilder = new StringBuilder().Append(message.HtmlBody);

            ContentType contentType = null;
            var subMessages = new ItemList<MimeMessage>();

            var iter = new MimeIterator(message);
            while (iter.MoveNext())
            {
                var multipart = iter.Parent as Multipart;
                var part = iter.Current as MimePart;
                var subMessage = iter.Current as MessagePart;

                if (multipart != null && part != null && !part.IsAttachment)
                {
                    if (contentType == null &&
                        (part.ContentType.IsMimeType("text", "plain") || part.ContentType.IsMimeType("text", "html")))
                    {
                        contentType = multipart.ContentType;
                    }
                }

                if (subMessage != null)
                    subMessages.Add(subMessage.Message);
            }

            contentType = contentType ?? message.Body.ContentType;

            if (contentType.MimeType.Equals("multipart/mixed", StringComparison.InvariantCultureIgnoreCase)
                && !string.IsNullOrEmpty(message.HtmlBody)
                && !string.IsNullOrEmpty(message.TextBody))
            {
                htmlBodyBuilder.AppendFormat("<p>{0}</p>", MakeHtmlFromText(message.TextBody));
            }
            else if (!string.IsNullOrEmpty(message.TextBody) && string.IsNullOrEmpty(message.HtmlBody))
            {
                var html = MakeHtmlFromText(message.TextBody);
                htmlBodyBuilder.AppendFormat("<body><pre>{0}</pre></body>", html);
                TextBodyOnly = true;
            }

            foreach (var subMessage in subMessages)
            {
                var toString = string.Join(", ", subMessage.To.Select(s => s.ToString()));
                htmlBodyBuilder.Append("<hr /><br/>")
                    .Append("<div style=\"padding-left:15px;\">")
                    .AppendFormat("<span><b>Subject:</b> {0}</span><br/>", subMessage.Subject)
                    .AppendFormat("<span><b>From:</b> {0}</span><br/>", subMessage.From)
                    .AppendFormat("<span><b>Date:</b> {0}</span><br/>", subMessage.Date)
                    .AppendFormat("<span><b>To:</b> {0}</span><br/></div>", toString)
                    .AppendFormat("<br/>{0}", subMessage.HtmlBody);
            }

            HtmlBody = htmlBodyBuilder.ToString();
            Introduction = GetIntroduction(HtmlBody);
        }

        public static string GetIntroduction(string htmlBody)
        {
            var introduction = string.Empty;

            if (string.IsNullOrEmpty(htmlBody)) 
                return introduction;

            try
            {
                introduction = MailUtil.ExtractTextFromHtml(htmlBody, 200);
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

        public void LoadEmbeddedAttachments(MimeMessage message)
        {
            var embeddedAttachments = new List<MimeEntity>();

            var iter = new MimeIterator(message);

            while (iter.MoveNext())
            {
                var part = iter.Current as MimePart;
                if (part == null)
                    continue;

                if (string.IsNullOrEmpty(part.ContentId) && part.ContentLocation == null)
                    continue;

                if(part is TextPart)
                    continue;

                embeddedAttachments.Add(part);
            }

            if(embeddedAttachments.Any())
                LoadAttachments(embeddedAttachments);
        }

        public void LoadAttachments(IEnumerable<MimeEntity> attachments)
        {
            if (Attachments == null)
                Attachments = new List<MailAttachment>();

            foreach (var attachment in attachments)
            {
                var messagePart = attachment as MessagePart;
                var mimePart = attachment as MimePart;

                if (messagePart == null && mimePart == null)
                    continue;

                byte[] bytes;

                using (var stream = new MemoryStream())
                {
                    if (messagePart != null)
                        messagePart.Message.WriteTo(stream);
                    else
                        mimePart.ContentObject.DecodeTo(stream);

                    bytes = stream.ToArray();
                }

                var filename = attachment.ContentDisposition != null &&
                               !string.IsNullOrEmpty(attachment.ContentDisposition.FileName)
                    ? attachment.ContentDisposition.FileName
                    : attachment.ContentType != null && !string.IsNullOrEmpty(attachment.ContentType.Name)
                        ? attachment.ContentType.Name
                        : "";

                filename = MailUtil.ImproveFilename(filename, attachment.ContentType);

                Attachments.Add(new MailAttachment
                {
                    contentId = attachment.ContentId,
                    size = bytes.Length,
                    fileName = filename,
                    contentType = attachment.ContentType != null ? attachment.ContentType.MimeType : null,
                    data = bytes,
                    contentLocation = attachment.ContentLocation != null ? attachment.ContentLocation.ToString() : null
                });
            }

        }

        public void ReplaceEmbeddedImages(ILogger log = null)
        {
            log = log ?? new NullLogger();

            try
            {
                var attchments = Attachments.Where(a => a.isEmbedded && !string.IsNullOrEmpty(a.storedFileUrl)).ToList();

                if (!attchments.Any())
                    return;

                foreach (var attach in attchments)
                {
                    if (!string.IsNullOrEmpty(attach.contentId))
                    {
                        HtmlBody = HtmlBody.Replace(string.Format("cid:{0}", attach.contentId.Trim('<').Trim('>')),
                            attach.storedFileUrl);
                    }
                    else if (!string.IsNullOrEmpty(attach.contentLocation))
                    {
                        HtmlBody = HtmlBody.Replace(string.Format("{0}", attach.contentLocation),
                            attach.storedFileUrl);
                    }
                    else
                    {
                        //This attachment is not embedded;
                        attach.contentId = null;
                        attach.contentLocation = null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("ReplaceEmbeddedImages() \r\n Exception: \r\n{0}\r\n", ex.ToString());
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
        public bool TextBodyOnly { get; set; }

        [DataMember]
        public long Size { get; set; }

        [DataMember]
        // ReSharper disable once InconsistentNaming
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

        [DataMember]
        public string CalendarUid { get; set; }

        [IgnoreDataMember]
        public int CalendarId { get; set; }

        [IgnoreDataMember]
        public string CalendarEventCharset { get; set; }

        [IgnoreDataMember]
        public string CalendarEventMimeType { get; set; }

        [IgnoreDataMember]
        public string CalendarEventIcs { get; set; }

        [IgnoreDataMember]
        public List<MailAddress> ToList { get; set; }

        [IgnoreDataMember]
        public List<MailAddress> CcList { get; set; }

        [IgnoreDataMember]
        public NameValueCollection HeaderFieldNames { get; set; }

        public class Options
        {
            public Options()
            {
                OnlyUnremoved = true;
                NeedSanitizer = true;
            }

            public bool LoadImages { get; set; }
            public bool LoadBody { get; set; }
            public bool NeedProxyHttp { get; set; }
            public bool NeedSanitizer { get; set; }
            public bool OnlyUnremoved { get; set; }

            public string ProxyHttpHandlerUrl { get; set; }
        }
    }
}