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


using System.Collections.Generic;
using ASC.Files.Core.Security;
using ASC.Mail.Aggregator.Common.Utils;

namespace ASC.Mail.Aggregator.Common
{
    public class MailDraft
    {
        private readonly string _calendarEventUid;
        private readonly string _calendarMethod;
        private readonly string _calendarIcs;

        public MailDraft(int id, MailBox mailBox, string from, List<string> to, List<string> cc, List<string> bcc,
                         string subject, string mimeMessageId, string mimeReplyToId, bool important,
                         List<int> tags, string body, string streamId, List<MailAttachment> attachments, string calendarIcs = "")
        {
            Id = id;
            Mailbox = mailBox;
            From = from;
            To = to;
            Cc = cc;
            Bcc = bcc;
            Subject = subject;
            MimeMessageId = mimeMessageId;
            MimeReplyToId = mimeReplyToId;
            Important = important;
            Labels = tags;
            HtmlBody = body;
            StreamId = streamId;
            Attachments = attachments;
            AttachmentsEmbedded = new List<MailAttachment>();

            if (!string.IsNullOrEmpty(calendarIcs))
            {
                _calendarIcs = calendarIcs;

                var calendar = MailUtil.ParseValidCalendar(_calendarIcs);
                if (calendar != null)
                {
                    _calendarMethod = calendar.Method;
                    _calendarEventUid = calendar.Events[0].Uid;
                }
            }
        }

        public int Id { get; set; }

        public MailBox Mailbox { get; set; }

        public List<string> To { get; set; }

        public List<string> Cc { get; set; }

        public List<string> Bcc { get; set; }

        public bool Important { get; set; }

        public string From { get; set; }

        public string Subject { get; set; }

        public string HtmlBody { get; set; }

        public List<MailAttachment> Attachments { get; set; }

        public List<MailAttachment> AttachmentsEmbedded { get; set; }

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