/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
            logOnly = "log".Equals(ConfigurationManagerExtension.AppSettings["core.notify.postman"], StringComparison.InvariantCultureIgnoreCase);
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