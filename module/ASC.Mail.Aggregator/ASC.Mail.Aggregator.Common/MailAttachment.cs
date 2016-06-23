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


using System.Runtime.Serialization;
using AjaxPro;

namespace ASC.Mail.Aggregator.Common
{
    [DataContract(Name = "attachment", Namespace = "")]
    public class MailAttachment
    {
        private string _contentId;
        public MailAttachment()
        {
            data = new byte[0];
        }

        //DO NOT RENAME field's lower case, for AjaxPro.JavaScriptSerializer (Upload handler) and Api.Serializer (Mail.Api) equal result;
        // ReSharper disable InconsistentNaming
        [DataMember(Name = "fileId", EmitDefaultValue = false)]
        public int fileId { get; set; }

        [DataMember(Name = "fileName", EmitDefaultValue = false)]
        public string fileName { get; set; }
        
        [DataMember(Name = "size", EmitDefaultValue = false)]
        public long size { get; set; }
        
        [DataMember(Name = "contentType", EmitDefaultValue = false)]
        public string contentType { get; set; }
        
        [DataMember(Name = "contentId", EmitDefaultValue = false)]
        public string contentId {
            get { return string.IsNullOrEmpty(_contentId) ? null : _contentId; }
            set { _contentId = value; }
        }
        
        [DataMember(Name = "fileNumber", EmitDefaultValue = false)]
        public int fileNumber { get; set; }
        
        [DataMember(Name = "storedName", EmitDefaultValue = false)]
        public string storedName { get; set; }

        [DataMember(Name = "streamId", EmitDefaultValue = false)]
        public string streamId { get; set; }

        [DataMember(Name = "savedToMyDocuments", EmitDefaultValue = false)]
        public bool attachedAsLink { get; set; }

        [IgnoreDataMember]
        [AjaxNonSerializable]
        public string storedFileUrl { get; set; }
        
        [IgnoreDataMember]
        [AjaxNonSerializable]
        public byte[] data { get; set; }

        [IgnoreDataMember]
        [AjaxNonSerializable]
        public string user { get; set; }

        [IgnoreDataMember]
        [AjaxNonSerializable]
        public int tenant { get; set; }

        [IgnoreDataMember]
        [AjaxNonSerializable]
        public bool isEmbedded {
            get { return !string.IsNullOrEmpty(contentId) || !string.IsNullOrEmpty(contentLocation); }
        }

        [IgnoreDataMember]
        [AjaxNonSerializable]
        public string contentLocation { get; set; }

        [IgnoreDataMember]
        [AjaxNonSerializable]
        public int mailboxId { get; set; }
        // ReSharper restore InconsistentNaming
    }
}