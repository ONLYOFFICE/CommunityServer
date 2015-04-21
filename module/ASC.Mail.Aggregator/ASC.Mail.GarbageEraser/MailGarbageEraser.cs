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
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Caching;
using ASC.Core;
using ASC.Core.Users;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Dal;
using ASC.Mail.Aggregator.DataStorage;
using ASC.Mail.Aggregator.Iterators;

namespace ASC.Mail.GarbageEraser
{
    public class MailGarbageEraser
    {
        private readonly ILogger _log;
        private readonly MailBoxManager _mailBoxManager;
        private readonly MailGarbageCleanDal _garbageManager;
        private const int TENANT_CACHE_DAYS = 1;

        public MailGarbageEraser(ILogger log = null)
        {
            _log = log ?? new NullLogger();

            _log.Info("Service started");

            _mailBoxManager = new MailBoxManager();

            _garbageManager = new MailGarbageCleanDal(_mailBoxManager);
        }

        #region - Public methods -

        public void ClearMailGarbage(ManualResetEvent resetEvent)
        {
            _log.Debug("Begin ClearMailGarbage()");
            
            var mailboxIterator = new MailboxIterator(_mailBoxManager);

            var mailbox = mailboxIterator.First();
            while (!mailboxIterator.IsDone)
            {
                try
                {
                    _log.Info("Search garbage on MailboxId = {0}, email = '{1}', tenant = '{2}'",
                              mailbox.MailBoxId, mailbox.Account, mailbox.TenantId);

                    if (!RemoveLongDeadTenantGarbageMailData(mailbox.TenantId))
                    {
                        if (resetEvent.WaitOne(0))
                        {
                            _log.Debug("ClearMailGarbage() is canceled");
                            return;
                        }

                        if (!RemoveTerminatedUserGarbageMailData(mailbox.TenantId, mailbox.UserId))
                        {
                            if (resetEvent.WaitOne(0))
                            {
                                _log.Debug("ClearMailGarbage() is canceled");
                                return;
                            }

                            RemoveGarbageMailboxData(mailbox);
                        }
                    }

                    if (resetEvent.WaitOne(0))
                    {
                        _log.Debug("ClearMailGarbage() is canceled");
                        return;
                    }

                    mailbox = mailboxIterator.Next();
                }
                catch (Exception ex)
                {
                    _log.Error(ex.ToString());
                }

                if (resetEvent.WaitOne(0))
                {
                    _log.Debug("ClearMailGarbage() is canceled");
                    return;
                }
            }

            _log.Debug("End ClearMailGarbage()\r\n");
        }

        public bool RemoveLongDeadTenantGarbageMailData(int tenant)
        {
            if (!IsPortalClosed(tenant)) return false;

            _log.Info(
                "Portal is long dead. All mail's data for this tenant = {0} will be removed!",
                tenant);

            try
            {
                var dataStorage = MailDataStore.GetDataStore(tenant);

                if (dataStorage.IsDirectory(string.Empty, string.Empty))
                {
                    _log.Debug("Trying remove all long dead tenant mail's files from storage");
                    dataStorage.DeleteDirectory(string.Empty, string.Empty);
                }
            }
            catch (Exception ex)
            {
                _log.Error("Can't remove all stored mail data for tenant {0}\r\nException: {1}", tenant, ex.ToString());
                return false;
            }

            _garbageManager.ClearTenantMailData(tenant);

            return true;
        }

