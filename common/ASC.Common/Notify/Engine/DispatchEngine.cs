/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Configuration;
using ASC.Common.Logging;
using ASC.Notify.Messages;

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