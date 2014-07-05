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
using ASC.Api.Attributes;
using ASC.Mail.Aggregator.Dal;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// Export all message's attachments to MyDocuments
        /// </summary>
        /// <param name="id_message">Id of any message</param>
        /// <returns>Count of exported attachments</returns>
        /// <category>Messages</category>
        [Update(@"attachments/mydocuments/export")]
        public int ExportAttachmentsToMyDocuments(int id_message)
        {
            if (id_message < 0)
                throw new ArgumentException("Invalid message id", "id_message");

            var documents_dal = new DocumentsDal(MailBoxManager, TenantId, Username);
            var saved_attachments_list = documents_dal.StoreAttachmentsToMyDocuments(id_message);

            return saved_attachments_list.Count;
        }

        /// <summary>
        /// Export attachment to MyDocuments
        /// </summary>
        /// <param name="id_attachment">Id of any attachment from the message</param>
        /// <returns>Id document in My Documents</returns>
        /// <category>Messages</category>
        [Update(@"attachment/mydocuments/export")]
        public int ExportAttachmentToMyDocuments(int id_attachment)
        {
            if (id_attachment < 0)
                throw new ArgumentException("Invalid attachment id", "id_attachment");

            var documents_dal = new DocumentsDal(MailBoxManager, TenantId, Username);
            var id_document = documents_dal.StoreAttachmentToMyDocuments(id_attachment);
            return id_document;
        }
    }
}
