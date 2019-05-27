/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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