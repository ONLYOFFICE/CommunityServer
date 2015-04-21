/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Dal.DbSchema;


namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {

        #region public methods

        public bool LockMailbox(int mailboxId, bool isAdditionalProccessedCheckNeeded = false)
        {
            return LockMailbox(mailboxId, isAdditionalProccessedCheckNeeded, null);
        }

        public bool LockMailbox(int mailboxId, bool isAdditionalProccessedCheckNeeded, DbManager dbManager)
        {
            _log.Debug("LockMailbox(MailboxId = {0}, checkAnotherProcess = {1})", mailboxId, isAdditionalProccessedCheckNeeded);
            
            var utcNow = DateTime.UtcNow;

            bool success;

            var updateQuery = new SqlUpdate(MailboxTable.name)
                .Set(MailboxTable.Columns.date_checked, utcNow)
                .Set(MailboxTable.Columns.is_processed, true)
                .Where(MailboxTable.Columns.id, mailboxId);

            if (isAdditionalProccessedCheckNeeded)
            {
                updateQuery = updateQuery
                               .Where(MailboxTable.Columns.is_processed, false)
                               .Where(MailboxTable.Columns.is_removed, false);
            }

            if (dbManager == null)
            {
                using (var db = GetDb())
                {
                    success = db.ExecuteNonQuery(updateQuery) > 0;
                }
            }
            else
            {
                success = dbManager.ExecuteNonQuery(updateQuery) > 0;    
            }

            _log.Debug("LockMailbox(MailboxId = {0}) {1}", mailboxId, success ? "SUCCEEDED" : "FAILED");

            return success;
        }

        public List<int> ReleaseLockedMailboxes(int timeoutInMinutes)
        {
            // Reset is_processed field for potentially crushed aggregators
            var query = new SqlQuery(MailboxTable.name)
                .Select(MailboxTable.Columns.id)
                .Where(MailboxTable.Columns.is_processed, true)
                .Where(string.Format("{0} is not null AND TIMESTAMPDIFF(MINUTE, {0}, UTC_TIMESTAMP()) > {1}",
                                     MailboxTable.Columns.date_checked, timeoutInMinutes));

            using (var db = GetDb())
            {
               var oldTasksList =
                    db.ExecuteList(query)
                      .ConvertAll(r => Convert.ToInt32(r[0]));

                if (oldTasksList.Any())
                {
                    var updateQuery = new SqlUpdate(MailboxTable.name)
                        .Set(MailboxTable.Columns.is_processed, false)
                        .Where(Exp.In(MailboxTable.Columns.id, oldTasksList.ToArray()));

                    var rowAffected = db.ExecuteNonQuery(updateQuery);

                    if (rowAffected == 0)
                        _log.Debug("ResetLockedMailboxes() No one locked mailboxes couldn't be released.");
                    else if (rowAffected != oldTasksList.Count)
                        _log.Debug("ResetLockedMailboxes() Some locked mailboxes couldn't be released.");

                }

                return oldTasksList;
            }
        }

        public List<MailBox> GetMailboxesForProcessing(TasksConfig tasksConfig, int needTasks)
        {
            var inactiveCount = (int) Math.Floor(needTasks*tasksConfig.InactiveMailboxesRatio/100);

            var activeCount = needTasks - inactiveCount;

            var mailboxes = GetActiveMailboxesForProcessing(tasksConfig, activeCount);

            var difference = inactiveCount + activeCount - mailboxes.Count;

            if (difference != 0)
                mailboxes.AddRange(GetInactiveMailboxesForProcessing(tasksConfig, difference));

            return mailboxes;
        }

        public void SetNextLoginDelayedForTenant(int tenant, TimeSpan delay)
        {
            using (var db = GetDb())
            {
                var updateAccountQuery = new SqlUpdate(MailboxTable.name)
                    .Where(MailboxTable.Columns.id_tenant, tenant)
                    .Where(MailboxTable.Columns.is_removed, false)
                    .Set(MailboxTable.Columns.is_processed, false)
                    .Set(MailboxTable.Columns.date_login_delay_expires, DateTime.UtcNow.Add(delay));

                db.ExecuteNonQuery(updateAccountQuery);
            }
        }

        public void DisableMailboxesForUser(int tenant, string user)
        {
            DisableMailboxes(tenant, user);
            CreateDisableAllMailboxesAlert(tenant, new List<string> {user});
        }

        public void DisableMailboxesForTenant(int tenant)
        {
            var userIds = GetUsersFromNotPaidTenant(tenant);
            DisableMailboxes(tenant);
            CreateDisableAllMailboxesAlert(tenant, userIds);
        }

        public void DisableMailboxes(int tenant, string user = "")
        {
            using (var db = GetDb())
            {
                var updateAccountQuery = new SqlUpdate(MailboxTable.name)
                    .Where(MailboxTable.Columns.id_tenant, tenant)
                    .Where(MailboxTable.Columns.is_removed, false)
                    .Where(MailboxTable.Columns.enabled, true)
                    .Set(MailboxTable.Columns.is_processed, false)
                    .Set(MailboxTable.Columns.enabled, false);

                if (!string.IsNullOrEmpty(user))
                    updateAccountQuery.Where(MailboxTable.Columns.id_user, user);

                db.ExecuteNonQuery(updateAccountQuery);
            }
        }

        public List<string> GetUsersFromNotPaidTenant(int tenant)
        {
            using (var db = GetDb())
            {
                return
                    db.ExecuteList(
                        new SqlQuery(MailboxTable.name)
                            .Select(MailboxTable.Columns.id_user)
                            .Where(MailboxTable.Columns.id_tenant, tenant)
                            .Where(MailboxTable.Columns.is_removed, false)
                            .Where(MailboxTable.Columns.enabled, true)
                            .Distinct()).ConvertAll(r => Convert.ToString(r[0]));
            }
        }

        public void SetMailboxProcessed(MailBox account, bool withError = false)
        {
            using (var db = GetDb())
            {
                var utcTicksNow = DateTime.UtcNow;

                Func<SqlUpdate> getBaseUpdate = () => new SqlUpdate(MailboxTable.name)
                    .Where(MailboxTable.Columns.id_tenant, account.TenantId)
                    .Where(MailboxTable.Columns.id, account.MailBoxId)
                    .Set(MailboxTable.Columns.is_processed, false);

                var updateAccountQuery = getBaseUpdate();

                if (account.QuotaErrorChanged)
                {
                    if (account.QuotaError)
                    {
                        CreateQuotaErrorWarningAlert(db, account.TenantId, account.UserId);
                    }
                    else
                    {
                        var quotaAlerts = FindAlerts(db, account.TenantId, account.UserId, -1, AlertTypes.QuotaError);

                        if (quotaAlerts.Any())
                        {
                            DeleteAlerts(db, account.TenantId, account.UserId, quotaAlerts.Select(al => al.id).ToList());
                        }
                    }

                    updateAccountQuery
                        .Set(MailboxTable.Columns.quota_error, account.QuotaError);
                }

                if (account.AuthErrorDate.HasValue)
                {
                    updateAccountQuery
                        .Set(MailboxTable.Columns.date_login_delay_expires,
                             DateTime.UtcNow.Add(TimeSpan.FromSeconds(account.ServerLoginDelay)));

                    var difference = DateTime.UtcNow - account.AuthErrorDate.Value;

                    if (difference > AuthErrorDisableTimeout)
                    {
                        updateAccountQuery
                            .Set(MailboxTable.Columns.enabled, false);

                        CreateAuthErrorDisableAlert(db, account.TenantId, account.UserId, account.MailBoxId);
                    }
                    else if (difference > AuthErrorWarningTimeout)
                    {
                        CreateAuthErrorWarningAlert(db, account.TenantId, account.UserId, account.MailBoxId);
                    }
                }
                else
                {
                    updateAccountQuery
                        .Set(MailboxTable.Columns.msg_count_last, account.MessagesCount)
                        .Set(MailboxTable.Columns.size_last, account.Size);

                    if (account.Imap && account.ImapFolderChanged)
                    {
                        updateAccountQuery
                            .Where(MailboxTable.Columns.begin_date, account.BeginDate)
                            .Set(MailboxTable.Columns.imap_intervals, account.ImapIntervalsJson)
                            .Set(MailboxTable.Columns.date_checked, utcTicksNow);

                        var result = db.ExecuteNonQuery(updateAccountQuery);

                        if (result == 0) // BeginDate has been changed
                        {
                            updateAccountQuery = getBaseUpdate();

                            if (account.QuotaErrorChanged)
                            {
                                updateAccountQuery
                                    .Set(MailboxTable.Columns.quota_error, account.QuotaError);
                            }

                            updateAccountQuery
                                .Set(MailboxTable.Columns.imap_intervals, "[]")
                                .Set(MailboxTable.Columns.date_checked, utcTicksNow);
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                db.ExecuteNonQuery(updateAccountQuery);
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
                        .Set(MailboxTable.Columns.date_login_delay_expires, expires));
            }
        }

        public void UpdateUserActivity(int tenant, string user, bool userOnline = true)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                   new SqlUpdate(MailboxTable.name)
                       .Where(MailboxTable.Columns.id_tenant, tenant)
                       .Where(MailboxTable.Columns.id_user, user)
                       .Where(MailboxTable.Columns.is_removed, false)
                       .Set(MailboxTable.Columns.date_user_checked, DateTime.UtcNow)
                       .Set(MailboxTable.Columns.user_online, userOnline));
            }
        }

        public void SetMailboxAuthError(MailBox mailbox, bool isError)
        {
            using (var db = GetDb())
            {
                var updateQuery = new SqlUpdate(MailboxTable.name)
                    .Where(MailboxTable.Columns.id, mailbox.MailBoxId)
                    .Where(MailboxTable.Columns.id_user, mailbox.UserId)
                    .Where(MailboxTable.Columns.id_tenant, mailbox.TenantId);

                if (isError)
                {
                    mailbox.AuthErrorDate = DateTime.UtcNow;
                    updateQuery.Where(MailboxTable.Columns.date_auth_error, null)
                               .Set(MailboxTable.Columns.date_auth_error, mailbox.AuthErrorDate.Value);
                }
                else
                {
                    updateQuery.Set(MailboxTable.Columns.date_auth_error, null);
                    mailbox.AuthErrorDate = null;
                }

                db.ExecuteNonQuery(updateQuery);
            }
        }

        #endregion

        #region private methods

        private List<MailBox> GetActiveMailboxesForProcessing(TasksConfig tasksConfig, int tasksLimit)
        {
            _log.Debug("GetActiveMailboxForProcessing()");

            var mailboxes = GetMailboxesForProcessing(tasksConfig, tasksLimit, true);

            _log.Debug("Found {0} active tasks", mailboxes.Count);

            return mailboxes;
        }

        private List<MailBox> GetInactiveMailboxesForProcessing(TasksConfig tasksConfig, int tasksLimit)
        {
            _log.Debug("GetInactiveMailboxForProcessing()");

            var mailboxes = GetMailboxesForProcessing(tasksConfig, tasksLimit, false);

            _log.Debug("Found {0} inactive tasks", mailboxes.Count);

            return mailboxes;
        }

        private List<MailBox> GetMailboxesForProcessing(TasksConfig tasksConfig, int tasksLimit, bool active)
        {
            using (var db = GetDb())
            {
                var whereLoginDelayExpiredString = string.Format("{0} < UTC_TIMESTAMP()",
                                                                 MailboxTable.Columns.date_login_delay_expires
                                                                             .Prefix(MAILBOX_ALIAS));

                var query = GetSelectMailBoxFieldsQuery()
                    .Where(MailboxTable.Columns.is_processed.Prefix(MAILBOX_ALIAS), false)
                    .Where(whereLoginDelayExpiredString)
                    .Where(MailboxTable.Columns.is_removed.Prefix(MAILBOX_ALIAS), false)
                    .Where(MailboxTable.Columns.enabled.Prefix(MAILBOX_ALIAS), true)
                    .OrderBy(MailboxTable.Columns.date_checked.Prefix(MAILBOX_ALIAS), true)
                    .SetMaxResults(tasksLimit);

                if (tasksConfig.OnlyTeamlabTasks)
                {
                    query
                        .Where(MailboxTable.Columns.is_teamlab_mailbox, true);
                }

                if (tasksConfig.EnableSignalr)
                {
                    query.Where(MailboxTable.Columns.user_online.Prefix(MAILBOX_ALIAS), active);
                }
                else
                {
                    var whereUserCheckedString =
                        string.Format("({0} IS NULL OR TIMESTAMPDIFF(SECOND, {0}, UTC_TIMESTAMP()) {1} {2})",
                                      MailboxTable.Columns.date_user_checked.Prefix(MAILBOX_ALIAS),
                                      active ? "<" : ">",
                                      tasksConfig.ActiveInterval.Seconds);

                    query.Where(whereUserCheckedString);
                }

                if (tasksConfig.WorkOnUsersOnly.Any())
                    query.Where(Exp.In(MailboxTable.Columns.id_user, tasksConfig.WorkOnUsersOnly));

                var listResults = db.ExecuteList(query);

                var seletedTasks = listResults.ConvertAll(ToMailBox).ToList();

                seletedTasks.ForEach(m => m.Active = active);

                return seletedTasks;
            }
        }

        #endregion
    }
}
