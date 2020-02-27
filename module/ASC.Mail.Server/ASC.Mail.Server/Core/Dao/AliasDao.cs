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


using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Mail.Server.Core.Dao.Interfaces;
using ASC.Mail.Server.Core.DbSchema;
using ASC.Mail.Server.Core.DbSchema.Interfaces;
using ASC.Mail.Server.Core.DbSchema.Tables;
using ASC.Mail.Server.Core.Entities;

namespace ASC.Mail.Server.Core.Dao
{
    public class AliasDao : BaseDao, IAliasDao
    {
        protected static ITable table = new MailServerTableFactory().Create<AliasTable>();

        public AliasDao(IDbManager dbManager) 
            : base(table, dbManager)
        {
        }

        public int Save(Alias alias)
        {
            var query = new SqlInsert(AliasTable.TABLE_NAME, true)
                .InColumnValue(AliasTable.Columns.ADDRESS, alias.Address)
                .InColumnValue(AliasTable.Columns.GOTO, alias.GoTo)
                .InColumnValue(AliasTable.Columns.DOMAIN, alias.Domain)
                .InColumnValue(AliasTable.Columns.CREATED, alias.Created)
                .InColumnValue(AliasTable.Columns.MODIFIED, alias.Modified)
                .InColumnValue(AliasTable.Columns.ACTIVE, alias.IsActive)
                .InColumnValue(AliasTable.Columns.IS_GROUP, alias.IsGroup);

            var result = Db.ExecuteNonQuery(query);
            return result;
        }

        public int Remove(string address)
        {
            var query = new SqlDelete(AliasTable.TABLE_NAME)
                .Where(AliasTable.Columns.ADDRESS, address);

            var result = Db.ExecuteNonQuery(query);
            return result;
        }

        public int RemoveByDomain(string domain)
        {
            var query = new SqlDelete(AliasTable.TABLE_NAME)
                .Where(AliasTable.Columns.DOMAIN, domain);

            var result = Db.ExecuteNonQuery(query);
            return result;
        }
    }
}