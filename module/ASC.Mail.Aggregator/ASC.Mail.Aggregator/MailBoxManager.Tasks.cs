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


using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.DbSchema;

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

            bool success;

            var updateQuery = new SqlUpdate(MailboxTable.Name)
                .Set(string.Format("{0} = UTC_TIMESTAMP()", MailboxTable.Columns.DateChecked))
                .Set(MailboxTable.Columns.IsProcessed, true)
                .Where(MailboxTable.Columns.Id, mailboxId);

            if (isAdditionalProccessedCheckNeeded)
            {
                updateQuery = updateQuery
                               .Where(MailboxTable.Columns.IsProcessed, false)
                               .Where(MailboxTable.Columns.IsRemoved, false);
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
            var query = new SqlQuery(MailboxTable.Name)
                .Select(MailboxTable.Columns.Id)
                .Where(MailboxTable.Columns.IsProcessed, true)
                .Where(string.Format("{0} is not null AND TIMESTAMPDIFF(MINUTE, {0}, UTC_TIMESTAMP()) > {1}",
                                     MailboxTable.Columns.DateChecked, timeoutInMinutes));

            using (var db = GetDb())
            {
               var oldTasksList =
                    db.ExecuteList(query)
                      .ConvertAll(r => Convert.ToInt32(r[0]));

                if (oldTasksList.Any())
                {
                    var updateQuery = new SqlUpdate(MailboxTable.Name)
                        .Set(MailboxTable.Columns.IsProcessed, false)
                        .Where(Exp.In(MailboxTable.Columns.Id, oldTasksList.ToArray()));

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
            var mailboxes = new List<MailBox>();

            var boundaryRatio = !(tasksConfig.InactiveMailboxesRatio > 0 && tasksConfig.InactiveMailboxesRatio < 100);

            if (needTasks > 1 || boundaryRatio)
            {
                var inactiveCount = (int) Math.Round(needTasks*tasksConfig.InactiveMailboxesRatio/100, MidpointRounding.AwayFromZero);

                var activeCount = needTasks - inactiveCount;

                if (activeCount == needTasks)
                {
                    mailboxes.AddRange(GetActiveMailboxesForProcessing(tasksConfig, activeCount));
                }
                else if (inactiveCount == needTasks)
                {
                    mailboxes.AddRange(GetInactiveMailboxesForProcessing(tasksConfig, inactiveCount));
                }
                else
                {
                    mailboxes.AddRange(GetActiveMailboxesForProcessing(tasksConfig, activeCount));

                    var difference = inactiveCount + activeCount - mailboxes.Count;

                    if (difference > 0)
                        mailboxes.AddRange(GetInactiveMailboxesForProcessing(tasksConfig, difference));
                }
            }
            else
            {
                mailboxes.AddRange(GetActiveMailboxesForProcessing(tasksConfig, 1));

                var difference = needTasks - mailboxes.Count;

                if (difference > 0)
                    mailboxes.AddRange(GetInactiveMailboxesForProcessing(tasksConfig, difference));
            }

            return mailboxes;
        }

        public void SetNextLoginDelayedForTenant(int tenant, TimeSpan delay)
        {
            using (var db = GetDb())
            {
                var updateAccountQuery = NextLoginDelayedQuery(tenant, (int)delay.TotalSeconds);
                db.ExecuteNonQuery(updateAccountQuery);
            }
        }

        public void SetNextLoginDelayedForUser(int tenant, string user, TimeSpan delay)
        {
            using (var db = GetDb())
            {
                var updateAccountQuery = NextLoginDelayedQuery(tenant, (int)delay.TotalSeconds);
                updateAccountQuery.Where(MailboxTable.Columns.User, user);
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
                var updateAccountQuery = new SqlUpdate(MailboxTable.Name)
                    .Where(MailboxTable.Columns.Tenant, tenant)
                    .Where(MailboxTable.Columns.IsRemoved, false)
                    .Where(MailboxTable.Columns.Enabled, true)
                    .Set(MailboxTable.Columns.IsProcessed, false)
                    .Set(MailboxTable.Columns.Enabled, false);

                if (!string.IsNullOrEmpty(user))
                    updateAccountQuery.Where(MailboxTable.Columns.User, user);

                db.ExecuteNonQuery(updateAccountQuery);
            }
        }

        public List<string> GetUsersFromNotPaidTenant(int tenant)
        {
            using (var db = GetDb())
            {
                return
                    db.ExecuteList(
                        new SqlQuery(MailboxTable.Name)
                            .Select(MailboxTable.Columns.User)
                            .Where(MailboxTable.Columns.Tenant, tenant)
                            .Where(MailboxTable.Columns.IsRemoved, false)
                            .Where(MailboxTable.Columns.Enabled, true)
                            .Distinct()).ConvertAll(r => Convert.ToString(r[0]));
            }
        }

        public void SetMailboxProcessed(MailBox account, bool withError = false)
        {
            using (var db = GetDb())
            {
                Func<SqlUpdate> getBaseUpdate = () => new SqlUpdate(MailboxTable.Name)
                    .Where(MailboxTable.Columns.Tenant, account.TenantId)
                    .Where(MailboxTable.Columns.Id, account.MailBoxId)
                    .Set(MailboxTable.Columns.IsProcessed, false)
                    .Set(string.Format("{0} = UTC_TIMESTAMP()", MailboxTable.Columns.DateChecked))
                    .Set(string.Format("{0} = DATE_ADD(UTC_TIMESTAMP(), INTERVAL {1} SECOND)",
                            MailboxTable.Columns.DateLoginDelayExpires,
                            account.ServerLoginDelay));

                var updateAccountQuery = getBaseUpdate();

                if (account.QuotaErrorChanged)
                {
                    if (account.QuotaError)
                    {
                        CreateQuotaErrorWarningAlert(account.TenantId, account.UserId, db);
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
                        .Set(MailboxTable.Columns.QuotaError, account.QuotaError);
                }

                if (account.AuthErrorDate.HasValue)
                {
                    var difference = DateTime.UtcNow - account.AuthErrorDate.Value;

                    if (difference > AuthErrorDisableTimeout)
                    {
                        updateAccountQuery
                            .Set(MailboxTable.Columns.Enabled, false);

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
                        .Set(MailboxTable.Columns.MsgCountLast, account.MessagesCount)
                        .Set(MailboxTable.Columns.SizeLast, account.Size);

                    if (account.Imap && account.ImapFolderChanged)
                    {
                        updateAccountQuery
                            .Where(MailboxTable.Columns.BeginDate, account.BeginDate)
                            .Set(MailboxTable.Columns.ImapIntervals, account.ImapIntervalsJson);

                        var result = db.ExecuteNonQuery(updateAccountQuery);

                        if (result == 0) // BeginDate has been changed
                        {
                            updateAccountQuery = getBaseUpdate();

                            if (account.QuotaErrorChanged)
                            {
                                updateAccountQuery
                                    .Set(MailboxTable.Columns.QuotaError, account.QuotaError);
                            }

                            updateAccountQuery
                                .Set(MailboxTable.Columns.ImapIntervals, null);
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

        public void SetEmailLoginDelayExpires(string email, int delaySeconds)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                    new SqlUpdate(MailboxTable.Name)
                        .Where(MailboxTable.Columns.Address, email)
                        .Where(MailboxTable.Columns.IsRemoved, false)
                        .Set(string.Format("{0} = DATE_ADD(UTC_TIMESTAMP(), INTERVAL {1} SECOND)",
                            MailboxTable.Columns.DateLoginDelayExpires,
                            delaySeconds)));
            }
        }

        public void UpdateUserActivity(int tenant, string user, bool userOnline = true)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                   new SqlUpdate(MailboxTable.Name)
                       .Where(MailboxTable.Columns.Tenant, tenant)
                       .Where(MailboxTable.Columns.User, user)
                       .Where(MailboxTable.Columns.IsRemoved, false)
                       .Set(string.Format("{0} = UTC_TIMESTAMP()", MailboxTable.Columns.DateUserChecked))
                       .Set(MailboxTable.Columns.UserOnline, userOnline));
            }
        }

        public void SetMailboxAuthError(MailBox mailbox, bool isError)
        {
            using (var db = GetDb())
            {
                var updateQuery = new SqlUpdate(MailboxTable.Name)
                    .Where(MailboxTable.Columns.Id, mailbox.MailBoxId)
                    .Where(MailboxTable.Columns.User, mailbox.UserId)
                    .Where(MailboxTable.Columns.Tenant, mailbox.TenantId);

                if (isError)
                {
                    mailbox.AuthErrorDate = DateTime.UtcNow;
                    updateQuery.Where(MailboxTable.Columns.DateAuthError, null)
                               .Set(MailboxTable.Columns.DateAuthError, mailbox.AuthErrorDate.Value);
                }
                else
                {
                    updateQuery.Set(MailboxTable.Columns.DateAuthError, null);
                    mailbox.AuthErrorDate = null;
                }

                db.ExecuteNonQuery(updateQuery);
            }
        }

        #endregion

        #region private methods

        private List<MailBox> GetActiveMailboxesForProcessing(TasksConfig tasksConfig, int tasksLimit)
        {
            if(tasksLimit <= 0)
                return new List<MailBox>();

            _log.Debug("GetActiveMailboxForProcessing()");
            
            var mailboxes = GetMailboxesForProcessing(tasksConfig, tasksLimit, true);

            _log.Debug("Found {0} active tasks", mailboxes.Count);

            return mailboxes;
        }

        private List<MailBox> GetInactiveMailboxesForProcessing(TasksConfig tasksConfig, int tasksLimit)
        {
            if (tasksLimit <= 0)
                return new List<MailBox>();

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
                                                                 MailboxTable.Columns.DateLoginDelayExpires
                                                                             .Prefix(MAILBOX_ALIAS));

                var query = GetSelectMailBoxFieldsQuery()
                    .LeftOuterJoin(AutoreplyTable.Name.Alias(AUTOREPLY_ALIAS),
                        Exp.EqColumns(AutoreplyTable.Columns.MailboxId.Prefix(AUTOREPLY_ALIAS),
                                      MailboxTable.Columns.Id.Prefix(MAILBOX_ALIAS)))
                    .Select(AutoreplyTable.Columns.TurnOn)
                    .Select(AutoreplyTable.Columns.OnlyContacts)
                    .Select(AutoreplyTable.Columns.TurnOnToDate)
                    .Select(AutoreplyTable.Columns.FromDate)
                    .Select(AutoreplyTable.Columns.ToDate)
                    .Select(AutoreplyTable.Columns.Subject)
                    .Select(AutoreplyTable.Columns.Html)
                    .Where(MailboxTable.Columns.IsProcessed.Prefix(MAILBOX_ALIAS), false)
                    .Where(whereLoginDelayExpiredString)
                    .Where(MailboxTable.Columns.IsRemoved.Prefix(MAILBOX_ALIAS), false)
                    .Where(MailboxTable.Columns.Enabled.Prefix(MAILBOX_ALIAS), true)
                    .OrderBy(MailboxTable.Columns.DateChecked.Prefix(MAILBOX_ALIAS), true)
                    .SetMaxResults(tasksLimit);

                if (tasksConfig.AggregateMode != TasksConfig.AggregateModeType.All)
                {
                    query
                        .Where(MailboxTable.Columns.IsTeamlabMailbox, tasksConfig.AggregateMode == TasksConfig.AggregateModeType.Internal);
                }

                if (tasksConfig.EnableSignalr)
                {
                    query.Where(MailboxTable.Columns.UserOnline.Prefix(MAILBOX_ALIAS), active);
                }
                else
                {
                    var whereUserCheckedString =
                        string.Format("({0} IS NULL OR TIMESTAMPDIFF(SECOND, {0}, UTC_TIMESTAMP()) {1} {2})",
                                      MailboxTable.Columns.DateUserChecked.Prefix(MAILBOX_ALIAS),
                                      active ? "<" : ">",
                                      tasksConfig.ActiveInterval.Seconds);

                    query.Where(whereUserCheckedString);
                }

                if (tasksConfig.WorkOnUsersOnly.Any())
                    query.Where(Exp.In(MailboxTable.Columns.User, tasksConfig.WorkOnUsersOnly));

                var listResults = db.ExecuteList(query);

                var seletedTasks = listResults.ConvertAll(ToMailBox).ToList();

                seletedTasks.ForEach(m => m.Active = active);

                return seletedTasks;
            }
        }

        private SqlUpdate NextLoginDelayedQuery(int tenant, int delaySeconds)
        {
            return new SqlUpdate(MailboxTable.Name)
                .Where(MailboxTable.Columns.Tenant, tenant)
                .Where(MailboxTable.Columns.IsRemoved, false)
                .Set(MailboxTable.Columns.IsProcessed, false)
                .Set(string.Format("{0} = DATE_ADD(UTC_TIMESTAMP(), INTERVAL {1} SECOND)",
                    MailboxTable.Columns.DateLoginDelayExpires,
                    delaySeconds));
        }

        #endregion
    }
}
