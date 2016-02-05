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
using System.Reflection;
using ActiveUp.Net.Common;
using ActiveUp.Net.Mail;
using ASC.Mail.Aggregator.Common.Extension;

namespace ASC.Mail.Aggregator.Common
{
    public static class MailServerHelper
    {
        public static bool TryTestSmtp(MailServerSettings settings, out string lastError)
        {
            try
            {
                lastError = String.Empty;
                return Test(MailClientBuilder.Smtp(), settings);
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }

        public static bool Test(BaseProtocolClient ingoingMailClient, MailServerSettings settings)
        {
            try
            {
                var sResult = ingoingMailClient.Authorize(settings, settings.MailServerOperationTimeoutInMilliseconds);
                if (sResult.ToLower().IndexOf("success", StringComparison.Ordinal) == -1 &&
                    sResult.ToLower().IndexOf("+", StringComparison.Ordinal) == -1 &&
                    sResult.ToLower().IndexOf("ok", StringComparison.Ordinal) == -1)
                {
                    if (ingoingMailClient is Imap4Client)
                        throw new ImapConnectionException(sResult);
                    if(ingoingMailClient is Pop3Client)
                        throw new Pop3ConnectionException(sResult);
                    else
                        throw new SmtpConnectionException(sResult);
                }

                return true;
            }
            catch (TargetInvocationException exTarget)
            {
                if (ingoingMailClient is Imap4Client)
                        throw new ImapConnectionException(exTarget.InnerException.Message);
                    if(ingoingMailClient is Pop3Client)
                        throw new Pop3ConnectionException(exTarget.InnerException.Message);
                    else
                        throw new SmtpConnectionException(exTarget.InnerException.Message);
            }
            catch (TimeoutException)
            {
                if (ingoingMailClient is Imap4Client)
                    throw new ImapConnectionTimeoutException();
                if (ingoingMailClient is Pop3Client)
                    throw new Pop3ConnectionTimeoutException();
                else
                    throw new SmtpConnectionTimeoutException();
            }
            finally
            {
                if (ingoingMailClient.IsConnected)
                {
                    try
                    {
                        ingoingMailClient.Disconnect();
                    }
                    catch {}
                    
                }
            }
        }

        public static bool TryTestImap(MailServerSettings settings, out string lastError)
        {
            try
            {
                lastError = String.Empty;
                return Test(MailClientBuilder.Imap(), settings);
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }

        public static bool TryTestPop(MailServerSettings settings, out string lastError)
        {
            try
            {
                lastError = String.Empty;
                return Test(MailClientBuilder.Pop(), settings);
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }

        public static bool TestOAuth(BaseProtocolClient client, MailBox account)
        {
            if (string.IsNullOrEmpty(account.RefreshToken)) return false;

            try
            {
                var imap4Client = client as Imap4Client;
                if (imap4Client != null)
                {
                    imap4Client.AuthenticateImapGoogleOAuth2(account);

                    return true;
                }

                var smtpClient = client as SmtpClient;
                if (smtpClient != null)
                {
                    smtpClient.AuthenticateSmtpGoogleOAuth2(account);

                    return true;
                }

            }
            finally
            {
                if (client.IsConnected)
                {
                    try
                    {
                        client.Disconnect();
                    }
                    catch { }

                }
            }

            return false;
        }

        public static bool Test(MailBox account)
        {
            var ingoingClient = account.Imap ? (BaseProtocolClient) MailClientBuilder.Imap() : MailClientBuilder.Pop();

            var outgoingClient = MailClientBuilder.Smtp();

            if (!string.IsNullOrEmpty(account.RefreshToken))
            {
                return TestOAuth(ingoingClient, account) && TestOAuth(outgoingClient, account);
            }

            Test(ingoingClient, new MailServerSettings
            {
                Url = account.Server,
                Port = account.Port,
                AccountName = account.Account,
                AccountPass = account.Password,
                AuthenticationType = account.AuthenticationTypeIn,
                EncryptionType = account.IncomingEncryptionType,
                MailServerOperationTimeoutInMilliseconds = 10000
            });

            Test(outgoingClient, new MailServerSettings
            {
                Url = account.SmtpServer,
                Port = account.SmtpPort,
                AccountName = account.SmtpAccount,
                AccountPass = account.SmtpPassword,
                AuthenticationType = account.AuthenticationTypeSmtp,
                EncryptionType = account.OutcomingEncryptionType,
                MailServerOperationTimeoutInMilliseconds = 10000
            });

            return true;
        }
    }
}
