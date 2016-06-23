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


using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Thrdparty.Configuration;
using ASC.VoipService.Twilio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASC.VoipService.Dao
{
    public class VoipDao : AbstractDao
    {
        public VoipDao(int tenantID, string storageKey)
            : base(tenantID, storageKey)
        {
        }


        public virtual VoipPhone SaveOrUpdateNumber(VoipPhone phone)
        {
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

        public virtual void DeleteNumber(VoipPhone phone)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(Delete("crm_voip_number").Where("id", phone.Id));
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
            return GetNumbers(id).FirstOrDefault();
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
                var query = Insert("crm_voip_calls_history")
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
                var query = Update("crm_voip_calls_history")
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

                var subQuery = new SqlQuery("crm_voip_calls tmp")
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
            var query = Query("crm_voip_calls ca")
                .Select("ca.id", "ca.number_from", "ca.number_to", "ca.answered_by", "ca.dial_date", "ca.price")
                .Select("ca.status", "ca.contact_id")
                .LeftOuterJoin("crm_contact co", Exp.EqColumns("ca.contact_id", "co.id"))
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
                var query = Query("crm_voip_calls_history")
                    .Select("id", "parent_call_id", "answered_by", "queue_date", "answer_date", "end_dial_date", "record_url", "record_duration", "price")
                    .Where(Exp.In("parent_call_id", parentId.ToList()));

                return db.ExecuteList(query).ConvertAll(ToCallHistory);
            }
        }

        public VoipCallHistory GetCallHistoryById(string parentId, string callId)
        {
            using (var db = GetDb())
            {
                var query = Query("crm_voip_calls_history")
                    .Select("id", "parent_call_id", "answered_by", "queue_date", "answer_date", "end_dial_date", "record_url", "record_duration", "price")
                    .Where(Exp.Eq("id", callId) & Exp.Eq("parent_call_id", parentId));

                return db.ExecuteList(query).ConvertAll(ToCallHistory).FirstOrDefault();
            }
        }

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