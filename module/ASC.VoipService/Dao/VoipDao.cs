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


using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.VoipService.Twilio;
using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core.Common.Configuration;

namespace ASC.VoipService.Dao
{
    public class VoipDao : AbstractDao
    {
        public VoipDao(int tenantID)
            : base(tenantID)
        {
        }

        public virtual VoipPhone SaveOrUpdateNumber(VoipPhone phone)
        {
            if (!string.IsNullOrEmpty(phone.Number))
            {
                phone.Number = phone.Number.TrimStart('+');
            }

            using (var db = GetDb())
            {
                var insert = Insert("crm_voip_number")
                    .InColumnValue("id", phone.Id)
                    .InColumnValue("number", phone.Number)
                    .InColumnValue("alias", phone.Alias);

                if (phone.Settings != null)
                {
                    insert.InColumnValue("settings", phone.Settings.ToString());
                }

                db.ExecuteNonQuery(insert);
            }
            return phone;
        }

        public virtual void DeleteNumber(string phoneId = "")
        {
            using (var db = GetDb())
            {
                var query = Delete("crm_voip_number");
                if (!string.IsNullOrEmpty(phoneId))
                {
                    query.Where("id", phoneId);
                }
                db.ExecuteNonQuery(query);
            }
        }

        public virtual IEnumerable<VoipPhone> GetNumbers(params object[] ids)
        {
            using (var db = GetDb())
            {
                var query = Query("crm_voip_number")
                    .Select("id", "number", "alias", "settings");

                if (ids.Any())
                {
                    query.Where(Exp.In("number", ids) | Exp.In("id", ids));
                }

                return db.ExecuteList(query).ConvertAll(ToPhone);
            }
        }

        public VoipPhone GetNumber(string id)
        {
            return GetNumbers(id.TrimStart('+')).FirstOrDefault();
        }

        public virtual VoipPhone GetCurrentNumber()
        {
            return GetNumbers().FirstOrDefault(r => r.Caller != null);
        }


        public VoipCall SaveOrUpdateCall(VoipCall call)
        {
            using (var db = GetDb())
            {
                var query = Insert("crm_voip_calls")
                    .InColumnValue("id", call.Id)
                    .InColumnValue("number_from", call.From)
                    .InColumnValue("number_to", call.To)
                    .InColumnValue("contact_id", call.ContactId);

                if (!string.IsNullOrEmpty(call.ParentID))
                {
                    query.InColumnValue("parent_call_id", call.ParentID);
                }

                if (call.Status.HasValue)
                {
                    query.InColumnValue("status", call.Status.Value);
                }

                if (!call.AnsweredBy.Equals(Guid.Empty))
                {
                    query.InColumnValue("answered_by", call.AnsweredBy);
                }

                if (call.DialDate == DateTime.MinValue)
                {
                    call.DialDate = DateTime.UtcNow;
                }

                query.InColumnValue("dial_date", TenantUtil.DateTimeToUtc(call.DialDate));

                if (call.DialDuration > 0)
                {
                    query.InColumnValue("dial_duration", call.DialDuration);
                }
                if (call.Price > Decimal.Zero)
                {
                    query.InColumnValue("price", call.Price);
                }

                if (call.VoipRecord != null)
                {
                    if (!string.IsNullOrEmpty(call.VoipRecord.Id))
                    {
                        query.InColumnValue("record_sid", call.VoipRecord.Id);
                    }
                    if (!string.IsNullOrEmpty(call.VoipRecord.Uri))
                    {
                        query.InColumnValue("record_url", call.VoipRecord.Uri);
                    }

                    if (call.VoipRecord.Duration != 0)
                    {
                        query.InColumnValue("record_duration", call.VoipRecord.Duration);
                    }

                    if (call.VoipRecord.Price != default(decimal))
                    {
                        query.InColumnValue("record_price", call.VoipRecord.Price);
                    }
                }

                db.ExecuteNonQuery(query);
            }

            return call;
        }

        public IEnumerable<VoipCall> GetCalls(VoipCallFilter filter)
        {
            using (var db = GetDb())
            {
                var query = GetCallsQuery(filter);

                if (filter.SortByColumn != null)
                {
                    query.OrderBy(filter.SortByColumn, filter.SortOrder);
                }

                query.SetFirstResult((int)filter.Offset);
                query.SetMaxResults((int)filter.Max * 3);

                var calls = db.ExecuteList(query).ConvertAll(ToCall);

                calls = calls.GroupJoin(calls, call => call.Id, h => h.ParentID, (call, h) =>
                {
                    call.ChildCalls.AddRange(h);
                    return call;
                }).Where(r=> string.IsNullOrEmpty(r.ParentID)).ToList();

                return calls;
            }
        }

        public VoipCall GetCall(string id)
        {
            return GetCalls(new VoipCallFilter { Id = id}).FirstOrDefault();
        }

