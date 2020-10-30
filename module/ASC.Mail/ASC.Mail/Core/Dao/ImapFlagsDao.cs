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

namespace ASC.Mail.Core.Dao
{
    public class ImapFlagsDao : BaseDao, IImapFlagsDao
    {
        protected static ITable table = new MailTableFactory().Create<ImapFlagsTable>();

        public ImapFlagsDao(IDbManager dbManager)
            : base(table, dbManager)
        {
        }

        public List<ImapFlag> GetImapFlags()
        {
            var query = Query();

            return Db.ExecuteList(query)
                .ConvertAll(ToImapFlag);
        }

        protected ImapFlag ToImapFlag(object[] r)
        {
            var imapFlag = new ImapFlag
            {
                FolderId = Convert.ToInt32(r[0]),
                Name = Convert.ToString(r[1]),
                Skip = Convert.ToBoolean(r[2]),
            };

            return imapFlag;
        }
    }
}