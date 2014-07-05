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
using System.Collections.Generic;
using ASC.Core;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.DataStorage;

namespace ASC.Mail.Aggregator.Dal
{
    public class DocumentsDal
    {
        private const string MyDocsFolderId = "@my";
        private readonly MailBoxManager _manager;
        private readonly int _tenantId;
        private readonly Guid _userId;
        private readonly string _userIdString;

        private MailBoxManager Manager { get { return _manager; } }
        private int TenantId { get { return _tenantId; } }
        private Guid UserId { get { return _userId; } }
        private string UserIdString { get { return _userIdString; } }

        public DocumentsDal(MailBoxManager manager, int tenant_id, string user_id)
        {
            _manager = manager;
            _tenantId = tenant_id;
            _userIdString = user_id;
            _userId = new Guid(user_id);
        }

        public List<int> StoreAttachmentsToMyDocuments(int message_id)
        {
            CoreContext.TenantManager.SetCurrentTenant(TenantId);

            var file_ids = new List<int>();

            var attachments = Manager.GetMessageAttachments(TenantId, UserIdString, message_id);

            foreach (var attachment in attachments)
            {
                using (var file = AttachmentManager.GetAttachmentStream(attachment))
                {
                    var uploaded_file_id = FilesUploader.UploadToFiles(file.FileStream, file.FileName, attachment.contentType, MyDocsFolderId, UserId);
                    if (uploaded_file_id > 0)
                    {
                        file_ids.Add(uploaded_file_id);
                    }
                }
            }

            return file_ids;
        }

        public int StoreAttachmentToMyDocuments(int attachment_id)
        {
            CoreContext.TenantManager.SetCurrentTenant(TenantId);

            var attachment = Manager.GetMessageAttachment(attachment_id, TenantId, UserIdString);

            if (attachment != null)
            {
                using (var file = Manager.GetAttachmentStream(attachment_id, TenantId, UserIdString))
                {
                    var uploaded_file_id = FilesUploader.UploadToFiles(file.FileStream, file.FileName, attachment.contentType, MyDocsFolderId, UserId);
                    return uploaded_file_id;
                }
            }

            return -1;
        }
    }
}