        public int GetCallsCount(VoipCallFilter filter)
        {
            using (var db = GetDb())
            {
                var query = GetCallsQuery(filter).Where("ca.parent_call_id", "");
                var queryCount = new SqlQuery().SelectCount().From(query, "t1");

                return db.ExecuteScalar<int>(queryCount);
            }
        }

        public IEnumerable<VoipCall> GetMissedCalls(Guid agent, long count = 0, DateTime? from = null)
        {
            using (var db = GetDb())
            {
                var query = GetCallsQuery(new VoipCallFilter { Agent = agent, SortBy = "date", SortOrder = true, Type = "missed" });

                var subQuery = new SqlQuery("crm_voip_calls tmp")
                    .SelectMax("tmp.dial_date")
                    .Where(Exp.EqColumns("ca.tenant_id", "tmp.tenant_id"))
                    .Where(Exp.EqColumns("ca.number_from", "tmp.number_from") | Exp.EqColumns("ca.number_from", "tmp.number_to"))
                    .Where(Exp.Lt("tmp.status", VoipCallStatus.Missed));

                if (from.HasValue)
                {
                    query.Where(Exp.Ge("ca.dial_date", TenantUtil.DateTimeFromUtc(from.Value)));
                }

                if (count != 0)
                {
                    query.SetMaxResults((int)count);
                }

                query.Select(subQuery, "tmp_date");

                query.Having(Exp.Sql("ca.dial_date >= tmp_date") | Exp.Eq("tmp_date", null));

                return db.ExecuteList(query).ConvertAll(ToCall);
            }
        }

        private SqlQuery GetCallsQuery(VoipCallFilter filter)
        {
            var query = Query("crm_voip_calls ca")
                .Select("ca.id", "ca.parent_call_id", "ca.number_from", "ca.number_to", "ca.answered_by", "ca.dial_date", "ca.dial_duration", "ca.price", "ca.status")
                .Select("ca.record_sid", "ca.record_url", "ca.record_duration", "ca.record_price")
                .Select("ca.contact_id", "co.is_company", "co.company_name", "co.first_name", "co.last_name")
                .LeftOuterJoin("crm_contact co", Exp.EqColumns("ca.contact_id", "co.id"))
                .GroupBy("ca.id");

            if (!string.IsNullOrEmpty(filter.Id))
            {
                query.Where(Exp.Eq("ca.id", filter.Id) | Exp.Eq("ca.parent_call_id", filter.Id));
            }

            if (filter.ContactID.HasValue)
            {
                query.Where(Exp.Eq("ca.contact_id", filter.ContactID.Value));
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                query.Where(Exp.Like("ca.id", filter.SearchText, SqlLike.StartWith));
            }

            if (filter.TypeStatus.HasValue)
            {
                query.Where("ca.status", filter.TypeStatus.Value);
            }

            if (filter.FromDate.HasValue)
            {
                query.Where(Exp.Ge("ca.dial_date", filter.FromDate.Value));
            }

            if (filter.ToDate.HasValue)
            {
                query.Where(Exp.Le("ca.dial_date", filter.ToDate.Value));
            }

            if (filter.Agent.HasValue)
            {
                query.Where("ca.answered_by", filter.Agent.Value);
            }

            return query;
        }

        #region Converters

        private static VoipPhone ToPhone(object[] r)
        {
            return GetProvider().GetPhone(r);
        }

        private static VoipCall ToCall(object[] r)
        {
            var call = new VoipCall
                {
                    Id = (string)r[0],
                    ParentID = (string)r[1],
                    From = (string)r[2],
                    To = (string)r[3],
                    AnsweredBy = new Guid((string)r[4]),
                    DialDate = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[5])),
                    DialDuration = Convert.ToInt32(r[6] ?? "0"),
                    Price = Convert.ToDecimal(r[7]),
                    Status = (VoipCallStatus)Convert.ToInt32(r[8]),
                    VoipRecord = new VoipRecord
                    {
                        Id = (string)r[9],
                        Uri = (string)r[10],
                        Duration = Convert.ToInt32(r[11] ?? "0"),
                        Price = Convert.ToDecimal(r[12])
                    },
                    ContactId = Convert.ToInt32(r[13]),
                    ContactIsCompany = Convert.ToBoolean(r[14])
                };

            if (call.ContactId != 0)
            {
                call.ContactTitle = call.ContactIsCompany
                                        ? r[15] == null ? null : Convert.ToString(r[15])
                                        : r[16] == null || r[17] == null ? null :
                                              string.Format("{0} {1}", Convert.ToString(r[16]), Convert.ToString(r[17]));
            }

            return call;
        }

        public static Consumer Consumer
        {
            get { return ConsumerFactory.GetByName("twilio"); }
        }

        public static TwilioProvider GetProvider()
        {
            return new TwilioProvider(Consumer["twilioAccountSid"], Consumer["twilioAuthToken"]);
        }

        public static bool ConfigSettingsExist
        {
            get
            {
                return !string.IsNullOrEmpty(Consumer["twilioAccountSid"]) &&
                       !string.IsNullOrEmpty(Consumer["twilioAuthToken"]);
            }
        }

        #endregion
    }
}