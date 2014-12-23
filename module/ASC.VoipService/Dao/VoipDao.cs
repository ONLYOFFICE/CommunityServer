/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Caching;
using ASC.Core.Tenants;
using ASC.Thrdparty.Configuration;
using ASC.VoipService.Twilio;

namespace ASC.VoipService.Dao
{
    public class CachedVoipDao : VoipDao
    {
        private readonly AscCache cache = new AscCache();
        private static readonly TimeSpan ExpirationTimeout = TimeSpan.FromDays(1);

        public CachedVoipDao(int tenantID, string storageKey)
            : base(tenantID, storageKey)
        {
        }

        public override VoipPhone SaveOrUpdateNumber(VoipPhone phone)
        {
            ResetCache();
            return base.SaveOrUpdateNumber(phone);
        }

        public override void DeleteNumber(VoipPhone phone)
        {
            ResetCache();
            base.DeleteNumber(phone);
        }

        public override IEnumerable<VoipPhone> GetNumbers(params object[] ids)
        {
            var numbers = cache.Get(TenantID.ToString(CultureInfo.InvariantCulture)) as IEnumerable<VoipPhone>;
            if (numbers == null)
            {
                numbers = base.GetNumbers();
                cache.Insert(TenantID.ToString(CultureInfo.InvariantCulture), numbers, DateTime.UtcNow.Add(ExpirationTimeout));
            }

            return ids.Any() ? numbers.Where(r => ids.Contains(r.Id) || ids.Contains(r.Number)) : numbers;
        }

        private void ResetCache()
        {
            cache.Remove(TenantID.ToString(CultureInfo.InvariantCulture));
        }
    }

    public class VoipDao : AbstractDao
    {
        private const string numberTable = "crm_voip_number";
        private const string callsTable = "crm_voip_calls";
        private const string callsHistoryTable = "crm_voip_calls_history";
        private const string contactsTable = "crm_contact";


        public VoipDao(int tenantID, string storageKey)
            : base(tenantID, storageKey)
        {
        }

        #region Numbers

        public virtual VoipPhone SaveOrUpdateNumber(VoipPhone phone)
        {
            using (var db = GetDb())
            {
                var insert = Insert(numberTable)
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

        public virtual void DeleteNumber(VoipPhone phone)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(Delete(numberTable).Where("id", phone.Id));
            }
        }

        public virtual IEnumerable<VoipPhone> GetNumbers(params object[] ids)
        {
            using (var db = GetDb())
            {
                var query = Query(numberTable)
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
            return GetNumbers(id).FirstOrDefault();
        }

        public virtual VoipPhone GetCurrentNumber()
        {
            return GetNumbers().FirstOrDefault(r => r.Caller != null);
        }

        #endregion

        #region Calls

        public VoipCall SaveOrUpdateCall(VoipCall call)
        {
            using (var db = GetDb())
            {
                var query = Insert(callsTable)
                    .InColumnValue("id", call.Id)
                    .InColumnValue("number_from", call.From)
                    .InColumnValue("number_to", call.To)
                    .InColumnValue("status", call.Status)
                    .InColumnValue("contact_id", call.ContactId);

                if (!call.AnsweredBy.Equals(Guid.Empty))
                {
                    query.InColumnValue("answered_by", call.AnsweredBy);
                }

                if (call.DialDate == DateTime.MinValue)
                {
                    call.DialDate = DateTime.UtcNow;
                }

                query.InColumnValue("dial_date", TenantUtil.DateTimeToUtc(call.DialDate));

                if (call.Price > 0)
                {
                    query.InColumnValue("price", call.Price);
                }

                db.ExecuteNonQuery(query);
            }

            return call;
        }

        public VoipCallHistory SaveOrUpdateCallHistory(VoipCallHistory callHistory)
        {
            using (var db = GetDb())
            {
                var query = Insert(callsHistoryTable)
                    .InColumnValue("id", callHistory.ID)
                    .InColumnValue("parent_call_id", callHistory.ParentID)
                    .InColumnValue("answered_by", callHistory.AnsweredBy)
                    .InColumnValue("queue_date", TenantUtil.DateTimeToUtc(callHistory.QueueDate))
                    .InColumnValue("answer_date", TenantUtil.DateTimeToUtc(callHistory.AnswerDate))
                    .InColumnValue("end_dial_date", TenantUtil.DateTimeToUtc(callHistory.EndDialDate))
                    .InColumnValue("record_url", callHistory.RecordUrl)
                    .InColumnValue("record_duration", callHistory.RecordDuration)
                    .InColumnValue("price", callHistory.Price);

                db.ExecuteNonQuery(query);
            }

            return callHistory;
        }

        public VoipCallHistory UpdateCallHistoryId(VoipCallHistory callHistory)
        {
            using (var db = GetDb())
            {
                var query = Update(callsHistoryTable)
                    .Set("id", callHistory.ID)
                    .Where(Exp.Eq("id", callHistory.ParentID));

                db.ExecuteNonQuery(query);
            }
            return callHistory;
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
                query.SetMaxResults((int)filter.Max);

                var calls = db.ExecuteList(query).ConvertAll(ToCall);
                var history = GetCallHistory(calls.Select(r => r.Id));

                calls = calls.GroupJoin(history, call => call.Id, h => h.ParentID, (call, h) =>
                {
                    call.History.AddRange(h);
                    return call;
                }).ToList();

                return calls;
            }
        }

