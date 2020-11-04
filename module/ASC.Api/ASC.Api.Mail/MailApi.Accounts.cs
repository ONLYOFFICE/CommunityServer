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
using ASC.Api.Attributes;
using ASC.Core;
using ASC.Mail;
using ASC.Mail.Authorization;
using ASC.Mail.Clients;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;
using ASC.Mail.Exceptions;
using ASC.Mail.Extensions;
using ASC.Web.Mail.Resources;
using EncryptionType = ASC.Mail.Enums.EncryptionType;
using SaslMechanism = ASC.Mail.Enums.SaslMechanism;
// ReSharper disable InconsistentNaming

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
            var accounts = MailEngineFactory.AccountEngine.GetAccountInfoList();
            return accounts.ToAccountData();
        }

        /// <summary>
        ///    Returns the information about the account.
        /// </summary>
        /// <param name="email">Account email address</param>
        /// <returns>Account with specified email</returns>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="NullReferenceException">Exception happens when mailbox wasn't found by email.</exception>
        /// <short>Get account by email</short> 
        /// <category>Accounts</category>
        [Read(@"accounts/single")]
        public MailBoxData GetAccount(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException(@"Email empty", "email");

            var mailbox =
                MailEngineFactory.MailboxEngine.GetMailboxData(new СoncreteUserMailboxExp(new MailAddress(email), TenantId,
                    Username));

            if (mailbox == null)
                throw new NullReferenceException(string.Format("Account wasn't found by email: {0}", email));

            if (mailbox.IsTeamlab)
            {
                if(!CoreContext.Configuration.Standalone)
                    throw new ArgumentException("Access to this account restricted");

                string mxHost = null;

                try
                {
                    mxHost = MailEngineFactory.ServerEngine.GetMailServerMxDomain();
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("GetMailServerMxDomain() failed. Exception: {0}", ex.ToString());
                }

                if (!string.IsNullOrEmpty(mxHost))
                {
                    mailbox.Server = mxHost;
                    mailbox.SmtpServer = mxHost;
                }
            }

            mailbox.Password = "";
            mailbox.SmtpPassword = "";

            return mailbox;
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

            try
            {
                List<LoginResult> loginResults;
                var account = MailEngineFactory.AccountEngine.CreateAccountSimple(email, password, out loginResults);

                if (account != null)
                    return account.ToAccountData().FirstOrDefault();

                var i = 0;

                foreach (var loginResult in loginResults)
                {
                    errorText += string.Format("#{0}:<br>", ++i);

                    if (!loginResult.IngoingSuccess)
                    {
                        errorText += GetFormattedTextError(loginResult.IngoingException,
                            loginResult.Imap ? ServerType.Imap : ServerType.Pop3, false) + "<br>";
                    }

                    if (!loginResult.OutgoingSuccess)
                    {
                        errorText += GetFormattedTextError(loginResult.OutgoingException, ServerType.Smtp, false) +
                                     "<br>";
                    }
                }

            }
            catch (Exception ex)
            {
                //TODO: change AttachmentsUnknownError to common unknown error text
                errorText = GetFormattedTextError(ex, MailApiResource.AttachmentsUnknownError);
            }

            throw new Exception(errorText);
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
            var mbox = new MailBoxData
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
                BeginDate = restrict
                    ? DateTime.Now.Subtract(new TimeSpan(MailBoxData.DefaultMailLimitedTimeDelta))
                    : new DateTime(MailBoxData.DefaultMailBeginTimestamp),
                Encryption = incoming_encryption_type,
                SmtpEncryption = outcoming_encryption_type,
                Authentication = auth_type_in,
                SmtpAuthentication = smtp_auth ? auth_type_smtp : SaslMechanism.None,
                Enabled = true
            };

            try
            {
                LoginResult loginResult;

                var accountInfo = MailEngineFactory.AccountEngine.CreateAccount(mbox, out loginResult);

                if (accountInfo != null)
                {
                    return accountInfo.ToAccountData().FirstOrDefault();
                }

                if (!loginResult.IngoingSuccess)
                {
                    errorText = GetFormattedTextError(loginResult.IngoingException,
                        mbox.Imap ? ServerType.Imap : ServerType.Pop3, false);
                    // exImap is ImapConnectionTimeoutException
                }

                if (!loginResult.OutgoingSuccess)
                {
                    if (!string.IsNullOrEmpty(errorText))
                        errorText += "\r\n";

                    errorText += GetFormattedTextError(loginResult.OutgoingException, ServerType.Smtp, false);
                    // exSmtp is SmtpConnectionTimeoutException);
                }
            }
            catch (Exception ex)
            {
                //TODO: change AttachmentsUnknownError to common unknown error text
                errorText = GetFormattedTextError(ex, MailApiResource.AttachmentsUnknownError);
            }

            throw new Exception(errorText ?? MailApiResource.AttachmentsUnknownError);

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
            if (string.IsNullOrEmpty(code))
                throw new ArgumentException(@"Empty oauth code", "code");

            try
            {
                var account = MailEngineFactory.AccountEngine.CreateAccountOAuth(code, type);
                return account.ToAccountData().FirstOrDefault();
            }
            catch (Exception imapException)
            {
                throw new Exception(GetFormattedTextError(imapException, ServerType.ImapOAuth,
                    imapException is ImapConnectionTimeoutException));
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
            string errorText = null;

            try
            {
                var accountInfo = MailEngineFactory.AccountEngine.UpdateAccountOAuth(mailboxId, code, type);

                if (accountInfo != null)
                {
                    return accountInfo.ToAccountData().FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                //TODO: change AttachmentsUnknownError to common unknown error text
                errorText = GetFormattedTextError(ex, MailApiResource.AttachmentsUnknownError);
            }

            throw new Exception(errorText ?? MailApiResource.AttachmentsUnknownError);
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

            string errorText = null;
            var mbox = new MailBoxData
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
                TenantId = TenantId,
                UserId = Username,
                BeginDate = restrict
                    ? DateTime.Now.Subtract(new TimeSpan(MailBoxData.DefaultMailLimitedTimeDelta))
                    : new DateTime(MailBoxData.DefaultMailBeginTimestamp),
                Encryption = incoming_encryption_type,
                SmtpEncryption = outcoming_encryption_type,
                Authentication = auth_type_in,
                SmtpAuthentication = smtp_auth ? auth_type_smtp : SaslMechanism.None
            };

            try
            {
                LoginResult loginResult;

                var accountInfo = MailEngineFactory.AccountEngine.UpdateAccount(mbox, out loginResult);

                if (accountInfo != null)
                {
                    return accountInfo.ToAccountData().FirstOrDefault();
                }

                if (!loginResult.IngoingSuccess)
                {
                    errorText = GetFormattedTextError(loginResult.IngoingException,
                        mbox.Imap ? ServerType.Imap : ServerType.Pop3, false);
                    // exImap is ImapConnectionTimeoutException
                }

                if (!loginResult.OutgoingSuccess)
                {
                    if (!string.IsNullOrEmpty(errorText))
                        errorText += "\r\n";

                    errorText += GetFormattedTextError(loginResult.OutgoingException, ServerType.Smtp, false);
                    // exSmtp is SmtpConnectionTimeoutException);
                }
            }
            catch (Exception ex)
            {
                //TODO: change AttachmentsUnknownError to common unknown error text
                errorText = GetFormattedTextError(ex, MailApiResource.AttachmentsUnknownError);
            }

            throw new Exception(errorText ?? MailApiResource.AttachmentsUnknownError);
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

            var mailbox =
                MailEngineFactory.MailboxEngine.GetMailboxData(new СoncreteUserMailboxExp(new MailAddress(email), TenantId,
                    Username));

            if (mailbox == null)
                throw new NullReferenceException(string.Format("Account wasn't found by email: {0}", email));

            if (mailbox.IsTeamlab)
                throw new ArgumentException("Mailbox with specified email can't be deleted");

            return MailEngineFactory.OperationEngine.RemoveMailbox(mailbox, TranslateMailOperationStatus);
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

            string errorText = null;
            LoginResult loginResult;

            var mailboxId = MailEngineFactory.AccountEngine.SetAccountEnable(new MailAddress(email), state, out loginResult);

            if (loginResult != null)
            {
                if (!loginResult.IngoingSuccess)
                {
                    errorText = GetFormattedTextError(loginResult.IngoingException,
                        loginResult.Imap ? ServerType.Imap : ServerType.Pop3, false); 
                    // exImap is ImapConnectionTimeoutException
                }

                if (!loginResult.OutgoingSuccess)
                {
                    if (!string.IsNullOrEmpty(errorText))
                        errorText += "\r\n";

                    errorText += GetFormattedTextError(loginResult.OutgoingException, ServerType.Smtp, false);
                    // exSmtp is SmtpConnectionTimeoutException);
                }

                if (!string.IsNullOrEmpty(errorText))
                    throw new Exception(errorText);
            }

            if (mailboxId < 0)
                throw new Exception("EnableMaibox failed.");

            return mailboxId;
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
                var accounts = MailEngineFactory.AccountEngine.GetAccountInfoList();

                var emailExist = false;

                foreach (var account in accounts)
                {
                    if (account.Email == email)
                    {
                        emailExist = true;
                        break;
                    }
                    if (account.Aliases.Any(address => address.Email == email))
                    {
                        emailExist = true;
                    }
                }

                if (!emailExist)
                    throw new ArgumentException("Account not found");
            }

            new MailBoxAccountSettings {DefaultEmail = isDefault ? email : string.Empty}.SaveForCurrentUser();

            return email;
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
        public MailBoxData GetAccountDefaults(string email, string action)
        {
            switch (action)
            {
                case Defines.GET_IMAP_POP_SETTINGS:
                    return
                        MailEngineFactory.MailboxEngine.GetDefaultMailboxData(email, "",
                            AuthorizationServiceType.None, true, true) ??
                        MailEngineFactory.MailboxEngine.GetDefaultMailboxData(email, "",
                            AuthorizationServiceType.None, false, false);
                case Defines.GET_IMAP_SERVER:
                case Defines.GET_IMAP_SERVER_FULL:
                    return MailEngineFactory.MailboxEngine.GetDefaultMailboxData(email, "",
                        AuthorizationServiceType.None, true, false);
                case Defines.GET_POP_SERVER:
                case Defines.GET_POP_SERVER_FULL:
                    return MailEngineFactory.MailboxEngine.GetDefaultMailboxData(email, "",
                        AuthorizationServiceType.None, false, false);
            }

            return MailEngineFactory.MailboxEngine.GetDefaultMailboxData(email, "",
                AuthorizationServiceType.None, null, false);
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
            if (mailbox_id < 0)
                throw new ArgumentNullException("mailbox_id");

            MailEngineFactory.AccountEngine.SetAccountEmailInFolder(mailbox_id, email_in_folder);
        }

        /// <summary>
        /// UpdateUserActivity
        /// </summary>
        /// <param name="userOnline"></param>
        /// <category>Accounts</category>
        [Update(@"accounts/updateuseractivity")]
        public void UpdateUserActivity(bool userOnline)
        {
            MailEngineFactory.AccountEngine.SetAccountsActivity(userOnline);
        }

        private static string GetFormattedTextError(Exception ex, ServerType mailServerType, bool timeoutFlag = true)
        {
            var headerText = string.Empty;
            var errorExplain = string.Empty;

            switch (mailServerType)
            {
                case ServerType.Imap:
                case ServerType.ImapOAuth:
                    headerText = MailApiResource.ImapResponse;
                    if (timeoutFlag)
                        errorExplain = MailApiResource.ImapConnectionTimeoutError;
                    break;
                case ServerType.Pop3:
                    headerText = MailApiResource.Pop3Response;
                    if (timeoutFlag)
                        errorExplain = MailApiResource.Pop3ConnectionTimeoutError;
                    break;
                case ServerType.Smtp:
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
