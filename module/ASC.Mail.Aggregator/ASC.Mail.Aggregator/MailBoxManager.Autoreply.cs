/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Dal;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Common.Utils;
using MailMessage = ASC.Mail.Aggregator.Common.MailMessage;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        private readonly string[] _listHeaderNames =
        {
            "List-Subscribe",
            "List-Unsubscribe",
            "List-Help",
            "List-Post",
            "List-Owner",
            "List-Archive",
            "List-ID"
        };

        private readonly string[] _listFromTextToSkip =
        {
            "noreply",
            "no-reply",
            "mailer-daemon",
            "mail-daemon",
            "notify",
            "postmaster",
            "postman"
        };

        public int AutoreplyDaysInterval
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["mail.autoreply-days-interval"] ?? "4"); }
        }

        public MailAutoreply UpdateOrCreateMailboxAutoreply(int mailboxId, string user, int tenant,
            bool turnOn, bool onlyContacts, bool turnOnToDate, DateTime fromDate, DateTime toDate, string subject,
            string html)
        {
            using (var db = GetDb())
            {
                CheckMailboxOwnage(mailboxId, user, tenant, db);

                var autoreply = new MailAutoreply(mailboxId, tenant, turnOn, onlyContacts, turnOnToDate, fromDate,
                    toDate, subject, html);
                var autoreplyDal = new AutoreplyDal(db);
                autoreplyDal.UpdateOrCreateAutoreply(mailboxId, tenant, autoreply);
                return autoreply;
            }
        }

        public void SendAutoreply(MailBox account, MailMessage messageItem, string httpContextScheme, ILogger log)
        {
            try
            {
                if (messageItem.Folder != MailFolder.Ids.inbox || account.MailAutoreply == null ||
                    !account.MailAutoreply.TurnOn)
                {
                    return;
                }

                var utcNow = DateTime.UtcNow.Date;

                if (account.MailAutoreply.TurnOnToDate &&
                    account.MailAutoreply.ToDate != DateTime.MinValue &&
                    utcNow > account.MailAutoreply.ToDate)
                {
                    account.MailAutoreply.TurnOn = false;
                    UpdateOrCreateMailboxAutoreply(account.MailBoxId, account.TenantId, account.MailAutoreply);
                    return;
                }

                if (account.MailAutoreply.FromDate <= utcNow &&
                    account.MailAutoreply.FromDate <= messageItem.Date &&
                    !IsMessageMassSending(messageItem) &&
                    !IsCurrentMailboxInFrom(account, messageItem))
                {
                    var apiHelper = new ApiHelper(httpContextScheme);

                    var autoreplyEmail = GetAutoreplyEmailInTo(account, messageItem);
                    if ((autoreplyEmail != null || !HasGroupsInTo(account, messageItem)) &&
                        !HasMailboxAutoreplyHistory(account, messageItem.FromEmail) &&
                        (!account.MailAutoreply.OnlyContacts || apiHelper.SearchEmails(messageItem.FromEmail).Count != 0))
                    {
                        apiHelper.SendMessage(CreateAutoreply(account, messageItem, autoreplyEmail), true);
                        account.MailAutoreplyHistory.Add(messageItem.FromEmail);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(
                    "SendAutoreply Error: {0}, innerException: {1}, account.MailBoxId = {2}, " +
                    "account.UserId = {3}, account.TenantId = {4}",
                    ex, ex.InnerException != null ? ex.InnerException.ToString() : String.Empty,
                    account.MailBoxId, account.UserId, account.TenantId);
            }
        }

        public void SaveAutoreplyHistory(MailBox account, MailMessage messageItem)
        {
            using (var db = GetDb())
            {
                var autoreplyDal = new AutoreplyDal(db);
                autoreplyDal.UpdateOrCreateAutoreplyHistory(account.MailBoxId, account.TenantId,
                    new MailAddress(messageItem.To).Address);
            }
        }

        private void UpdateOrCreateMailboxAutoreply(int mailBoxId, int tenantId, MailAutoreply autoreply)
        {
            using (var db = GetDb())
            {
                var autoreplyDal = new AutoreplyDal(db);
                autoreplyDal.UpdateOrCreateAutoreply(mailBoxId, tenantId, autoreply);
            }
        }

        private bool IsMessageMassSending(MailMessage message)
        {
            var isMassSending =
                (message.HeaderFieldNames != null &&
                 _listHeaderNames.Any(h =>
                     message.HeaderFieldNames.AllKeys.FirstOrDefault(
                         k => k.Equals(h, StringComparison.OrdinalIgnoreCase)) != null)) ||
                (!string.IsNullOrEmpty(message.From) &&
                 _listFromTextToSkip.Any(f =>
                     message.From.IndexOf(f, StringComparison.OrdinalIgnoreCase) != -1)) ||
                message.HtmlBodyStream.Length > 0
                    ? MailUtil.HasUnsubscribeLink(message.HtmlBodyStream)
                    : MailUtil.HasUnsubscribeLink(message.HtmlBody);

            return isMassSending;
        }

        private bool HasMailboxAutoreplyHistory(MailBox account, string email)
        {
            if (account.MailAutoreplyHistory != null)
                return account.MailAutoreplyHistory.Contains(email);

            using (var db = GetDb())
            {
                var autoreplyDal = new AutoreplyDal(db);
                account.MailAutoreplyHistory = autoreplyDal.GetAutoreplyHistory(account.MailBoxId, email,
                    AutoreplyDaysInterval);
            }
            return account.MailAutoreplyHistory.Contains(email);
        }

        private MailMessage CreateAutoreply(MailBox account, MailMessage messageItem, string autoreplyEmail)
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
                account.MailSignature = GetMailboxSignature(account.MailBoxId, account.UserId, account.TenantId);

            if (account.MailSignature.IsActive)
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

        private bool HasGroupsInTo(MailBox account, MailMessage messageItem)
        {
            if (!account.IsTeamlab)
                return false;

            if (account.Groups == null)
                account.Groups = GetGroups(account.MailBoxId);

            foreach (var group in account.Groups)
            {
                if (messageItem.ToList.Any(
                    address =>
                        string.Equals(address.Address, @group.Email, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }

                if (messageItem.CcList.Any(
                    address =>
                        string.Equals(address.Address, @group.Email, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }

        private string GetAutoreplyEmailInTo(MailBox account, MailMessage messageItem)
        {
            var autoreplyAddress = GetAddressInList(account, messageItem.ToList) ??
                                   GetAddressInList(account, messageItem.CcList);
            return autoreplyAddress;
        }

        private string GetAddressInList(MailBox account, List<MailAddress> list)
        {
            if (list.Any(
                address =>
                    string.Equals(address.Address, account.EMail.Address, StringComparison.InvariantCultureIgnoreCase)))
            {
                return account.EMail.ToString();
            }

            if (!account.IsTeamlab)
                return null;

            if (account.Aliases == null)
                account.Aliases = GetAliases(account.MailBoxId);

            return (from address in list
                from alias in account.Aliases
                where string.Equals(address.Address, alias.Email, StringComparison.InvariantCultureIgnoreCase)
                select alias.Email).FirstOrDefault();
        }

        private bool IsCurrentMailboxInFrom(MailBox account, MailMessage messageItem)
        {
            if (string.Equals(messageItem.FromEmail, account.EMail.Address, StringComparison.InvariantCultureIgnoreCase))
                return true;

            if (!account.IsTeamlab)
                return false;

            if (account.Aliases == null)
            {
                account.Aliases = GetAliases(account.MailBoxId);
            }
            return
                account.Aliases.Any(
                    alias =>
                        string.Equals(messageItem.FromEmail, alias.Email, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
