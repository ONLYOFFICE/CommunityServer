/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;

namespace ASC.Mail.Core.Dao
{
    public class TagAddressDao: BaseDao, ITagAddressDao
    {
        protected static ITable table = new MailTableFactory().Create<TagAddressTable>();

        protected string CurrentUserId { get; private set; }

        public TagAddressDao(IDbManager dbManager, int tenant, string user) 
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public List<int> GetTagIds(string email)
        {
            var query = new SqlQuery(TagAddressTable.TABLE_NAME)
                .Distinct()
                .Select(TagAddressTable.Columns.TagId)
                .Where(TagAddressTable.Columns.Address, email)
                .Where(Exp.In(TagAddressTable.Columns.TagId,
                    new SqlQuery(TagTable.TABLE_NAME)
                        .Select(TagTable.Columns.Id)
                        .Where(TagTable.Columns.Tenant, Tenant)
                        .Where(TagTable.Columns.User, CurrentUserId)
                    )
                );

            var tagIds = Db.ExecuteList(query)
                .ConvertAll(r => Convert.ToInt32(r[0]));

            return tagIds;
        }

        public List<string> GetTagAddresses(int tagId)
        {
            var query = new SqlQuery(TagAddressTable.TABLE_NAME)
                .Select(TagAddressTable.Columns.Address)
                .Where(TagAddressTable.Columns.TagId, tagId)
                .Where(TagAddressTable.Columns.Tenant, Tenant);

            var list = Db.ExecuteList(query)
                .ConvertAll(r => Convert.ToString(r[0]));

            return list;
        }

        public int Save(int tagId, string email)
        {
            var query = new SqlInsert(TagAddressTable.TABLE_NAME, true)
                .InColumnValue(TagAddressTable.Columns.TagId, tagId)
                .InColumnValue(TagAddressTable.Columns.Address, email)
                .InColumnValue(TagAddressTable.Columns.Tenant, Tenant);

            var result = Db.ExecuteNonQuery(query);
            return result;
        }

        public int Delete(int tagId, string email = null)
        {
            var query = new SqlDelete(TagAddressTable.TABLE_NAME)
                .Where(TagAddressTable.Columns.TagId, tagId)
                .Where(TagAddressTable.Columns.Tenant, Tenant);

            if (!string.IsNullOrEmpty(email))
            {
                query.Where(TagAddressTable.Columns.Address, email);
            }

            var result = Db.ExecuteNonQuery(query);
            return result;
        }

        public int DeleteAll()
        {
            var query = new SqlDelete(TagAddressTable.TABLE_NAME)
                .Where(TagAddressTable.Columns.Tenant, Tenant);

            var result = Db.ExecuteNonQuery(query);
            return result;
        }
    }
}