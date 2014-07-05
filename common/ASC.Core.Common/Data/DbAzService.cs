/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Security.Authorizing;
using ASC.Core.Tenants;

namespace ASC.Core.Data
{
    public class DbAzService : DbBaseService, IAzService
    {
        public DbAzService(ConnectionStringSettings connectionString)
            : base(connectionString, "tenant")
        {
        }

        public IEnumerable<AzRecord> GetAces(int tenant, DateTime from)
        {
            // row with tenant = -1 - common for all tenants, but equal row with tenant != -1 escape common row for the portal

            var q = new SqlQuery("core_acl")
                .Select("subject", "action", "object", "acetype")
                .Where(Exp.Eq("tenant", Tenant.DEFAULT_TENANT));

            var commonAces = ExecList(q)
                .ConvertAll(r => ToAzRecord(r, tenant))
                .ToDictionary(a => string.Concat(a.Tenant.ToString(), a.SubjectId.ToString(), a.ActionId.ToString(), a.ObjectId));

            q = new SqlQuery("core_acl")
                .Select("subject", "action", "object", "acetype")
                .Where(Exp.Eq("tenant", tenant));

            var tenantAces = ExecList(q)
                .ConvertAll(r => new AzRecord(new Guid((string)r[0]), new Guid((string)r[1]), (AceType)Convert.ToInt32(r[3]), string.Empty.Equals(r[2]) ? null : (string)r[2]) { Tenant = tenant });

            // remove excaped rows
            foreach (var a in tenantAces.ToList())
            {
                var key = string.Concat(a.Tenant.ToString(), a.SubjectId.ToString(), a.ActionId.ToString(), a.ObjectId);
                if (commonAces.ContainsKey(key))
                {
                    var common = commonAces[key];
                    commonAces.Remove(key);
                    if (common.Reaction == a.Reaction)
                    {
                        tenantAces.Remove(a);
                    }
                }
            }

            return commonAces.Values.Concat(tenantAces);
        }

        public AzRecord SaveAce(int tenant, AzRecord r)
        {
            r.Tenant = tenant;
            ExecAction(db =>
            {
                if (!ExistEscapeRecord(db, r))
                {
                    InsertRecord(db, r);
                }
                else
                {
                    // unescape
                    DeleteRecord(db, r);
                }
            });

            return r;
        }

        public void RemoveAce(int tenant, AzRecord r)
        {
            r.Tenant = tenant;
            ExecAction(db =>
            {
                if (ExistEscapeRecord(db, r))
                {
                    // escape
                    InsertRecord(db, r);
                }
                else
                {
                    DeleteRecord(db, r);
                }
            });
        }


        private bool ExistEscapeRecord(IDbExecuter db, AzRecord r)
        {
            var q = Query("core_acl", Tenant.DEFAULT_TENANT)
                .SelectCount()
                .Where("subject", r.SubjectId.ToString())
                .Where("action", r.ActionId.ToString())
                .Where("object", r.ObjectId ?? string.Empty)
                .Where("acetype", r.Reaction);
            return db.ExecScalar<int>(q) != 0;
        }

        private void DeleteRecord(IDbExecuter db, AzRecord r)
        {
            var q = Delete("core_acl", r.Tenant)
                .Where("subject", r.SubjectId.ToString())
                .Where("action", r.ActionId.ToString())
                .Where("object", r.ObjectId ?? string.Empty)
                .Where("acetype", r.Reaction);
            db.ExecNonQuery(q);
        }

        private void InsertRecord(IDbExecuter db, AzRecord r)
        {
            var q = Insert("core_acl", r.Tenant)
                .InColumnValue("subject", r.SubjectId.ToString())
                .InColumnValue("action", r.ActionId.ToString())
                .InColumnValue("object", r.ObjectId ?? string.Empty)
                .InColumnValue("acetype", r.Reaction);
            db.ExecNonQuery(q);
        }

        private AzRecord ToAzRecord(object[] r, int tenant)
        {
            return new AzRecord(
                new Guid((string)r[0]),
                new Guid((string)r[1]),
                (AceType)Convert.ToInt32(r[3]),
                string.Empty.Equals(r[2]) ? null : (string)r[2]) { Tenant = tenant };
        }
    }
}
