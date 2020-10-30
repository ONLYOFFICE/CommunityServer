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


using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Mail.Server.Core.Dao.Interfaces;
using ASC.Mail.Server.Core.DbSchema;
using ASC.Mail.Server.Core.DbSchema.Interfaces;
using ASC.Mail.Server.Core.DbSchema.Tables;
using ASC.Mail.Server.Core.Entities;
using ASC.Mail.Server.Utils;

namespace ASC.Mail.Server.Core.Dao
{
    public class MailboxDao : BaseDao, IMailboxDao
    {
        protected static ITable table = new MailServerTableFactory().Create<MailboxTable>();

        public MailboxDao(IDbManager dbManager) 
            : base(table, dbManager)
        {
        }

        public int Save(Mailbox mailbox, bool deliver = true)
        {
            var query = new SqlInsert(MailboxTable.TABLE_NAME, true)
                .InColumnValue(MailboxTable.Columns.USERNAME, mailbox.Login)
                .InColumnValue(MailboxTable.Columns.NAME, mailbox.Name)
                .InColumnValue(MailboxTable.Columns.PASSWORD, PostfixPasswordEncryptor.EncryptString(HashType.Md5, mailbox.Password))
                .InColumnValue(MailboxTable.Columns.MAILDIR, mailbox.Maldir)
                .InColumnValue(MailboxTable.Columns.LOCAL_PART, mailbox.LocalPart)
                .InColumnValue(MailboxTable.Columns.DOMAIN, mailbox.Domain)
                .InColumnValue(MailboxTable.Columns.CREATED, mailbox.Created)
                .InColumnValue(MailboxTable.Columns.MODIFIED, mailbox.Modified)
                .InColumnValue(MailboxTable.Columns.ENABLE_IMAP, deliver)
                .InColumnValue(MailboxTable.Columns.ENABLE_IMAP_SECURED, deliver)
                .InColumnValue(MailboxTable.Columns.ENABLE_POP, deliver)
                .InColumnValue(MailboxTable.Columns.ENABLE_POP_SECURED, deliver)
                .InColumnValue(MailboxTable.Columns.ENABLE_DELIVER, deliver)
                .InColumnValue(MailboxTable.Columns.ENABLE_LDA, deliver);

            var result = Db.ExecuteNonQuery(query);
            return result;
        }

        public int ChangePassword(string username, string newPassword)
        {
            var query = new SqlUpdate(MailboxTable.TABLE_NAME)
                .Set(MailboxTable.Columns.PASSWORD, PostfixPasswordEncryptor.EncryptString(HashType.Md5, newPassword))
                .Where(MailboxTable.Columns.USERNAME, username);

            var result = Db.ExecuteNonQuery(query);
            return result;
        }

        public int Remove(string address)
        {
            var query = new SqlDelete(MailboxTable.TABLE_NAME)
                .Where(MailboxTable.Columns.USERNAME, address);

            var result = Db.ExecuteNonQuery(query);
            return result;
        }

        public int RemoveByDomain(string domain)
        {
            var query = new SqlDelete(MailboxTable.TABLE_NAME)
                .Where(MailboxTable.Columns.DOMAIN, domain);

            var result = Db.ExecuteNonQuery(query);
            return result;
        }
    }
}