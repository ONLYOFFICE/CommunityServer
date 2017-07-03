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
using System.Net.Mail;
using System.Security.Authentication;
using System.Threading;
using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Api.Mail.DataContracts;
using ASC.Api.Mail.Extensions;
using ASC.Api.Mail.Resources;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Authorization;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Mail.Aggregator.ComplexOperations.Base;
using ASC.Mail.Aggregator.Core.Clients;
using EncryptionType = ASC.Mail.Aggregator.Common.EncryptionType;
using SaslMechanism = ASC.Mail.Aggregator.Common.SaslMechanism;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        ///    Returns lists of all mailboxes, aliases and groups for user.
        /// </summary>
        /// <param name="username" visible="false">User id</param>
        /// <returns>Mailboxes, aliases and groups list</returns>
        /// <short>Get mailboxes, aliases and groups list</short> 
        /// <category>Accounts</category>
        [Read(@"accounts")]
        public IEnumerable<MailAccountData> GetAccounts(string username = "")
        {
            var userId = string.IsNullOrEmpty(username) ? Username : username;
            var accounts = MailBoxManager.CachedAccounts.Get(userId);
            if (accounts == null)
            {
                accounts = MailBoxManager.GetAccountInfo(TenantId, userId);
                MailBoxManager.CachedAccounts.Set(userId, accounts);
            }
            return accounts.ToAddressData();
        }

        /// <summary>
        ///    Creates an account based on email and password.
        /// </summary>
        /// <param name="email">Account email in string format like: name@domain</param>
        /// <param name="password">Password as plain text.</param>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="Exception">Exception contains text description of happened error.</exception>
        /// <returns>Created account</returns>
        /// <short>Create new account by email and password</short> 
        /// <category>Accounts</category>
        [Create(@"accounts/simple")]
        public MailAccountData CreateAccountSimple(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException(@"Empty email", "email");

            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            string errorText = null;
            MailBox mbox = null;

            var domain = email.Substring(email.IndexOf('@') + 1);

            var mailboxSettings = MailBoxManager.GetMailBoxSettings(domain);

            if (mailboxSettings == null)
            {
                errorText = GetFormattedTextError(new ItemNotFoundException("Unknown mail provider settings."));
            }
            else
            {
                try
                {

                    var testMailboxes = mailboxSettings.ToMailboxList(email, password, TenantId, Username);

                    var results = new List<LoginResult>();

                    foreach (var mb in testMailboxes)
                    {
                        LoginResult loginResult;

                        using (var client = new MailClient(mb, CancellationToken.None, 5000, SslCertificatesErrorPermit, log: _log))
                        {
                            loginResult = client.TestLogin();
                        }

                        results.Add(loginResult);

                        if (loginResult.IngoingSuccess && loginResult.OutgoingSuccess)
                        {
                            mbox = mb;
                            break;
                        }
                    }

                    if (mbox == null)
                    {
                        var i = 0;

                        foreach (var loginResult in results)
                        {
                            errorText += string.Format("#{0}:<br>", ++i);

                            if (!loginResult.IngoingSuccess)
                            {
                                errorText += GetFormattedTextError(loginResult.IngoingException,
                                    loginResult.Imap ? MailServerType.Imap : MailServerType.Pop3, false) + "<br>";
                                    // exImap is ImapConnectionTimeoutException
                            }

                            if (!loginResult.OutgoingSuccess)
                            {
                                errorText += GetFormattedTextError(loginResult.OutgoingException, MailServerType.Smtp, false) + "<br>";
                                // exSmtp is SmtpConnectionTimeoutException);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    errorText = GetFormattedTextError(ex);
                }
            }

            if (!string.IsNullOrEmpty(errorText))
                throw new Exception(errorText);

            try
            {
                MailBoxManager.SaveMailBox(mbox);
                MailBoxManager.CachedAccounts.Clear(Username);

                if (IsSignalRAvailable)
                    MailBoxManager.UpdateUserActivity(TenantId, Username);

                var account = new AccountInfo(mbox.MailBoxId, mbox.EMailView, mbox.Name, mbox.Enabled, mbox.QuotaError,
                    MailBox.AuthProblemType.NoProblems, new MailSignature(mbox.MailBoxId, TenantId, "", false),
                    new MailAutoreply(mbox.MailBoxId, TenantId, false, false, false, DateTime.MinValue,
                        DateTime.MinValue, String.Empty, String.Empty), false, mbox.EMailInFolder, false, false);

                return account.ToAddressData().FirstOrDefault();

            }
            catch (Exception ex)
            {
                //TODO: change AttachmentsUnknownError to common unknown error text
                errorText = GetFormattedTextError(ex, MailApiResource.AttachmentsUnknownError);
            }

            throw new Exception(errorText);
        }

        /// <summary>
        ///    Creates Mail account with OAuth authentication. Only Google OAuth supported.
        /// </summary>
        /// <param name="code">Oauth code</param>
        /// <param name="type">Type of OAuth service. 0- Unknown, 1 - Google.</param>
        /// <exception cref="Exception">Exception contains text description of happened error.</exception>
        /// <returns>Created account</returns>
        /// <short>Create OAuth account</short> 
        /// <category>Accounts</category>
        [Create(@"accounts/oauth")]
        public MailAccountData CreateAccountOAuth(string code, byte type)
        {
            if (string.IsNullOrEmpty(code)) throw new ArgumentException(@"Empty oauth code", "code");

            var oAuthToken = OAuth20TokenHelper.GetAccessToken(GoogleLoginProvider.GoogleOauthTokenUrl,
                                                          GoogleLoginProvider.GoogleOAuth20ClientId,
                                                          GoogleLoginProvider.GoogleOAuth20ClientSecret,
                                                          GoogleLoginProvider.GoogleOAuth20RedirectUrl,
                                                          code);

            if (oAuthToken == null) throw new Exception(@"Empty oauth token");

            var loginProfile = new GoogleLoginProvider().GetLoginProfile(oAuthToken.AccessToken);
            var email = loginProfile.EMail;

            if (string.IsNullOrEmpty(email)) throw new Exception(@"Empty email");

            var beginDate = DateTime.UtcNow.Subtract(new TimeSpan(MailBox.DefaultMailLimitedTimeDelta));

            var mboxImap = MailBoxManager.ObtainMailboxSettings(TenantId, Username, email, "", (AuthorizationServiceType) type, true, false);
            mboxImap.OAuthToken = oAuthToken.ToJson();
            mboxImap.BeginDate = beginDate; // Apply restrict for download

            try
            {
                MailBoxManager.SaveMailBox(mboxImap, (AuthorizationServiceType)type);
                MailBoxManager.CachedAccounts.Clear(Username);

                if (IsSignalRAvailable)
                    MailBoxManager.UpdateUserActivity(TenantId, Username);

                var account = new AccountInfo(mboxImap.MailBoxId, mboxImap.EMailView, mboxImap.Name, mboxImap.Enabled, mboxImap.QuotaError,
                                               MailBox.AuthProblemType.NoProblems, new MailSignature(mboxImap.MailBoxId, TenantId, "", false),
                                               new MailAutoreply(mboxImap.MailBoxId, TenantId, false, false, false, DateTime.MinValue,
                                                   DateTime.MinValue, String.Empty, String.Empty), true, mboxImap.EMailInFolder, false, false);

                return account.ToAddressData().FirstOrDefault();
            }
            catch (Exception imapException)
            {
                throw new Exception(GetFormattedTextError(imapException, MailServerType.ImapOAuth, imapException is ImapConnectionTimeoutException));
            }
        }

        /// <summary>
        ///    Update Mail account with OAuth authentication. Only Google OAuth supported.
        /// </summary>
        /// <param name="code">Oauth code</param>
        /// <param name="type">Type of OAuth service. 0- Unknown, 1 - Google.</param>
        /// <param name="mailboxId">Mailbox ID to update</param>
        /// <exception cref="Exception">Exception contains text description of happened error.</exception>
        /// <returns>Updated OAuth account</returns>
        /// <short>Update OAuth account</short> 
        /// <category>Accounts</category>
        [Update(@"accounts/oauth")]
        public MailAccountData UpdateAccountOAuth(string code, byte type, int mailboxId)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentException(@"Empty oauth code", "code");

            var oAuthToken = OAuth20TokenHelper.GetAccessToken(GoogleLoginProvider.GoogleOauthTokenUrl,
                                                          GoogleLoginProvider.GoogleOAuth20ClientId,
                                                          GoogleLoginProvider.GoogleOAuth20ClientSecret,
                                                          GoogleLoginProvider.GoogleOAuth20RedirectUrl,
                                                          code);

            if (oAuthToken == null)
                throw new Exception(@"Empty oauth token");

            var loginProfile = new GoogleLoginProvider().GetLoginProfile(oAuthToken.AccessToken);
            var email = loginProfile.EMail;

            if (string.IsNullOrEmpty(email))
                throw new Exception(@"Empty email");

            try
            {
                var mbox = MailBoxManager.GetMailBox(mailboxId);

                if (null == mbox)
                    throw new ArgumentException("Mailbox with specified email doesn't exist.");

                if (mbox.IsTeamlab || !mbox.IsOAuth)
                    throw new ArgumentException("Mailbox with specified email can't be updated");

                if(!mbox.EMail.Address.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                    throw new ArgumentException("Mailbox with specified email can't be updated");

                mbox.OAuthToken = oAuthToken.ToJson();
                mbox.AccessTokenRefreshed = true;

                MailBoxManager.SaveMailBox(mbox, (AuthorizationServiceType)type);
                MailBoxManager.CachedAccounts.Clear(Username);

                if (IsSignalRAvailable)
                    MailBoxManager.UpdateUserActivity(TenantId, Username);

                var accountInfo = new AccountInfo(mbox.MailBoxId, mbox.EMailView, mbox.Name, mbox.Enabled, mbox.QuotaError,
                                               MailBox.AuthProblemType.NoProblems, new MailSignature(mbox.MailBoxId, TenantId, "", false),
                                               new MailAutoreply(mbox.MailBoxId, TenantId, false, false, false, DateTime.MinValue,
                                                   DateTime.MinValue, String.Empty, String.Empty), false, mbox.EMailInFolder, false, false);

                return accountInfo.ToAddressData().FirstOrDefault();
            }
            catch (Exception imapException)
            {
                throw new Exception(GetFormattedTextError(imapException, MailServerType.ImapOAuth, imapException is ImapConnectionTimeoutException));
            }
        }

        /// <summary>
        ///    Creates account using full information about mail servers.
        /// </summary>
        /// <param name="name">Account name in Teamlab</param>
        /// <param name="email">Account email in string format like: name@domain.</param>
        /// <param name="account">Login for imap or pop server.</param>
        /// <param name="password">Password for imap or pop server</param>
        /// <param name="port">Port for imap or pop server</param>
        /// <param name="server">Imap or pop server address or IP.</param>
        /// <param name="smtp_account">Login for smtp server</param>
        /// <param name="smtp_password">Password for smtp server</param>
        /// <param name="smtp_port">Smtp server port</param>
        /// <param name="smtp_server">Smtp server adress or IP.</param>
        /// <param name="smtp_auth">Flag is smtp server authentication needed. Value: true or false.</param>
        /// <param name="imap">Flag is imap server using for incoming mails. Value: true or false.</param>
        /// <param name="restrict">Flag is all mails needed for download. Value: true or false. If vslue true, it will be downloaded messages from last 30 days only.</param>
        /// <param name="incoming_encryption_type">Specify encription type for imap or pop server. 0- None, 1 - SSL, 2- StartTLS</param>
        /// <param name="outcoming_encryption_type">Specify encription type for smtp server. 0- None, 1 - SSL, 2- StartTLS</param>
        /// <param name="auth_type_in">Specify authentication type for imap or pop server. 1 - Login, 4 - CremdMd5, 5 - OAuth2</param>
        /// <param name="auth_type_smtp">Specify authentication type for imap or pop server. 0- None, 1 - Login, 4 - CremdMd5, 5 - OAuth2</param>
        /// <returns>Created account</returns>
        /// <exception cref="Exception">Exception contains text description of happened error.</exception>
        /// <short>Create account with custom mail servers.</short> 
        /// <category>Accounts</category>
        [Create(@"accounts")]
        public MailAccountData CreateAccount(string name,
            string email,
            string account,
            string password,
            int port,
            string server,
            string smtp_account,
            string smtp_password,
            int smtp_port,
            string smtp_server,
            bool smtp_auth,
            bool imap,
            bool restrict,
            EncryptionType incoming_encryption_type,
            EncryptionType outcoming_encryption_type,
            SaslMechanism auth_type_in,
            SaslMechanism auth_type_smtp)
        {
            string errorText = null;
            var mbox = new MailBox
                {
                    Name = name,
                    EMail = new MailAddress(email),
                    Account = account,
                    Password = password,
                    Port = port,
                    Server = server,
                    SmtpAccount = smtp_account,
                    SmtpPassword = smtp_password,
                    SmtpPort = smtp_port,
                    SmtpServer = smtp_server,
                    Imap = imap,
                    TenantId = TenantId,
                    UserId = Username,
                    BeginDate = restrict ? 
                        DateTime.Now.Subtract(new TimeSpan(MailBox.DefaultMailLimitedTimeDelta)) : 
                        new DateTime(MailBox.DefaultMailBeginTimestamp),
                    Encryption = incoming_encryption_type,
                    SmtpEncryption = outcoming_encryption_type,
                    Authentication = auth_type_in,
                    SmtpAuthentication = smtp_auth ? auth_type_smtp : SaslMechanism.None
        };

            LoginResult loginResult;

            using (var client = new MailClient(mbox, CancellationToken.None, 5000, SslCertificatesErrorPermit, log: _log))
            {
                loginResult = client.TestLogin();
            }

            if (!loginResult.IngoingSuccess)
            {
                errorText = GetFormattedTextError(loginResult.IngoingException,
                    mbox.Imap ? MailServerType.Imap : MailServerType.Pop3, false); // exImap is ImapConnectionTimeoutException
            }

            if (!loginResult.OutgoingSuccess)
            {
                if (!string.IsNullOrEmpty(errorText))
                    errorText += "\r\n";

                errorText += GetFormattedTextError(loginResult.OutgoingException, MailServerType.Smtp, false);
                    // exSmtp is SmtpConnectionTimeoutException);
            }

            if (!string.IsNullOrEmpty(errorText))
                throw new Exception(errorText);

            try
            {
                MailBoxManager.SaveMailBox(mbox);
                MailBoxManager.CachedAccounts.Clear(Username);

                if (IsSignalRAvailable)
                    MailBoxManager.UpdateUserActivity(TenantId, Username);

                var accountInfo = new AccountInfo(mbox.MailBoxId, mbox.EMailView, mbox.Name, mbox.Enabled, mbox.QuotaError,
                                               MailBox.AuthProblemType.NoProblems, new MailSignature(mbox.MailBoxId, TenantId, "", false),
                                               new MailAutoreply(mbox.MailBoxId, TenantId, false, false, false, DateTime.MinValue,
                                                   DateTime.MinValue, String.Empty, String.Empty), false, mbox.EMailInFolder, false, false);

                return accountInfo.ToAddressData().FirstOrDefault();
            }
            catch (Exception ex)
            {
                //TODO: change AttachmentsUnknownError to common unknown error text
                errorText = GetFormattedTextError(ex, MailApiResource.AttachmentsUnknownError);
            }

            throw new Exception(errorText);

        }

        /// <summary>
        ///    Updates the existing account.
        /// </summary>
        /// <param name="name">Account name in Teamlab</param>
        /// <param name="email">Account email in string format like: name@domain.</param>
        /// <param name="account">Login for imap or pop server.</param>
        /// <param name="password">Password for imap or pop server</param>
        /// <param name="port">Port for imap or pop server</param>
        /// <param name="server">Imap or pop server address or IP.</param>
        /// <param name="smtp_account">Login for smtp server</param>
        /// <param name="smtp_password">Password for smtp server</param>
        /// <param name="smtp_port">Smtp server port</param>
        /// <param name="smtp_server">Smtp server adress or IP.</param>
        /// <param name="smtp_auth">Flag is smtp server authentication needed. Value: true or false.</param>
        /// <param name="restrict">Flag is all mails needed for download. Value: true or false. If vslue true, it will be downloaded messages from last 30 days only.</param>
        /// <param name="incoming_encryption_type">Specify encription type for imap or pop server. 0- None, 1 - SSL, 2- StartTLS</param>
        /// <param name="outcoming_encryption_type">Specify encription type for smtp server. 0- None, 1 - SSL, 2- StartTLS</param>
        /// <param name="auth_type_in">Specify authentication type for imap or pop server. 1 - Login, 4 - CremdMd5, 5 - OAuth2</param>
        /// <param name="auth_type_smtp">Specify authentication type for imap or pop server. 0- None, 1 - Login, 4 - CremdMd5, 5 - OAuth2</param>
        /// <returns>Updated account</returns>
        /// <exception cref="Exception">Exception contains text description of happened error.</exception>
        /// <short>Update account</short> 
        /// <category>Accounts</category>
        [Update(@"accounts")]
        public MailAccountData UpdateAccount(string name,
            string email,
            string account,
            string password,
            int port,
            string server,
            string smtp_account,
            string smtp_password,
            int smtp_port,
            string smtp_server,
            bool smtp_auth,
            bool restrict,
            EncryptionType incoming_encryption_type,
            EncryptionType outcoming_encryption_type,
            SaslMechanism auth_type_in,
            SaslMechanism auth_type_smtp)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException();

            var mbox = MailBoxManager.GetMailBox(TenantId, Username, new MailAddress(email));

            if (null == mbox)
                throw new ArgumentException("Mailbox with specified email doesn't exist.");

            if (mbox.IsTeamlab)
                throw new ArgumentException("Mailbox with specified email can't be updated");

            if (string.IsNullOrEmpty(password))
                password = mbox.Password;
            if (string.IsNullOrEmpty(smtp_password))
                smtp_password = mbox.SmtpPassword;

            string errorText = null;

            mbox.Account = account;
            mbox.Name = name;
            mbox.Password = password;
            mbox.SmtpAccount = smtp_account;
            mbox.SmtpPassword = smtp_password;
            mbox.Port = port;
            mbox.Server = server;
            mbox.SmtpPort = smtp_port;
            mbox.SmtpServer = smtp_server;

            mbox.BeginDate = restrict
                ? DateTime.Now.Subtract(new TimeSpan(mbox.MailLimitedTimeDelta))
                : mbox.MailBeginTimestamp;
            mbox.Encryption = incoming_encryption_type;
            mbox.SmtpEncryption = outcoming_encryption_type;
            mbox.Authentication = auth_type_in;
            mbox.SmtpAuthentication = smtp_auth ? auth_type_smtp : SaslMechanism.None;

            try
            {
                if (string.IsNullOrEmpty(mbox.OAuthToken))
                {
                    LoginResult loginResult;

                    using (var client = new MailClient(mbox, CancellationToken.None, 5000, SslCertificatesErrorPermit, log: _log))
                    {
                        loginResult = client.TestLogin();
                    }

                    if (!loginResult.IngoingSuccess)
                    {
                        errorText = GetFormattedTextError(loginResult.IngoingException,
                            mbox.Imap ? MailServerType.Imap : MailServerType.Pop3, false); // exImap is ImapConnectionTimeoutException
                    }

                    if (!loginResult.OutgoingSuccess)
                    {
                        if (!string.IsNullOrEmpty(errorText))
                            errorText += "\r\n";

                        errorText += GetFormattedTextError(loginResult.OutgoingException, MailServerType.Smtp, false);
                        // exSmtp is SmtpConnectionTimeoutException);
                    }

                    if (!string.IsNullOrEmpty(errorText))
                        throw new Exception(errorText);
                }

                if (!MailBoxManager.SaveMailBox(mbox))
                    throw new Exception("Failed to_addresses update account");

                MailBoxManager.CachedAccounts.Clear(Username);

                var accountInfo = new AccountInfo(mbox.MailBoxId, mbox.EMailView, mbox.Name, mbox.Enabled, mbox.QuotaError,
                                               MailBox.AuthProblemType.NoProblems, new MailSignature(mbox.MailBoxId, TenantId, "", false),
                                               new MailAutoreply(mbox.MailBoxId, TenantId, false, false, false, DateTime.MinValue,
                                                   DateTime.MinValue, String.Empty, String.Empty), false, mbox.EMailInFolder, false, false);

                return accountInfo.ToAddressData().FirstOrDefault();
            }
            catch (Exception ex)
            {
                errorText = GetFormattedTextError(ex);
            }

            throw new Exception(errorText);
        }

        /// <summary>
        ///    Deletes account by email.
        /// </summary>
        /// <param name="email">Email the account to delete</param>
        /// <returns>MailOperationResult object</returns>
        /// <exception cref="ArgumentException">Exception happens when some parameters are invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="NullReferenceException">Exception happens when mailbox wasn't found.</exception>
        /// <short>Delete account</short> 
        /// <category>Accounts</category>
        [Delete(@"accounts")]
        public MailOperationStatus DeleteAccount(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException(@"Email empty", "email");

            var mailbox = MailBoxManager.GetMailBox(TenantId, Username, new MailAddress(email));
            if (mailbox == null)
                throw new NullReferenceException(string.Format("Account wasn't founded by email: {0}", email));

            if (mailbox.IsTeamlab)
                throw new ArgumentException("Mailbox with specified email can't be deleted");

            return MailBoxManager.RemoveMailbox(mailbox, TranslateMailOperationStatus);
        }

        /// <summary>
        ///    Sets the state for the account specified in the request
        /// </summary>
        /// <param name="email">Email of the account</param>
        /// <param name="state">Account activity state. Value: true or false. True - enabled, False - disabled.</param>
        /// <returns>Account mailbox id</returns>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="Exception">Exception happens when update operation failed.</exception>
        /// <short>Set account state</short> 
        /// <category>Accounts</category>
        [Update(@"accounts/state")]
        public int SetAccountEnable(string email, bool state)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException("email");

            var mailbox = MailBoxManager.GetMailBox(TenantId, Username, new MailAddress(email));
            if (mailbox == null)
                throw new NullReferenceException(String.Format("Account wasn't founded by email: {0}", email));

            if (state)
            {
                // Check account connection setting on activation

                string errorText = null;

                LoginResult loginResult;

                using (var client = new MailClient(mailbox, CancellationToken.None, 5000, SslCertificatesErrorPermit, log: _log))
                {
                    loginResult = client.TestLogin();
                }

                if (!loginResult.IngoingSuccess)
                {
                    errorText = GetFormattedTextError(loginResult.IngoingException,
                        mailbox.Imap ? MailServerType.Imap : MailServerType.Pop3, false); // exImap is ImapConnectionTimeoutException
                }

                if (!loginResult.OutgoingSuccess)
                {
                    if (!string.IsNullOrEmpty(errorText))
                        errorText += "\r\n";

                    errorText += GetFormattedTextError(loginResult.OutgoingException, MailServerType.Smtp, false);
                    // exSmtp is SmtpConnectionTimeoutException);
                }

                if (!string.IsNullOrEmpty(errorText))
                    throw new AuthenticationException(errorText);

            }

            if (!MailBoxManager.EnableMaibox(mailbox, state))
                throw new Exception("EnableMaibox failed.");

            MailBoxManager.CachedAccounts.Clear(Username);
            return mailbox.MailBoxId;
        }

        /// <summary>
        ///    Sets the default account specified in the request
        /// </summary>
        /// <param name="email">Email of the account</param>
        /// <param name="isDefault">Set or reset account as default</param>
        /// <returns>Account email address</returns>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="Exception">Exception happens when update operation failed.</exception>
        /// <short>Set default account</short> 
        /// <category>Accounts</category>
        [Update(@"accounts/default")]
        public string SetDefaultAccount(string email, bool isDefault)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException("email");

            email = email.ToLowerInvariant();
            if (isDefault)
            {
                var accounts = MailBoxManager.GetAccountInfo(TenantId, Username);
                bool emailExist = false;
                
                for (int i = 0; i < accounts.Count; i++)
                {
                    if (accounts[i].Email == email)
                    {
                        emailExist = true;
                        break;
                    }
                    for (int j = 0; j < accounts[i].Aliases.Count; j++)
                    {
                        if (accounts[i].Aliases[j].Email == email)
                        {
                            emailExist = true;
                            break;
                        }
                    }
                }
                if (!emailExist)
                    throw new ArgumentException(String.Format("Account wasn't founded by email: {0}", email));
            }
            var settings = new MailBoxAccountSettings
            {
                DefaultEmail = isDefault ? email : String.Empty
            };
            SettingsManager.Instance.SaveSettingsFor<MailBoxAccountSettings>(settings, SecurityContext.CurrentAccount.ID);

            return email;
        }

        /// <summary>
        ///    Returns the information about the account.
        /// </summary>
        /// <param name="email">Account email address</param>
        /// <returns>Account with specified email</returns>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="NullReferenceException">Exception happens when mailbox wasn't founded by email.</exception>
        /// <short>Get account by email</short> 
        /// <category>Accounts</category>
        [Read(@"accounts/single")]
        public MailBox GetAccount(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException(@"Email empty", "email");

            var mailbox = MailBoxManager.GetMailBox(TenantId, Username, new MailAddress(email));

            if (mailbox == null)
                throw new NullReferenceException(String.Format("Account wasn't founded by email: {0}", email));

            if (mailbox.IsTeamlab)
                throw new ArgumentException("Access to this account restricted");

            mailbox.Password = "";
            mailbox.SmtpPassword = "";

            return mailbox;
        }

        /// <summary>
        ///    Gets the default settings for the account based on the email domain.
        /// </summary>
        /// <param name="email">Account email address</param>
        /// <param name="action">This string parameter specifies action for default settings. Values:
        /// "get_imap_pop_settings" - get imap or pop settings, imap settings are prior.
        /// "get_imap_server" | "get_imap_server_full" - get imap settings
        /// "get_pop_server" | "get_pop_server_full" - get pop settings
        /// By default returns default imap settings.
        /// </param>
        /// <returns>Account with default settings</returns>
        /// <short>Get default account settings</short> 
        /// <category>Accounts</category>
        [Read(@"accounts/setups")]
        public MailBox GetAccountsDefaults(string email, string action)
        {
            if (action == "get_imap_pop_settings")
            {
                return MailBoxManager.ObtainMailboxSettings(TenantId, Username, email, "", AuthorizationServiceType.None, true, true) ??
                       MailBoxManager.ObtainMailboxSettings(TenantId, Username, email, "", AuthorizationServiceType.None, false, false);
            }
            if (action == "get_imap_server" || action == "get_imap_server_full")
            {
                return MailBoxManager.ObtainMailboxSettings(TenantId, Username, email, "", AuthorizationServiceType.None, true, false);
            }
            
            if (action == "get_pop_server" || action == "get_pop_server_full")
            {
                return MailBoxManager.ObtainMailboxSettings(TenantId, Username, email, "", AuthorizationServiceType.None, false, false);
            }

            return MailBoxManager.ObtainMailboxSettings(TenantId, Username, email, "", AuthorizationServiceType.None, null, false);
        }

        /// <summary>
        ///    Sets the state for the account specified in the request
        /// </summary>
        /// <param name="mailbox_id">Id of the account</param>
        /// <param name="email_in_folder">Document's folder Id</param>
        /// <returns>Account email address</returns>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="Exception">Exception happens when update operation failed.</exception>
        /// <short>Set account state</short> 
        /// <category>Accounts</category>
        [Update(@"accounts/emailinfolder")]
        public void SetAccountEMailInFolder(int mailbox_id, string email_in_folder)
        {
            if (null == email_in_folder)
                throw new ArgumentNullException("email_in_folder");

            MailBoxManager.SetMailboxEmailInFolder(TenantId, Username, mailbox_id, email_in_folder);
            MailBoxManager.CachedAccounts.Clear(Username);
        }

        private static string GetFormattedTextError(Exception ex, MailServerType mailServerType, bool timeoutFlag = true)
        {
            var headerText = string.Empty;
            var errorExplain = string.Empty;

            switch (mailServerType)
            {
                case MailServerType.Imap:
                case MailServerType.ImapOAuth:
                    headerText = MailApiResource.ImapResponse;
                    if (timeoutFlag)
                        errorExplain = MailApiResource.ImapConnectionTimeoutError;
                    break;
                case MailServerType.Pop3:
                    headerText = MailApiResource.Pop3Response;
                    if (timeoutFlag)
                        errorExplain = MailApiResource.Pop3ConnectionTimeoutError;
                    break;
                case MailServerType.Smtp:
                    headerText = MailApiResource.SmtRresponse;
                    if (timeoutFlag)
                        errorExplain = MailApiResource.SmtpConnectionTimeoutError;
                    break;
            }

            return GetFormattedTextError(ex, errorExplain, headerText);

        }

        private static string GetFormattedTextError(Exception ex, string errorExplain = "", string headerText = "")
        {
            if (!string.IsNullOrEmpty(headerText))
                headerText = string.Format("<span class=\"attempt_header\">{0}</span><br/>", headerText);

            if (string.IsNullOrEmpty(errorExplain))
                errorExplain = ex.InnerException == null ||
                                string.IsNullOrEmpty(ex.InnerException.Message)
                                    ? ex.Message
                                    : ex.InnerException.Message;

            var errorText = string.Format("{0}{1}",
                          headerText,
                          errorExplain);

            return errorText;
        }
    }
}
