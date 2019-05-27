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