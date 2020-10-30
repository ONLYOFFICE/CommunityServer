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
using ASC.Common.Data;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Enums;

namespace ASC.Mail.Core.Dao
{
    public class ImapSpecialMailboxDao : BaseDao, IImapSpecialMailboxDao
    {
        protected static ITable table = new MailTableFactory().Create<ImapSpecialMailboxTable>();

        public ImapSpecialMailboxDao(IDbManager dbManager)
            : base(table, dbManager)
        {
        }

        public List<ImapSpecialMailbox> GetImapSpecialMailboxes()
        {
            var query = Query();

            return Db.ExecuteList(query)
                .ConvertAll(ToImapSpecialMailbox);
        }

        protected ImapSpecialMailbox ToImapSpecialMailbox(object[] r)
        {
            var obj = new ImapSpecialMailbox
            {
                Server = Convert.ToString(r[0]),
                MailboxName = Convert.ToString(r[1]),
                FolderId = (FolderType) Convert.ToInt32(r[2]),
                Skip = Convert.ToBoolean(r[3])
            };

            return obj;
        }
    }
}