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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Core;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Extensions;
using ASC.Web.CRM.Core.Enums;

namespace ASC.Mail.Core.Dao
{
    public class CrmContactDao : ICrmContactDao
    {
        public IDbManager Db { get; private set; }
        public int Tenant { get; private set; }

        protected string CurrentUserId { get; private set; }

        private ILog Log { get; set; }

        public CrmContactDao(IDbManager dbManager, int tenant, string user)
        {
            Db = dbManager;
            Tenant = tenant;
            CurrentUserId = user;
            Log = LogManager.GetLogger("ASC.Mail.CrmContactDao");
        }

        private const string CC_ALIAS = "cc";
        private const string CCI_ALIAS = "cci";

        public List<int> GetCrmContactIds(string email)
        {
            var ids = new List<int>();

            if (string.IsNullOrEmpty(email))
                return ids;
            try
            {
                var q = new SqlQuery(CrmContactTable.TABLE_NAME.Alias(CC_ALIAS))
                    .Select(CrmContactTable.Columns.Id.Prefix(CC_ALIAS),
                        CrmContactTable.Columns.IsCompany.Prefix(CC_ALIAS),
                        CrmContactTable.Columns.IsShared.Prefix(CC_ALIAS))
                    .InnerJoin(CrmContactInfoTable.TABLE_NAME.Alias(CCI_ALIAS),
                        Exp.EqColumns(CrmContactTable.Columns.Tenant.Prefix(CC_ALIAS),
                            CrmContactInfoTable.Columns.Tenant.Prefix(CCI_ALIAS)) &
                        Exp.EqColumns(CrmContactTable.Columns.Id.Prefix(CC_ALIAS),
                            CrmContactInfoTable.Columns.ContactId.Prefix(CCI_ALIAS)))
                    .Where(CrmContactTable.Columns.Tenant.Prefix(CC_ALIAS), Tenant)
                    .Where(CrmContactInfoTable.Columns.Type.Prefix(CCI_ALIAS), (int) ContactInfoType.Email)
                    .Where(CrmContactInfoTable.Columns.Data.Prefix(CCI_ALIAS), email);

                var contactList = Db.ExecuteList(q)
                    .ConvertAll(
                        r =>
                            new
                            {
                                Id = Convert.ToInt32(r[0]),
                                Company = Convert.ToBoolean(r[1]),
                                ShareType = (ShareType) Convert.ToInt32(r[2])
                            });

                if (!contactList.Any())
                    return ids;

                CoreContext.TenantManager.SetCurrentTenant(Tenant);
                SecurityContext.AuthenticateMe(new Guid(CurrentUserId));

                foreach (var info in contactList)
                {
                    var contact = info.Company
                        ? new Company()
                        : (Contact) new Person();

                    contact.ID = info.Id;
                    contact.ShareType = info.ShareType;

                    if (CRMSecurity.CanAccessTo(contact))
                    {
                        ids.Add(info.Id);
                    }
                }
            }
            catch (Exception e)
            {
                Log.WarnFormat("GetCrmContactsId(tenandId='{0}', userId='{1}', email='{2}') Exception:\r\n{3}\r\n",
                    Tenant, CurrentUserId, email, e.ToString());
            }

            return ids;
        }
    }
}