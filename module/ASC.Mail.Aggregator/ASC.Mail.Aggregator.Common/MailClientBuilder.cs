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
using System.Web.Configuration;
using ActiveUp.Net.Common;
using ActiveUp.Net.Mail;
using NLog;

namespace ASC.Mail.Aggregator.Common
{
    public class MailClientBuilder
    {
        private enum MailClientType
        {
            Imap,
            Pop3,
            Smtp
        }

        public static Imap4Client Imap()
        {
            return (Imap4Client) BuildMailClient(MailClientType.Imap);
        }

        public static Pop3Client Pop()
        {
            return (Pop3Client)BuildMailClient(MailClientType.Pop3);
        }

        public static SmtpClient Smtp()
        {
            return (SmtpClient)BuildMailClient(MailClientType.Smtp);
        }

        private static BaseProtocolClient BuildMailClient(MailClientType type)
        {
            BaseProtocolClient client;
            switch (type)
            {
                case MailClientType.Imap: 
                    client = new Imap4Client();
                    break;
                case MailClientType.Pop3: 
                    client = new Pop3Client();
                    break;
                case MailClientType.Smtp: 
                    client = new SmtpClient();
                    break;
                default:
                    throw new ArgumentException(String.Format("Unknown client type: {0}", type));
            }

            try
            {
                client.SendTimeout = Convert.ToInt32(WebConfigurationManager.AppSettings["mail.send-tcp-timeout"] ?? "30000");
                client.ReceiveTimeout = Convert.ToInt32(WebConfigurationManager.AppSettings["mail.recieve-tcp-timeout"] ?? "30000");
                client.CertificatePermit = Convert.ToBoolean(WebConfigurationManager.AppSettings["mail.certificate-permit"] ?? "false");
            }
            catch (Exception e)
            {
                client.ReceiveTimeout = 30000;
                client.SendTimeout = 30000;
                client.CertificatePermit = false;

                var logger = LogManager.GetLogger("MailBoxManager");
                var message = String.Format("Problems with config parsing for SendTimeout: {0} or RecieveTimeout: {1}. Values was reseted to default - 30000.\n",
                        WebConfigurationManager.AppSettings["mail.send-tcp-timeout"],
                        WebConfigurationManager.AppSettings["mail.recieve-tcp-timeout"]);
                logger.DebugException(message, e);
            }

            return client;
        }
    }
}
