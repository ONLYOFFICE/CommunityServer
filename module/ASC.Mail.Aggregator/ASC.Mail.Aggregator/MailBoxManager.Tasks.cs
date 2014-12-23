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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using System.Web.Configuration;
using System.Net;
using System.IO;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Core.Tenants;
using ASC.Mail.Aggregator.Dal.DbSchema;
using Newtonsoft.Json.Linq;


namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        public enum TariffType
        {
            Active = 0,
            Overdue,
            LongDead
        };

        #region public methods

        public bool LockMailbox(int mailbox_id, Int64 utc_ticks_time)
        {
            return LockMailbox(mailbox_id, utc_ticks_time, false, null);
        }

        public bool LockMailbox(int mailbox_id, Int64 utc_ticks_time, bool is_additional_proccessed_check_needed, DbManager external_db)
        {
            var update_query = new SqlUpdate(MailboxTable.name)
                .Set(MailboxTable.Columns.time_checked, utc_ticks_time)
                .Set(MailboxTable.Columns.is_processed, true)
                .Where(MailboxTable.Columns.id, mailbox_id);

            if (is_additional_proccessed_check_needed)
            {
                update_query = update_query
                               .Where(MailboxTable.Columns.is_processed, false)
                               .Where(MailboxTable.Columns.is_removed, false);
            }

            if (external_db == null)
            {
                using (var db = GetDb())
                {
                    return db.ExecuteNonQuery(update_query) > 0;
                }
            }

            return external_db.ExecuteNonQuery(update_query) > 0;
        }

        public List<int> KillOldTasks(int old_tasks_timeout_in_minutes)
        {
            // Reset is_processed field for potentially crushed aggregators
            List<int> old_tasks_list;
            var aggregator_timeout = TimeSpan.FromMinutes(old_tasks_timeout_in_minutes).Ticks;

            using (var db = GetDb())
            {
                var utc_ticks = DateTime.UtcNow.Ticks;

                old_tasks_list =
                    db.ExecuteList(
                        new SqlQuery(MailboxTable.name)
                            .Select(MailboxTable.Columns.id)
                            .Where(MailboxTable.Columns.is_processed, true)
                            .Where(Exp.Gt(utc_ticks.ToString(CultureInfo.InvariantCulture), MailboxTable.Columns.time_checked))
                            .Where(Exp.Gt(string.Format("{0} - {1}", utc_ticks, MailboxTable.Columns.time_checked),
                                          aggregator_timeout)))
                      .ConvertAll(r => Convert.ToInt32(r[0]));

                if (old_tasks_list.Any())
                {
                    var mail_boxes = "";

                    old_tasks_list.ForEach(i => mail_boxes += i.ToString(CultureInfo.InvariantCulture) + "|");

                    db.ExecuteNonQuery(
                        new SqlUpdate(MailboxTable.name)
                            .Set(MailboxTable.Columns.is_processed, false)
                            .Where(Exp.In(MailboxTable.Columns.id, old_tasks_list.ToArray())));
                }
            }
            return old_tasks_list;
        }

        public TariffType GetTariffType(int tenant_id)
        {
            TariffType result;
            CoreContext.TenantManager.SetCurrentTenant(tenant_id);
            var tenant_info = CoreContext.TenantManager.GetCurrentTenant();

            if (tenant_info.Status != TenantStatus.Active)
                return TariffType.LongDead;

            var response_api = GetApiResponse("portal/tariff.json", tenant_info);
            var date_string = JObject.Parse(response_api)["response"];
            var state = int.Parse(date_string["state"].ToString());
            if (state == 0 || state == 1) result = TariffType.Active;
            else
            {
                var due_date = DateTime.Parse(date_string["dueDate"].ToString());
                _log.Debug("GetTariffType response: {0}", response_api);
                _log.Debug("state={0}, dueDate={1}", state, due_date);
                result = due_date.AddDays(TenantOverdueDays) <= DateTime.UtcNow ? TariffType.LongDead : TariffType.Overdue;
            }
            return result;
        }

        public MailBox GetMailboxForProcessing(TasksConfig tasks_config)
        {
            bool inactive_flag;

            lock (ticks)
            {
                inactive_flag = ticks.Tick();
            }

            MailBox mail;

            if (inactive_flag || null == (mail = GetActiveMailboxForProcessing(tasks_config)))
            {
                mail = GetInactiveMailboxForProcessing(tasks_config);
                if (mail != null)
                {
                    mail.Active = false;
                }
            }
            else mail.Active = true;

            return mail;
        }

        public void SetNextLoginDelayedForTenant(int id_tenant, TimeSpan delay)
        {
            using (var db = GetDb())
            {
                var update_account_query = new SqlUpdate(MailboxTable.name)
                    .Where(MailboxTable.Columns.id_tenant, id_tenant)
                    .Where(MailboxTable.Columns.is_removed, false)
                    .Set(MailboxTable.Columns.is_processed, false)
                    .Set(MailboxTable.Columns.login_delay_expires, DateTime.UtcNow.Add(delay).Ticks);

                db.ExecuteNonQuery(update_account_query);
            }
        }


        public void SetNextLoginDelayedFor(MailBox account, TimeSpan delay)
        {
            using (var db = GetDb())
            {
                var update_account_query = new SqlUpdate(MailboxTable.name)
                    .Where(MailboxTable.Columns.id, account.MailBoxId)
                    .Where(MailboxTable.Columns.id_tenant, account.TenantId)
                    .Set(MailboxTable.Columns.is_processed, false)
                    .Set(MailboxTable.Columns.login_delay_expires, DateTime.UtcNow.Add(delay).Ticks);

                db.ExecuteNonQuery(update_account_query);
            }
        }

        public void DisableMailboxesForUser(int id_tenant, string id_user)
        {
            DisableMailboxes(id_tenant, id_user);
            CreateDisableAllMailboxesAlert(id_tenant, new[] { id_user });
        }

        public void DisableMailboxesForTenant(int id_tenant)
        {
            var user_ids = GetUsersFromNotPaidTenant(id_tenant);
            DisableMailboxes(id_tenant);
            CreateDisableAllMailboxesAlert(id_tenant, user_ids);
        }

        public void DisableMailboxes(int id_tenant, string id_user = "")
        {
            using (var db = GetDb())
            {
                var update_account_query = new SqlUpdate(MailboxTable.name)
                    .Where(MailboxTable.Columns.id_tenant, id_tenant)
                    .Where(MailboxTable.Columns.is_removed, false)
                    .Where(MailboxTable.Columns.enabled, true)
                    .Set(MailboxTable.Columns.is_processed, false)
                    .Set(MailboxTable.Columns.enabled, false);

                if (!string.IsNullOrEmpty(id_user))
                    update_account_query.Where(MailboxTable.Columns.id_user, id_user);

                db.ExecuteNonQuery(update_account_query);
            }
        }

        public List<string> GetUsersFromNotPaidTenant(int id_tenant)
        {
            using (var db = GetDb())
            {
                return
                    db.ExecuteList(
                        new SqlQuery(MailboxTable.name)
                            .Select(MailboxTable.Columns.id_user)
                            .Where(MailboxTable.Columns.id_tenant, id_tenant)
                            .Where(MailboxTable.Columns.is_removed, false)
                            .Where(MailboxTable.Columns.enabled, true)
                            .Distinct()).ConvertAll(r => Convert.ToString(r[0]));
            }
        }


        private static SqlUpdate GetBaseUpdateAccountQueryOnMailboxProccessingComplete(MailBox account)
        {
            var utc_ticks_now = DateTime.UtcNow.Ticks;

            return new SqlUpdate(MailboxTable.name)
                    .Where(MailboxTable.Columns.id, account.MailBoxId)
                    .Set(MailboxTable.Columns.is_processed, false)
                    .Set(MailboxTable.Columns.msg_count_last, account.MessagesCount)
                    .Set(MailboxTable.Columns.size_last, account.Size)
                    .Set(MailboxTable.Columns.time_checked, utc_ticks_now); //Its needed for more uniform distribution in GetMailBoxForProccessing().
        }

        public void MailboxProcessingCompleted(MailBox account)
        {
            using (var db = GetDb())
            {
                var update_account_query = GetBaseUpdateAccountQueryOnMailboxProccessingComplete(account);
                if (account.ImapFolderChanged)
                {
                    update_account_query
                        .Where(MailboxTable.Columns.begin_date, account.BeginDate)
                        .Set(MailboxTable.Columns.imap_folders, account.ImapFoldersJson);
                }

                var result = db.ExecuteNonQuery(update_account_query);

                if (result == 0) // BeginDate has been changed
                {
                    db.ExecuteNonQuery(GetBaseUpdateAccountQueryOnMailboxProccessingComplete(account)
                                        .Set(MailboxTable.Columns.imap_folders, "[]"));
                }
            }
        }

        public void MailboxProcessingError(MailBox account, Exception exception)
        {
            SetNextLoginDelayedFor(account, TimeSpan.FromSeconds(account.ServerLoginDelay));
        }

        public void SetMailboxQuotaError(MailBox account, bool state)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                        new SqlUpdate(MailboxTable.name)
                            .Where(MailboxTable.Columns.id, account.MailBoxId)
                            .Where(MailboxTable.Columns.id_tenant, account.TenantId)
                            .Set(MailboxTable.Columns.quota_error, state));
            }
        }

        public void SetEmailLoginDelayExpires(string email, DateTime expires)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                    new SqlUpdate(MailboxTable.name)
                        .Where(MailboxTable.Columns.address, email)
                        .Where(MailboxTable.Columns.is_removed, false)
                        .Set(MailboxTable.Columns.login_delay_expires, expires.Ticks));
            }
        }

        public void UpdateUserActivity(int id_tenant, string id_user)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                   new SqlUpdate(MailboxTable.name)
                       .Where(MailboxTable.Columns.id_tenant, id_tenant)
                       .Where(MailboxTable.Columns.id_user, id_user)
                       .Set(MailboxTable.Columns.user_time_checked, DateTime.UtcNow.Ticks));
            }
        }

        public void SetAuthError(MailBox mailbox, bool error)
        {
            using (var db = GetDb())
            {
                var instr = new SqlUpdate(MailboxTable.name)
                    .Where(MailboxTable.Columns.id, mailbox.MailBoxId)
                    .Where(MailboxTable.Columns.id_user, mailbox.UserId)
                    .Where(MailboxTable.Columns.id_tenant, mailbox.TenantId);

                db.ExecuteNonQuery(error
                                       ? instr.Where(MailboxTable.Columns.auth_error, null)
                                              .Set(MailboxTable.Columns.auth_error, DateTime.UtcNow.Ticks)
                                       : instr.Set(MailboxTable.Columns.auth_error, null));

                if (mailbox.AuthError == MailBox.AuthProblemType.NoProblems)
                    return;

                switch (mailbox.AuthError)
                {
                    case MailBox.AuthProblemType.ConnectError:
                        CreateAuthErrorWarningAlert(mailbox.TenantId, mailbox.UserId, mailbox.MailBoxId);
                        break;
                    case MailBox.AuthProblemType.TooManyErrors:
                        CreateAuthErrorDisableAlert(mailbox.TenantId, mailbox.UserId, mailbox.MailBoxId);
                        EnableMaibox(mailbox, false);
                        break;
                    default:
                        return;
                }


            }
        }

        #endregion

        #region private methods

        public string GetAuthCookie(Tenant tenant_info)
        {
            return SecurityContext.AuthenticateMe(tenant_info.OwnerId);
        }

        private string GetApiResponse(string api_url, Tenant tenant_info)
        {
            var request_uri_builder = new UriBuilder(Uri.UriSchemeHttp, CoreContext.TenantManager.GetCurrentTenant().TenantAlias);

            api_url = string.Format("{0}/{1}", WebConfigurationManager.AppSettings["api.url"].Trim('~', '/'), api_url.TrimStart('/'));

            if (CoreContext.TenantManager.GetCurrentTenant().TenantAlias == "localhost")
            {
                var virtual_dir = WebConfigurationManager.AppSettings["core.virtual-dir"];
                api_url = virtual_dir.Trim('/') + "/" + api_url;

                var host = WebConfigurationManager.AppSettings["core.host"];
                if (!string.IsNullOrEmpty(host)) request_uri_builder.Host = host;

                var port = WebConfigurationManager.AppSettings["core.port"];
                if (!string.IsNullOrEmpty(port)) request_uri_builder.Port = int.Parse(port);
            }
            else
                request_uri_builder.Host += "." + WebConfigurationManager.AppSettings["core.base-domain"];

            request_uri_builder.Path = api_url;

            var api_request = (HttpWebRequest)WebRequest.Create(request_uri_builder.Uri);
            api_request.Headers.Add("Payment-Info", "false");
            api_request.AllowAutoRedirect = true;
            api_request.CookieContainer = new CookieContainer();
            api_request.CookieContainer.Add(new Cookie("asc_auth_key", GetAuthCookie(tenant_info), "/", request_uri_builder.Host));

            using (var api_response = (HttpWebResponse)api_request.GetResponse())
            using (var resp_stream = api_response.GetResponseStream())
            {
                return resp_stream != null ? new StreamReader(resp_stream).ReadToEnd() : null;
            }
        }

        private MailBox GetActiveMailboxForProcessing(TasksConfig tasks_config)
        {
            var mail = GetMailboxForProcessing(tasks_config, "(cast({0} as decimal) - " + MailboxTable.Columns.user_time_checked.Prefix(mail_mailbox_alias) + " ) < {1}");
            return mail;
        }

        private MailBox GetInactiveMailboxForProcessing(TasksConfig tasks_config)
        {
            var mail = GetMailboxForProcessing(tasks_config, "(cast({0} as decimal) - " + MailboxTable.Columns.user_time_checked.Prefix(mail_mailbox_alias) + " ) > {1}");
            return mail;
        }

        private MailBox GetMailboxForProcessing(TasksConfig tasks_config, string where_usertime_sql_format)
        {
            using (var db = GetDb())
            {
                int? locker = 0;
                try
                {
                    locker = db.ExecuteScalar<int?>("SELECT GET_LOCK('lock_id', 5)");

                    if (locker == 1)
                    {
                        var utc_ticks = DateTime.UtcNow.Ticks;

                        var query = GetSelectMailBoxFieldsQuery()
                            .Where(MailboxTable.Columns.is_processed.Prefix(mail_mailbox_alias), false)
                            .Where(string.Format(where_usertime_sql_format, utc_ticks, tasks_config.ActiveInterval.Ticks))
                            .Where(Exp.Le(MailboxTable.Columns.login_delay_expires.Prefix(mail_mailbox_alias), utc_ticks))
                            .Where(Exp.And(Exp.Eq(MailboxTable.Columns.is_removed.Prefix(mail_mailbox_alias), false), Exp.Eq(MailboxTable.Columns.enabled.Prefix(mail_mailbox_alias), true)))
                            .Where(MailboxTable.Columns.is_teamlab_mailbox, tasks_config.OnlyTeamlabTasks)
                            .OrderBy(MailboxTable.Columns.time_checked.Prefix(mail_mailbox_alias), true)
                            .SetMaxResults(1);

                        if (tasks_config.WorkOnUsersOnly != null && tasks_config.WorkOnUsersOnly.Any())
                            query.Where(Exp.In(MailboxTable.Columns.id_user, tasks_config.WorkOnUsersOnly));

                        var list_results = db.ExecuteList(query);

                        var selected_box = list_results.ConvertAll(ToMailBox).FirstOrDefault();

                        if (selected_box != null)
                        {
                            var is_successed = LockMailbox(selected_box.MailBoxId, utc_ticks, true, db);
                            if (!is_successed)
                            {
                                selected_box = null;
                            }
                        }

                        return selected_box;
                    }
                }
                finally
                {
                    if (locker == 1)
                    {
                        db.ExecuteScalar<int>("SELECT RELEASE_LOCK('lock_id')");
                    }
                }
            }

            return null;
        }

        #endregion
    }
}
