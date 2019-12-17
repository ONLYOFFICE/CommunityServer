/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.IO;
using System.Web;
using ASC.Api.Attributes;
using ASC.Mail.Core.Engine;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Utils;
using ASC.Mail.Core.Engine.Operations.Base;
using System.Threading;

// ReSharper disable InconsistentNaming

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// Export all message's attachments to MyDocuments
        /// </summary>
        /// <param name="id_message">Id of any message</param>
        /// <param name="id_folder" optional="true">Id of Documents folder (if empty then @My)</param>
        /// <returns>Count of exported attachments</returns>
        /// <category>Messages</category>
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
        /// Export attachment to MyDocuments
        /// </summary>
        /// <param name="id_attachment">Id of any attachment from the message</param>
        /// <param name="id_folder" optional="true">Id of Documents folder (if empty then @My)</param>
        /// <returns>Id document in My Documents</returns>
        /// <category>Messages</category>
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
        /// Add attachment to draft
        /// </summary>
        /// <param name="id_message">Id of any message</param>
        /// <param name="name">File name</param>
        /// <param name="file">File stream</param>
        /// <param name="content_type">File content type</param>
        /// <returns>MailAttachment</returns>
        /// <category>Messages</category>
        [Create(@"messages/attachment/add")]
        public MailAttachmentData AddAttachment(int id_message, string name, Stream file, string content_type)
        {
            var attachment = MailEngineFactory.AttachmentEngine
                .AttachFileToDraft(TenantId, Username, id_message, name, file, file.Length, content_type);

            return attachment;
        }

        /// <summary>
        /// Add attachment to draft
        /// </summary>
        /// <param name="id_message">Id of any message</param>
        /// <param name="ical_body">File name</param>
        /// <returns>MailAttachment</returns>
        /// <category>Messages</category>
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
        /// Download all attachments from message
        /// </summary>
        /// <short>
        /// Download all attachments from message
        /// </short>
        /// <param name="messageId">Id of message</param>
        /// <returns>Attachment Archive</returns>
        [Update(@"messages/attachment/downloadall/{messageId}")]
        public MailOperationStatus DownloadAllAttachments(int messageId)
        {
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            return MailEngineFactory.OperationEngine.DownloadAllAttachments(messageId, TranslateMailOperationStatus);
        }
    }
}
