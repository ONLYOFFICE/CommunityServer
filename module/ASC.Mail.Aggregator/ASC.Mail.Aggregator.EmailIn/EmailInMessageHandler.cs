/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Net;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.DataStorage;
using ActiveUp.Net.Mail;

namespace ASC.Mail.Aggregator.EmailIn
{
    public class EmailInMessageHandler : MessageHandlerBase
    {
        private readonly MailBoxManager mail_box_manager;
        
        private MailBoxManager MailBoxManager
        {
            get { return mail_box_manager; }
        }

        public EmailInMessageHandler(ILogger log, string connection_string_name)
            : base(log, connection_string_name)
        {
            mail_box_manager = new MailBoxManager(0, log);
        }

        public override void HandleRetrievedMessage(MailBox mailbox, Message message , MailMessageItem message_item, int folder_id, string uidl, string md5_hash, bool unread,
                                                    int[] tags_ids)
        {
            if (string.IsNullOrEmpty(mailbox.EMailInFolder))
                return;

            try
            {

                foreach (var attachment in message_item.Attachments)
                {
                    using (var file = AttachmentManager.GetAttachmentStream(attachment))
                    {
                        log.Debug("EmailInMessageHandler HandleRetrievedMessage file name: {0}, folder id: {1}",
                                  file.FileName, mailbox.EMailInFolder);
                        var uploaded_file_id = FilesUploader.UploadToFiles(file.FileStream, file.FileName,
                                                                           attachment.contentType, mailbox.EMailInFolder,
                                                                           new Guid(mailbox.UserId), log);
                        if (uploaded_file_id < 0)
                        {
                            log.Error("EmailInMessageHandler HandleRetrievedMessage uploaded_file_id < 0");
                        }
                    }
                }

            }
            catch (WebException we)
            {
                var status_code = ((HttpWebResponse) we.Response).StatusCode;

                if (status_code == HttpStatusCode.NotFound || status_code == HttpStatusCode.Forbidden)
                {
                    MailBoxManager.CreateUploadToDocumentsFailureAlert(mailbox.TenantId, mailbox.UserId,
                                                                       mailbox.MailBoxId,
                                                                       (status_code == HttpStatusCode.NotFound)
                                                                           ? MailBoxManager.UploadToDocumentsErrorType
                                                                                           .FolderNotFound
                                                                           : MailBoxManager.UploadToDocumentsErrorType
                                                                                           .AccessDenied);

                    MailBoxManager.SetMailboxEmailInFolder(mailbox.TenantId, mailbox.UserId, mailbox.MailBoxId, null);
                    mailbox.EMailInFolder = null;
                }

                throw;
            }
        }
    }
}
