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
using System.Configuration;
using ASC.Notify.Messages;
using log4net;

namespace ASC.Notify.Engine
{
    public class DispatchEngine
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Notify");
        private static readonly ILog logMessages = LogManager.GetLogger("ASC.Notify.Messages");

        private readonly Context context;
        private readonly bool logOnly;


        public DispatchEngine(Context context)
        {
            if (context == null) throw new ArgumentNullException("context");

            this.context = context;
            logOnly = "log".Equals(ConfigurationManager.AppSettings["core.notify.postman"], StringComparison.InvariantCultureIgnoreCase);
            log.DebugFormat("LogOnly: {0}", logOnly);
        }

        public SendResponse Dispatch(INoticeMessage message, string senderName)
        {
            var response = new SendResponse(message, senderName, SendResult.OK);
            if (!logOnly)
            {
                var sender = context.NotifyService.GetSender(senderName);
                if (sender != null)
                {
                    response = sender.DirectSend(message);
                }
                else
                {
                    response = new SendResponse(message, senderName, SendResult.Impossible);
                }

                LogResponce(message, response, sender != null ? sender.SenderName : string.Empty);
            }
            LogMessage(message, senderName);
            return response;
        }
        
        private void LogResponce(INoticeMessage message, SendResponse response, string senderName)
        {
            var logmsg = string.Format("[{0}] sended to [{1}] over {2}, status: {3} ", message.Subject, message.Recipient, senderName, response.Result);
            if (response.Result == SendResult.Inprogress)
            {
                log.Debug(logmsg, response.Exception);
            }
            else if (response.Result == SendResult.Impossible)
            {
                log.Error(logmsg, response.Exception);
            }
            else
            {
                log.Debug(logmsg);
            }
        }
        
        private void LogMessage(INoticeMessage message, string senderName)
        {
            try
            {
                if (logMessages.IsDebugEnabled)
                {
                    logMessages.DebugFormat("[{5}]->[{1}] by [{6}] to [{2}] at {0}\r\n\r\n[{3}]\r\n{4}\r\n{7}",
                        DateTime.Now,
                        message.Recipient.Name,
                        0 < message.Recipient.Addresses.Length ? message.Recipient.Addresses[0] : string.Empty,
                        message.Subject,
                        (message.Body ?? string.Empty).Replace(Environment.NewLine, Environment.NewLine + @"   "),
                        message.Action,
                        senderName,
                        new string('-', 80));
                }
            }
            catch { }
        }
    }
}