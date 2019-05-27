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


using System.Collections.Generic;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao
{
    public class ContactInfoDao : BaseDao, IContactInfoDao
    {
        protected static ITable table = new MailTableFactory().Create<ContactInfoTable>();

        protected string CurrentUserId { get; private set; }

        public ContactInfoDao(IDbManager dbManager, int tenant, string user) 
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public int SaveContactInfo(ContactInfo contactInfo)
        {
            var query = new SqlInsert(ContactInfoTable.TABLE_NAME, true)
                .InColumnValue(ContactInfoTable.Columns.Id, contactInfo.Id)
                .InColumnValue(ContactInfoTable.Columns.Tenant, contactInfo.Tenant)
                .InColumnValue(ContactInfoTable.Columns.User, contactInfo.User)
                .InColumnValue(ContactInfoTable.Columns.ContactId, contactInfo.ContactId)
                .InColumnValue(ContactInfoTable.Columns.Data, contactInfo.Data)
                .InColumnValue(ContactInfoTable.Columns.Type, contactInfo.Type)
                .InColumnValue(ContactInfoTable.Columns.IsPrimary, contactInfo.IsPrimary);

            return Db.ExecuteScalar<int>(query);
        }

        public int RemoveContactInfo(int id)
        {
            var query = new SqlDelete(ContactInfoTable.TABLE_NAME)
                .Where(ContactInfoTable.Columns.Id, id)
                .Where(ContactInfoTable.Columns.Tenant, Tenant)
                .Where(ContactInfoTable.Columns.User, CurrentUserId);

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        public int RemoveByContactIds(List<int> contactIds)
        {
            var deleteContact = new SqlDelete(ContactsTable.TABLE_NAME)
                .Where(Exp.In(ContactsTable.Columns.Id, contactIds))
                .Where(ContactInfoTable.Columns.Tenant, Tenant)
                .Where(ContactInfoTable.Columns.User, CurrentUserId);

            return Db.ExecuteNonQuery(deleteContact);
        }
    }
}