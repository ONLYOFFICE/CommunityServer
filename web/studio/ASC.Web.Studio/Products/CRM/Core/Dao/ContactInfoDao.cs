/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


#region Import

using System;
using System.Collections.Generic;
using ASC.Collections;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Common.Data;

#endregion

namespace ASC.CRM.Core.Dao
{
    public class CachedContactInfo : ContactInfoDao
    {
        private readonly HttpRequestDictionary<ContactInfo> _contactInfoCache = new HttpRequestDictionary<ContactInfo>("crm_contact_info");

        public CachedContactInfo(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {

        }

        public override ContactInfo GetByID(int id)
        {
            return _contactInfoCache.Get(id.ToString(), () => GetByIDBase(id));
        }

        public override void Delete(int id)
        {

            ResetCache(id);

            base.Delete(id);
        }

        private ContactInfo GetByIDBase(int id)
        {
            return base.GetByID(id);
        }

        private void ResetCache(int id)
        {
            _contactInfoCache.Reset(id.ToString());
        }

        public override void DeleteByContact(int contactID)
        {
            _contactInfoCache.Clear();

            base.DeleteByContact(contactID);
        }

        public override int Update(ContactInfo contactInfo)
        {
            ResetCache(contactInfo.ID);

            return base.Update(contactInfo);
        }

    }

    public class ContactInfoDao : AbstractDao
    {
        #region Constructor

        public ContactInfoDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {
        }

        #endregion

        public virtual ContactInfo GetByID(int id)
        {
            using (var db = GetDb())
            {
                var sqlResult = db.ExecuteList(GetSqlQuery(Exp.Eq("id", id))).ConvertAll(row => ToContactInfo(row));

                if (sqlResult.Count == 0) return null;

                return sqlResult[0];
            }
        }

        public virtual void Delete(int id)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(Delete("crm_contact_info").Where(Exp.Eq("id", id)));
            }
        }

        public virtual void DeleteByContact(int contactID)
        {
            if (contactID <= 0) return;

            using (var db = GetDb())
            {
                db.ExecuteNonQuery(Delete("crm_contact_info").Where(Exp.Eq("contact_id", contactID)));
            }
        }

        public virtual int Update(ContactInfo contactInfo)
        {
            using (var db = GetDb())
            {
                return Update(contactInfo, db);
            }
        }

        private int Update(ContactInfo contactInfo, DbManager db)
        {
            if (contactInfo == null || contactInfo.ID == 0 || contactInfo.ContactID == 0)
                throw new ArgumentException();

            db.ExecuteNonQuery(Update("crm_contact_info")
                                              .Where("id", contactInfo.ID)
                                              .Set("data", contactInfo.Data)
                                              .Set("category", contactInfo.Category)
                                              .Set("is_primary", contactInfo.IsPrimary)
                                              .Set("contact_id", contactInfo.ContactID)
                                              .Set("type", (int)contactInfo.InfoType)
                                              .Set("last_modifed_on", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))
                                              .Set("last_modifed_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                                               );
            return contactInfo.ID;
        }


        public int Save(ContactInfo contactInfo)
        {
            using (var db = GetDb())
            {
                return Save(contactInfo, db);        
            }
        }

        private int Save(ContactInfo contactInfo, DbManager db)
        {
            return db.ExecuteScalar<int>(Insert("crm_contact_info")
                                                               .InColumnValue("id", 0)
                                                               .InColumnValue("data", contactInfo.Data)
                                                               .InColumnValue("category", contactInfo.Category)
                                                               .InColumnValue("is_primary", contactInfo.IsPrimary)
                                                               .InColumnValue("contact_id", contactInfo.ContactID)
                                                               .InColumnValue("type", (int)contactInfo.InfoType)
                                                               .InColumnValue("last_modifed_on", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))
                                                               .InColumnValue("last_modifed_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                                                               .Identity(1, 0, true));
        }

        public List<String> GetListData(int contactID, ContactInfoType infoType)
        {
            return GetList(contactID, infoType, null, null).ConvertAll(item => item.Data);
        }

        public List<ContactInfo> GetAll()
        {
            return GetList(0, null, null, null);
        }

        public List<ContactInfo> GetAll(int[] contactID)
        {

            if (contactID == null || contactID.Length == 0) return null;

            SqlQuery sqlQuery = GetSqlQuery(null);

            sqlQuery.Where(Exp.In("contact_id", contactID));

            using (var db = GetDb())
            {
                return db.ExecuteList(sqlQuery).ConvertAll(row => ToContactInfo(row));
            }
        }

        public virtual List<ContactInfo> GetList(int contactID, ContactInfoType? infoType, int? categoryID, bool? isPrimary)
        {
            SqlQuery sqlQuery = GetSqlQuery(null);

            if (contactID > 0)
                sqlQuery.Where(Exp.Eq("contact_id", contactID));

            if (infoType.HasValue)
                sqlQuery.Where(Exp.Eq("type", infoType.Value));

            if (categoryID.HasValue)
                sqlQuery.Where(Exp.Eq("category", categoryID.Value));

            if (isPrimary.HasValue)
                sqlQuery.Where(Exp.Eq("is_primary", isPrimary.Value));

            sqlQuery.OrderBy("type", true);
            // sqlQuery.OrderBy("category", true);
            //  sqlQuery.OrderBy("is_primary", true);


            using (var db = GetDb())
            {
                return db.ExecuteList(sqlQuery).ConvertAll(row => ToContactInfo(row));
            }
        }


        public int[] UpdateList(List<ContactInfo> items)
        {

            if (items == null || items.Count == 0) return null;

            var result = new List<int>();

            using (var db = GetDb())
            using (var tx = db.BeginTransaction(true))
            {
                foreach (var contactInfo in items)
                    result.Add(Update(contactInfo, db));


                tx.Commit();
            }

            return result.ToArray();
        }




        public int[] SaveList(List<ContactInfo> items)
        {
            if (items == null || items.Count == 0) return null;

            var result = new List<int>();

            using (var db = GetDb())
            using (var tx = db.BeginTransaction(true))
            {
                foreach (var contactInfo in items)
                    result.Add(Save(contactInfo, db));


                tx.Commit();
            }

            return result.ToArray();
        }

        protected static ContactInfo ToContactInfo(object[] row)
        {
            return new ContactInfo
                       {
                           ID = Convert.ToInt32(row[0]),
                           Category = Convert.ToInt32(row[1]),
                           Data = row[2].ToString(),
                           InfoType = (ContactInfoType)Convert.ToInt32(row[3]),
                           IsPrimary = Convert.ToBoolean(row[4]),
                           ContactID = Convert.ToInt32(row[5])
                       };
        }

        private SqlQuery GetSqlQuery(Exp where)
        {
            var sqlQuery = Query("crm_contact_info")
                .Select("id",
                        "category",
                        "data",
                        "type",
                        "is_primary",
                        "contact_id");

            if (where != null)
                sqlQuery.Where(where);

            return sqlQuery;

        }
    }
}