/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System.Runtime.Serialization;
using ASC.Data.Storage;
using ASC.Mail.Aggregator.Common.Utils;
using AjaxPro;

namespace ASC.Mail.Aggregator.Common
{
    [DataContract(Name = "attachment", Namespace = "")]
    public class MailAttachment
    {
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
        public string contentId { get; set; }
        
        [DataMember(Name = "fileNumber", EmitDefaultValue = false)]
        public int fileNumber { get; set; }
        
        [DataMember(Name = "storedName", EmitDefaultValue = false)]
        public string storedName { get; set; }
        
        [DataMember(Name = "streamId", EmitDefaultValue = false)]
        public string streamId { get; set; }

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

        // ReSharper restore InconsistentNaming
    }
}