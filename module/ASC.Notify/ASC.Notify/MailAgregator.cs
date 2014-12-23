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
