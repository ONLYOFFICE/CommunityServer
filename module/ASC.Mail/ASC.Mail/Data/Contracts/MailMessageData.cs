/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

        ///<type>ASC.Mail.Data.Contracts.MailAttachmentData, ASC.Mail</type>
        ///<collection>list</collection>
        [DataMember(EmitDefaultValue = false)]
        public List<MailAttachmentData> Attachments { get; set; }

        ///<example>Introduction</example>
        [DataMember(EmitDefaultValue = false)]
        public string Introduction { get; set; }

        private string _htmlBody;

        ///<example>HtmlBody</example>
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

        ///<example>true</example>
        [DataMember]
        public bool ContentIsBlocked { get; set; }

        ///<example>true</example>
        [DataMember]
        public bool Important { get; set; }

        ///<example>Subject</example>
        [DataMember]
        public string Subject { get; set; }

        ///<example>true</example>
        [DataMember]
        public bool HasAttachments { get; set; }

        ///<example>Bcc</example>
        [DataMember(EmitDefaultValue = false)]
        public string Bcc { get; set; }

        ///<example>Cc</example>
        [DataMember(EmitDefaultValue = false)]
        public string Cc { get; set; }

        ///<example>To</example>
        [DataMember]
        public string To { get; set; }

        ///<example>Address</example>
        [DataMember]
        public string Address { get; set; }

        ///<example>From</example>
        [DataMember]
        public string From { get; set; }

        public string FromEmail { get; set; }

        ///<example>ReplyTo</example>
        [DataMember(EmitDefaultValue = false)]
        public string ReplyTo { get; set; }

        ///<example type="int">555</example>
        [DataMember]
        public int Id { get; set; }

        ///<example>ChainId</example>
        [DataMember(EmitDefaultValue = false)]
        public string ChainId { get; set; }

        public DateTime ChainDate { get; set; }

        ///<example name="ChainDate">ChainDate</example>
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

        ///<example name="Date">Date</example>
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

        ///<example name="DateDisplay">DateDisplay</example>
        [DataMember(Name = "DateDisplay")]
        public string DateDisplay
        {
            get { return Date.ToVerbString(); }
        }

        ///<example type="int" name="TagIds">1234</example>
        ///<collection>list</collection>
        [DataMember(EmitDefaultValue = false)]
        public List<int> TagIds { get; set; }

        public string LabelsString
        {
            set
            {
                TagIds = new List<int>(MailUtil.GetLabelsFromString(value));
            }
        }

        ///<example name="LabelsInString">LabelsInString</example>
        [DataMember(Name = "LabelsInString")]
        public string LabelsInString
        {
            get { return MailUtil.GetStringFromLabels(TagIds); }
        }

        ///<example>true</example>
        [DataMember]
        public bool IsNew { get; set; }

        ///<example>true</example>
        [DataMember]
        public bool IsAnswered { get; set; }

        ///<example>true</example>
        [DataMember]
        public bool IsForwarded { get; set; }

        ///<example>true</example>
        [DataMember]
        public bool TextBodyOnly { get; set; }

        ///<example type="int">555555</example>
        [DataMember]
        public long Size { get; set; }

        ///<example>EMLLink</example>
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public string EMLLink { get; set; }

        ///<example>StreamId</example>
        [DataMember]
        public string StreamId { get; set; }

        ///<example type="int">1</example>
        [DataMember]
        public FolderType RestoreFolderId { get; set; }

        ///<example type="int">1</example>
        [DataMember]
        public FolderType Folder { get; set; }

        ///<example type="int">144</example>
        [DataMember]
        public uint? UserFolderId { get; set; }

        ///<example type="int">144</example>
        [DataMember]
        public int ChainLength { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool WasNew { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool IsToday { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool IsYesterday { get; set; }

        ///<example>2020-12-18T08:12:09.1209967Z</example>
        [DataMember]
        public ApiDateTime ReceivedDate
        {
            get { return (ApiDateTime)Date; }
        }

        public int MailboxId { get; set; }

        public List<CrmContactData> LinkedCrmEntityIds { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool IsBodyCorrupted { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool HasParseError { get; set; }

        ///<example>MimeMessageId</example>
        [DataMember]
        public string MimeMessageId { get; set; }

        ///<example>MimeReplyToId</example>
        [DataMember]
        public string MimeReplyToId { get; set; }

        ///<example>CalendarUid</example>
        [DataMember]
        public string CalendarUid { get; set; }

        [DataMember]
        public bool ReadRequestStatus { get; set; }

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