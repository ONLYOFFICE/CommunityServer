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


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using ASC.Common.Logging;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Storage;
using ASC.Mail.Enums;
using ASC.Mail.Utils;
using MailMessage = ASC.Mail.Data.Contracts.MailMessageData;

namespace ASC.Mail.Core.Engine
{
    public class AutoreplyEngine
    {
        public string UserId { get; set; }
        public int TenantId { get; set; }

        public int AutoreplyDaysInterval { get; set; }

        public ILog Log { get; private set; }

        public AutoreplyEngine(int tenant, string userId, ILog log = null)
        {
            TenantId = tenant;
            UserId = userId;

            Log = log ?? LogManager.GetLogger("ASC.Mail.AutoreplyEngine");

            AutoreplyDaysInterval = Defines.AutoreplyDaysInterval;
        }

        public MailAutoreplyData SaveAutoreply(int mailboxId, bool turnOn, bool onlyContacts,
            bool turnOnToDate, DateTime fromDate, DateTime toDate, string subject, string html)
        {
            if (fromDate == DateTime.MinValue)
                throw new ArgumentException(@"Invalid parameter", "fromDate");

            if (turnOnToDate && toDate == DateTime.MinValue)
                throw new ArgumentException(@"Invalid parameter", "toDate");

            if (turnOnToDate && toDate < fromDate)
                throw new ArgumentException(@"Wrong date interval, toDate < fromDate", "fromDate");

            if (string.IsNullOrEmpty(html))
                throw new ArgumentException(@"Invalid parameter", "html");

            var imagesReplacer = new StorageManager(TenantId, UserId);
            html = imagesReplacer.ChangeEditorImagesLinks(html, mailboxId);

            var autoreply = new MailboxAutoreply
            {
                MailboxId = mailboxId,
                Tenant = TenantId,
                FromDate = fromDate,
                ToDate = toDate,
                Html = html,
                OnlyContacts = onlyContacts,
                TurnOnToDate = turnOnToDate,
                Subject = subject,
                TurnOn = turnOn
            };

            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                if (!daoMailbox.CanAccessTo(
                    new СoncreteUserMailboxExp(mailboxId, TenantId, UserId)))
                {
                    throw new AccessViolationException("Mailbox is not owned by user.");
                }

                var result = daoFactory.CreateMailboxAutoreplyDao(TenantId, UserId).SaveAutoreply(autoreply);

                if (result <= 0)
                    throw new InvalidOperationException();
            }

            var resp = new MailAutoreplyData(autoreply.MailboxId, autoreply.Tenant, autoreply.TurnOn, autoreply.OnlyContacts,
                autoreply.TurnOnToDate, autoreply.FromDate, autoreply.ToDate, autoreply.Subject, autoreply.Html);

            return resp;
        }

        public void EnableAutoreply(MailBoxData account, bool enabled)
        {
            account.MailAutoreply.TurnOn = enabled;

            var autoreply = new MailboxAutoreply
            {
                MailboxId = account.MailBoxId,
                Tenant = account.TenantId,
                FromDate = account.MailAutoreply.FromDate,
                ToDate = account.MailAutoreply.ToDate,
                Html = account.MailAutoreply.Html,
                OnlyContacts = account.MailAutoreply.OnlyContacts,
                TurnOnToDate = account.MailAutoreply.TurnOnToDate,
                Subject = account.MailAutoreply.Subject,
                TurnOn = account.MailAutoreply.TurnOn
            };

            using (var daoFactory = new DaoFactory())
            {
                var autoreplyDao = daoFactory.CreateMailboxAutoreplyDao(account.TenantId, account.UserId);

                var result = autoreplyDao.SaveAutoreply(autoreply);

                if (result <= 0)
                    throw new InvalidOperationException();

                var autoreplyHistoryDao = daoFactory.CreateMailboxAutoreplyHistoryDao(account.TenantId,
                    account.UserId);

                result = autoreplyHistoryDao.DeleteAutoreplyHistory(account.MailBoxId);

                if (result <= 0)
                    throw new InvalidOperationException();
            }
        }