        public VoipCall GetCall(string id)
        {
            return GetCalls(new VoipCallFilter { Ids = new object[] { id } }).FirstOrDefault();
        }

        public int GetCallsCount(VoipCallFilter filter)
        {
            using (var db = GetDb())
            {
                var query = GetCallsQuery(filter);
                var queryCount = new SqlQuery().SelectCount().From(query, "t1");

                return db.ExecuteScalar<int>(queryCount);
            }
        }

        public IEnumerable<VoipCall> GetMissedCalls(Guid agent)
        {
            using (var db = GetDb())
            {
                var query = GetCallsQuery(new VoipCallFilter { Agent = agent, SortBy = "date", SortOrder = true, Type = "missed" });

                var subQuery = new SqlQuery(callsTable + " tmp")
                    .SelectMax("tmp.dial_date")
                    .Where(Exp.EqColumns("ca.tenant_id", "tmp.tenant_id"))
                    .Where(Exp.EqColumns("ca.number_from", "tmp.number_from") | Exp.EqColumns("ca.number_from", "tmp.number_to"))
                    .Where(!Exp.Eq("tmp.status", 3));

                query.Select(subQuery, "tmp_date");

                query.Having(Exp.Sql("adddate(ca.dial_date, interval 2 second)") | Exp.Eq("tmp_date", null));

                return db.ExecuteList(query).ConvertAll(ToCall);
            }
        }

        private SqlQuery GetCallsQuery(VoipCallFilter filter)
        {
            var query = Query(callsTable + " ca")
                .Select("ca.id", "ca.number_from", "ca.number_to", "ca.answered_by", "ca.dial_date", "ca.price")
                .Select("ca.status", "ca.contact_id")
                .LeftOuterJoin(contactsTable + " co", Exp.EqColumns("ca.contact_id", "co.id"))
                .Select("co.is_company", "co.company_name", "co.first_name", "co.last_name");

            if (filter.Ids != null && filter.Ids.Any())
            {
                query.Where(Exp.In("ca.id", filter.Ids));
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

        public IEnumerable<VoipCallHistory> GetCallHistory(string parentId)
        {
            return GetCallHistory(new[] { parentId });
        }

        public IEnumerable<VoipCallHistory> GetCallHistory(IEnumerable<string> parentId)
        {
            using (var db = GetDb())
            {
                var query = Query(callsHistoryTable)
                    .Select("id", "parent_call_id", "answered_by", "queue_date", "answer_date", "end_dial_date", "record_url", "record_duration", "price")
                    .Where(Exp.In("parent_call_id", parentId.ToList()));

                return db.ExecuteList(query).ConvertAll(ToCallHistory);
            }
        }

        public VoipCallHistory GetCallHistoryById(string parentId, string callId)
        {
            using (var db = GetDb())
            {
                var query = Query(callsHistoryTable)
                    .Select("id", "parent_call_id", "answered_by", "queue_date", "answer_date", "end_dial_date", "record_url", "record_duration", "price")
                    .Where(Exp.Eq("id", callId) & Exp.Eq("parent_call_id", parentId));

                return db.ExecuteList(query).ConvertAll(ToCallHistory).FirstOrDefault();
            }
        }

        #endregion

        #region Converters

        private static VoipPhone ToPhone(object[] r)
        {
            return GetVoipProvider().GetPhone(r);
        }

        private static VoipCall ToCall(object[] r)
        {
            var call = new VoipCall
                {
                    Id = (string)r[0],
                    From = (string)r[1],
                    To = (string)r[2],
                    AnsweredBy = new Guid((string)r[3]),
                    DialDate = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[4])),
                    Price = Convert.ToDecimal(r[5]),
                    Status = (VoipCallStatus)Convert.ToInt32(r[6]),
                    ContactId = Convert.ToInt32(r[7]),
                    ContactIsCompany = Convert.ToBoolean(r[8])
                };

            if (call.ContactId != 0)
            {
                call.ContactTitle = call.ContactIsCompany
                                        ? r[9] == null ? null : Convert.ToString(r[9])
                                        : r[10] == null || r[11] == null ? null :
                                              string.Format("{0} {1}", Convert.ToString(r[10]), Convert.ToString(r[11]));
            }

            return call;
        }

        private static VoipCallHistory ToCallHistory(object[] r)
        {
            return new VoipCallHistory
            {
                ID = (string)r[0],
                ParentID = (string)r[1],
                AnsweredBy = new Guid((string)r[2]),
                QueueDate = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[3])),
                AnswerDate = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[4])),
                EndDialDate = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[5])),
                RecordUrl = (string)r[6],
                RecordDuration = Convert.ToInt32(r[7]),
                Price = Convert.ToDecimal(r[8])
            };
        }

        public static TwilioProvider GetVoipProvider()
        {
            return new TwilioProvider(KeyStorage.Get("twilioAccountSid"), KeyStorage.Get("twilioAuthToken"));
        }

        #endregion
    }
}