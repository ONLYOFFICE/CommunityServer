/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Reflection;
using ASC.Mail.Aggregator.Common.Extension;
using ActiveUp.Net.Common;
using ActiveUp.Net.Mail;

namespace ASC.Mail.Aggregator.Common
{
    public static class MailServerHelper
    {
        public static bool TryTestSmtp(MailServerSettings settings, out string last_error)
        {
            try
            {
                last_error = String.Empty;
                return Test(MailClientBuilder.Smtp(), settings);
            }
            catch (Exception ex)
            {
                last_error = ex.Message;
                return false;
            }
        }

        public static bool Test(BaseProtocolClient ingoing_mail_client, MailServerSettings settings)
        {
            try
            {
                var s_result = ingoing_mail_client.Authorize(settings, settings.MailServerOperationTimeoutInMilliseconds);
                if (s_result.ToLower().IndexOf("success", StringComparison.Ordinal) == -1 &&
                    s_result.ToLower().IndexOf("+", StringComparison.Ordinal) == -1 &&
                    s_result.ToLower().IndexOf("ok", StringComparison.Ordinal) == -1)
                {
                    if (ingoing_mail_client is Imap4Client)
                        throw new ImapConnectionException(s_result);
                    if(ingoing_mail_client is Pop3Client)
                        throw new Pop3ConnectionException(s_result);
                    else
                        throw new SmtpConnectionException(s_result);
                }

                return true;
            }
            catch (TargetInvocationException ex_target)
            {
                if (ingoing_mail_client is Imap4Client)
                        throw new ImapConnectionException(ex_target.InnerException.Message);
                    if(ingoing_mail_client is Pop3Client)
                        throw new Pop3ConnectionException(ex_target.InnerException.Message);
                    else
                        throw new SmtpConnectionException(ex_target.InnerException.Message);
            }
            catch (TimeoutException)
            {
                if (ingoing_mail_client is Imap4Client)
                    throw new ImapConnectionTimeoutException();
                if (ingoing_mail_client is Pop3Client)
                    throw new Pop3ConnectionTimeoutException();
                else
                    throw new SmtpConnectionTimeoutException();
            }
            finally
            {
                if (ingoing_mail_client.IsConnected)
                {
                    try
                    {
                        ingoing_mail_client.Disconnect();
                    }
                    catch {}
                    
                }
            }
        }

        public static bool TryTestImap(MailServerSettings settings, out string last_error)
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
            var ingoing_client = account.Imap ? (BaseProtocolClient) MailClientBuilder.Imap() : MailClientBuilder.Pop();

            Test(ingoing_client, new MailServerSettings
            {
                Url = account.Server,
                Port = account.Port,
                AccountName = account.Account,
                AccountPass = account.Password,
                AuthenticationType = account.AuthenticationTypeIn,
                EncryptionType = account.IncomingEncryptionType,
                MailServerOperationTimeoutInMilliseconds = 10000
            });

            var outgoing_client = MailClientBuilder.Smtp();

            Test(outgoing_client, new MailServerSettings
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
