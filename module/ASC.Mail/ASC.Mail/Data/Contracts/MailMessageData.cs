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
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Text;
using ASC.Mail.Enums;
using ASC.Mail.Extensions;
using ASC.Mail.Utils;
using ASC.Specific;

namespace ASC.Mail.Data.Contracts
{
    [DataContract(Namespace = "")]
    public class MailMessageData
    {
        public MailMessageData()
        {
            ToList = new List<MailAddress>();
            CcList = new List<MailAddress>();
            BccList = new List<MailAddress>();
        }

        ~MailMessageData()
        {
            if (HtmlBodyStream != null)
                HtmlBodyStream.Dispose();
        }

        [DataMember(EmitDefaultValue = false)]
        public List<MailAttachmentData> Attachments { get; set; }

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
        public int Id { get; set; }

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
        public List<int> TagIds { get; set; }

        public string LabelsString
        {
            set
            {
                TagIds = new List<int>(MailUtil.GetLabelsFromString(value));
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
        public FolderType RestoreFolderId { get; set; }

        [DataMember]
        public FolderType Folder { get; set; }

        [DataMember]
        public uint? UserFolderId { get; set; }

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

        public List<CrmContactData> LinkedCrmEntityIds { get; set; }

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
        public List<MailAddress> BccList { get; set; }

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