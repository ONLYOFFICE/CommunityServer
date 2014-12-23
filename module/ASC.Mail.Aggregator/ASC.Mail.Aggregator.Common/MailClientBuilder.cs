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
            BaseProtocolClient client = null;
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
                client.SendTimeout = Convert.ToInt32(WebConfigurationManager.AppSettings["mail.SendTcpTimeout"]);
                client.ReceiveTimeout = Convert.ToInt32(WebConfigurationManager.AppSettings["mail.RecieveTcpTimeout"]);
            }
            catch (Exception e)
            {
                client.ReceiveTimeout = 30000;
                client.SendTimeout = 30000;

                var logger = LogManager.GetLogger("MailBoxManager");
                var message = String.Format("Problems with config parsing for SendTimeout: {0} or RecieveTimeout: {1}. Values was reseted to default - 30000.\n",
                        WebConfigurationManager.AppSettings["mail.SendTcpTimeout"],
                        WebConfigurationManager.AppSettings["mail.RecieveTcpTimeout"]);
                logger.DebugException(message, e);
            }

            return client;
        }
    }
}
