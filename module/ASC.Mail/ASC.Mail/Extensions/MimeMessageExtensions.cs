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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using ASC.Common.Logging;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;
using ASC.Mail.Utils;
using MimeKit;

namespace ASC.Mail.Extensions
{
    public static class MimeMessageExtensions
    {
        public static void FixEncodingIssues(this MimeMessage mimeMessage, ILog logger = null)
        {
            if (logger == null)
                logger = new NullLog();

            try
            {
                foreach (var mimeEntity in mimeMessage.BodyParts)
                {
                    var textPart = mimeEntity as TextPart;

                    if (textPart == null ||
                        textPart.Content == null ||
                        textPart.Content.Encoding != ContentEncoding.Default)
                    {
                        continue;
                    }

                    try
                    {
                        string charset;
                        using (var stream = new MemoryStream())
                        {
                            textPart.Content.DecodeTo(stream);
                            var bytes = stream.ToArray();
                            charset = EncodingTools.DetectCharset(bytes);
                        }

                        if (!string.IsNullOrEmpty(charset) &&
                            (textPart.ContentType == null ||
                             string.IsNullOrEmpty(textPart.ContentType.Charset) ||
                             textPart.ContentType.Charset != charset))
                        {
                            var encoding = EncodingTools.GetEncodingByCodepageName(charset);

                            if(encoding == null)
                                continue;

                            var newText = textPart.GetText(charset);

                            textPart.SetText(encoding, newText);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.WarnFormat("MimeMessage.FixEncodingIssues->ImproveBodyEncoding: {0}", ex.Message);
                    }
                }

                if (mimeMessage.Headers.Contains(HeaderId.From))
                {
                    var fromParsed = mimeMessage.From.FirstOrDefault();
                    if (fromParsed != null && !string.IsNullOrEmpty(fromParsed.Name))
                    {
                        var fromHeader = mimeMessage.Headers.FirstOrDefault(h => h.Id == HeaderId.From);
                        fromHeader.FixEncodingIssues(logger);
                    }
                }

                if (!mimeMessage.Headers.Contains(HeaderId.Subject))
                    return;

                var subjectHeader = mimeMessage.Headers.FirstOrDefault(h => h.Id == HeaderId.Subject);
                subjectHeader.FixEncodingIssues(logger);

            }
            catch (Exception ex)
            {
                logger.WarnFormat("MimeMessage.FixEncodingIssues: {0}", ex.Message);
            }
        }

        public static void FixEncodingIssues(this Header header, ILog logger = null)
        {
            if (logger == null)
                logger = new NullLog();

            try
            {
                var rawValueString = Encoding.UTF8.GetString(header.RawValue).Trim();
                if (rawValueString.IndexOf("?q?", StringComparison.InvariantCultureIgnoreCase) > -1 ||
                    rawValueString.IndexOf("?b?", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    return;
                }

                var charset = EncodingTools.DetectCharset(header.RawValue);

                if(string.IsNullOrEmpty(charset))
                    return;

                var newValue = header.GetValue(charset);

                if (header.Value.Equals(newValue, StringComparison.InvariantCultureIgnoreCase))
                    return;

                var encoding = EncodingTools.GetEncodingByCodepageName(charset);
                header.SetValue(encoding, newValue);
            }
            catch (Exception ex)
            {
                logger.WarnFormat("Header.FixEncodingIssues: {0}", ex.Message);
            }
        }

        public static void FixDateIssues(this MimeMessage mimeMessage, DateTimeOffset? internalDate = null, ILog logger = null)
        {
            if (logger == null)
                logger = new NullLog();

            try
            {
                if (!mimeMessage.Headers.Contains(HeaderId.Date) || mimeMessage.Date > DateTimeOffset.UtcNow)
                {
                    mimeMessage.Date = internalDate ?? DateTimeOffset.UtcNow;
                }
            }
            catch (Exception ex)
            {
                logger.WarnFormat("MimeMessage.FixEncodingIssues: {0}", ex.Message);
            }
        }

        public static MailMessageData CreateMailMessage(this MimeMessage message,
            int mailboxId = -1,
            FolderType folder = FolderType.Inbox,
            bool unread = false,
            string chainId = "",
            DateTime? chainDate = null,
            string streamId = "",
            ILog log = null)
        {
            var mail = new MailMessageData();

            if (message == null)
                throw new ArgumentNullException("message");

            log = log ?? new NullLog();

            mail.MailboxId = mailboxId;

            var now = DateTime.UtcNow;

            mail.Date = MailUtil.IsDateCorrect(message.Date.UtcDateTime) ? message.Date.UtcDateTime : now;

            mail.MimeMessageId = (string.IsNullOrEmpty(message.MessageId)
                ? MailUtil.CreateMessageId()
                : message.MessageId)
                .Trim('<', '>');

            mail.ChainId = string.IsNullOrEmpty(chainId) ? mail.MimeMessageId : chainId;

            mail.ChainDate = chainDate ?? now;

            mail.MimeReplyToId = mail.ChainId.Equals(mail.MimeMessageId) || string.IsNullOrEmpty(message.InReplyTo)
                ? null
                : message.InReplyTo.Trim('<', '>');

            mail.ReplyTo = message.ReplyTo.ToString();

            mail.From = message.From.ToString();

            mail.FromEmail = message.From != null && message.From.Mailboxes != null && message.From.Mailboxes.Any()
                ? message.From.Mailboxes.First().Address
                : "";

            mail.ToList = message.To.Mailboxes.Select(s => new MailAddress(s.Address, s.Name)).ToList();

            mail.To = string.Join(", ", message.To.Mailboxes.Select(s => s.ToString()));

            mail.CcList = message.Cc.Mailboxes.Select(s => new MailAddress(s.Address, s.Name)).ToList();

            mail.Cc = string.Join(", ", message.Cc.Mailboxes.Select(s => s.ToString()));

            mail.Bcc = string.Join(", ", message.Bcc.Mailboxes.Select(s => s.ToString()));

            mail.Subject = message.Subject ?? string.Empty;

            mail.Important = message.Importance == MessageImportance.High || message.Priority == MessagePriority.Urgent;

            mail.TextBodyOnly = false;

            mail.Introduction = "";

            mail.Attachments = new List<MailAttachmentData>();

            mail.HtmlBodyStream = new MemoryStream();

            mail.ExtractMainParts(message);

            mail.Size = mail.HtmlBodyStream.Length > 0 ? mail.HtmlBodyStream.Length : mail.HtmlBody.Length;

            mail.HeaderFieldNames = new NameValueCollection();

            message.Headers
                .ToList()
                .ForEach(h => mail.HeaderFieldNames.Add(h.Field, h.Value));

            mail.Folder = folder;

            mail.IsNew = unread;

            mail.StreamId = string.IsNullOrEmpty(streamId) ? MailUtil.CreateStreamId() : streamId;

            mail.LoadCalendarInfo(message, log);

            return mail;
        }

        public static MailMessageData CreateCorruptedMesage(this MimeMessage message, 
            FolderType folder = FolderType.Inbox,
            bool unread = false, 
            string chainId = "", 
            string streamId = "")
        {
            var mailMessage = new MailMessageData
            {
                HasParseError = true
            };

            MailUtil.SkipErrors(() => mailMessage.Date = MailUtil.IsDateCorrect(message.Date.UtcDateTime)
                ? message.Date.UtcDateTime
                : DateTime.UtcNow);

            MailUtil.SkipErrors(() => mailMessage.MimeMessageId = (string.IsNullOrEmpty(message.MessageId) ? MailUtil.CreateMessageId() : message.MessageId)
                .Trim('<', '>'));

            MailUtil.SkipErrors(() => mailMessage.ChainId = string.IsNullOrEmpty(chainId) ? mailMessage.MimeMessageId : chainId);

            MailUtil.SkipErrors(() => mailMessage.MimeReplyToId = mailMessage.ChainId.Equals(mailMessage.MimeMessageId) ? null : message.InReplyTo.Trim('<', '>'));

            MailUtil.SkipErrors(() => mailMessage.ReplyTo = message.ReplyTo.ToString());

            MailUtil.SkipErrors(() => mailMessage.From = message.From.ToString());

            MailUtil.SkipErrors(() =>
                mailMessage.FromEmail =
                    message.From != null && message.From.Mailboxes != null && message.From.Mailboxes.Any()
                        ? message.From.Mailboxes.First().Address
                        : "");

            MailUtil.SkipErrors(() => mailMessage.ToList = message.To.Mailboxes.Select(s => MailUtil.ExecuteSafe(() => new MailAddress(s.Address, s.Name))).ToList());

            MailUtil.SkipErrors(() => mailMessage.To = string.Join(", ", message.To.Mailboxes.Select(s => s.ToString())));

            MailUtil.SkipErrors(() => mailMessage.CcList = message.Cc.Mailboxes.Select(s => MailUtil.ExecuteSafe(() => new MailAddress(s.Address, s.Name))).ToList());

            MailUtil.SkipErrors(() => mailMessage.Cc = string.Join(", ", message.Cc.Mailboxes.Select(s => s.ToString())));

            MailUtil.SkipErrors(() => mailMessage.Bcc = string.Join(", ", message.Bcc.Mailboxes.Select(s => s.ToString())));

            MailUtil.SkipErrors(() => mailMessage.Subject = message.Subject ?? string.Empty);

            MailUtil.SkipErrors(() => mailMessage.Important = message.Importance == MessageImportance.High || message.Priority == MessagePriority.Urgent);

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
                .ForEach(h => MailUtil.SkipErrors(() => mailMessage.HeaderFieldNames.Add(h.Field, h.Value)));

            mailMessage.Folder = folder;
            mailMessage.IsNew = unread;
            mailMessage.StreamId = string.IsNullOrEmpty(streamId) ? MailUtil.CreateStreamId() : streamId;
            mailMessage.TextBodyOnly = true;
            mailMessage.Introduction = "";
            mailMessage.Attachments = new List<MailAttachmentData>();

            MailUtil.SkipErrors(() =>
            {
                var mailAttach = new MailAttachmentData
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
    }
}