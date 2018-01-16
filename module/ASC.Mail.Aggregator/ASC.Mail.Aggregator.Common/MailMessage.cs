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
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Text;
using ASC.Mail.Aggregator.Common.Collection;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Specific;

namespace ASC.Mail.Aggregator.Common
{
    [DataContract(Namespace = "")]
    public class MailMessage
    {
        public MailMessage()
        {
        }

        ~MailMessage()
        {
            if (HtmlBodyStream != null)
                HtmlBodyStream.Dispose();
        }

        [DataMember(EmitDefaultValue = false)]
        public List<MailAttachment> Attachments { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Introduction { get; set; }

        private string _htmlBody;

        [DataMember(EmitDefaultValue = false)]
        public string HtmlBody
        {
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
            get { return Date.ToString("G", CultureInfo.InvariantCulture); }
            set
            {
                DateTime date;
                if (DateTime.TryParseExact(value, "g", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date))
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
        public Stream HtmlBodyStream { get; set; }

        [IgnoreDataMember]
        public string Uidl { get; set; }

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
            public bool LoadEmebbedAttachements { get; set; }

            public string ProxyHttpHandlerUrl { get; set; }
        }
    }
}