        public bool RemoveTerminatedUserGarbageMailData(int tenant, string user)
        {
            var userInfo = GetUserInfo(tenant, user);
            if (userInfo == null || userInfo.Status != EmployeeStatus.Terminated) return false;

            _log.Info(
                "User is terminated. All mail's data for this user = '{0}' (Tenant={1}) will be removed!",
                user, tenant);

            try
            {
                var dataStorage = MailDataStore.GetDataStore(tenant);

                var path = MailStoragePathCombiner.GetUserMailsDirectory(userInfo.ID.ToString());

                if (dataStorage.IsDirectory(string.Empty, path))
                {
                    _log.Debug("Trying remove all user mail's files from storage '{0}' on path '{1}'",
                               Defines.MODULE_NAME, path);
                    dataStorage.DeleteDirectory(string.Empty, path);
                }
            }
            catch (Exception ex)
            {
                _log.Error("Can't remove all stored mail data for user '{0}' on tenant {1}\r\nException: {2}", user,
                           tenant, ex.ToString());
                return false;
            }

            _log.Debug("Trying remove garbage from db");
            _garbageManager.ClearUserMailData(tenant, userInfo);

            return true;
        }

        public bool RemoveGarbageMailboxData(MailBox mailbox)
        {
            if (!mailbox.IsRemoved)
                return false;

            _log.Info(
                "Mailbox is removed. All mail's data for this mailboxId = {0} will be removed!",
                mailbox.MailBoxId);

            try
            {
                var dataStorage = MailDataStore.GetDataStore(mailbox.TenantId);

                var mailboxMessagesIterator = new MailboxMessagesIterator(mailbox, _mailBoxManager);

                var message = mailboxMessagesIterator.First();
                while (!mailboxMessagesIterator.IsDone)
                {
                    var path = MailStoragePathCombiner.GetMessageDirectory(mailbox.UserId, message.StreamId);

                    try
                    {
                        if (dataStorage.IsDirectory(string.Empty, path))
                        {
                            _log.Debug("Trying remove files on path '{0}'", path);
                            dataStorage.DeleteDirectory(string.Empty, path);
                        }
                    }
                    catch (Exception)
                    {
                        _log.Error("Can't remove stored mail data in path = '{0}' for mailboxId = {0} on tenant {1}",
                                   path, mailbox.MailBoxId, mailbox.TenantId);
                    }

                    _log.Info("Clear MessageId = {1}", mailbox.MailBoxId, message.Id);
                    message = mailboxMessagesIterator.Next();
                }

                _log.Debug("Trying remove garbage from db");
                _garbageManager.ClearMailboxData(mailbox);

            }
            catch (Exception)
            {
                _log.Error("Can't remove all stored mail data for mailboxId = {0} on tenant {1}", mailbox.MailBoxId, mailbox.TenantId);
                return false;
            }

            return true;
        }

        #endregion

        #region - Private methods -

        private bool IsPortalClosed(int tenant)
        {
            var absence = false;
            var type = HttpRuntime.Cache.Get(tenant.ToString(CultureInfo.InvariantCulture));
            if (type == null)
            {
                _log.Info("Tenant {0} isn't in cache", tenant);
                absence = true;
                try
                {
                    type = _mailBoxManager.GetTariffType(tenant);
                }
                catch (Exception e)
                {
                    _log.Error("Collector.GetItem() -> GetTariffType Exception: {0}", e.ToString());
                    type = MailBoxManager.TariffType.Active;
                }
            }
            else
            {
                _log.Info("Tenant {0} is in cache", tenant);
            }

            if (absence)
            {
                var mboxKey = tenant.ToString(CultureInfo.InvariantCulture);
                HttpRuntime.Cache.Remove(mboxKey);
                HttpRuntime.Cache.Insert(mboxKey, type, null,
                                         DateTime.UtcNow.AddDays(TENANT_CACHE_DAYS), Cache.NoSlidingExpiration);
            }

            return MailBoxManager.TariffType.LongDead == (MailBoxManager.TariffType)type;
        }

        private UserInfo GetUserInfo(int tenant, string user)
        {
            try
            {
                CoreContext.TenantManager.SetCurrentTenant(tenant);
                var userInfo = CoreContext.UserManager.GetUsers(new Guid(user));
                return userInfo;
            }
            catch (Exception ex)
            {
                _log.Error("Can't find user '{0}' on tenant = {1}\r\nException: {2}", user, tenant, ex.ToString());
            }

            return null;
        }

        #endregion
    }
}
