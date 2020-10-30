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
using System.IO;
using System.Linq;
using System.Text;
using ASC.Common.Logging;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Utils;
using HtmlAgilityPack;
using MimeKit;
using MimeKit.Text;
using MimeKit.Tnef;

namespace ASC.Mail.Extensions
{
    public static class MailMessageExtensions
    {
        public static void LoadCalendarInfo(this MailMessageData mail, MimeMessage message, ILog log = null)
        {
            if (!message.BodyParts.Any())
                return;

            log = log ?? new NullLog();

            try
            {
                var calendarParts = message.BodyParts.Where(p => p.ContentType.IsMimeType("text", "calendar")).ToList();

                if (!calendarParts.Any())
                    return;

                log.DebugFormat("LoadCalendarInfo found {0} calendars", calendarParts.Count);

                foreach (var calendarPart in calendarParts)
                {
                    var p = calendarPart as TextPart;
                    if (p == null)
                        continue;

                    var ics = p.Text;

                    var calendar = MailUtil.ParseValidCalendar(ics, log);

                    if (calendar == null)
                        continue;

                    if (calendar.Events[0].Organizer == null &&
                        calendar.Method.Equals(Defines.ICAL_REPLY, StringComparison.OrdinalIgnoreCase))
                    {
                        // Fix reply organizer (Outlook style of Reply)
                        var toAddress = message.To.Mailboxes.FirstOrDefault();
                        if (toAddress != null)
                        {
                            calendar.Events[0].Organizer = new Ical.Net.DataTypes.Organizer("mailto:" + toAddress.Address)
                            {
                                CommonName = string.IsNullOrEmpty(toAddress.Name)
                                    ? toAddress.Address
                                    : toAddress.Name
                            };

                            ics = MailUtil.SerializeCalendar(calendar);
                        }
                    }

                    mail.CalendarUid = calendar.Events[0].Uid;
                    mail.CalendarId = -1;
                    mail.CalendarEventIcs = ics;
                    mail.CalendarEventCharset = string.IsNullOrEmpty(p.ContentType.Charset) ? Encoding.UTF8.HeaderName : p.ContentType.Charset;
                    mail.CalendarEventMimeType = p.ContentType.MimeType;

                    log.DebugFormat("Calendar UID: {0} Method: {1} ics: {2}", mail.CalendarUid, calendar.Method, mail.CalendarEventIcs);

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
                                            p.Content.DecodeTo(stream);
                                            var bytes = stream.ToArray();
                                            var encoding = MailUtil.GetEncoding(p.ContentType.Charset);
                                            icsAttach = encoding.GetString(bytes);
                                        }
                                    }

                                    var cal = MailUtil.ParseValidCalendar(icsAttach, log);

                                    if (cal == null)
                                        return false;

                                    return mail.CalendarUid == cal.Events[0].Uid;
                                });

                    if (calendarExists)
                    {
                        log.Debug("Calendar exists as attachment");
                        continue;
                    }

                    if (calendarPart.ContentDisposition == null)
                    {
                        calendarPart.ContentDisposition = new ContentDisposition();
                    }

                    if (string.IsNullOrEmpty(calendarPart.ContentDisposition.FileName))
                    {
                        calendarPart.ContentDisposition.FileName = calendar.Method == Defines.ICAL_REQUEST
                            ? "invite.ics"
                            : calendar.Method == Defines.ICAL_REPLY ? "reply.ics" : "cancel.ics";
                    }

