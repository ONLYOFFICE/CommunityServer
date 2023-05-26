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
using System.Threading;
using System.Web;

using ASC.Api.Attributes;
using ASC.Mail.Core.Engine;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Utils;

// ReSharper disable InconsistentNaming

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// Exports all the message attachments to the folder with documents.
        /// </summary>
        /// <short>Export message attachments</short>
        /// <param type="System.Int32, System" name="id_message">Message ID</param>
        /// <param type="System.String, System" name="id_folder" optional="true">Folder ID (if this parameter is empty, the "My documents" folder is used)</param>
        /// <returns>Number of attachments exported</returns>
        /// <category>Attachments</category>
        /// <path>api/2.0/mail/messages/attachments/export</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"messages/attachments/export")]
        public int ExportAttachmentsToDocuments(int id_message, string id_folder = null)
        {
            if (id_message < 1)
                throw new ArgumentException(@"Invalid message id", "id_message");

            if (string.IsNullOrEmpty(id_folder))
                id_folder = DocumentsEngine.MY_DOCS_FOLDER_ID;

            var scheme = HttpContext.Current == null ? Uri.UriSchemeHttp : HttpContext.Current.Request.GetUrlRewriter().Scheme;
            var documentsDal = new DocumentsEngine(TenantId, Username, scheme);
            var savedAttachmentsList = documentsDal.StoreAttachmentsToDocuments(id_message, id_folder);

            return savedAttachmentsList.Count;
        }

        /// <summary>
        /// Exports an attachment with the ID specified in the request to the folder with documents.
        /// </summary>
        /// <short>Export an attachment</short>
        /// <param type="System.Int32, System" name="id_attachment">Attachment ID</param>
        /// <param type="System.String, System" name="id_folder" optional="true">Folder ID (if this parameter is empty, the "My documents" folder is used)</param>
        /// <returns>Document ID in the folder with documents</returns>
        /// <category>Attachments</category>
        /// <path>api/2.0/mail/messages/attachment/export</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"messages/attachment/export")]
        public object ExportAttachmentToDocuments(int id_attachment, string id_folder = null)
        {
            if (id_attachment < 1)
                throw new ArgumentException(@"Invalid attachment id", "id_attachment");

            if (string.IsNullOrEmpty(id_folder))
                id_folder = DocumentsEngine.MY_DOCS_FOLDER_ID;

            var scheme = HttpContext.Current == null ? Uri.UriSchemeHttp : HttpContext.Current.Request.GetUrlRewriter().Scheme;

            var documentsDal = new DocumentsEngine(TenantId, Username, scheme);
            var documentId = documentsDal.StoreAttachmentToDocuments(id_attachment, id_folder);
            return documentId;
        }

        /// <summary>
        /// Adds an attachment to the draft with the ID specified in the request.
        /// </summary>
        /// <short>Add an attachment</short>
        /// <param type="System.Int32, System" name="id_message">Message ID</param>
        /// <param type="System.String, System" name="name">File name</param>
        /// <param type="System.IO.Stream, System.IO" name="file">File stream</param>
        /// <param type="System.String, System" name="content_type">File content type</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailAttachmentData, ASC.Mail">Mail attachment</returns>
        /// <category>Attachments</category>
        /// <path>api/2.0/mail/messages/attachment/add</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"messages/attachment/add")]
        public MailAttachmentData AddAttachment(int id_message, string name, Stream file, string content_type)
        {
            var attachment = MailEngineFactory.AttachmentEngine
                .AttachFileToDraft(TenantId, Username, id_message, name, file, file.Length, content_type);

            return attachment;
        }

        /// <summary>
        /// Adds an iCal body to the draft with the ID specified in the request.
        /// </summary>
        /// <short>Add a calendar</short>
        /// <param type="System.Int32, System" name="id_message">Message ID</param>
        /// <param type="System.String, System" name="ical_body">iCal body</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailAttachmentData, ASC.Mail">Mail attachment</returns>
        /// <category>Attachments</category>
        /// <path>api/2.0/mail/messages/calendarbody/add</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"messages/calendarbody/add")]
        public MailAttachmentData AddCalendarBody(int id_message, string ical_body)
        {
            if (string.IsNullOrEmpty(ical_body))
                throw new ArgumentException(@"Empty calendar body", "ical_body");

            var calendar = MailUtil.ParseValidCalendar(ical_body, _log);

            if (calendar == null)
                throw new ArgumentException(@"Invalid calendar body", "ical_body");

            using (var ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms))
                {
                    writer.Write(ical_body);
                    writer.Flush();
                    ms.Position = 0;

                    var attachment = MailEngineFactory.AttachmentEngine
                        .AttachFileToDraft(TenantId, Username, id_message, calendar.Method.ToLowerInvariant() + ".ics",
                            ms, ms.Length, "text/calendar");

                    return attachment;
                }
            }
        }

        /// <summary>
        /// Downloads all the attachments from the message with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Download attachments
        /// </short>
        /// <category>Attachments</category>
        /// <param type="System.Int32, System" method="url" name="messageId">Message ID</param>
        /// <returns type="ASC.Mail.Core.Engine.Operations.Base.MailOperationStatus, ASC.Mail">Attachment archive</returns>
        /// <path>api/2.0/mail/messages/attachment/downloadall/{messageId}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"messages/attachment/downloadall/{messageId}")]
        public MailOperationStatus DownloadAllAttachments(int messageId)
        {
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            return MailEngineFactory.OperationEngine.DownloadAllAttachments(messageId, TranslateMailOperationStatus);
        }
    }
}
