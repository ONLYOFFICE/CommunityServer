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
using ASC.Common.Data.Sql;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Data.Contracts;

namespace ASC.Mail.Core.Dao
{
    public class CrmLinkDao : ICrmLinkDao
    {
        public IDbManager Db { get; private set; }
        public int Tenant { get; private set; }

        protected string CurrentUserId { get; private set; }

        public CrmLinkDao(IDbManager dbManager, int tenant, string user)
        {
            Db = dbManager;
            Tenant = tenant;
            CurrentUserId = user;
        }

        public List<CrmContactData> GetLinkedCrmContactEntities(string chainId, int mailboxId)
        {
            var query = new SqlQuery(ChainXCrmContactEntityTable.TABLE_NAME)
                .Select(ChainXCrmContactEntityTable.Columns.EntityId)
                .Select(ChainXCrmContactEntityTable.Columns.EntityType)
                .Where(ChainXCrmContactEntityTable.Columns.MailboxId, mailboxId)
                .Where(ChainXCrmContactEntityTable.Columns.Tenant, Tenant)
                .Where(ChainXCrmContactEntityTable.Columns.ChainId, chainId);

            return Db.ExecuteList(query)
                .ConvertAll(r => new CrmContactData
                {
                    Id = Convert.ToInt32(r[0]),
                    Type = (CrmContactData.EntityTypes) r[1]
                });
        }

        public int SaveCrmLinks(string chainId, int mailboxId, IEnumerable<CrmContactData> crmContactEntities)
        {
            var query = new SqlInsert(ChainXCrmContactEntityTable.TABLE_NAME)
                .InColumns(ChainXCrmContactEntityTable.Columns.ChainId,
                    ChainXCrmContactEntityTable.Columns.MailboxId,
                    ChainXCrmContactEntityTable.Columns.Tenant,
                    ChainXCrmContactEntityTable.Columns.EntityId,
                    ChainXCrmContactEntityTable.Columns.EntityType);

            foreach (var contactEntity in crmContactEntities)
            {
                query.Values(chainId, mailboxId, Tenant, contactEntity.Id, contactEntity.Type);
            }
            
            return Db.ExecuteNonQuery(query);
        }

        public int UpdateCrmLinkedMailboxId(string chainId, int oldMailboxId, int newMailboxId)
        {
            var updateOldChainIdQuery = new SqlUpdate(ChainXCrmContactEntityTable.TABLE_NAME)
                .Set(ChainXCrmContactEntityTable.Columns.MailboxId, newMailboxId)
                .Where(ChainXCrmContactEntityTable.Columns.ChainId, chainId)
                .Where(ChainXCrmContactEntityTable.Columns.MailboxId, oldMailboxId)
                .Where(ChainXCrmContactEntityTable.Columns.Tenant, Tenant);

            return Db.ExecuteNonQuery(updateOldChainIdQuery);
        }

        public int UpdateCrmLinkedChainId(string chainId, int mailboxId, string newChainId)
        {
            var query = new SqlUpdate(ChainXCrmContactEntityTable.TABLE_NAME)
                .Set(ChainXCrmContactEntityTable.Columns.ChainId, newChainId)
                .Where(ChainXCrmContactEntityTable.Columns.ChainId, chainId)
                .Where(ChainXCrmContactEntityTable.Columns.MailboxId, mailboxId)
                .Where(ChainXCrmContactEntityTable.Columns.Tenant, Tenant);

           return Db.ExecuteNonQuery(query);
        }

        public void RemoveCrmLinks(string chainId, int mailboxId, IEnumerable<CrmContactData> crmContactEntities)
        {
            foreach (var crmContactEntity in crmContactEntities)
            {
                var removeLinkQuery = new SqlDelete(ChainXCrmContactEntityTable.TABLE_NAME)
                    .Where(ChainXCrmContactEntityTable.Columns.ChainId, chainId)
                    .Where(ChainXCrmContactEntityTable.Columns.MailboxId, mailboxId)
                    .Where(ChainXCrmContactEntityTable.Columns.Tenant, Tenant)
                    .Where(ChainXCrmContactEntityTable.Columns.EntityId, crmContactEntity.Id)
                    .Where(ChainXCrmContactEntityTable.Columns.EntityType, crmContactEntity.Type);

                Db.ExecuteNonQuery(removeLinkQuery);
            }
        }

        public int RemoveCrmLinks(int mailboxId)
        {
            var query =
                new SqlDelete(ChainXCrmContactEntityTable.TABLE_NAME)
                    .Where(ChainXCrmContactEntityTable.Columns.Tenant, Tenant)
                    .Where(ChainXCrmContactEntityTable.Columns.MailboxId, mailboxId);

            return Db.ExecuteNonQuery(query);
        }
    }
}