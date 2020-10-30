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
using System.Collections.Generic;
using System.ServiceModel;
using System.Text.RegularExpressions;

using ASC.Common.Logging;
using ASC.Core.Common.Notify;
using ASC.Notify.Messages;

namespace ASC.Core.Notify.Senders
{
    public class TelegramSender : INotifySender
    {
        private static readonly ILog log = LogManager.GetLogger("ASC");

        public void Init(IDictionary<string, string> properties)
        {
        }

        public NoticeSendResult Send(NotifyMessage m)
        {
            if (!string.IsNullOrEmpty(m.Content))
            {
                m.Content = m.Content.Replace("\r\n", "\n").Trim('\n', '\r', ' ');
                m.Content = Regex.Replace(m.Content, "\n{3,}", "\n\n");
            }
            try
            {
                TelegramHelper.Instance.SendMessage(m);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Unexpected error, {0}, {1}, {2}",
                       e.Message, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
            return NoticeSendResult.OK;
        }
    }
}
