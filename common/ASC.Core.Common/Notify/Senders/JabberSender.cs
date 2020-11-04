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
using System.Text.RegularExpressions;
using ASC.Common.Logging;
using ASC.Core.Notify.Jabber;
using ASC.Notify.Messages;

namespace ASC.Core.Notify.Senders
{
    public class JabberSender : INotifySender
    {
        private JabberServiceClient service = new JabberServiceClient();
        private static readonly ILog log = LogManager.GetLogger("ASC");

        public void Init(IDictionary<string, string> properties)
        {
        }

        public NoticeSendResult Send(NotifyMessage m)
        {
            var text = m.Content;
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Replace("\r\n", "\n").Trim('\n', '\r');
                text = Regex.Replace(text, "\n{3,}", "\n\n");
            }
            try
            {
                service.SendMessage(m.Tenant, null, m.To, text, m.Subject);
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
