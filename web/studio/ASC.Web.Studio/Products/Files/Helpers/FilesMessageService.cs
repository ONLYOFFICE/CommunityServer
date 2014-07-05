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
using System.Configuration;
using System.ServiceModel.Channels;
using System.Web;
using ASC.Core;
using ASC.Files.Core;
using ASC.MessagingSystem;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using UAParser;
using log4net;

namespace ASC.Web.Files.Helpers
{
    public static class FilesMessageService
    {
        private const string unityContainerName = "messaging";
        private static readonly ILog log = LogManager.GetLogger("ASC.Messaging");

        private static readonly IMessageSender sender;

        private static bool MessagingEnabled
        {
            get
            {
                var setting = ConfigurationManager.AppSettings["messaging.enabled"];
                return !string.IsNullOrEmpty(setting) && setting == "true";
            }
        }

        static FilesMessageService()
        {
            if (!MessagingEnabled) return;

            var unitySection = ConfigurationManager.GetSection("unity");
            if (unitySection == null)
            {
                log.Error("Required unity configuration for Message Sender");
                return;
            }

            try
            {
                var unity = new UnityContainer().LoadConfiguration(unityContainerName);
                if (unity.IsRegistered<IMessageSender>())
                {
                    sender = unity.Resolve<IMessageSender>();
                }
                else
                {
                    log.Error("Required unity configuration for Message Sender");
                }
            }
            catch(Exception ex)
            {
                log.Error("Error while resolving Message Sender: " + ex);
            }
        }


        public static void Send(HttpRequestMessageProperty request, MessageAction action)
        {
            SendRequestMessage(request, action, null);
        }

        public static void Send(FileEntry entry, HttpRequestMessageProperty request, MessageAction action, params string[] description)
        {
            // do not log actions in users folder
            if (entry == null || entry.RootFolderType == FolderType.USER) return;

            SendRequestMessage(request, action, description);
        }

        public static void Send(FileEntry entry1, FileEntry entry2, HttpRequestMessageProperty request, MessageAction action, params string[] description)
        {
            // do not log actions in users folder
            if (entry1 == null || entry2 == null || entry1.RootFolderType == FolderType.USER || entry2.RootFolderType == FolderType.USER) return;

            SendRequestMessage(request, action, description);
        }

        private static void SendRequestMessage(HttpRequestMessageProperty request, MessageAction action, params string[] description)
        {
            if (sender == null) return;

            if (request == null)
            {
                log.Debug(string.Format("Empty Request Message for \"{0}\" type of event", action));
                return;
            }

            var message = Create(request, action, description);
            if (!MessagePolicy.Check(message)) return;

            sender.Send(message);
        }

        public static void Send(Dictionary<string, string> headers, MessageAction action)
        {
            SendHeadersMessage(headers, action, null);
        }

        public static void Send(FileEntry entry, Dictionary<string, string> headers, MessageAction action, params string[] description)
        {
            // do not log actions in users folder
            if (entry == null || entry.RootFolderType == FolderType.USER) return;

            SendHeadersMessage(headers, action, description);
        }

        public static void Send(FileEntry entry1, FileEntry entry2, Dictionary<string, string> headers, MessageAction action, params string[] description)
        {
            // do not log actions in users folder
            if (entry1 == null || entry2 == null || entry1.RootFolderType == FolderType.USER || entry2.RootFolderType == FolderType.USER) return;

            SendHeadersMessage(headers, action, description);
        }

        private static void SendHeadersMessage(Dictionary<string, string> headers, MessageAction action, params string[] description)
        {
            if (sender == null) return;

            if (headers == null)
            {
                log.Debug(string.Format("Empty Request Headers for \"{0}\" type of event", action));
                return;
            }

            var message = Create(headers, action, description);
            if (!MessagePolicy.Check(message)) return;

            sender.Send(message);
        }

        public static void Send(FileEntry entry, HttpRequest request, MessageAction action, params string[] description)
        {
            if (sender == null) return;

            // do not log actions in users folder
            if (entry == null || entry.RootFolderType == FolderType.USER) return;

            if (request == null)
            {
                log.Debug(string.Format("Empty Http Request for \"{0}\" type of event", action));
                return;
            }

            var message = Create(request, action, description);
            if (!MessagePolicy.Check(message)) return;

            sender.Send(message);
        }

        public static void Send(FileEntry entry, MessageInitiator initiator, MessageAction action, params string[] description)
        {
            SendInitiatorMessage(entry, initiator, action, description);
        }


        private static void SendInitiatorMessage(FileEntry entry, MessageInitiator initiator, MessageAction action, params string[] description)
        {
            if (sender == null) return;

            // do not log actions in users folder
            if (entry == null || entry.RootFolderType == FolderType.USER) return;

            var message = Create(initiator.ToString(), action, description);
            if (!MessagePolicy.Check(message)) return;

            sender.Send(message);
        }

        private static EventMessage Create(HttpRequestMessageProperty request, MessageAction action, params string[] description)
        {
            try
            {
                var uaParser = Parser.GetDefault();
                var clientInfo = uaParser.Parse(request.Headers["User-Agent"]);

                return new EventMessage
                    {
                        IP = request.Headers["X-Forwarded-For"] ?? request.Headers["Host"],
                        Browser = string.Format("{0} {1}", clientInfo.UserAgent.Family, clientInfo.UserAgent.Major),
                        Platform = string.Format("{0} {1}", clientInfo.OS.Family, clientInfo.OS.Major),
                        Date = DateTime.UtcNow,
                        TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                        UserId = SecurityContext.CurrentAccount.ID,
                        Page = request.Headers["Referer"],
                        Action = action,
                        Description = description
                    };
            }
            catch(Exception ex)
            {
                log.Debug(string.Format("Error while parse Http Message for \"{0}\" type of event: {1}", action, ex));
                return null;
            }
        }

        private static EventMessage Create(Dictionary<string, string> headers, MessageAction action, params string[] description)
        {
            if (!headers.ContainsKey("User-Agent") || !headers.ContainsKey("Host") || !headers.ContainsKey("Referer"))
            {
                log.Debug(string.Format("Empty Request Headers for \"{0}\" type of event", action));
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
            catch(Exception ex)
            {
                log.Debug(string.Format("Error while parse Http Message for \"{0}\" type of event: {1}", action, ex));
                return null;
            }
        }

        private static EventMessage Create(HttpRequest request, MessageAction action, params string[] description)
        {
            try
            {
                return new EventMessage
                    {
                        IP = request.Headers["X-Forwarded-For"] ?? request.UserHostAddress,
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
                log.Debug(string.Format("Error while parse Http Request for \"{0}\" type of event: {1}", action, ex));
                return null;
            }
        }

        private static EventMessage Create(string initiator, MessageAction action, params string[] description)
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
            catch(Exception ex)
            {
                log.Debug(string.Format("Error while parse Http Request for \"{0}\" type of event: {1}", action, ex));
                return null;
            }
        }
    }
}