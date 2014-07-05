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
using System.Collections.Generic;
using System.Web;
using ASC.Api.Impl;
using ASC.Core;
using UAParser;
using log4net;

namespace ASC.MessagingSystem
{
    internal static class MessageFactory
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Messaging");

        public static EventMessage Create(ApiContext apiContext, string initiator, MessageAction action, params string[] description)
        {
            try
            {
                var request = apiContext.RequestContext.HttpContext.Request;
                return new EventMessage
                    {
                        IP = request.Headers["X-Forwarded-For"] ?? request.UserHostAddress,
                        Initiator = initiator,
                        Browser = string.Format("{0} {1}", request.Browser.Browser, request.Browser.Version),
                        Mobile = request.Browser.IsMobileDevice,
                        Platform = request.Browser.Platform,
                        Date = DateTime.UtcNow,
                        TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                        UserId = SecurityContext.CurrentAccount.ID,
                        Page = request.UrlReferrer == null
                                   ? string.Empty :
                                   request.UrlReferrer.ToString(),
                        Action = action,
                        Description = description
                    };
            }
            catch(Exception ex)
            {
                log.Error(string.Format("Error while parse Api Context for \"{0}\" type of event: {1}", action, ex));
                return null;
            }
        }

        public static EventMessage Create(HttpRequest request, string initiator, MessageAction action, params string[] description)
        {
            try
            {
                return new EventMessage
                    {
                        IP = request.Headers["X-Forwarded-For"] ?? request.UserHostAddress,
                        Initiator = initiator,
                        Browser = string.Format("{0} {1}", request.Browser.Browser, request.Browser.Version),
                        Platform = request.Browser.Platform,
                        Date = DateTime.UtcNow,
                        TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                        UserId = SecurityContext.CurrentAccount.ID,
                        Page = request.UrlReferrer == null
                                   ? string.Empty :
                                   request.UrlReferrer.ToString(),
                        Action = action,
                        Description = description
                    };
            }
            catch(Exception ex)
            {
                log.Error(string.Format("Error while parse Http Request for \"{0}\" type of event: {1}", action, ex));
                return null;
            }
        }

        public static EventMessage Create(Dictionary<string, string> headers, MessageAction action, params string[] description)
        {
            if (!headers.ContainsKey("User-Agent") || !headers.ContainsKey("Host") || !headers.ContainsKey("Referer"))
            {
                log.Error(string.Format("Empty Request Headers for \"{0}\" type of event", action));
                return null;
            }

            try
            {
                var uaParser = Parser.GetDefault();
                var clientInfo = uaParser.Parse(headers["User-Agent"]);

                return new EventMessage
                {
                    IP = headers.ContainsKey("X-Forwarded-For") ? headers["X-Forwarded-For"] : headers["Host"],
                    Browser = string.Format("{0} {1}", clientInfo.UserAgent.Family, clientInfo.UserAgent.Major),
                    Platform = string.Format("{0} {1}", clientInfo.OS.Family, clientInfo.OS.Major),
                    Date = DateTime.UtcNow,
                    TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    UserId = SecurityContext.CurrentAccount.ID,
                    Page = headers["Referer"],
                    Action = action,
                    Description = description
                };
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while parse Http Message for \"{0}\" type of event: {1}", action, ex));
                return null;
            }
        }

        public static EventMessage Create(string initiator, MessageAction action, params string[] description)
        {
            try
            {
                return new EventMessage
                {
                    Initiator = initiator,
                    Date = DateTime.UtcNow,
                    TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    Action = action,
                    Description = description
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