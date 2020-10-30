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


using System.Collections.Generic;
using System.Linq;
using ASC.Files.Core.Security;
using ASC.Mail.Enums;
using ASC.Mail.Exceptions;
using ASC.Mail.Utils;

namespace ASC.Mail.Data.Contracts.Base
{
    public class MailComposeBase
    {
        private readonly string _calendarEventUid;
        private readonly string _calendarMethod;
        private readonly string _calendarIcs;

        public MailComposeBase(int id, MailBoxData mailBoxData, FolderType folder, string from, List<string> to, List<string> cc, List<string> bcc,
                         string subject, string mimeMessageId, string mimeReplyToId, bool important,
                         List<int> tags, string body, string streamId, List<MailAttachmentData> attachments, string calendarIcs = "")
        {
            Id = id;
            Mailbox = mailBoxData;
            Folder = folder;
            From = from;
            To = to ?? new List<string>();
            Cc = cc ?? new List<string>();
            Bcc = bcc ?? new List<string>();
            Subject = subject;
            MimeMessageId = mimeMessageId;
            MimeReplyToId = mimeReplyToId;
            Important = important;
            Labels = tags;
            HtmlBody = body ?? "";
            StreamId = streamId;

            var distinct = attachments == null ? new List<MailAttachmentData>() : attachments.Distinct().ToList();

            if (distinct.Sum(a => a.size) > Defines.ATTACHMENTS_TOTAL_SIZE_LIMIT)
                throw new DraftException(DraftException.ErrorTypes.TotalSizeExceeded,
                    "Total size of all files exceeds limit!");

            Attachments = distinct;
            AttachmentsEmbedded = new List<MailAttachmentData>();

            if (string.IsNullOrEmpty(calendarIcs)) 
                return;

            _calendarIcs = calendarIcs;

            var calendar = MailUtil.ParseValidCalendar(_calendarIcs);
            if (calendar != null)
            {
                _calendarMethod = calendar.Method;
                _calendarEventUid = calendar.Events[0].Uid;
            }
        }

        public int Id { get; set; }

        public MailBoxData Mailbox { get; set; }

        public FolderType Folder { get; set; }

        public List<string> To { get; set; }

        public List<string> Cc { get; set; }

        public List<string> Bcc { get; set; }

        public bool Important { get; set; }

        public string From { get; set; }

        public string Subject { get; set; }

        public string HtmlBody { get; set; }

        public List<MailAttachmentData> Attachments { get; set; }

        public List<MailAttachmentData> AttachmentsEmbedded { get; set; }

        public string StreamId { get; set; }

        public int ReplyToId { get; set; }

        public List<int> Labels { get; set; }

        public string MimeMessageId { get; set; }

        public string MimeReplyToId { get; set; }

        public FileShare FileLinksShareMode { get; set; }

        public bool AccountChanged {
            get { return Mailbox.MailBoxId != PreviousMailboxId; }
        }

        public int PreviousMailboxId { get; set; }

        public string CalendarIcs {
            get { return _calendarIcs; }
        }

        public string CalendarEventUid {
            get { return _calendarEventUid; }
        }

        public string CalendarMethod {
            get { return _calendarMethod; }
        }
    }
}