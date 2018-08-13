/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Collections.Generic;
using System.Web;
using ASC.Core;
using log4net;

namespace ASC.MessagingSystem
{
    static class MessageFactory
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Messaging");
        private const string userAgentHeader = "User-Agent";
        private const string forwardedHeader = "X-Forwarded-For";
        private const string hostHeader = "Host";
        private const string refererHeader = "Referer";
        

        public static EventMessage Create(HttpRequest request, string initiator, MessageAction action, MessageTarget target, params string[] description)
        {
            try
            {
                return new EventMessage
                    {
                        IP = request != null ? request.Headers[forwardedHeader] ?? request.UserHostAddress : null,
                        Initiator = initiator,
                        Date = DateTime.UtcNow,
                        TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                        UserId = SecurityContext.CurrentAccount.ID,
                        Page = request != null && request.UrlReferrer != null ? request.UrlReferrer.ToString() : null,
                        Action = action,
                        Description = description,
                        Target = target,
                        UAHeader = request != null ? request.Headers[userAgentHeader] : null
                    };
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while parse Http Request for {0} type of event: {1}", action, ex);
                return null;
            }
        }

        public static EventMessage Create(MessageUserData userData, Dictionary<string, string> headers, MessageAction action, MessageTarget target, params string[] description)
        {
            try
            {
                var message = new EventMessage
                    {
                        Date = DateTime.UtcNow,
                        TenantId = userData == null ? CoreContext.TenantManager.GetCurrentTenant().TenantId : userData.TenantId,
                        UserId = userData == null ? SecurityContext.CurrentAccount.ID : userData.UserId,
                        Action = action,
                        Description = description,
                        Target = target
                    };

                if (headers != null)
                {
                    var userAgent = headers.ContainsKey(userAgentHeader) ? headers[userAgentHeader] : null;
                    var forwarded = headers.ContainsKey(forwardedHeader) ? headers[forwardedHeader] : null;
                    var host = headers.ContainsKey(hostHeader) ? headers[hostHeader] : null;
                    var referer = headers.ContainsKey(refererHeader) ? headers[refererHeader] : null;

                    message.IP = forwarded ?? host;
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


    }
}