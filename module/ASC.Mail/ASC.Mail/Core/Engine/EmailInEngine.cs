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
