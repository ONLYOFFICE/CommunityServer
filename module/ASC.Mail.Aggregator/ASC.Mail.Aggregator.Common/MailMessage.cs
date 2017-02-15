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
using ASC.Mail.Aggregator.Common.Collection;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Specific;
using DDay.iCal;
using HtmlAgilityPack;
using MimeKit;
using MimeKit.Text;
using MimeKit.Tnef;

namespace ASC.Mail.Aggregator.Common
{
    [DataContract(Namespace = "")]
    public class MailMessage
    {
        private static readonly DateTime DefaultMysqlMinimalDate = new DateTime(1975, 01, 01, 0, 0, 0); // Common decision of TLMail developers 

        public MailMessage() {}

        public MailMessage(MimeMessage message, int folder = 1, bool unread = false, string chainId = "",
            string streamId = "")
        {
            if (message == null)
                throw new ArgumentNullException("message");

            Date = IsDateCorrect(message.Date.UtcDateTime) ? message.Date.UtcDateTime : DateTime.UtcNow;
            MimeMessageId = (string.IsNullOrEmpty(message.MessageId) ? MailUtil.CreateMessageId() : message.MessageId)
                .Trim(new[] {'<', '>'});
            ChainId = string.IsNullOrEmpty(chainId) ? MimeMessageId : chainId;
            MimeReplyToId = ChainId.Equals(MimeMessageId) ? null : message.InReplyTo.Trim('<', '>');
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

            Introduction = "";

            Attachments = new List<MailAttachment>();

            HtmlBodyStream = new MemoryStream();

            ExtractMainParts(message);

            Size = HtmlBodyStream.Length > 0 ? HtmlBodyStream.Length : HtmlBody.Length;

            HeaderFieldNames = new NameValueCollection();

            message.Headers
                .ToList()
                .ForEach(h => HeaderFieldNames.Add(h.Field, h.Value));

            Folder = folder;
            IsNew = unread;
            StreamId = string.IsNullOrEmpty(streamId) ? MailUtil.CreateStreamId() : streamId;

            LoadCalendarInfo(message);
        }

