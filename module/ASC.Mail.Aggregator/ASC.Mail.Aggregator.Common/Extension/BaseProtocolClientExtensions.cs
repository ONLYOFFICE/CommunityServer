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
using ASC.Mail.Aggregator.Common.Logging;
using ActiveUp.Net.Common;
using ActiveUp.Net.Mail;

namespace ASC.Mail.Aggregator.Common.Extension
{
    public static class BaseProtocolClientExtensions
    {
        public static string Authorize(this BaseProtocolClient ingoingMailClient, MailServerSettings settings, int waitTimeout = 5000, ILogger log = null)
        {
            if (log == null)
                log = new NullLogger();

            string lastResponse;
            switch (settings.EncryptionType)
            {
                case EncryptionType.SSL:
                    var timeout = TimeSpan.FromMinutes(3); // 3 minutes
                    log.Debug("SSL connecting to {0} (timeout = {1} minutes)", settings.Url, timeout.TotalMinutes);
                    lastResponse = ingoingMailClient.ConnectSsl(settings.Url, settings.Port, (int)timeout.TotalMilliseconds);

                    break;
                default:
                    log.Debug("PLAIN connecting to {0}", settings.Url);
                    lastResponse = ingoingMailClient.ConnectPlain(settings.Url, settings.Port);

                    if (ingoingMailClient is SmtpClient &&
                        (settings.AuthenticationType != SaslMechanism.None ||
                         settings.EncryptionType == EncryptionType.StartTLS))
                    {
                        lastResponse = ingoingMailClient.SendEhloHelo();
                    }

                    if (settings.EncryptionType == EncryptionType.StartTLS)
                    {
                        log.Debug("StartTLS {0}", settings.Url);
                        lastResponse = ingoingMailClient.StartTLS(settings.Url);
                    }

                    break;
            }

            if (settings.AuthenticationType == SaslMechanism.Login)
            {
                log.Debug("Login as {0} with secret password", settings.AccountName);
                lastResponse = ingoingMailClient.Login(settings.AccountName, settings.AccountPass);
            }
            else
            {
                if (ingoingMailClient is SmtpClient && settings.AuthenticationType == SaslMechanism.None)
                {
                    log.Debug("Authentication not required");
                    return lastResponse;
                }

                log.Debug("Authenticate as {0} with secret password", settings.AccountName);
                lastResponse = ingoingMailClient.Authenticate(settings.AccountName, settings.AccountPass, settings.AuthenticationType);

            }

            return lastResponse;
        }
    }
}
