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
using System.IO;
using System.Runtime.Serialization;

using AjaxPro;

namespace ASC.Mail.Data.Contracts
{
    [DataContract(Name = "attachment", Namespace = "")]
    public class MailAttachmentData : ICloneable
    {
        private string _contentId;
        public MailAttachmentData()
        {
            data = null;
            dataStream = null;
        }

        ~MailAttachmentData()
        {
            if (dataStream != null)
                dataStream.Dispose();
        }

        //DO NOT RENAME field's lower case, for AjaxPro.JavaScriptSerializer (Upload handler) and Api.Serializer (Mail.Api) equal result;
        // ReSharper disable InconsistentNaming
        ///<example type="int" name="fileId">1234</example>
        [DataMember(Name = "fileId", EmitDefaultValue = false)]
        public int fileId { get; set; }

        ///<example name="fileName">fileName</example>
        [DataMember(Name = "fileName", EmitDefaultValue = false)]
        public string fileName { get; set; }

        ///<example type="int" name="size">1234</example>
        [DataMember(Name = "size", EmitDefaultValue = false)]
        public long size { get; set; }

        ///<example name="contentType">contentType</example>
        [DataMember(Name = "contentType", EmitDefaultValue = false)]
        public string contentType { get; set; }

        ///<example name="needSaveToTemp">true</example>
        [DataMember(Name = "needSaveToTemp", EmitDefaultValue = false)]
        public bool needSaveToTemp { get; set; }

        ///<example name="contentId">contentId</example>
        [DataMember(Name = "contentId", EmitDefaultValue = false)]
        public string contentId
        {
            get { return string.IsNullOrEmpty(_contentId) ? null : _contentId; }
            set { _contentId = value; }
        }

        ///<example type="int" name="fileNumber">333</example>
        [DataMember(Name = "fileNumber", EmitDefaultValue = false)]
        public int fileNumber { get; set; }

        ///<example name="storedName">storedName</example>
        [DataMember(Name = "storedName", EmitDefaultValue = false)]
        public string storedName { get; set; }

        ///<example name="streamId">streamId</example>
        [DataMember(Name = "streamId", EmitDefaultValue = false)]
        public string streamId { get; set; }

        ///<example name="savedToMyDocuments">true</example>
        [DataMember(Name = "savedToMyDocuments", EmitDefaultValue = false)]
        public bool attachedAsLink { get; set; }

        ///<example name="tempStoredUrl">tempStoredUrl</example>
        [DataMember(Name = "tempStoredUrl", EmitDefaultValue = false)]
        public string tempStoredUrl { get; set; }

        [IgnoreDataMember]
        [AjaxNonSerializable]
        public bool isTemp
        {
            get { return !string.IsNullOrEmpty(tempStoredUrl); }
        }

        [IgnoreDataMember]
        [AjaxNonSerializable]
        public string storedFileUrl { get; set; }

        [IgnoreDataMember]
        [AjaxNonSerializable]
        public byte[] data { get; set; }

        [IgnoreDataMember]
        [AjaxNonSerializable]
        public Stream dataStream { get; set; }

        [IgnoreDataMember]
        [AjaxNonSerializable]
        public string user { get; set; }

        [IgnoreDataMember]
        [AjaxNonSerializable]
        public int tenant { get; set; }

        [IgnoreDataMember]
        [AjaxNonSerializable]
        public bool isEmbedded
        {
            get { return !string.IsNullOrEmpty(contentId) || !string.IsNullOrEmpty(contentLocation); }
        }

        [IgnoreDataMember]
        [AjaxNonSerializable]
        public string contentLocation { get; set; }

        [IgnoreDataMember]
        [AjaxNonSerializable]
        public int mailboxId { get; set; }
        // ReSharper restore InconsistentNaming

        public override int GetHashCode()
        {
            return fileId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var a = obj as MailAttachmentData;
            return a != null && a.fileId == fileId && a.fileName == fileName && a.storedName == storedName &&
                   a.mailboxId == mailboxId && a.tenant == tenant && a.user == user;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}