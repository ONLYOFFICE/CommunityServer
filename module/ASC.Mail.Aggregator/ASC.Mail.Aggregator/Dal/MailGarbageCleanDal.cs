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
using System.Data;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Users;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Dal.DbSchema;

namespace ASC.Mail.Aggregator.Dal
{
    public class MailGarbageCleanDal
    {
        private readonly MailBoxManager _manager;

        public MailGarbageCleanDal(MailBoxManager manager)
        {
            _manager = manager;
        }


        public void ClearMailboxData(MailBox mailbox)
        {
            if(!mailbox.IsRemoved)
                throw new Exception("Mailbox is not removed.");
            
            var deleteMailboxQuery = new SqlDelete(MailboxTable.name)
                                            .Where(MailboxTable.Columns.id, mailbox.MailBoxId)
                                            .Where(MailboxTable.Columns.id_tenant, mailbox.TenantId)
                                            .Where(MailTable.Columns.id_user, mailbox.UserId);

            var deleteMailboxMessagesQuery = new SqlDelete(MailTable.name)
                                            .Where(MailTable.Columns.id_mailbox, mailbox.MailBoxId)
                                            .Where(MailTable.Columns.id_tenant, mailbox.TenantId)
                                            .Where(MailTable.Columns.id_user, mailbox.UserId);

            var deleteMailboxAttachmentsQuery = new SqlDelete(AttachmentTable.name)
                                            .Where(AttachmentTable.Columns.id_mailbox, mailbox.MailBoxId)
                                            .Where(AttachmentTable.Columns.id_tenant, mailbox.TenantId);

            var deleteServerSetupForUserOnlyQuery = new SqlDelete(MailboxServerTable.name)
                .Where(Exp.In(MailboxServerTable.Columns.id, new[] {mailbox.SmtpServerId, mailbox.InServerId}))
                .Where(MailboxServerTable.Columns.is_user_data, 1);

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    db.ExecuteNonQuery(deleteMailboxAttachmentsQuery);

                    db.ExecuteNonQuery(deleteMailboxMessagesQuery);

                    db.ExecuteNonQuery(deleteServerSetupForUserOnlyQuery);

                    db.ExecuteNonQuery(deleteMailboxQuery);

                    tx.Commit();
                }
            }

        }

        public void ClearUserMailData(int tenant, UserInfo user)
        {
            var mailboxes = _manager.GetMailBoxes(tenant, user.ID.ToString(), false);

            foreach (var mailbox in mailboxes)
            {
                if (!mailbox.IsRemoved)
                {
                    if (!mailbox.IsTeamlab)
                    {
                        _manager.RemoveMailBox(mailbox);
                    }
                    else
                    {
                        CoreContext.TenantManager.SetCurrentTenant(mailbox.TenantId);
                        SecurityContext.AuthenticateMe(user.ID);

                        ApiHelper.RemoveTeamlabMailbox(mailbox.MailBoxId);
                    }

                    mailbox.IsRemoved = true;
                }

                ClearMailboxData(mailbox);
            }
        }

        public void ClearTenantMailData(int tenant)
        {
            CoreContext.TenantManager.SetCurrentTenant(tenant);
            var userInfoList = CoreContext.UserManager.GetUsers();

            foreach (var userInfo in userInfoList)
            {
                ClearUserMailData(tenant, userInfo);
            }

        }

        private DbManager GetDb()
        {
            return new DbManager(MailBoxManager.CONNECTION_STRING_NAME);
        }

    }

    
}
