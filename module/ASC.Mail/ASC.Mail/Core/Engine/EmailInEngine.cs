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
using System.IO;
using System.Linq;
using System.Net;
using ASC.Common.Logging;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Storage;
using ASC.Mail.Enums;
using ASC.Mail.Exceptions;
using ASC.Mail.Utils;

namespace ASC.Mail.Core.Engine
{
    public class EmailInEngine
    {
        public ILog Log { get; private set; }

        public EmailInEngine(ILog log = null)
        {
            Log = log ?? LogManager.GetLogger("ASC.Mail.EmailInEngine");
        }

        public void SaveEmailInData(MailBoxData mailbox, MailMessageData message, string httpContextScheme = null)
        {
            if (string.IsNullOrEmpty(mailbox.EMailInFolder))
                return;

            if (Log == null)
                Log = new NullLog();

            try
            {
                foreach (var attachment in message.Attachments.Where(a => !a.isEmbedded))
                {
                    if (attachment.dataStream != null)
                    {
                        Log.DebugFormat("SaveEmailInData->ApiHelper.UploadToDocuments(fileName: '{0}', folderId: {1})",
                                      attachment.fileName, mailbox.EMailInFolder);

                        attachment.dataStream.Seek(0, SeekOrigin.Begin);

                        UploadToDocuments(attachment.dataStream, attachment.fileName, attachment.contentType, mailbox, httpContextScheme, Log);
                    }
                    else
                    {
                        using (var file = attachment.ToAttachmentStream())
                        {
                            Log.DebugFormat("SaveEmailInData->ApiHelper.UploadToDocuments(fileName: '{0}', folderId: {1})",
                                      file.FileName, mailbox.EMailInFolder);

                            UploadToDocuments(file.FileStream, file.FileName, attachment.contentType, mailbox, httpContextScheme, Log);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Log.ErrorFormat("SaveEmailInData(tenant={0}, userId='{1}', messageId={2}) Exception:\r\n{3}\r\n",
                           mailbox.TenantId, mailbox.UserId, message.Id, e.ToString());
            }
        }

        private static void UploadToDocuments(Stream fileStream, string fileName, string contentType, MailBoxData mailbox, string httpContextScheme, ILog log = null)
        {
            if (log == null)
                log = new NullLog();

            try
            {
                var apiHelper = new ApiHelper(httpContextScheme);
                var uploadedFileId = apiHelper.UploadToDocuments(fileStream, fileName, contentType, mailbox.EMailInFolder, true);

                log.InfoFormat(
                    "EmailInEngine->UploadToDocuments(): file '{0}' has been uploaded to document folder '{1}' uploadedFileId = {2}",
                    fileName, mailbox.EMailInFolder, uploadedFileId);
            }
            catch (ApiHelperException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound || ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    log.InfoFormat(
                        "EmailInEngine->UploadToDocuments() EMailIN folder '{0}' is unreachable. Try to unlink EMailIN...",
                        mailbox.EMailInFolder);

                    var engine = new EngineFactory(mailbox.TenantId, mailbox.UserId);

                    engine.AccountEngine.SetAccountEmailInFolder(mailbox.MailBoxId, null);

                    mailbox.EMailInFolder = null;

                    engine.AlertEngine.CreateUploadToDocumentsFailureAlert(mailbox.TenantId, mailbox.UserId,
                        mailbox.MailBoxId,
                        ex.StatusCode == HttpStatusCode.NotFound
                            ? UploadToDocumentsErrorType
                                .FolderNotFound
                            : UploadToDocumentsErrorType
                                .AccessDenied);

                    throw;
                }

                log.ErrorFormat("EmailInEngine->UploadToDocuments(fileName: '{0}', folderId: {1}) Exception:\r\n{2}\r\n",
                                      fileName, mailbox.EMailInFolder, ex.ToString());

            }
        }
    }
}