        public void SendAutoreply(MailBoxData account, MailMessage message, string httpContextScheme, ILog log)
        {
            try
            {
                if (message.Folder != FolderType.Inbox
                    || account.MailAutoreply == null
                    || !account.MailAutoreply.TurnOn)
                {
                    return;
                }

                var utcNow = DateTime.UtcNow.Date;

                if (account.MailAutoreply.TurnOnToDate &&
                    account.MailAutoreply.ToDate != DateTime.MinValue &&
                    account.MailAutoreply.ToDate < utcNow)
                {
                    log.InfoFormat("DisableAutoreply(MailboxId = {0}) -> time is over", account.MailBoxId);

                    EnableAutoreply(account, false);

                    return;
                }

                if (account.MailAutoreply.FromDate > utcNow)
                {
                    log.Info("Skip MailAutoreply: FromDate > utcNow");
                    return;
                }

                if (account.MailAutoreply.FromDate > message.Date)
                {
                    log.Info("Skip MailAutoreply: FromDate > message.Date");
                    return;
                }

                if (MailUtil.IsMessageAutoGenerated(message))
                {
                    log.Info("Skip MailAutoreply: found some auto-generated header");
                    return;
                }

                if (IsCurrentMailboxInFrom(account, message))
                {
                    log.Info("Skip MailAutoreply: message from current account");
                    return;
                }

                var apiHelper = new ApiHelper(httpContextScheme);

                var autoreplyEmail = GetAutoreplyEmailInTo(account, message);

                if (string.IsNullOrEmpty(autoreplyEmail))
                {
                    log.Info("Skip MailAutoreply: autoreplyEmail not found");
                    return;
                }

                if (HasGroupsInTo(account, message))
                {
                    log.Info("Skip MailAutoreply: has group address in TO, CC");
                    return;
                }

                if (HasMailboxAutoreplyHistory(account, message.FromEmail))
                {
                    log.Info("Skip MailAutoreply: already sent to this address (history)");
                    return;
                }

                if (account.MailAutoreply.OnlyContacts && !apiHelper.SearchEmails(message.FromEmail).Any())
                {
                    log.Info("Skip MailAutoreply: message From address is not a part of user's contacts");
                    return;
                }

                apiHelper.SendMessage(CreateAutoreply(account, message, autoreplyEmail), true);
                account.MailAutoreplyHistory.Add(message.FromEmail);

                log.InfoFormat("AutoreplyEngine->SendAutoreply: auto-reply message has been sent to '{0}' email", autoreplyEmail);
            }
            catch (Exception ex)
            {
                log.ErrorFormat(
                    "AutoreplyEngine->SendAutoreply Error: {0}, innerException: {1}, account.MailBoxId = {2}, " +
                    "account.UserId = {3}, account.TenantId = {4}",
                    ex, ex.InnerException != null ? ex.InnerException.ToString() : string.Empty,
                    account.MailBoxId, account.UserId, account.TenantId);
            }
        }

        public void SaveAutoreplyHistory(MailBoxData account, MailMessage messageItem)
        {
            using (var daoFactory = new DaoFactory())
            {
                var autoReplyHistory = new MailboxAutoreplyHistory
                {
                    MailboxId = account.MailBoxId,
                    SendingDate = DateTime.UtcNow,
                    SendingEmail = new MailAddress(messageItem.To).Address,
                    Tenant = account.TenantId
                };

                daoFactory.CreateMailboxAutoreplyHistoryDao(account.TenantId, account.UserId)
                    .SaveAutoreplyHistory(autoReplyHistory);
            }
        }

        #region .Private

