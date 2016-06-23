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
using System.Text;
using ASC.Notify.Messages;
using System.Threading;
using System.Net.Mail;
using log4net;
using System.ComponentModel;
using System.Configuration;
using ASC.Common.Utils;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace ASC.Notify
{
    class MailAgregator
    {
        private SmtpClient clientSMTP = new SmtpClient();
        private readonly ILog log = LogManager.GetLogger(typeof(NotifyService));
        private const string HtmlForm =
            @"<!DOCTYPE html PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"">
<html>
<head>
  <meta content=""text/html;charset=UTF-8"" http-equiv=""Content-Type"">
</head>
<body>{0}</body>
</html>";

        public MailAgregator()
        {
        }

        #region SMTPclient
        internal void SendBySmtpClient(NotifyMessage m)
        {
            using (var message = BuildMailMessage(m))
            {
                clientSMTP.Send(message);
            }
        }
        #endregion


        #region AmazonSES
        internal void SendByAmazonSES(NotifyMessage m)
        {
            using (var message = BuildMailMessage(m))
            {
                var awsEmail = new AWSEmail();
                awsEmail.SendEmail(message);
            }
        }
        #endregion AmazonSES

        
        private MailMessage BuildMailMessage(NotifyMessage message)
        {
            var email = new MailMessage
            {
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
                From = new MailAddress(message.From),
            };

            var recipients = message.To.Split('|');
            foreach (string address in recipients)
            {
                email.To.Add(address);
            }

            email.ReplyTo = message.ReplayTo!=null ? new MailAddress(message.ReplayTo) : email.From;

            email.Subject = message.Subject.Trim(' ', '\t', '\n', '\r');

            if (message.ContentType == "html")
            {
                email.Body = HtmlUtil.GetText(message.Content);
                var html = String.Format(HtmlForm, message.Content);
                var alternate = AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, "text/html");
                email.AlternateViews.Add(alternate);
            }
            else
            {
                email.Body = message.Content;
            }

            return email;
        }
    }
}
