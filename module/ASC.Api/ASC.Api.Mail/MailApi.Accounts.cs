/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using ASC.Api.Attributes;
using ASC.Mail.Aggregator;
using ASC.Api.Mail.Resources;
using ASC.Mail.Aggregator.Authorization;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Dal;
using ActiveUp.Net.Mail;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {

        /// <summary>
        ///    Returns the list of all accounts connected to the Mail.
        /// </summary>
        /// <returns>Accounts list</returns>
        /// <short>Get accounts list</short> 
        /// <category>Accounts</category>
        [Read(@"accounts")]
        public IEnumerable<MailAccount> GetAccounts()
        {
            var boxes = MailBoxManager.GetMailBoxes(TenantId, Username);

            boxes
                .FindAll(b => b.QuotaError)
                .ForEach(b => MailBoxManager.SetMailboxQuotaError(b, false));

            var signatures = new List<SignatureDto>();

            if (boxes.Any())
            {
                var mailboxes_ids = boxes.Select(b => b.MailBoxId).ToList();
                signatures = MailBoxManager.GetMailboxesSignatures(mailboxes_ids, Username, TenantId);
            }

            return boxes.ConvertAll(x =>
                                    new MailAccount(
                                        x.EMail.ToString(),
                                        x.Name,
                                        x.Enabled,
                                        x.QuotaError,
                                        x.AuthError,
                                        x.MailBoxId,
                                        !string.IsNullOrEmpty(x.RefreshToken),
                                        signatures.FirstOrDefault(s => s.MailboxId == x.MailBoxId),
                                        x.EMailInFolder));
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
        public MailBox CreateAccountSimple(string email, string password)
        {
            if (string.IsNullOrEmpty(email)) throw new ArgumentException("Empty email", "email");

            var begin_date = DateTime.Now.Subtract(new TimeSpan(MailBox.DefaultMailLimitedTimeDelta));

            string error_text;
            try
            {
                var mbox = MailBoxManager.SearchMailboxSettings(email, password, Username, TenantId);

                mbox.BeginDate = begin_date; // Apply restrict for download

                MailServerHelper.Test(mbox);

                mbox.InServerId = MailBoxManager.SaveMailServerSettings(mbox.EMail,
                        new MailServerSettings
                        {
                            AccountName = mbox.Account,
                            AccountPass = mbox.Password,
                            AuthenticationType = mbox.AuthenticationTypeIn,
                            EncryptionType = mbox.IncomingEncryptionType,
                            Port = mbox.Port,
                            Url = mbox.Server
                        },
                        mbox.Imap ? "imap" : "pop3", AuthorizationServiceType.Unknown);

                mbox.SmtpServerId = MailBoxManager.SaveMailServerSettings(mbox.EMail,
                                        new MailServerSettings
                                        {
                                            AccountName = mbox.SmtpAccount,
                                            AccountPass = mbox.SmtpPassword,
                                            AuthenticationType = mbox.AuthenticationTypeSmtp,
                                            EncryptionType = mbox.OutcomingEncryptionType,
                                            Port = mbox.SmtpPort,
                                            Url = mbox.SmtpServer
                                        },
                                        "smtp", AuthorizationServiceType.Unknown);

                MailBoxManager.SaveMailBox(mbox);
                return mbox;
            }
            catch (ImapConnectionException ex_imap)
            {
                error_text = GetFormattedTextError(ex_imap, ServerType.Imap, ex_imap is ImapConnectionTimeoutException);
            }
            catch (Pop3ConnectionException ex_pop)
            {
                error_text = GetFormattedTextError(ex_pop, ServerType.Pop3, ex_pop is Pop3ConnectionTimeoutException);
            }
            catch (SmtpConnectionException ex_smtp)
            {
                error_text = GetFormattedTextError(ex_smtp, ServerType.Smtp, ex_smtp is SmtpConnectionTimeoutException);
            }
            catch (Exception ex)
            {
                error_text = GetFormattedTextError(ex, ServerType.Imap);
            }
            throw new Exception(string.Format("{0}", error_text));
        }

        /// <summary>
        ///    Creates Mail account with OAuth authentication. Only Google OAuth supported.
        /// </summary>
        /// <param name="email">Account email in string format like: name@domain</param>
        /// <param name="token">Oauth token</param>
        /// <param name="type">Type of OAuth service. 0- Unknown, 1 - Google.</param>
        /// <exception cref="Exception">Exception contains text description of happened error.</exception>
        /// <returns>Created account</returns>
        /// <short>Create OAuth account</short> 
        /// <category>Accounts</category>
        [Create(@"accounts/oauth")]
        public MailBox CreateAccountOAuth(string email, string token, byte type)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("Empty oauth token", "token");
            if (string.IsNullOrEmpty(email)) throw new ArgumentException("Empty email", "email");

            var begin_date = DateTime.Now.Subtract(new TimeSpan(MailBox.DefaultMailLimitedTimeDelta));

            var mbox_imap = MailBoxManager.ObtainMailboxSettings(TenantId, Username, email, "", (AuthorizationServiceType) type, true, false);
            mbox_imap.RefreshToken = token;
            mbox_imap.BeginDate = begin_date; // Apply restrict for download

            try
            {
                mbox_imap.InServerId = MailBoxManager.SaveMailServerSettings(mbox_imap.EMail,
                                        new MailServerSettings
                                        {
                                            AccountName = mbox_imap.Account,
                                            AccountPass = mbox_imap.Password,
                                            AuthenticationType = mbox_imap.AuthenticationTypeIn,
                                            EncryptionType = mbox_imap.IncomingEncryptionType,
                                            Port = mbox_imap.Port,
                                            Url = mbox_imap.Server
                                        },
                                        "imap",
                                        (AuthorizationServiceType)type);
                mbox_imap.SmtpServerId = MailBoxManager.SaveMailServerSettings(mbox_imap.EMail,
                                        new MailServerSettings
                                        {
                                            AccountName = mbox_imap.SmtpAccount,
                                            AccountPass = mbox_imap.SmtpPassword,
                                            AuthenticationType = mbox_imap.AuthenticationTypeSmtp,
                                            EncryptionType = mbox_imap.OutcomingEncryptionType,
                                            Port = mbox_imap.SmtpPort,
                                            Url = mbox_imap.SmtpServer
                                        },
                                        "smtp",
                                        (AuthorizationServiceType)type);

                MailBoxManager.SaveMailBox(mbox_imap);
                return mbox_imap;
            }
            catch (Exception imap_exception)
            {
                throw new Exception(GetFormattedTextError(imap_exception, ServerType.ImapOAuth));
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
        public MailBox CreateAccount(string name,
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

            string error_text;
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
                    SmtpAuth = smtp_auth,
                    Imap = imap,
                    Restrict = restrict,
                    TenantId = TenantId,
                    UserId = Username,
                    BeginDate = restrict ? 
                        DateTime.Now.Subtract(new TimeSpan(MailBox.DefaultMailLimitedTimeDelta)) : 
                        new DateTime(MailBox.DefaultMailBeginTimestamp),
                    IncomingEncryptionType = incoming_encryption_type,
                    OutcomingEncryptionType = outcoming_encryption_type,
                    AuthenticationTypeIn = auth_type_in,
                    AuthenticationTypeSmtp = auth_type_smtp
                };

            try
            {
                MailServerHelper.Test(mbox);

                mbox.InServerId = MailBoxManager.SaveMailServerSettings(mbox.EMail,
                                        new MailServerSettings
                                        {
                                            AccountName = mbox.Account,
                                            AccountPass = mbox.Password,
                                            AuthenticationType = mbox.AuthenticationTypeIn,
                                            EncryptionType = mbox.IncomingEncryptionType,
                                            Port = mbox.Port,
                                            Url = mbox.Server
                                        },
                                        imap ? "imap" : "pop3", AuthorizationServiceType.Unknown);
                mbox.SmtpServerId = MailBoxManager.SaveMailServerSettings(mbox.EMail,
                                        new MailServerSettings
                                        {
                                        AccountName = mbox.SmtpAccount,
                                        AccountPass = mbox.SmtpPassword,
                                        AuthenticationType = mbox.AuthenticationTypeSmtp,
                                        EncryptionType = mbox.OutcomingEncryptionType,
                                        Port = mbox.SmtpPort,
                                        Url = mbox.SmtpServer
                                        },
                                        "smtp", AuthorizationServiceType.Unknown);

                MailBoxManager.SaveMailBox(mbox);
                return mbox;
            }
            catch (ImapConnectionException ex_imap)
            {
                error_text = GetFormattedTextError(ex_imap, ServerType.Imap, ex_imap is ImapConnectionTimeoutException);
            }
            catch (Pop3ConnectionException ex_pop3)
            {
                error_text = GetFormattedTextError(ex_pop3, ServerType.Pop3, ex_pop3 is Pop3ConnectionTimeoutException);
            }
            catch (SmtpConnectionException ex_smtp)
            {
                error_text = GetFormattedTextError(ex_smtp, ServerType.Smtp, ex_smtp is SmtpConnectionTimeoutException);
            }
            catch (Exception ex)
            {
                error_text = GetFormattedTextError(ex, imap ? ServerType.Imap : ServerType.Pop3);
            }

            throw new Exception(error_text);

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
        public MailBox UpdateAccount(string name,
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

            if (string.IsNullOrEmpty(password))
                password = mbox.Password;
            if (string.IsNullOrEmpty(smtp_password))
                smtp_password = mbox.SmtpPassword;

            string error_text;

            mbox.Account = account;
            mbox.Name = name;
            mbox.Password = password;
            mbox.SmtpAccount = smtp_account;
            mbox.SmtpPassword = smtp_password;
            mbox.Port = port;
            mbox.Server = server;
            mbox.SmtpPort = smtp_port;
            mbox.SmtpServer = smtp_server;
            mbox.SmtpAuth = smtp_auth;
            mbox.Restrict = restrict;
            mbox.BeginDate = mbox.Restrict ? 
                DateTime.Now.Subtract(new TimeSpan(mbox.MailLimitedTimeDelta)) : 
                mbox.MailBeginTimestamp;
            mbox.IncomingEncryptionType = incoming_encryption_type;
            mbox.OutcomingEncryptionType = outcoming_encryption_type;
            mbox.AuthenticationTypeIn = auth_type_in;
            mbox.AuthenticationTypeSmtp = auth_type_smtp;

            mbox.InServerId = MailBoxManager.SaveMailServerSettings(mbox.EMail,
                        new MailServerSettings
                        {
                            AccountName = mbox.Account,
                            AccountPass = mbox.Password,
                            AuthenticationType = mbox.AuthenticationTypeIn,
                            EncryptionType = mbox.IncomingEncryptionType,
                            Port = mbox.Port,
                            Url = mbox.Server
                        },
                        mbox.Imap ? "imap" : "pop3", AuthorizationServiceType.Unknown);
            mbox.SmtpServerId = MailBoxManager.SaveMailServerSettings(mbox.EMail,
                                    new MailServerSettings
                                    {
                                        AccountName = mbox.SmtpAccount,
                                        AccountPass = mbox.SmtpPassword,
                                        AuthenticationType = mbox.AuthenticationTypeSmtp,
                                        EncryptionType = mbox.OutcomingEncryptionType,
                                        Port = mbox.SmtpPort,
                                        Url = mbox.SmtpServer
                                    },
                                    "smtp", AuthorizationServiceType.Unknown);

            try
            {
                if (!string.IsNullOrEmpty(mbox.RefreshToken) || MailServerHelper.Test(mbox))
                {
                    if (!MailBoxManager.SaveMailBox(mbox))
                        throw new Exception("Failed to_addresses update account");
                }

                return mbox;
            }
            catch (ImapConnectionException ex_imap)
            {
                error_text = GetFormattedTextError(ex_imap, ServerType.Imap, ex_imap is ImapConnectionTimeoutException);
            }
            catch (Pop3ConnectionException ex_pop3)
            {
                error_text = GetFormattedTextError(ex_pop3, ServerType.Pop3, ex_pop3 is Pop3ConnectionTimeoutException);
            }
            catch (SmtpConnectionException ex_smtp)
            {
                error_text = GetFormattedTextError(ex_smtp, ServerType.Smtp, ex_smtp is SmtpConnectionTimeoutException);
            }
            catch (Exception ex)
            {
                error_text = GetFormattedTextError(ex, mbox.Imap ? ServerType.Imap : ServerType.Pop3);
            }

            throw new Exception(error_text);
        }

        /// <summary>
        ///    Deletes account by email.
        /// </summary>
        /// <param name="email">Email the account to delete</param>
        /// <returns>Account email address</returns>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="NullReferenceException">Exception happens when mailbox wasn't founded by email.</exception>
        /// <short>Delete account</short> 
        /// <category>Accounts</category>
        [Delete(@"accounts/{email}")]
        public string DeleteAccount(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email empty", "email");

            var mailbox = MailBoxManager.GetMailBox(TenantId, Username, new MailAddress(email));
            if (mailbox == null)
                throw new NullReferenceException(String.Format("Account wasn't founded by email: {0}", email));

            MailBoxManager.RemoveMailBox(mailbox);
            return email;
        }

        /// <summary>
        ///    Sets the state for the account specified in the request
        /// </summary>
        /// <param name="email">Email of the account</param>
        /// <param name="state">Account activity state. Value: true or false. True - enabled, False - disabled.</param>
        /// <returns>Account email address</returns>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="Exception">Exception happens when update operation failed.</exception>
        /// <short>Set account state</short> 
        /// <category>Accounts</category>
        [Update(@"accounts/{email}/state")]
        public string SetAccountState(string email, bool state)
        {
            //Todo: rename this method to disable enable
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException("email");

            var mailbox = MailBoxManager.GetMailBox(TenantId, Username, new MailAddress(email));
            if (mailbox == null)
                throw new NullReferenceException(String.Format("Account wasn't founded by email: {0}", email));

            if (!MailBoxManager.EnableMaibox(mailbox, state))
                throw new Exception("EnableMaibox failed.");

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
        [Read(@"accounts/{email}")]
        public MailBox GetAccount(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email empty", "email");

            var mailbox = MailBoxManager.GetMailBox(TenantId, Username, new MailAddress(email));
            if (mailbox == null)
                throw new NullReferenceException(String.Format("Account wasn't founded by email: {0}", email));

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
        [Read(@"accounts/{email}/default")]
        public MailBox GetAccountsDefaults(string email, string action)
        {
            if (action == "get_imap_pop_settings")
            {
                return MailBoxManager.ObtainMailboxSettings(TenantId, Username, email, "", AuthorizationServiceType.Unknown, true, true) ??
                       MailBoxManager.ObtainMailboxSettings(TenantId, Username, email, "", AuthorizationServiceType.Unknown, false, false);
            }
            if (action == "get_imap_server" || action == "get_imap_server_full")
            {
                return MailBoxManager.ObtainMailboxSettings(TenantId, Username, email, "", AuthorizationServiceType.Unknown, true, false);
            }
            
            if (action == "get_pop_server" || action == "get_pop_server_full")
            {
                return MailBoxManager.ObtainMailboxSettings(TenantId, Username, email, "", AuthorizationServiceType.Unknown, false, false);
            }

            return MailBoxManager.ObtainMailboxSettings(TenantId, Username, email, "", AuthorizationServiceType.Unknown, null, false);
        }

        /// <summary>
        ///    Sets the state for the account specified in the request
        /// </summary>
        /// <param name="mailbox_id">Id of the account</param>
        /// <param name="email_in_folder">Account EMailIn folder</param>
        /// <returns>Account email address</returns>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="Exception">Exception happens when update operation failed.</exception>
        /// <short>Set account state</short> 
        /// <category>Accounts</category>
        [Update(@"accounts/{mailbox_id:[0-9]+}/emailinfolder")]
        public void SetAccountEMailInFolder(int mailbox_id, string email_in_folder)
        {
            if (null == email_in_folder)
                throw new ArgumentNullException("email_in_folder");

            MailBoxManager.SetMailboxEmailInFolder(TenantId, Username, mailbox_id, email_in_folder);
        }

        enum ServerType
        {
            ImapOAuth = 0,
            Imap = 1,
            Pop3 = 2,
            Smtp = 3
        }

        private static string GetFormattedTextError(Exception ex, ServerType server_type, bool timeout_flag = true)
        {
            var header_text = string.Empty;
            var error_explain = string.Empty;

            switch (server_type)
            {
                case ServerType.Imap:
                case ServerType.ImapOAuth:
                    header_text = MailApiResource.ImapResponse;
                    if (timeout_flag)
                        error_explain = MailApiResource.ImapConnectionTimeoutError;
                    break;
                case ServerType.Pop3:
                    header_text = MailApiResource.Pop3Response;
                    if (timeout_flag)
                        error_explain = MailApiResource.Pop3ConnectionTimeoutError;
                    break;
                case ServerType.Smtp:
                    header_text = MailApiResource.SmtRresponse;
                    if (timeout_flag)
                        error_explain = MailApiResource.SmtpConnectionTimeoutError;
                    break;
            }

            if (!string.IsNullOrEmpty(header_text))
                header_text = string.Format("<span class=\"attempt_header\">{0}</span><br/>", header_text);

            if (string.IsNullOrEmpty(error_explain))
                error_explain = ex.InnerException == null ||
                                string.IsNullOrEmpty(ex.InnerException.Message)
                                    ? ex.Message
                                    : ex.InnerException.Message;

            var error_text = string.Format("{0}{1}",
                          header_text,
                          error_explain);
            
            return error_text;
        }
    }
}
