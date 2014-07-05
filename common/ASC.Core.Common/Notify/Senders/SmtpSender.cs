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
using System.Net;
using System.Net.Mail;
using System.Text;
using ASC.Common.Utils;
using ASC.Notify.Messages;
using ASC.Notify.Patterns;
using log4net;

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


        public SmtpSender()
        {
            Log = LogManager.GetLogger("ASC.Notify");
        }


        public virtual void Init(IDictionary<string, string> properties)
        {
            if (properties.ContainsKey("useCoreSettings") && bool.Parse(properties["useCoreSettings"]))
            {
                InitUseCoreSettings();
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
            var smtpClient = new SmtpClient(host, port) { Credentials = credentials, EnableSsl = ssl, };
            var result = NoticeSendResult.TryOnceAgain;
            try
            {
                try
                {
                    var mail = BuildMailMessage(m);
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
    }
}