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
using System.Linq;
using ASC.Common.Logging;
using ASC.Core.Common.Notify.Push;
using ASC.Core.Configuration;
using ASC.Notify.Messages;
using ASC.Notify.Sinks;

namespace ASC.Core.Common.Notify
{
    class PushSenderSink : Sink
    {
        private readonly ILog _log = LogManager.GetLogger("ASC");
        private bool configured = true;


        public override SendResponse ProcessMessage(INoticeMessage message)
        {
            try
            {
                var notification = new PushNotification
                    {
                        Module = GetTagValue<PushModule>(message, PushConstants.PushModuleTagName),
                        Action = GetTagValue<PushAction>(message, PushConstants.PushActionTagName),
                        Item = GetTagValue<PushItem>(message, PushConstants.PushItemTagName),
                        ParentItem = GetTagValue<PushItem>(message, PushConstants.PushParentItemTagName),
                        Message = message.Body,
                        ShortMessage = message.Subject
                    };

                if (configured)
                {
                    try
                    {
                        using (var pushClient = new PushServiceClient())
                        {
                            pushClient.EnqueueNotification(
                                CoreContext.TenantManager.GetCurrentTenant().TenantId,
                                message.Recipient.ID,
                                notification,
                                new List<string>());
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        configured = false;
                        _log.Debug("push sender endpoint is not configured!");
                    }
                }
                else
                {
                    _log.Debug("push sender endpoint is not configured!");
                }

                return new SendResponse(message, Constants.NotifyPushSenderSysName, SendResult.OK);
            }
            catch (Exception error)
            {
                return new SendResponse(message, Constants.NotifyPushSenderSysName, error);
            }
        }

        private T GetTagValue<T>(INoticeMessage message, string tagName)
        {
            var tag = message.Arguments.FirstOrDefault(arg => arg.Tag == tagName);
            return tag != null ? (T)tag.Value : default(T);
        }
    }
}
