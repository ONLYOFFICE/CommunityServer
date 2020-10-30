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
using System.Linq;
using ASC.Core;
using ASC.Mail.Core.Dao.Expressions.Attachment;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Storage;
using ASC.Mail.Utils;

namespace ASC.Mail.Core.Engine
{
    public class DocumentsEngine
    {
        public const string MY_DOCS_FOLDER_ID = "@my";
        private readonly int _tenantId;
        private readonly string _userId;

        private int Tenant
        {
            get { return _tenantId; }
        }

        private string User
        {
            get { return _userId; }
        }

        private string HttpContextScheme { get; set; }

        public DocumentsEngine(int tenant, string user, string httpContextScheme)
        {
            _tenantId = tenant;
            _userId = user;

            HttpContextScheme = httpContextScheme;

            if (SecurityContext.IsAuthenticated) return;

            CoreContext.TenantManager.SetCurrentTenant(Tenant);
            SecurityContext.AuthenticateMe(new Guid(_userId));
        }

        public List<object> StoreAttachmentsToMyDocuments(int messageId)
        {
            return StoreAttachmentsToDocuments(messageId, MY_DOCS_FOLDER_ID);
        }

        public object StoreAttachmentToMyDocuments(int attachmentId)
        {
            return StoreAttachmentToDocuments(attachmentId, MY_DOCS_FOLDER_ID);
        }

        public List<object> StoreAttachmentsToDocuments(int messageId, string folderId)
        {
            var engine = new EngineFactory(Tenant, User);
            var attachments =
                engine.AttachmentEngine.GetAttachments(new ConcreteMessageAttachmentsExp(messageId, Tenant, User));

            return
                attachments.Select(attachment => StoreAttachmentToDocuments(attachment, folderId))
                    .Where(uploadedFileId => uploadedFileId != null)
                    .ToList();
        }

        public object StoreAttachmentToDocuments(int attachmentId, string folderId)
        {
            var engine = new EngineFactory(Tenant, User);
            var attachment = engine.AttachmentEngine.GetAttachment(
                new ConcreteUserAttachmentExp(attachmentId, Tenant, User));

            if (attachment == null)
                return -1;

            return StoreAttachmentToDocuments(attachment, folderId);
        }

        public object StoreAttachmentToDocuments(MailAttachmentData mailAttachmentData, string folderId)
        {
            if (mailAttachmentData == null)
                return -1;

            var apiHelper = new ApiHelper(HttpContextScheme);

            using (var file = mailAttachmentData.ToAttachmentStream())
            {
                var uploadedFileId = apiHelper.UploadToDocuments(file.FileStream, file.FileName,
                    mailAttachmentData.contentType, folderId, true);

                return uploadedFileId;
            }
        }
    }
}
