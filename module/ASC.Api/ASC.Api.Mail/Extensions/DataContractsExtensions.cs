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


using System.Collections.Generic;
using System.Linq;
using ASC.Api.Mail.DataContracts;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Dal;
using ASC.Web.Core.Utility.Settings;
using ASC.Core;

namespace ASC.Api.Mail.Extensions
{
    public static class DataContractsExtensions
    {

        public static List<MailAccountData> ToAddressData(this AccountInfo account)
        {
            var fromEmailList = new List<MailAccountData>();
            var mailBoxAccountSettings =
                SettingsManager.Instance.LoadSettingsFor<MailBoxAccountSettings>(SecurityContext.CurrentAccount.ID);

            var emailData = new MailAccountData
            {
                MailboxId = account.Id,
                Email = account.Email,
                Name = account.Name,
                Enabled = account.Enabled,
                OAuthConnection = account.OAuthConnection,
                AuthError = account.AuthError,
                QuotaError = account.QuotaError,
                Signature = account.Signature,
                EMailInFolder = account.EMailInFolder,
                IsAlias = false,
                IsGroup = false,
                IsTeamlabMailbox = account.IsTeamlabMailbox,
                IsDefault = mailBoxAccountSettings.DefaultEmail == account.Email,
                IsSharedDomainMailbox = account.IsSharedDomainMailbox
            };
            fromEmailList.Add(emailData);

            foreach (var alias in account.Aliases)
            {
                emailData = new MailAccountData
                {
                    MailboxId = account.Id,
                    Email = alias.Email,
                    Name = account.Name,
                    Enabled = account.Enabled,
                    OAuthConnection = false,
                    AuthError = account.AuthError,
                    QuotaError = account.QuotaError,
                    Signature = account.Signature,
                    EMailInFolder = account.EMailInFolder,
                    IsAlias = true,
                    IsGroup = false,
                    IsTeamlabMailbox = account.IsTeamlabMailbox,
                    IsDefault = mailBoxAccountSettings.DefaultEmail == alias.Email
                };
                fromEmailList.Add(emailData);
            }

            foreach (var group in account.Groups)
            {
                if (fromEmailList.FindIndex(e => e.Email.Equals(group.Email)) != -1) continue;
                emailData = new MailAccountData
                {
                    MailboxId = account.Id,
                    Email = group.Email,
                    Name = "",
                    Enabled = true,
                    OAuthConnection = false,
                    AuthError = false,
                    QuotaError = false,
                    Signature = new SignatureDto(-1, account.Signature.Tenant, "", false),
                    EMailInFolder = "",
                    IsAlias = false,
                    IsGroup = true,
                    IsTeamlabMailbox = true
                };
                fromEmailList.Add(emailData);
            }

            return fromEmailList;
        }

        public static List<MailAccountData> ToAddressData(this List<AccountInfo> accounts)
        {
            var fromEmailList = new List<MailAccountData>();

            foreach (var account in accounts)
            {
                fromEmailList = fromEmailList.Concat(account.ToAddressData()).ToList();
            }

            return fromEmailList;
        }

        public static void GetNeededAccounts(this List<MailAccountData> accounts, out MailAccountData defaultAccount,
            out List<MailAccountData> commonAccounts, out List<MailAccountData> serverAccounts,
            out List<MailAccountData> aliases, out List<MailAccountData> groups)
        {
            defaultAccount = null;
            commonAccounts = new List<MailAccountData>();
            serverAccounts = new List<MailAccountData>();
            aliases = new List<MailAccountData>();
            groups = new List<MailAccountData>();

            if (accounts == null)
            {
                return;
            }

            for (int i = 0; i < accounts.Count; i++)
            {
                if (accounts[i].IsDefault)
                {
                    defaultAccount = accounts[i];
                }
                else if (accounts[i].IsGroup)
                {
                    groups.Add(accounts[i]);
                }
                else if (accounts[i].IsAlias)
                {
                    aliases.Add(accounts[i]);
                }
                else if (accounts[i].IsTeamlabMailbox)
                {
                    serverAccounts.Add(accounts[i]);
                }
                else
                {
                    commonAccounts.Add(accounts[i]);
                }
            }
        }
    }
}
