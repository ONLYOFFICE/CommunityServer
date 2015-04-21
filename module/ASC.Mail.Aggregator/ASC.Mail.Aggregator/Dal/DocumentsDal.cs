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
using System.Collections.Generic;
using ASC.Core;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.DataStorage;

namespace ASC.Mail.Aggregator.Dal
{
    public class DocumentsDal
    {
        private const string MY_DOCS_FOLDER_ID = "@my";
        private readonly MailBoxManager _manager;
        private readonly int _tenantId;
        private readonly string _userId;

        private MailBoxManager Manager { get { return _manager; } }
        private int Tenant { get { return _tenantId; } }
        private string User { get { return _userId; } }

        public DocumentsDal(MailBoxManager manager, int tenant, string user)
        {
            _manager = manager;
            _tenantId = tenant;
            _userId = user;

            if (SecurityContext.IsAuthenticated) return;

            CoreContext.TenantManager.SetCurrentTenant(Tenant);
            SecurityContext.AuthenticateMe(new Guid(_userId));
        }

        public List<int> StoreAttachmentsToMyDocuments(int messageId)
        {
            var fileIds = new List<int>();

            var attachments = Manager.GetMessageAttachments(Tenant, User, messageId);

            foreach (var attachment in attachments)
            {
                using (var file = AttachmentManager.GetAttachmentStream(attachment))
                {
                    var uploadedFileId = ApiHelper.UploadToDocuments(file.FileStream, file.FileName,
                                                                     attachment.contentType, MY_DOCS_FOLDER_ID, true);

                    if (uploadedFileId > 0)
                    {
                        fileIds.Add(uploadedFileId);
                    }
                }
            }

            return fileIds;
        }

        public int StoreAttachmentToMyDocuments(int attachmentId)
        {
            var attachment = Manager.GetMessageAttachment(attachmentId, Tenant, User);

            if (attachment != null)
            {
                using (var file = Manager.GetAttachmentStream(attachmentId, Tenant, User))
                {
                    var uploadedFileId = ApiHelper.UploadToDocuments(file.FileStream, file.FileName,
                                                                     attachment.contentType, MY_DOCS_FOLDER_ID, true);

                    return uploadedFileId;
                }
            }

            return -1;
        }

    }
}
