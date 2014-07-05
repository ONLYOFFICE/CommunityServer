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
using System.IO;
using System.Security.Authentication;
using ActiveUp.Net.Common;
using ActiveUp.Net.Mail;

namespace ASC.Mail.Aggregator.Common
{
    public static class MailServerHelper
    {
        private const int WAIT_TIMEOUT = 5000;

        static public bool TestSmtp(MailServerSettings settings)
        {
            var smtp = MailClientBuilder.Smtp();
            string s_result = String.Empty;
            try
            {
                IAsyncResult async_res;
                if (settings.EncryptionType == EncryptionType.None || settings.EncryptionType == EncryptionType.StartTLS)
                {
                    async_res = smtp.BeginConnect(settings.Url, settings.Port, null);

                    if (!async_res.AsyncWaitHandle.WaitOne(WAIT_TIMEOUT))
                        throw new SmtpConnectionTimeoutException();

                    if (settings.AuthenticationType != SaslMechanism.None || settings.EncryptionType == EncryptionType.StartTLS)
                        smtp.SendEhloHelo();

                    if (settings.EncryptionType == EncryptionType.StartTLS)
                        smtp.StartTLS(settings.Url);

                    if (settings.AuthenticationType != SaslMechanism.None)
                    {
                        s_result = smtp.Authenticate(settings.AccountName, settings.AccountPass, settings.AuthenticationType);
                    }
                }
                else
                {
                    async_res = smtp.BeginConnectSsl(settings.Url, settings.Port, null);

                    if (!async_res.AsyncWaitHandle.WaitOne(WAIT_TIMEOUT))
                        throw new SmtpConnectionTimeoutException();

                    if (settings.AuthenticationType != SaslMechanism.None)
                    {
                        s_result = smtp.Authenticate(settings.AccountName, settings.AccountPass, settings.AuthenticationType);
                    }
                }

                if (settings.AuthenticationType != SaslMechanism.None && !s_result.StartsWith("+"))
                    throw new SmtpConnectionException(s_result);

                return true;
            }
            finally
            {
                if (smtp.IsConnected) smtp.Disconnect();
            }
        }

        static public bool TryTestSmtp(MailServerSettings settings, out string last_error)
        {
            try
            {
                last_error = String.Empty;
                return TestSmtp(settings);
            }
            catch (Exception ex)
            {
                last_error = ex.Message;
                return false;
            }
        }

        static public bool Test(BaseProtocolClient ingoing_mail_client, MailServerSettings settings)
        {
            try
            {
                IAsyncResult async_res;
                switch (settings.EncryptionType)
                {
                    case EncryptionType.StartTLS:
                        async_res = ingoing_mail_client.BeginConnect(settings.Url, settings.Port, null);
                        break;
                    case EncryptionType.SSL:
                        async_res = ingoing_mail_client.BeginConnectSsl(settings.Url, settings.Port, null);
                        break;
                    default:
                        async_res = ingoing_mail_client.BeginConnect(settings.Url, settings.Port, null);
                        break;
                }

                if (!async_res.AsyncWaitHandle.WaitOne(WAIT_TIMEOUT))
                    throw new ImapConnectionTimeoutException();

                if (settings.EncryptionType == EncryptionType.StartTLS)
                {
                    ingoing_mail_client.StartTLS(settings.Url);
                }

                if (settings.AuthenticationType == SaslMechanism.Login)
                {
                    ingoing_mail_client.Login(settings.AccountName, settings.AccountPass, "");
                }
                else
                {
                    async_res = ingoing_mail_client.BeginAuthenticate(settings.AccountName, settings.AccountPass, settings.AuthenticationType, null);
                }

                if (!async_res.AsyncWaitHandle.WaitOne(WAIT_TIMEOUT))
                    throw new ImapConnectionTimeoutException();

                if (async_res.AsyncState == null)
                    throw new AuthenticationException("Auth failed. Check your settings.");

                string s_result = ingoing_mail_client.EndConnectSsl(async_res).ToLowerInvariant();

                if (s_result.IndexOf("success", StringComparison.Ordinal) == -1 &&
                    s_result.IndexOf("+", StringComparison.Ordinal) == -1 &&
                    s_result.IndexOf("ok", StringComparison.Ordinal) == -1)
                    throw new ImapConnectionException(s_result);

                return true;
            }
            finally
            {
                if (ingoing_mail_client.IsConnected)
                {
                    ingoing_mail_client.Disconnect();
                }
            }
        }

        static public bool TryTestImap(MailServerSettings settings, out string last_error)
        {
            try
            {
                last_error = String.Empty;
                return Test(MailClientBuilder.Imap(), settings);
            }
            catch (Exception ex)
            {
                last_error = ex.Message;
                return false;
            }
        }

        public static bool TryTestPop(MailServerSettings settings, out string last_error)
        {
            try
            {
                last_error = String.Empty;
                return Test(MailClientBuilder.Pop(), settings);
            }
            catch (Exception ex)
            {
                last_error = ex.Message;
                return false;
            }
        }

        public static bool Test(MailBox account)
        {
            BaseProtocolClient ingoing_client = (account.Imap) ? (BaseProtocolClient)MailClientBuilder.Imap() : MailClientBuilder.Pop();

            MailServerHelper.Test(ingoing_client, new MailServerSettings
            {
                Url = account.Server,
                Port = account.Port,
                AccountName = account.Account,
                AccountPass = account.Password,
                AuthenticationType = account.AuthenticationTypeIn,
                EncryptionType = account.IncomingEncryptionType
            });

            MailServerHelper.TestSmtp(new MailServerSettings
            {
                Url = account.SmtpServer,
                Port = account.SmtpPort,
                AccountName = account.SmtpAccount,
                AccountPass = account.SmtpPassword,
                AuthenticationType = account.AuthenticationTypeSmtp,
                EncryptionType = account.OutcomingEncryptionType
            });

            return true;
        }
    }
}
