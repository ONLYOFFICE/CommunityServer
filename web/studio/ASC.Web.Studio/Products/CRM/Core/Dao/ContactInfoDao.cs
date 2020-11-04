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


#region Import

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Collections;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Common.Data;
using ASC.CRM.Core.Entities;
using ASC.ElasticSearch;
using ASC.Web.CRM.Core.Search;

#endregion

namespace ASC.CRM.Core.Dao
{
    public class CachedContactInfo : ContactInfoDao
    {
        private readonly HttpRequestDictionary<ContactInfo> _contactInfoCache = new HttpRequestDictionary<ContactInfo>("crm_contact_info");

        public CachedContactInfo(int tenantID)
            : base(tenantID)
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

        public ContactInfoDao(int tenantID)
            : base(tenantID)
        {
        }

        #endregion

        public virtual ContactInfo GetByID(int id)
        {
            var sqlResult = Db.ExecuteList(GetSqlQuery(Exp.Eq("id", id))).ConvertAll(row => ToContactInfo(row));

            if (sqlResult.Count == 0) return null;

            return sqlResult[0];
        }

        public virtual void Delete(int id)
        {
            Db.ExecuteNonQuery(Delete("crm_contact_info").Where(Exp.Eq("id", id)));
            FactoryIndexer<InfoWrapper>.DeleteAsync(r => r.Where(a => a.Id, id));
        }

        public virtual void DeleteByContact(int contactID)
        {
            if (contactID <= 0) return;
            Db.ExecuteNonQuery(Delete("crm_contact_info").Where(Exp.Eq("contact_id", contactID)));
            FactoryIndexer<InfoWrapper>.DeleteAsync(r => r.Where(a => a.ContactId, contactID));

            var infos = GetList(contactID, ContactInfoType.Email, null, null);
            FactoryIndexer<EmailWrapper>.Update(new EmailWrapper { Id = contactID, EmailInfoWrapper = infos.Select(r => (EmailInfoWrapper)r).ToList() }, UpdateAction.Replace, r => r.EmailInfoWrapper);
        }

        public virtual int Update(ContactInfo contactInfo)
        {
            var result = UpdateInDb(contactInfo);
            
            if (contactInfo.InfoType == ContactInfoType.Email)
            {
                var infos = GetList(contactInfo.ContactID, ContactInfoType.Email, null, null);

                FactoryIndexer<EmailWrapper>.Update(new EmailWrapper { Id = contactInfo.ContactID, EmailInfoWrapper = infos.Select(r => (EmailInfoWrapper)r).ToList() }, UpdateAction.Replace, r => r.EmailInfoWrapper);
            }

            FactoryIndexer<InfoWrapper>.UpdateAsync(contactInfo);

            return result;
        }

        private int UpdateInDb(ContactInfo contactInfo)
        {
            if (contactInfo == null || contactInfo.ID == 0 || contactInfo.ContactID == 0)
                throw new ArgumentException();

            Db.ExecuteNonQuery(Update("crm_contact_info")
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
            var id = SaveInDb(contactInfo);

            contactInfo.ID = id;

            FactoryIndexer<InfoWrapper>.IndexAsync(contactInfo);

            if (contactInfo.InfoType == ContactInfoType.Email)
            {
                FactoryIndexer<EmailWrapper>.Index(new EmailWrapper
                {
                    Id = contactInfo.ContactID, 
                    TenantId = TenantID, 
                    EmailInfoWrapper = new List<EmailInfoWrapper>
                    {
                        contactInfo
                    }
                });
            }

            return id;
        }

        private int SaveInDb(ContactInfo contactInfo)
        {
            return Db.ExecuteScalar<int>(Insert("crm_contact_info")
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

            return Db.ExecuteList(sqlQuery).ConvertAll(row => ToContactInfo(row));
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


            return Db.ExecuteList(sqlQuery).ConvertAll(row => ToContactInfo(row));
        }


        public int[] UpdateList(List<ContactInfo> items, Contact contact = null)
        {

            if (items == null || items.Count == 0) return null;

            var result = new List<int>();

            using (var tx = Db.BeginTransaction(true))
            {
                foreach (var contactInfo in items)
                    result.Add(UpdateInDb(contactInfo));


                tx.Commit();
            }

            if (contact != null)
            {
                FactoryIndexer<EmailWrapper>.IndexAsync(EmailWrapper.ToEmailWrapper(contact, items.Where(r => r.InfoType == ContactInfoType.Email).ToList()));
                foreach (var item in items.Where(r => r.InfoType != ContactInfoType.Email))
                {
                    FactoryIndexer<InfoWrapper>.IndexAsync(item);
                }
            }

            return result.ToArray();
        }




        public int[] SaveList(List<ContactInfo> items, Contact contact = null)
        {
            if (items == null || items.Count == 0) return null;

            var result = new List<int>();

            using (var tx = Db.BeginTransaction(true))
            {
                foreach (var contactInfo in items)
                {
                    var contactInfoId = SaveInDb(contactInfo);
                    contactInfo.ID = contactInfoId;
                    result.Add(contactInfoId);
                }


                tx.Commit();
            }

            if (contact != null)
            {
                FactoryIndexer<EmailWrapper>.IndexAsync(EmailWrapper.ToEmailWrapper(contact, items.Where(r => r.InfoType == ContactInfoType.Email).ToList()));
                foreach (var item in items.Where(r => r.InfoType != ContactInfoType.Email))
                {
                    FactoryIndexer<InfoWrapper>.IndexAsync(item);
                }
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