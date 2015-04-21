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
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using ASC.Common.Utils;
using ASC.Notify.Messages;
using ASC.Notify.Patterns;
using log4net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace ASC.Core.Notify.Senders
{
    class SmtpSender : INotifySender
    {
        private const string htmlFormat =
            @"<!DOCTYPE html PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"">
<html>
<head>
<meta content=""text/html;charset=UTF-8"" http-equiv=""Content-Type"">
</head>
<body>{0}</body>
</html>";

        protected ILog Log
        {
            get;
            private set;
        }

        private string host;
        private int port;
        private bool ssl;
        private ICredentialsByHost credentials;
        private bool useCoreSettings;


        public SmtpSender()
        {
            Log = LogManager.GetLogger("ASC.Notify");
        }


        public virtual void Init(IDictionary<string, string> properties)
        {
            if (properties.ContainsKey("useCoreSettings") && bool.Parse(properties["useCoreSettings"]))
            {
                useCoreSettings = true;
            }
            else
            {
                host = properties["host"];
                port = properties.ContainsKey("port") ? int.Parse(properties["port"]) : 25;
                ssl = properties.ContainsKey("enableSsl") ? bool.Parse(properties["enableSsl"]) : false;
                if (properties.ContainsKey("userName"))
                {
                    credentials = new NetworkCredential(
                        properties["userName"],
                        properties["password"],
                        properties.ContainsKey("domain") ? properties["domain"] : string.Empty);
                }
            }
        }

        private void InitUseCoreSettings()
        {
            var s = CoreContext.Configuration.SmtpSettings;
            host = s.Host;
            port = s.Port;
            ssl = s.EnableSSL;
            credentials = new NetworkCredential(s.CredentialsUserName, s.CredentialsUserPassword, s.CredentialsDomain);
        }

        public virtual NoticeSendResult Send(NotifyMessage m)
        {
            CoreContext.TenantManager.SetCurrentTenant(m.Tenant);
            var smtpClient = GetSmtpClient();
            var result = NoticeSendResult.TryOnceAgain;
            try
            {
                try
                {
                    var mail = BuildMailMessage(m);

                    if (WorkContext.IsMono)
                    {
                        ServicePointManager.ServerCertificateValidationCallback = (s, cert, c, p) => true;
                    }
   
                    smtpClient.Send(mail);
                    result = NoticeSendResult.OK;
                }
                catch (Exception e)
                {
                    Log.ErrorFormat("Tenant: {0}, To: {1} - {2}", m.Tenant, m.To, e);
                    throw;
                }
            }
            catch (ArgumentException)
            {
                result = NoticeSendResult.MessageIncorrect;
            }
            catch (ObjectDisposedException)
            {
                result = NoticeSendResult.SendingImpossible;
            }
            catch (InvalidOperationException)
            {
                result = string.IsNullOrEmpty(smtpClient.Host) || smtpClient.Port == 0 ? NoticeSendResult.SendingImpossible : NoticeSendResult.TryOnceAgain;
            }
            catch (SmtpFailedRecipientException e)
            {
                if (e.StatusCode == SmtpStatusCode.MailboxBusy ||
                    e.StatusCode == SmtpStatusCode.MailboxUnavailable ||
                    e.StatusCode == SmtpStatusCode.ExceededStorageAllocation)
                {
                    result = NoticeSendResult.TryOnceAgain;
                }
                else if (e.StatusCode == SmtpStatusCode.MailboxNameNotAllowed ||
                         e.StatusCode == SmtpStatusCode.UserNotLocalWillForward ||
                         e.StatusCode == SmtpStatusCode.UserNotLocalTryAlternatePath)
                {
                    result = NoticeSendResult.MessageIncorrect;
                }
                else if (e.StatusCode != SmtpStatusCode.Ok)
                {
                    result = NoticeSendResult.TryOnceAgain;
                }
            }
            catch (SmtpException)
            {
                result = NoticeSendResult.SendingImpossible;
            }
            finally
            {
                smtpClient.Dispose();
            }
            return result;
        }

        private MailMessage BuildMailMessage(NotifyMessage m)
        {
            var email = new MailMessage
            {
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
                From = MailAddressUtils.Create(m.From),
                Subject = m.Subject,
            };

            foreach (var to in m.To.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                email.To.Add(MailAddressUtils.Create(to));
            }

            if (m.ContentType == Pattern.HTMLContentType)
            {
                email.Body = HtmlUtil.GetText(m.Content);
                email.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(GetHtmlView(m.Content), Encoding.UTF8, "text/html"));
            }
            else
            {
                email.Body = m.Content;
            }

            if (!string.IsNullOrEmpty(m.ReplyTo))
            {
                email.ReplyToList.Add(MailAddressUtils.Create(m.ReplyTo));
            }

            return email;
        }

        protected string GetHtmlView(string body)
        {
            return string.Format(htmlFormat, body);
        }

        private SmtpClient GetSmtpClient()
        {
            if (useCoreSettings)
            {
                InitUseCoreSettings();
            }
            Log.DebugFormat("SmtpSender - host={0}; port={1}; enableSsl={2}", host, port, ssl);
            var smtpClient = new SmtpClient(host, port);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = ssl;
            if (credentials != null)
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = credentials;
            }
            return smtpClient;
        }
    }
}