                    mail.LoadAttachments(new List<MimeEntity> { calendarPart }, true);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("LoadCalendarInfo() \r\n Exception: \r\n{0}\r\n", ex.ToString());
            }
        }

        private static string ConvertToHtml(TextPart entity)
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

        public static void ExtractMainParts(this MailMessageData mail, MimeMessage message)
        {
            var htmlStream = new MemoryStream();
            var textStream = new MemoryStream();

            Action<MimeEntity> loadRealAttachment = (part) =>
            {
                if ((part.ContentDisposition != null && !string.IsNullOrEmpty(part.ContentDisposition.FileName))
                    || part.ContentType != null && !string.IsNullOrEmpty(part.ContentType.Name))
                {
                    mail.LoadAttachments(new List<MimeEntity> { part });
                }
            };

            Action<MimePart> setBodyOrAttachment = (part) =>
            {
                var entity = part as TextPart;

                if (htmlStream == null || textStream == null)
                    throw new Exception("Streams are not initialized");

                if ((entity == null
                     || htmlStream.Length != 0 && entity.IsHtml
                     || textStream.Length != 0 && !entity.IsHtml))
                {
                    loadRealAttachment(part);
                }
                else
                {

                    if (mail.Introduction.Length < 200 && (!entity.IsHtml && !string.IsNullOrEmpty(entity.Text)))
                    {
                        if (string.IsNullOrEmpty(mail.Introduction))
                        {
                            mail.Introduction = (entity.Text.Length > 200 ? entity.Text.Substring(0, 200) : entity.Text);
                        }
                        else
                        {
                            var need = 200 - mail.Introduction.Length;
                            mail.Introduction += (entity.Text.Length > need ? entity.Text.Substring(0, need) : entity.Text);
                        }

                        mail.Introduction = mail.Introduction.Replace("\r\n", " ").Replace("\n", " ").Trim();
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

            var newPathIndex = "";

            using (var sw = new StreamWriter(mail.HtmlBodyStream, Encoding.UTF8, 1024, true))
            {
                using (var iter = new MimeIterator(message))
                {
                    while (string.IsNullOrEmpty(newPathIndex) ? iter.MoveNext() : iter.MoveTo(newPathIndex))
                    {
                        if (!string.IsNullOrEmpty(newPathIndex))
                            newPathIndex = "";

                        var multipart = iter.Parent as Multipart;
                        var part = iter.Current as MimePart;

                        var subMessage = iter.Current as MessagePart;
                        if (subMessage != null)
                        {
                            if ((subMessage.ContentDisposition != null &&
                                 !string.IsNullOrEmpty(subMessage.ContentDisposition.FileName))
                                || subMessage.ContentType != null && !string.IsNullOrEmpty(subMessage.ContentType.Name))
                            {
                                mail.LoadAttachments(new List<MimeEntity> { subMessage }, true);
                            }
                            else
                            {
                                subMessage.ContentDisposition = new ContentDisposition
                                {
                                    Disposition = ContentDisposition.Attachment,
                                    FileName = "message.eml"
                                };

                                mail.LoadAttachments(new List<MimeEntity> { subMessage }, true);
                            }

                            float pathIndex;

                            if (float.TryParse(iter.PathSpecifier, out pathIndex))
                            {
                                pathIndex++;
                                newPathIndex = ((int)pathIndex).ToString();
                                continue;
                            }
                        }

                        if (part == null || iter.Parent is MessagePart)
                        {
                            continue;
                        }

                        if (part.IsAttachment)
                        {
                            if (part is TnefPart)
                            {
                                var tnefPart = iter.Current as TnefPart;

                                if (tnefPart != null)
                                    mail.LoadAttachments(tnefPart.ExtractAttachments(), true);
                            }
                            else
                            {
                                mail.LoadAttachments(new List<MimeEntity> { part },
                                    multipart == null || !multipart.ContentType.MimeType.ToLowerInvariant().Equals("multipart/related"));
                            }
                        }
                        else if (multipart != null)
                        {
                            switch (multipart.ContentType.MimeType.ToLowerInvariant())
                            {
                                case "multipart/report":
                                    if (part is TextPart)
                                    {
                                        var entity = part as TextPart;

                                        if (entity == null)
                                        {
                                            entity = new TextPart(TextFormat.Plain);

                                            entity.SetText(Encoding.UTF8, part.ToString());
                                        }

                                        var body = ConvertToHtml(entity);

                                        if (mail.HtmlBodyStream.Length == 0)
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
                                    else if (part is MessageDeliveryStatus)
                                    {
                                        var entity = new TextPart(TextFormat.Plain);

                                        var mds = (MessageDeliveryStatus)part;

                                        using (var memory = new MemoryStream())
                                        {
                                            mds.Content.DecodeTo(memory);

                                            var text =
                                                Encoding.ASCII.GetString(memory.GetBuffer(), 0, (int) memory.Length)
                                                    .Replace("\r\n", "\n");

                                            entity.SetText(Encoding.UTF8, text);
                                        }

                                        var body = ConvertToHtml(entity);

                                        if (mail.HtmlBodyStream.Length == 0)
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
                                        loadRealAttachment(part);
                                    }

                                    break;
                                case "multipart/mixed":
                                    if (part is TextPart)
                                    {
                                        var entity = part as TextPart;

                                        var body = ConvertToHtml(entity);

                                        if (mail.HtmlBodyStream.Length == 0)
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
                                        if (part.ContentType != null
                                            && part.ContentType.MediaType.Equals("image",
                                            StringComparison.InvariantCultureIgnoreCase)
                                            && part.ContentDisposition != null
                                            && part.ContentDisposition.Disposition.Equals(ContentDisposition.Inline,
                                                StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            if (string.IsNullOrEmpty(part.ContentId))
                                                part.ContentId = Guid.NewGuid().ToString("N").ToLower();

                                            mail.LoadAttachments(new List<MimeEntity> { part });

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
                                        mail.LoadAttachments(new List<MimeEntity> { part });
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
                    if (mail.HtmlBodyStream.Length > 0)
                    {
                        sw.Write("<hr /><br/>");
                        sw.Flush();
                    }

                    htmlStream.CopyTo(sw.BaseStream);
                }
                else if (textStream.Length != 0)
                {
                    if (mail.HtmlBodyStream.Length == 0)
                    {
                        mail.TextBodyOnly = true;
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

            if (mail.HtmlBodyStream.Length != 0)
                return;

            mail.HtmlBody = "<body><pre>&nbsp;</pre></body>";
            mail.Introduction = "";
            mail.TextBodyOnly = true;
        }

        public static void LoadAttachments(this MailMessageData mail, IEnumerable<MimeEntity> attachments, bool skipContentId = false)
        {
            if (mail.Attachments == null)
                mail.Attachments = new List<MailAttachmentData>();

            foreach (var attachment in attachments)
            {
                var mailAttach = ConvertToMailAttachment(attachment, skipContentId);

                if (mailAttach == null)
                    continue;

                mail.Attachments.Add(mailAttach);
                mail.HasAttachments = true;
            }

        }

        private static MailAttachmentData ConvertToMailAttachment(MimeEntity attachment, bool skipContentId = false)
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

            var mailAttach = new MailAttachmentData
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
                mimePart.Content.DecodeTo(mailAttach.dataStream);

            mailAttach.size = mailAttach.dataStream.Length;

            return mailAttach;
        }

        public static void ReplaceEmbeddedImages(this MailMessageData mail, ILog log = null)
        {
            log = log ?? new NullLog();

            try
            {
                var attchments = mail.Attachments.Where(a => a.isEmbedded && !string.IsNullOrEmpty(a.storedFileUrl)).ToList();

                if (!attchments.Any())
                    return;

                if (mail.HtmlBodyStream.Length > 0)
                {
                    mail.HtmlBodyStream.Seek(0, SeekOrigin.Begin);

                    var doc = new HtmlDocument();
                    doc.Load(mail.HtmlBodyStream, Encoding.UTF8);

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

                    mail.HtmlBodyStream.Seek(0, SeekOrigin.Begin);

                    if (!hasChanges)
                        return;

                    mail.HtmlBodyStream.Close();
                    mail.HtmlBodyStream.Dispose();

                    mail.HtmlBodyStream = new MemoryStream();

                    using (var sw = new StreamWriter(mail.HtmlBodyStream, Encoding.UTF8, 1024, true))
                    {
                        doc.DocumentNode.WriteTo(sw);
                        sw.Flush();
                        mail.HtmlBodyStream.Seek(0, SeekOrigin.Begin);
                    }

                    mail.HtmlBodyStream.Seek(0, SeekOrigin.Begin);
                }
                else
                {
                    foreach (var attach in attchments)
                    {
                        if (!string.IsNullOrEmpty(attach.contentId))
                        {
                            mail.HtmlBody = mail.HtmlBody.Replace(string.Format("cid:{0}", attach.contentId.Trim('<').Trim('>')),
                                attach.storedFileUrl);
                        }
                        else if (!string.IsNullOrEmpty(attach.contentLocation))
                        {
                            mail.HtmlBody = mail.HtmlBody.Replace(string.Format("{0}", attach.contentLocation),
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
                log.ErrorFormat("ReplaceEmbeddedImages() \r\n Exception: \r\n{0}\r\n", ex.ToString());
            }
        }
    }
}