        private static MailMessage CreateAutoreply(MailBoxData account, MailMessage messageItem, string autoreplyEmail)
        {
            var mailMessage = new MailMessage();
            var stringBuilder = new StringBuilder(account.MailAutoreply.Subject);

            if (!string.IsNullOrEmpty(account.MailAutoreply.Subject))
                stringBuilder.Append(" ");

            mailMessage.Subject = stringBuilder.AppendFormat("Re: {0}", messageItem.Subject).ToString();
            mailMessage.HtmlBody = account.MailAutoreply.Html;
            mailMessage.MimeReplyToId = messageItem.MimeMessageId;
            mailMessage.To = messageItem.From;
            mailMessage.From = autoreplyEmail ?? account.EMail.ToString();

            if (account.MailSignature == null)
            {
                using (var daoFactory = new DaoFactory())
                {
                    var signature = daoFactory.CreateMailboxSignatureDao(account.TenantId, account.UserId).GetSignature(account.MailBoxId);

                    if (signature != null)
                    {
                        account.MailSignature = new MailSignatureData(signature.MailboxId, signature.Tenant, signature.Html,
                            signature.IsActive);
                    }
                    else
                    {
                        account.MailSignature = new MailSignatureData(account.MailBoxId, account.TenantId, "", false);
                    }
                }
            }

            if (account.MailSignature != null && account.MailSignature.IsActive)
            {
                mailMessage.HtmlBody = new StringBuilder(mailMessage.HtmlBody).AppendFormat(
                    @"<div class='tlmail_signature' mailbox_id='{0}'
                         style='font-family:open sans,sans-serif; font-size:12px; margin:0px;'>
                         <div>{1}</div>
                      </div>",
                    account.MailBoxId, account.MailSignature.Html).ToString();
            }
            return mailMessage;
        }

        private bool HasMailboxAutoreplyHistory(MailBoxData account, string email)
        {
            if (account.MailAutoreplyHistory == null)
                account.MailAutoreplyHistory = new List<string>();

            if (account.MailAutoreplyHistory.Contains(email))
                return true;

            List<string> emails;

            using (var daoFactory = new DaoFactory())
            {
                var autoreplyHistoryDao = daoFactory.CreateMailboxAutoreplyHistoryDao(account.TenantId, account.UserId);

                emails = autoreplyHistoryDao.GetAutoreplyHistorySentEmails(account.MailBoxId, email,
                    AutoreplyDaysInterval);
            }

            if (!emails.Any()) 
                return false;

            account.MailAutoreplyHistory.Add(email);
            return true;
        }

        private static bool HasGroupsInTo(MailBoxData account, MailMessage messageItem)
        {
            if (!account.IsTeamlab)
                return false;

            if (account.Groups == null)
            {
                var engine = new EngineFactory(account.TenantId, account.UserId);
                account.Groups = engine.ServerEngine.GetGroups(account.MailBoxId);
            }

            foreach (var group in account.Groups)
            {
                if (messageItem.ToList.Any(
                    address =>
                        string.Equals(address.Address, group.Email, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }

                if (messageItem.CcList.Any(
                    address =>
                        string.Equals(address.Address, group.Email, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetAutoreplyEmailInTo(MailBoxData account, MailMessage messageItem)
        {
            var autoreplyAddress = GetAddressInList(account, messageItem.ToList) ??
                                   GetAddressInList(account, messageItem.CcList);
            return autoreplyAddress;
        }

        private static string GetAddressInList(MailBoxData account, List<MailAddress> list)
        {
            if (list.Any(address =>
                    string.Equals(address.Address, account.EMail.Address, StringComparison.InvariantCultureIgnoreCase)))
            {
                return account.EMail.ToString();
            }

            if (!account.IsTeamlab)
                return null;

            if (account.Aliases == null)
            {
                var engine = new EngineFactory(account.TenantId, account.UserId);
                account.Aliases = engine.ServerEngine.GetAliases(account.MailBoxId);
            }

            var result = (from address in list
                from alias in account.Aliases
                where string.Equals(address.Address, alias.Email, StringComparison.InvariantCultureIgnoreCase)
                select alias.Email)
                .FirstOrDefault();

            return result;
        }

        private static bool IsCurrentMailboxInFrom(MailBoxData account, MailMessage messageItem)
        {
            if (string.Equals(messageItem.FromEmail, account.EMail.Address, StringComparison.InvariantCultureIgnoreCase))
                return true;

            if (!account.IsTeamlab)
                return false;

            if (account.Aliases == null)
            {
                var engine = new EngineFactory(account.TenantId, account.UserId);
                account.Aliases = engine.ServerEngine.GetAliases(account.MailBoxId);
            }

            return
                account.Aliases.Any(
                    alias =>
                        string.Equals(messageItem.FromEmail, alias.Email, StringComparison.InvariantCultureIgnoreCase));
        }

        #endregion

    }
}
