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
using ASC.Mail.Aggregator.Common.Logging;
using ActiveUp.Net.Common;
using ActiveUp.Net.Mail;

namespace ASC.Mail.Aggregator.Common.Extension
{
    public static class BaseProtocolClientExtensions
    {
        public static string Authorize(this BaseProtocolClient ingoing_mail_client, MailServerSettings settings, int wait_timeout = 5000, ILogger log = null)
        {
            if (log == null)
                log = new NullLogger();

            string last_response;

            IAsyncResult async_res;
            switch (settings.EncryptionType)
            {
                case EncryptionType.SSL:
                    log.Debug("SSL connecting to {0}", settings.Url);
                    async_res = ingoing_mail_client.BeginConnectSsl(settings.Url, settings.Port,
                                                                    result =>
                                                                    log.Debug("OnConnectSSL: completed {0}",
                                                                              result.IsCompleted ? "SUCCESS" : "FAIL"));
                    if (!async_res.AsyncWaitHandle.WaitOne(wait_timeout))
                    {
                        log.Warn("BeginConnectSsl: operation timeout = {0} seconds",
                                 TimeSpan.FromMilliseconds(wait_timeout).Seconds);
                        ingoing_mail_client.EndAsyncOperation(async_res);
                        async_res.AsyncWaitHandle.Close();
                        throw new TimeoutException();
                    }
                    last_response = ingoing_mail_client.EndAsyncOperation(async_res);

                    break;
                default:
                    log.Debug("PLAIN connecting to {0}", settings.Url);
                    async_res = ingoing_mail_client.BeginConnectPlain(settings.Url, settings.Port, result =>
                                                                                                   log.Debug(
                                                                                                       "OnConnect: completed {0}",
                                                                                                       result
                                                                                                           .IsCompleted
                                                                                                           ? "SUCCESS"
                                                                                                           : "FAIL"));

                    if (!async_res.AsyncWaitHandle.WaitOne(wait_timeout))
                    {
                        log.Warn("BeginConnect: operation timeout = {0} seconds",
                                 TimeSpan.FromMilliseconds(wait_timeout).Seconds);
                        ingoing_mail_client.EndAsyncOperation(async_res);
                        async_res.AsyncWaitHandle.Close();
                        throw new TimeoutException();
                    }
                    last_response = ingoing_mail_client.EndAsyncOperation(async_res);

                    if (ingoing_mail_client is SmtpClient &&
                        (settings.AuthenticationType != SaslMechanism.None ||
                         settings.EncryptionType == EncryptionType.StartTLS))
                    {
                        last_response = ingoing_mail_client.SendEhloHelo();
                    }

                    if (settings.EncryptionType == EncryptionType.StartTLS)
                    {
                        log.Debug("StartTLS {0}", settings.Url);
                        last_response = ingoing_mail_client.StartTLS(settings.Url);
                    }

                    break;
            }

            if (settings.AuthenticationType == SaslMechanism.Login)
            {
                log.Debug("Login as {0} with secret password", settings.AccountName);
                async_res = ingoing_mail_client.BeginLogin(settings.AccountName, settings.AccountPass, result =>
                                                                    log.Debug("OnLogin: completed {0}",
                                                                              result.IsCompleted ? "SUCCESS" : "FAIL"));

                if (!async_res.AsyncWaitHandle.WaitOne(wait_timeout))
                {
                    log.Warn("BeginLogin: operation timeout = {0} seconds", TimeSpan.FromMilliseconds(wait_timeout).Seconds);
                    ingoing_mail_client.EndAsyncOperation(async_res);
                    async_res.AsyncWaitHandle.Close();
                    throw new TimeoutException();
                }

                last_response = ingoing_mail_client.EndAsyncOperation(async_res);
            }
            else
            {
                if (ingoing_mail_client is SmtpClient && settings.AuthenticationType == SaslMechanism.None)
                {
                    log.Debug("Authentication not required");
                    return last_response;
                }

                log.Debug("Authenticate as {0} with secret password", settings.AccountName);
                async_res = ingoing_mail_client.BeginAuthenticate(settings.AccountName, settings.AccountPass, settings.AuthenticationType, result =>
                                                                    log.Debug("OnAuthenticate: completed {0}",
                                                                              result.IsCompleted ? "SUCCESS" : "FAIL"));

                if (!async_res.AsyncWaitHandle.WaitOne(wait_timeout))
                {
                    log.Warn("BeginAuthenticate: operation timeout = {0} seconds", TimeSpan.FromMilliseconds(wait_timeout).Seconds);
                    ingoing_mail_client.EndAsyncOperation(async_res);
                    async_res.AsyncWaitHandle.Close();
                    throw new TimeoutException();
                }

                last_response = ingoing_mail_client.EndAsyncOperation(async_res);
            }

            return last_response;
        }
    }
}
