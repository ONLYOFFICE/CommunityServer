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
using System.Threading;
using System.Threading.Tasks;
using ActiveUp.Net.Common;
using ActiveUp.Net.Mail;
using ASC.Core.Notify.Signalr;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using log4net;

namespace ASC.Mail.Aggregator.CollectionService.Workers
{
    public abstract class BaseWorker : IDisposable
    {
        private const int SIGNALR_WAIT_SECONDS = 30;
        private const int CHECKED_WAIT_MINIUTES = 3;

        protected readonly MailBoxManager mailBoxManager;
        private readonly MailBox _mailBox;
        protected readonly ILogger log;

        private static SignalrServiceClient _signalrServiceClient;
        private DateTime _lastSignal;
        private bool _needSignal;
        
        private DateTime _lastTimeItemChecked;
        protected int _maxMessagesPerSession;

        protected CancellationToken cancelToken;

        public MailBox Account {
            get { return _mailBox; }
        }

        protected TasksConfig tasksConfig;

        protected BaseWorker(MailBoxManager mailBoxManager, MailBox mailBox, TasksConfig tasksConfig,
                             CancellationToken cancelToken, ILogger log = null)
        {
            this.mailBoxManager = mailBoxManager;
            _mailBox = mailBox;
            this.log = log ?? new NullLogger();
            _lastTimeItemChecked = DateTime.UtcNow;
            _maxMessagesPerSession = tasksConfig.MaxMessagesPerSession;
            this.tasksConfig = tasksConfig;
            _signalrServiceClient = new SignalrServiceClient();

            _needSignal = false;

            this.cancelToken = cancelToken;

            if (tasksConfig.ShowActiveUpLogs)
            {
                Logger.Log4NetLogger = LogManager.GetLogger(string.Format("Task_{0}->ActiveUp", Task.CurrentId));
                Logger.Disabled = false;
            }
            else
            {
                Logger.Log4NetLogger = null;
                Logger.Disabled = true;
            }
        }

        protected virtual void UpdateTimeCheckedIfNeeded()
        {
            if ((DateTime.UtcNow - _lastTimeItemChecked).TotalMinutes <= CHECKED_WAIT_MINIUTES) return;

            mailBoxManager.LockMailbox(Account.MailBoxId);
            _lastTimeItemChecked = DateTime.UtcNow;
            log.Debug("Checked time was updated for mailbox: {0}", Account.MailBoxId);
        }

        protected virtual void RetrieveMessage(Message message, int folderId, string uidl, string md5Hash, bool hasParseError, bool unread = true, int[] tagsIds = null)
        {
            MailMessage messageItem;
            if (mailBoxManager.MailReceive(Account, message, folderId, uidl, md5Hash, hasParseError, unread, tagsIds, out messageItem) < 1)
                throw new Exception("MailReceive() returned id < 1;");

            if (messageItem == null) return;

            mailBoxManager.AddRelationshipEventForLinkedAccounts(Account, messageItem, log);

            mailBoxManager.SaveEmailInData(Account, messageItem, log);
            
            var now = DateTime.UtcNow;

            if ((now - _lastSignal).TotalSeconds > SIGNALR_WAIT_SECONDS)
            {
                log.Debug("InvokeOnRetrieve add to signalr cache UserId = {0} TenantId = {1}", Account.UserId, Account.TenantId);
                _signalrServiceClient.SendUnreadUser(Account.TenantId, Account.UserId);
                _needSignal = false;
                _lastSignal = now;
            }
            else
            {
                log.Debug("InvokeOnRetreive UserId = {0} TenantId = {1} already exists in signalr cache", Account.UserId, Account.TenantId);
                _needSignal = true;
            }
        }

        protected abstract BaseProtocolClient Connect();
        protected abstract bool ProcessMessages(BaseProtocolClient baseProtocolClient);
        public abstract void Aggregate();

        public void Dispose()
        {
            Logger.Log4NetLogger = null;
            Logger.Disabled = true;

            if(_needSignal)
                _signalrServiceClient.SendUnreadUser(Account.TenantId, Account.UserId);
        }
    }
}
