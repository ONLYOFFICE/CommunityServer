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
using System.Linq;
using System.Net.Mail;
using System.Threading;
using ASC.Common.Logging;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Mail.Authorization;
using ASC.Mail.Clients;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Utils;

namespace ASC.Mail.Core.Engine
{
    public class AccountEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public ILog Log { get; private set; }

        private MailboxEngine MailboxEngine { get; set; }

        public AccountEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.AccountEngine");

            var engine = new EngineFactory(tenant, user);

            MailboxEngine = engine.MailboxEngine;
        }

        public List<AccountInfo> GetAccountInfoList()
        {
            var accountInfoList = CacheEngine.Get(User);
            if (accountInfoList != null)
                return accountInfoList;

            List<Account> accounts;
            List<MailSignatureData> signatures;
            List<MailAutoreplyData> autoreplies;

            using (var daoFactory = new DaoFactory())
            {
                accounts = daoFactory.CreateAccountDao(Tenant, User)
                    .GetAccounts();

                var mailboxIds = accounts.Select(a => a.MailboxId).ToList();

                signatures = daoFactory.CreateMailboxSignatureDao(Tenant, User)
                    .GetSignatures(mailboxIds)
                    .ConvertAll(s => new MailSignatureData(s.MailboxId, s.Tenant, s.Html, s.IsActive))
                    .ToList();

                autoreplies = daoFactory.CreateMailboxAutoreplyDao(Tenant, User)
                    .GetAutoreplies(mailboxIds)
                    .ConvertAll(
                        r =>
                            new MailAutoreplyData(r.MailboxId, r.Tenant, r.TurnOn, r.OnlyContacts, r.TurnOnToDate,
                                r.FromDate, r.ToDate, r.Subject, r.Html))
                    .ToList();
            }

            accountInfoList = ToAccountInfoList(accounts, signatures, autoreplies);

            CacheEngine.Set(User, accountInfoList);

            return accountInfoList;
        }

        public AccountInfo CreateAccount(MailBoxData mbox, out LoginResult loginResult)
        {
            if (mbox == null)
                throw new NullReferenceException("mbox");

            using (var client = new MailClient(mbox, CancellationToken.None,
                    certificatePermit: Defines.SslCertificatesErrorPermit, log: Log))
            {
                loginResult = client.TestLogin();
            }

            if (!loginResult.IngoingSuccess || !loginResult.OutgoingSuccess)
                return null;

            if (!MailboxEngine.SaveMailBox(mbox))
                throw new Exception(string.Format("SaveMailBox {0} failed", mbox.EMail));

            CacheEngine.Clear(User);

            var account = new AccountInfo(mbox.MailBoxId, mbox.EMailView, mbox.Name, mbox.Enabled, mbox.QuotaError,
                MailBoxData.AuthProblemType.NoProblems, new MailSignatureData(mbox.MailBoxId, Tenant, "", false),
                new MailAutoreplyData(mbox.MailBoxId, Tenant, false, false, false, DateTime.MinValue,
                    DateTime.MinValue, string.Empty, string.Empty), false, mbox.EMailInFolder, false, false);

            return account;
        }

        public AccountInfo CreateAccountSimple(string email, string password, out List<LoginResult> loginResults)
        {
            MailBoxData mbox = null;

            var domain = email.Substring(email.IndexOf('@') + 1);

            var engine = new EngineFactory(Tenant, User);

            var mailboxSettings = engine.MailBoxSettingEngine.GetMailBoxSettings(domain);

            if (mailboxSettings == null)
            {
                throw new Exception("Unknown mail provider settings.");
            }

            var testMailboxes = mailboxSettings.ToMailboxList(email, password, Tenant, User);

            loginResults = new List<LoginResult>();

            foreach (var mb in testMailboxes)
            {
                LoginResult loginResult;

                using (var client = new MailClient(mb, CancellationToken.None, Defines.TcpTimeout,
                        Defines.SslCertificatesErrorPermit, log: Log))
                {
                    loginResult = client.TestLogin();
                }

                loginResults.Add(loginResult);

                if (!loginResult.IngoingSuccess || !loginResult.OutgoingSuccess)
                    continue;

                mbox = mb;
                break;
            }

            if (mbox == null)
                return null;

            if (!MailboxEngine.SaveMailBox(mbox))
                throw new Exception(string.Format("SaveMailBox {0} failed", email));

            CacheEngine.Clear(User);

            var account = new AccountInfo(mbox.MailBoxId, mbox.EMailView, mbox.Name, mbox.Enabled, mbox.QuotaError,
                MailBoxData.AuthProblemType.NoProblems, new MailSignatureData(mbox.MailBoxId, Tenant, "", false),
                new MailAutoreplyData(mbox.MailBoxId, Tenant, false, false, false, DateTime.MinValue,
                    DateTime.MinValue, string.Empty, string.Empty), false, mbox.EMailInFolder, false, false);

            return account;
        }

        public AccountInfo CreateAccountOAuth(string code, byte type)
        {
            var oAuthToken = OAuth20TokenHelper.GetAccessToken<GoogleLoginProvider>(code);

            if (oAuthToken == null)
                throw new Exception(@"Empty oauth token");

            var loginProfile = GoogleLoginProvider.Instance.GetLoginProfile(oAuthToken.AccessToken);
            var email = loginProfile.EMail;

            if (string.IsNullOrEmpty(email))
                throw new Exception(@"Empty email");

            var beginDate = DateTime.UtcNow.Subtract(new TimeSpan(MailBoxData.DefaultMailLimitedTimeDelta));

            var mboxImap = MailboxEngine.GetDefaultMailboxData(email, "", (AuthorizationServiceType)type,
                true, false);

            mboxImap.OAuthToken = oAuthToken.ToJson();
            mboxImap.BeginDate = beginDate; // Apply restrict for download

            if (!MailboxEngine.SaveMailBox(mboxImap, (AuthorizationServiceType)type))
                throw new Exception(string.Format("SaveMailBox {0} failed", email));

            CacheEngine.Clear(User);

            if (Defines.IsSignalRAvailable)
            {
                var engine = new EngineFactory(Tenant, User);
                engine.AccountEngine.SetAccountsActivity();
            }

            var account = new AccountInfo(mboxImap.MailBoxId, mboxImap.EMailView, mboxImap.Name, mboxImap.Enabled,
                mboxImap.QuotaError,
                MailBoxData.AuthProblemType.NoProblems, new MailSignatureData(mboxImap.MailBoxId, Tenant, "", false),
                new MailAutoreplyData(mboxImap.MailBoxId, Tenant, false, false, false, DateTime.MinValue,
                    DateTime.MinValue, string.Empty, string.Empty), true, mboxImap.EMailInFolder, false, false);

            return account;
        }

        public AccountInfo UpdateAccount(MailBoxData newMailBoxData, out LoginResult loginResult)
        {
            if (newMailBoxData == null)
                throw new NullReferenceException("mbox");

            Mailbox mbox;

            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                mbox =
                    daoMailbox.GetMailBox(
                        new СoncreteUserMailboxExp(
                            newMailBoxData.EMail,
                            Tenant, User));

                if (null == mbox)
                    throw new ArgumentException("Mailbox with specified email doesn't exist.");

                if (mbox.IsTeamlabMailbox)
                    throw new ArgumentException("Mailbox with specified email can't be updated");

                if (!string.IsNullOrEmpty(mbox.OAuthToken))
                {
                    var needSave = false;

                    if (!mbox.Name.Equals(newMailBoxData.Name))
                    {
                        mbox.Name = newMailBoxData.Name;
                        needSave = true;
                    }

                    if (!mbox.BeginDate.Equals(newMailBoxData.BeginDate))
                    {
                        mbox.BeginDate = newMailBoxData.BeginDate;
                        mbox.ImapIntervals = null;
                        needSave = true;
                    }

                    if (needSave)
                    {
                        daoMailbox.SaveMailBox(mbox);

                        CacheEngine.Clear(User);
                    }

                    var accountInfo = new AccountInfo(mbox.Id, mbox.Address, mbox.Name, mbox.Enabled, mbox.QuotaError,
                        MailBoxData.AuthProblemType.NoProblems, new MailSignatureData(mbox.Id, Tenant, "", false),
                        new MailAutoreplyData(mbox.Id, Tenant, false, false, false, DateTime.MinValue,
                            DateTime.MinValue, string.Empty, string.Empty), false, mbox.EmailInFolder, false, false);

                    loginResult = new LoginResult
                    {
                        Imap = mbox.Imap,
                        IngoingSuccess = true,
                        OutgoingSuccess = true
                    };

                    return accountInfo;
                }
            }

            newMailBoxData.Password = string.IsNullOrEmpty(newMailBoxData.Password)
                ? mbox.Password
                : newMailBoxData.Password;

            newMailBoxData.SmtpPassword = string.IsNullOrEmpty(newMailBoxData.SmtpPassword)
                ? mbox.SmtpPassword
                : newMailBoxData.SmtpPassword;

            newMailBoxData.Imap = mbox.Imap;

            return CreateAccount(newMailBoxData, out loginResult);
        }

        public AccountInfo UpdateAccountOAuth(int mailboxId, string code, byte type)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentException(@"Empty OAuth code", "code");

            var oAuthToken = OAuth20TokenHelper.GetAccessToken<GoogleLoginProvider>(code);

            if (oAuthToken == null)
                throw new Exception(@"Empty OAuth token");

            if (string.IsNullOrEmpty(oAuthToken.AccessToken))
                throw new Exception(@"Empty OAuth AccessToken");

            if (string.IsNullOrEmpty(oAuthToken.RefreshToken))
                throw new Exception(@"Empty OAuth RefreshToken");

            if (oAuthToken.IsExpired)
                throw new Exception(@"OAuth token is expired");

            var loginProfile = GoogleLoginProvider.Instance.GetLoginProfile(oAuthToken.AccessToken);
            var email = loginProfile.EMail;

            if (string.IsNullOrEmpty(email))
                throw new Exception(@"Empty email");

            Mailbox mbox;

            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                mbox = daoMailbox.GetMailBox(
                    new СoncreteUserMailboxExp(
                        mailboxId,
                        Tenant, User));

                if (null == mbox)
                    throw new ArgumentException("Mailbox with specified email doesn't exist.");

                if (mbox.IsTeamlabMailbox || string.IsNullOrEmpty(mbox.OAuthToken))
                    throw new ArgumentException("Mailbox with specified email can't be updated");

                if (!mbox.Address.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                    throw new ArgumentException("Mailbox with specified email can't be updated");

                mbox.OAuthToken = oAuthToken.ToJson();

                var result = daoMailbox.SaveMailBox(mbox);

                mbox.Id = result;
            }

            CacheEngine.Clear(User);

            if (Defines.IsSignalRAvailable)
            {
                var engine = new EngineFactory(Tenant, User);
                engine.AccountEngine.SetAccountsActivity();
            }

            var accountInfo = new AccountInfo(mbox.Id, mbox.Address, mbox.Name, mbox.Enabled, mbox.QuotaError,
                MailBoxData.AuthProblemType.NoProblems, new MailSignatureData(mbox.Id, Tenant, "", false),
                new MailAutoreplyData(mbox.Id, Tenant, false, false, false, DateTime.MinValue,
                    DateTime.MinValue, string.Empty, string.Empty), true, mbox.EmailInFolder, false, false);

            return accountInfo;
        }

        public int SetAccountEnable(MailAddress address, bool enabled, out LoginResult loginResult)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            var engine = new EngineFactory(Tenant);

            var tuple = engine.MailboxEngine.GetMailboxFullInfo(new СoncreteUserMailboxExp(address, Tenant, User));

            if (tuple == null)
                throw new NullReferenceException(string.Format("Account wasn't found by email: {0}", address.Address));

            if (enabled)
            {
                // Check account connection setting on activation
                using (var client = new MailClient(tuple.Item1, CancellationToken.None,
                        certificatePermit: Defines.SslCertificatesErrorPermit, log: Log))
                {
                    loginResult = client.TestLogin();
                }

                if (!loginResult.IngoingSuccess || !loginResult.OutgoingSuccess)
                {
                    return -1;
                }
            }

            int mailboxId;

            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                loginResult = null;
                mailboxId =
                    daoMailbox.Enable(new СoncreteUserMailboxExp(tuple.Item2.Id, tuple.Item2.Tenant, tuple.Item2.User),
                        enabled)
                        ? tuple.Item2.Id
                        : -1;
            }

            if (mailboxId == -1)
                return mailboxId;

            CacheEngine.Clear(User);

            return mailboxId;
        }

        public bool SetAccountEmailInFolder(int mailboxId, string emailInFolder)
        {
            if (mailboxId < 0)
                throw new ArgumentNullException("mailboxId");

            bool saved;

            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                var mailbox = daoMailbox.GetMailBox(
                    new СoncreteUserMailboxExp(
                        mailboxId,
                        Tenant, User)
                    );

                if (mailbox == null)
                    return false;

                saved = daoMailbox.SetMailboxEmailIn(mailbox, emailInFolder);
            }

            if (!saved)
                return saved;

            CacheEngine.Clear(User);

            return saved;
        }

        public bool SetAccountsActivity(bool userOnline = true)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                return daoMailbox.SetMailboxesActivity(Tenant, User, userOnline);
            }
        }

        public List<string> SearchAccountEmails(string searchText)
        {
            var accounts = GetAccountInfoList();
            var emails = new List<string>();

            foreach (var account in accounts)
            {
                var email = string.IsNullOrEmpty(account.Name)
                                ? account.Email
                                : MailUtil.CreateFullEmail(account.Name, account.Email);
                emails.Add(email);

                foreach (var alias in account.Aliases)
                {
                    email = string.IsNullOrEmpty(account.Name)
                                ? account.Email
                                : MailUtil.CreateFullEmail(account.Name, alias.Email);
                    emails.Add(email);
                }

                foreach (var group in account.Groups.Where(group => emails.IndexOf(group.Email) == -1))
                {
                    emails.Add(group.Email);
                }
            }

            return emails.Where(e => e.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) > -1).ToList();
        }

        private static List<AccountInfo> ToAccountInfoList(IEnumerable<Account> accounts,
            IReadOnlyCollection<MailSignatureData> signatures, IReadOnlyCollection<MailAutoreplyData> autoreplies)
        {
            var accountInfoList = new List<AccountInfo>();

            foreach (var account in accounts)
            {
                var mailboxId = account.MailboxId;
                var accountIndex = accountInfoList.FindIndex(a => a.Id == mailboxId);

                var signature = signatures.First(s => s.MailboxId == mailboxId);
                var autoreply = autoreplies.First(s => s.MailboxId == mailboxId);
                var isAlias = account.ServerAddressIsAlias;

                if (!isAlias)
                {
                    var groupAddress = account.ServerMailGroupAddress;
                    MailAddressInfo group = null;

                    if (!string.IsNullOrEmpty(groupAddress))
                    {
                        group = new MailAddressInfo(account.ServerMailGroupId,
                            groupAddress,
                            account.ServerDomainId);
                    }

                    if (accountIndex == -1)
                    {
                        var authErrorType = MailBoxData.AuthProblemType.NoProblems;

                        if (account.MailboxDateAuthError.HasValue)
                        {
                            var authErrorDate = account.MailboxDateAuthError.Value;

                            if (DateTime.UtcNow - authErrorDate > Defines.AuthErrorDisableTimeout)
                                authErrorType = MailBoxData.AuthProblemType.TooManyErrors;
                            else if (DateTime.UtcNow - authErrorDate > Defines.AuthErrorWarningTimeout)
                                authErrorType = MailBoxData.AuthProblemType.ConnectError;
                        }

                        var accountInfo = new AccountInfo(
                            mailboxId,
                            account.MailboxAddress,
                            account.MailboxAddressName,
                            account.MailboxEnabled,
                            account.MailboxQuotaError,
                            authErrorType, signature, autoreply,
                            !string.IsNullOrEmpty(account.MailboxOAuthToken),
                            account.MailboxEmailInFolder,
                            account.MailboxIsTeamlabMailbox,
                            account.ServerDomainTenant == Defines.SHARED_TENANT_ID);

                        if (group != null) accountInfo.Groups.Add(group);

                        accountInfoList.Add(accountInfo);
                    }
                    else if (group != null)
                    {
                        accountInfoList[accountIndex].Groups.Add(group);
                    }
                }
                else
                {
                    var alias = new MailAddressInfo(account.ServerAddressId,
                        string.Format("{0}@{1}", account.ServerAddressName, account.ServerDomainName),
                        account.ServerDomainId);

                    accountInfoList[accountIndex].Aliases.Add(alias);
                }
            }

            return accountInfoList;
        }
    }
}
