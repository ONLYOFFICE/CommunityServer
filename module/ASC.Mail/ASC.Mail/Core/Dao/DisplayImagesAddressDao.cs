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
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;

namespace ASC.Mail.Core.Dao
{
    public class DisplayImagesAddressDao : BaseDao, IDisplayImagesAddressDao
    {
        protected static ITable table = new MailTableFactory().Create<DisplayImagesTable>();

        protected string CurrentUserId { get; private set; }

        public DisplayImagesAddressDao(IDbManager dbManager, int tenant, string user)
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public List<string> GetDisplayImagesAddresses()
        {
            var query = new SqlQuery(DisplayImagesTable.TABLE_NAME)
                .Select(DisplayImagesTable.Columns.Address)
                .Where(DisplayImagesTable.Columns.User, CurrentUserId)
                .Where(DisplayImagesTable.Columns.Tenant, Tenant);

            var addresses = Db.ExecuteList(query)
                .ConvertAll(fields => fields[0].ToString());

            return addresses;
        }

        public void AddDisplayImagesAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException(@"Invalid address. Address can't be empty.", "address");

            var query = new SqlInsert(DisplayImagesTable.TABLE_NAME, true)
                .InColumnValue(DisplayImagesTable.Columns.Tenant, Tenant)
                .InColumnValue(DisplayImagesTable.Columns.User, CurrentUserId)
                .InColumnValue(DisplayImagesTable.Columns.Address, address);

            Db.ExecuteNonQuery(query);
        }

        public void RemovevDisplayImagesAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException(@"Invalid address. Address can't be empty.", "address");

            var query = new SqlDelete(DisplayImagesTable.TABLE_NAME)
                .Where(DisplayImagesTable.Columns.User, CurrentUserId)
                .Where(DisplayImagesTable.Columns.Tenant, Tenant)
                .Where(DisplayImagesTable.Columns.Address, address);

            Db.ExecuteNonQuery(query);
        }
    }
}