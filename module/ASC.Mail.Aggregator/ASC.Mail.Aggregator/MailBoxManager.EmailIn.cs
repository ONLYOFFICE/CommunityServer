/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Net;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.DataStorage;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        public void SaveEmailInData(MailBox mailbox, MailMessage message, ILogger log)
        {
            if (string.IsNullOrEmpty(mailbox.EMailInFolder))
                return;

            try
            {
                foreach (var attachment in message.Attachments)
                {
                    using (var file = AttachmentManager.GetAttachmentStream(attachment))
                    {
                        log.Debug("SaveEmailInData->ApiHelper.UploadToDocuments(fileName: '{0}', folderId: {1})",
                                  file.FileName, mailbox.EMailInFolder);

                        UploadToDocuments(file, attachment.contentType, mailbox, log);
                    }

                }

            }
            catch (Exception e)
            {
                _log.Error("SaveEmailInData(tenant={0}, userId='{1}', messageId={2}) Exception:\r\n{3}\r\n",
                           mailbox.TenantId, mailbox.UserId, message.Id, e.ToString());
            }
        }


        private void UploadToDocuments(AttachmentStream file, string contentType, MailBox mailbox, ILogger log)
        {
            try
            {
                var uploadedFileId = ApiHelper.UploadToDocuments(file.FileStream, file.FileName, contentType, mailbox.EMailInFolder, true);

                log.Debug("ApiHelper.UploadToDocuments() -> uploadedFileId = {0}", uploadedFileId);
            }
            catch (ApiHelperException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound || ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    log.Info("ApiHelper.UploadToDocuments() EMailIN folder '{0}' is unreachable. Try to unlink EMailIN...", mailbox.EMailInFolder);

                    SetMailboxEmailInFolder(mailbox.TenantId, mailbox.UserId, mailbox.MailBoxId, null);

                    mailbox.EMailInFolder = null;

                    CreateUploadToDocumentsFailureAlert(mailbox.TenantId, mailbox.UserId,
                                                        mailbox.MailBoxId,
                                                        (ex.StatusCode == HttpStatusCode.NotFound)
                                                            ? UploadToDocumentsErrorType
                                                                  .FolderNotFound
                                                            : UploadToDocumentsErrorType
                                                                  .AccessDenied);

                    throw;
                }

                log.Error("SaveEmailInData->ApiHelper.UploadToDocuments(fileName: '{0}', folderId: {1}) Exception:\r\n{2}\r\n",
                                      file.FileName, mailbox.EMailInFolder, ex.ToString());

            }
        }
    }
}