        ~MailMessage()
        {
            if(HtmlBodyStream != null)
                HtmlBodyStream.Dispose();
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

                    LoadAttachments(new List<MimeEntity> {calendarPart}, true);
                }
            }
            catch
            {
                // Ignore
            }
        }

        private string ConvertToHtml(TextPart entity)
        {
            try
            {
                TextConverter converter;

                if (entity.IsHtml)
                {
                    converter = new HtmlToHtml();
                }
                else if (entity.IsFlowed)
                {
                    var flowed = new FlowedToHtml();
                    string delsp;

                    if (entity.ContentType.Parameters.TryGetValue("delsp", out delsp))
                        flowed.DeleteSpace = delsp.ToLowerInvariant() == "yes";

                    converter = flowed;

                }
                else
                {
                    converter = new TextToHtml();
                }

                var body = converter.Convert(entity.Text);

                return body;
            }
            catch (Exception)
            {
                // skip
            }

            return entity.Text ?? "";
        }

        private void ExtractMainParts(MimeMessage message)
        {
            var htmlStream = new MemoryStream();
            var textStream = new MemoryStream();

            Action<MimeEntity> loadRealAttachment = (part) =>
            {
                if ((part.ContentDisposition != null && !string.IsNullOrEmpty(part.ContentDisposition.FileName))
                    || part.ContentType != null && !string.IsNullOrEmpty(part.ContentType.Name))
                {
                    LoadAttachments(new List<MimeEntity> { part });
                }
            };

            Action<MimePart> setBodyOrAttachment = (part) =>
            {
                var entity = part as TextPart;

                if(htmlStream == null || textStream == null)
                    throw new Exception("Streams are not initialized");

                if ((entity == null
                     || htmlStream.Length != 0 && entity.IsHtml
                     || textStream.Length != 0 && !entity.IsHtml))
                {
                    loadRealAttachment(part);
                }
                else
                {

                    if (Introduction.Length < 200 && (!entity.IsHtml && !string.IsNullOrEmpty(entity.Text)))
                    {
                        if (string.IsNullOrEmpty(Introduction))
                        {
                            Introduction = (entity.Text.Length > 200 ? entity.Text.Substring(0, 200) : entity.Text);
                        }
                        else
                        {
                            var need = 200 - Introduction.Length;
                            Introduction += (entity.Text.Length > need ? entity.Text.Substring(0, need) : entity.Text);
                        }

                        Introduction = Introduction.Replace("\r\n", " ").Replace("\n", " ").Trim();
                    }

                    var body = ConvertToHtml(entity);

                    if (entity.IsHtml)
                    {
                        using (var sw = new StreamWriter(htmlStream, Encoding.UTF8, 1024, true))
                        {
                            sw.Write(body);
                            sw.Flush();
                            htmlStream.Seek(0, SeekOrigin.Begin);
                        }
                    }
                    else
                    {
                        using (var sw = new StreamWriter(textStream, Encoding.UTF8, 1024, true))
                        {
                            sw.Write(body);
                            sw.Flush();
                            textStream.Seek(0, SeekOrigin.Begin);
                        }
                    }
                }
            };

            using (var sw = new StreamWriter(HtmlBodyStream, Encoding.UTF8, 1024, true))
            {
                using (var iter = new MimeIterator(message))
                {
                    while (iter.MoveNext())
                    {
                        var multipart = iter.Parent as Multipart;
                        var part = iter.Current as MimePart;

                        var subMessage = iter.Current as MessagePart;
                        if (subMessage != null)
                        {
                            if ((subMessage.ContentDisposition != null &&
                                 !string.IsNullOrEmpty(subMessage.ContentDisposition.FileName))
                                || subMessage.ContentType != null && !string.IsNullOrEmpty(subMessage.ContentType.Name))
                            {
                                LoadAttachments(new List<MimeEntity> {subMessage}, true);
                            }
                            else
                            {
                                subMessage.ContentDisposition = new ContentDisposition
                                {
                                    Disposition = ContentDisposition.Attachment,
                                    FileName = "message.eml"
                                };
                                LoadAttachments(new List<MimeEntity> {subMessage}, true);
                            }
                        }

                        if (part == null)
                        {
                            continue;
                        }

                        if (part.IsAttachment)
                        {
                            if (part is TnefPart)
                            {
                                var tnefPart = iter.Current as TnefPart;

                                if (tnefPart != null)
                                    LoadAttachments(tnefPart.ExtractAttachments(), true);
                            }
                            else
                            {
                                LoadAttachments(new List<MimeEntity> {part},
                                    multipart == null || !multipart.ContentType.MimeType.ToLowerInvariant().Equals("multipart/related"));
                            }
                        }
                        else if (multipart != null)
                        {
                            switch (multipart.ContentType.MimeType.ToLowerInvariant())
                            {
                                case "multipart/mixed":
                                    if (part is TextPart)
                                    {
                                        var entity = part as TextPart;

                                        var body = ConvertToHtml(entity);

                                        if (HtmlBodyStream.Length == 0)
                                        {
                                            sw.Write(body);
                                        }
                                        else
                                        {
                                            sw.Write("<hr /><br/>");
                                            sw.Write(body);
                                        }

                                        sw.Flush();
                                    }
                                    else
                                    {
                                        if (part.ContentType.MediaType.Equals("image",
                                            StringComparison.InvariantCultureIgnoreCase)
                                            && part.ContentDisposition.Disposition == ContentDisposition.Inline)
                                        {
                                            if (string.IsNullOrEmpty(part.ContentId))
                                                part.ContentId = Guid.NewGuid().ToString("N").ToLower();

                                            LoadAttachments(new List<MimeEntity> {part});

                                            sw.Write("<hr /><br/>");
                                            sw.Write("<img src=\"cid:{0}\">", part.ContentId);
                                            sw.Flush();
                                        }
                                        else
                                        {
                                            loadRealAttachment(part);
                                        }
                                    }

                                    break;
                                case "multipart/alternative":
                                    if (part is TextPart)
                                    {
                                        setBodyOrAttachment(part);
                                    }
                                    else
                                    {
                                        loadRealAttachment(part);
                                    }
                                    break;
                                case "multipart/related":
                                    if (part is TextPart)
                                    {
                                        setBodyOrAttachment(part);
                                    }
                                    else if (!string.IsNullOrEmpty(part.ContentId) || part.ContentLocation != null)
                                    {
                                        LoadAttachments(new List<MimeEntity> { part });
                                    }
                                    else
                                    {
                                        loadRealAttachment(part);
                                    }

                                    break;
                                default:
                                    if (part is TextPart)
                                    {
                                        setBodyOrAttachment(part);
                                    }
                                    else
                                    {
                                        loadRealAttachment(part);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            if (part is TextPart)
                            {
                                setBodyOrAttachment(part);
                            }
                            else
                            {
                                loadRealAttachment(part);
                            }
                        }
                    }
                }

                if (htmlStream.Length != 0)
                {
                    if (HtmlBodyStream.Length > 0)
                    {
                        sw.Write("<hr /><br/>");
                        sw.Flush();
                    }

                    htmlStream.CopyTo(sw.BaseStream);
                }
                else if (textStream.Length != 0)
                {
                    if (HtmlBodyStream.Length == 0)
                    {
                        TextBodyOnly = true;
                    }
                    else
                    {
                        sw.Write("<hr /><br/>");
                        sw.Flush();
                    }

                    textStream.CopyTo(sw.BaseStream);
                }

                htmlStream.Dispose();
                textStream.Dispose();   
            }

            if (HtmlBodyStream.Length != 0)
                return;

            HtmlBody = "<body><pre>&nbsp;</pre></body>";
            Introduction = "";
            TextBodyOnly = true;
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

        private static MailAttachment ConvertToMailAttachment(MimeEntity attachment, bool skipContentId = false)
        {
            var messagePart = attachment as MessagePart;
            var mimePart = attachment as MimePart;

            if (messagePart == null && mimePart == null)
                return null;

            var filename = attachment.ContentDisposition != null &&
                           !string.IsNullOrEmpty(attachment.ContentDisposition.FileName)
                ? attachment.ContentDisposition.FileName
                : attachment.ContentType != null && !string.IsNullOrEmpty(attachment.ContentType.Name)
                    ? attachment.ContentType.Name
                    : "";

            filename = MailUtil.ImproveFilename(filename, attachment.ContentType);

            var mailAttach = new MailAttachment
            {
                contentId = skipContentId ? null : attachment.ContentId,
                fileName = filename,
                contentType = attachment.ContentType != null ? attachment.ContentType.MimeType : null,
                contentLocation =
                    !skipContentId && attachment.ContentLocation != null
                        ? attachment.ContentLocation.ToString()
                        : null,
                dataStream = new MemoryStream()
            };

            if (messagePart != null)
                messagePart.Message.WriteTo(mailAttach.dataStream);
            else
                mimePart.ContentObject.DecodeTo(mailAttach.dataStream);

            mailAttach.size = mailAttach.dataStream.Length;

            return mailAttach;
        }

        public void LoadAttachments(IEnumerable<MimeEntity> attachments, bool skipContentId = false)
        {
            if (Attachments == null)
                Attachments = new List<MailAttachment>();

            foreach (var attachment in attachments)
            {
                var mailAttach = ConvertToMailAttachment(attachment, skipContentId);

                if (mailAttach == null)
                    continue;

                Attachments.Add(mailAttach);
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

                if (HtmlBodyStream.Length > 0)
                {
                    HtmlBodyStream.Seek(0, SeekOrigin.Begin);

                    var doc = new HtmlDocument();
                    doc.Load(HtmlBodyStream, Encoding.UTF8);

                    var hasChanges = false;

                    foreach (var attach in attchments)
                    {
                        HtmlNodeCollection oldNodes = null;

                        if (!string.IsNullOrEmpty(attach.contentId))
                        {
                            oldNodes =
                                doc.DocumentNode.SelectNodes("//img[@src and (contains(@src,'cid:" +
                                                             attach.contentId.Trim('<').Trim('>') + "'))]");
                        }

                        if (!string.IsNullOrEmpty(attach.contentLocation) && oldNodes == null)
                        {
                            oldNodes =
                                doc.DocumentNode.SelectNodes("//img[@src and (contains(@src,'" +
                                                             attach.contentLocation + "'))]");
                        }

                        if (oldNodes == null)
                        {
                            //This attachment is not embedded;
                            attach.contentId = null;
                            attach.contentLocation = null;
                            continue;
                        }

                        foreach (var node in oldNodes)
                        {
                            node.SetAttributeValue("src", attach.storedFileUrl);
                            hasChanges = true;
                        }
                    }

                    HtmlBodyStream.Seek(0, SeekOrigin.Begin);

                    if (!hasChanges)
                        return;

                    HtmlBodyStream.Close();
                    HtmlBodyStream.Dispose();

                    HtmlBodyStream = new MemoryStream();

                    using (var sw = new StreamWriter(HtmlBodyStream, Encoding.UTF8, 1024, true))
                    {
                        doc.DocumentNode.WriteTo(sw);
                        sw.Flush();
                        HtmlBodyStream.Seek(0, SeekOrigin.Begin);
                    }

                    HtmlBodyStream.Seek(0, SeekOrigin.Begin);
                }
                else
                {
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
            }
            catch (Exception ex)
            {
                log.Error("ReplaceEmbeddedImages() \r\n Exception: \r\n{0}\r\n", ex.ToString());
            }
        }


        private static bool IsDateCorrect(DateTime date)
        {
            return date >= DefaultMysqlMinimalDate && date <= DateTime.Now;
        }

        static void SkipErrors(Action method)
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

        static T ExecuteSafe<T>(Func<T> func)
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

        public static MailMessage CreateCorruptedMesage(MimeMessage message, int folder = 1,
            bool unread = false, string chainId = "", string streamId = "")
        {
            var mailMessage = new MailMessage
            {
                HasParseError = true
            };

            SkipErrors(() => mailMessage.Date = IsDateCorrect(message.Date.UtcDateTime)
                ? message.Date.UtcDateTime
                : DateTime.UtcNow);

            SkipErrors(() => mailMessage.MimeMessageId = (string.IsNullOrEmpty(message.MessageId) ? MailUtil.CreateMessageId() : message.MessageId)
                .Trim('<', '>'));

            SkipErrors(() => mailMessage.ChainId = string.IsNullOrEmpty(chainId) ? mailMessage.MimeMessageId : chainId);

            SkipErrors(() => mailMessage.MimeReplyToId = mailMessage.ChainId.Equals(mailMessage.MimeMessageId) ? null : message.InReplyTo.Trim('<', '>'));

            SkipErrors(() => mailMessage.ReplyTo = message.ReplyTo.ToString());

            SkipErrors(() => mailMessage.From = message.From.ToString());

            SkipErrors(() =>
                mailMessage.FromEmail =
                    message.From != null && message.From.Mailboxes != null && message.From.Mailboxes.Any()
                        ? message.From.Mailboxes.First().Address
                        : "");

            SkipErrors(() => mailMessage.ToList = message.To.Mailboxes.Select(s => ExecuteSafe(() => new MailAddress(s.Address, s.Name))).ToList());

            SkipErrors(() => mailMessage.To = string.Join(", ", message.To.Mailboxes.Select(s => s.ToString())));

            SkipErrors(() => mailMessage.CcList = message.Cc.Mailboxes.Select(s => ExecuteSafe(() => new MailAddress(s.Address, s.Name))).ToList());

            SkipErrors(() => mailMessage.Cc = string.Join(", ", message.Cc.Mailboxes.Select(s => s.ToString())));

            SkipErrors(() => mailMessage.Bcc = string.Join(", ", message.Bcc.Mailboxes.Select(s => s.ToString())));

            SkipErrors(() => mailMessage.Subject = message.Subject ?? string.Empty);

            SkipErrors(() => mailMessage.Important = message.Importance == MessageImportance.High || message.Priority == MessagePriority.Urgent);

            mailMessage.HtmlBodyStream = new MemoryStream();

            using (var sw = new StreamWriter(mailMessage.HtmlBodyStream, Encoding.UTF8, 1024, true))
            {
                sw.Write("<body><pre>&nbsp;</pre></body>");
                sw.Flush();
            }

            mailMessage.Size = mailMessage.HtmlBodyStream.Length;

            mailMessage.HeaderFieldNames = new NameValueCollection();

            message.Headers
                .ToList()
                .ForEach(h => SkipErrors(() => mailMessage.HeaderFieldNames.Add(h.Field, h.Value)));

            mailMessage.Folder = folder;
            mailMessage.IsNew = unread;
            mailMessage.StreamId = string.IsNullOrEmpty(streamId) ? MailUtil.CreateStreamId() : streamId;
            mailMessage.TextBodyOnly = true;
            mailMessage.Introduction = "";
            mailMessage.Attachments = new List<MailAttachment>();

            SkipErrors(() =>
            {
                var mailAttach = new MailAttachment
                {
                    contentId = null,
                    fileName = "message.eml",
                    contentType = "message/rfc822",
                    contentLocation = null,
                    dataStream = new MemoryStream()
                };

                message.WriteTo(mailAttach.dataStream);

                mailAttach.size = mailAttach.dataStream.Length;

                mailMessage.Attachments.Add(mailAttach);
            });

            return mailMessage;
        }

        [DataMember(EmitDefaultValue = false)]
        public List<MailAttachment> Attachments { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Introduction { get; set; }

        private string _htmlBody;

        [DataMember(EmitDefaultValue = false)]
        public string HtmlBody {
            get
            {
                if (HtmlBodyStream == null || HtmlBodyStream.Length <= 0)
                    return _htmlBody;

                HtmlBodyStream.Seek(0, SeekOrigin.Begin);
                _htmlBody = Encoding.UTF8.GetString(HtmlBodyStream.ReadToEnd());
                HtmlBodyStream.Seek(0, SeekOrigin.Begin);

                return _htmlBody;
            }
            set { _htmlBody = value; }
        }

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

        [IgnoreDataMember]
        public Stream HtmlBodyStream { get; private set; }

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