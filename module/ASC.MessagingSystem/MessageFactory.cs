/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Web;

using ASC.Common.Logging;
using ASC.Core;

namespace ASC.MessagingSystem
{
    static class MessageFactory
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Messaging");

        public static EventMessage Create(HttpRequest request, string initiator, DateTime? dateTime, MessageAction action, MessageTarget target, params string[] description)
        {
            try
            {
                return new EventMessage
                {
                    IP = MessageSettings.GetFullIPAddress(request),
                    Initiator = initiator,
                    Date = dateTime.HasValue ? dateTime.Value : DateTime.UtcNow,
                    TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    UserId = SecurityContext.CurrentAccount.ID,
                    Page = MessageSettings.GetUrlReferer(request),
                    Action = action,
                    Description = description,
                    Target = target,
                    UAHeader = MessageSettings.GetUAHeader(request)
                };
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while parse Http Request for {0} type of event: {1}", action, ex);
                return null;
            }
        }

        public static EventMessage Create(MessageUserData userData, Dictionary<string, string> headers, MessageAction action, MessageTarget target, string initiator, params string[] description)
        {
            try
            {
                var message = new EventMessage
                {
                    Initiator = initiator,
                    Date = DateTime.UtcNow,
                    TenantId = userData == null ? CoreContext.TenantManager.GetCurrentTenant().TenantId : userData.TenantId,
                    UserId = userData == null ? SecurityContext.CurrentAccount.ID : userData.UserId,
                    Action = action,
                    Description = description,
                    Target = target
                };

                if (headers != null)
                {
                    var ip = MessageSettings.GetFullIPAddress(headers);
                    var userAgent = MessageSettings.GetUAHeader(headers);
                    var referer = MessageSettings.GetReferer(headers);

                    message.IP = ip;
                    message.UAHeader = userAgent;
                    message.Page = referer;
                }

                return message;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while parse Http Message for \"{0}\" type of event: {1}", action, ex));
                return null;
            }
        }

        public static EventMessage Create(string initiator, MessageAction action, MessageTarget target, params string[] description)
        {
            try
            {
                return new EventMessage
                {
                    Initiator = initiator,
                    Date = DateTime.UtcNow,
                    TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    Action = action,
                    Description = description,
                    Target = target
                };
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while parse Initiator Message for \"{0}\" type of event: {1}", action, ex));
                return null;
            }
        }

        public static EventMessage Create(HttpRequest request, MessageUserData userData, MessageAction action)
        {
            try
            {
                var message = new EventMessage
                {
                    Date = DateTime.UtcNow,
                    TenantId = userData == null ? CoreContext.TenantManager.GetCurrentTenant().TenantId : userData.TenantId,
                    UserId = userData == null ? SecurityContext.CurrentAccount.ID : userData.UserId,
                    Action = action,
                    Active = true
                };

                if (request != null)
                {
                    var ip = MessageSettings.GetFullIPAddress(request);
                    var userAgent = MessageSettings.GetUAHeader(request);
                    var referer = MessageSettings.GetReferer(request);

                    message.IP = ip;
                    message.UAHeader = userAgent;
                    message.Page = referer;
                }

                return message;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while parse Initiator Message for \"{0}\" type of event: {1}", action, ex));
                return null;
            }
        }
    }
}