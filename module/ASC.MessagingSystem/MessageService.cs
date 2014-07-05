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
using System.Web;
using ASC.Api.Impl;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using log4net;

namespace ASC.MessagingSystem
{
    public static class MessageService
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

        static MessageService()
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

        public static void Send(ApiContext context, MessageAction action)
        {
            SendApiMessage(context, null, action);
        }

        public static void Send(ApiContext context, string user, MessageAction action)
        {
            SendApiMessage(context, user, action);
        }

        public static void Send(ApiContext context, MessageAction action, string d1)
        {
            SendApiMessage(context, null, action, d1);
        }

        public static void Send(ApiContext context, MessageAction action, string d1, string d2)
        {
            SendApiMessage(context, null, action, d1, d2);
        }

        public static void Send(ApiContext context, MessageAction action, string d1, string d2, string d3)
        {
            SendApiMessage(context, null, action, d1, d2, d3);
        }

        public static void Send(ApiContext context, MessageAction action, string d1, string d2, string d3, string d4)
        {
            SendApiMessage(context, null, action, d1, d2, d3, d4);
        }

        public static void Send(ApiContext context, MessageAction action, IEnumerable<string> d1)
        {
            SendApiMessage(context, null, action, string.Join(", ", d1));
        }

        public static void Send(ApiContext context, MessageAction action, IEnumerable<string> d1, string d2)
        {
            SendApiMessage(context, null, action, string.Join(", ", d1), d2);
        }

        public static void Send(ApiContext context, MessageAction action, string d1, IEnumerable<string> d2)
        {
            SendApiMessage(context, null, action, d1, string.Join(", ", d2));
        }

        public static void Send(ApiContext context, MessageAction action, string d1, string d2, IEnumerable<string> d3)
        {
            SendApiMessage(context, null, action, d1, d2, string.Join(", ", d3));
        }

        private static void SendApiMessage(ApiContext context, string loginName, MessageAction action, params string[] description)
        {
            if (sender == null) return;

            if (context == null)
            {
                log.Debug(string.Format("Empty Api Context for \"{0}\" type of event", action));
                return;
            }

            var message = MessageFactory.Create(context, loginName, action, description);
            if (!MessagePolicy.Check(message)) return;

            sender.Send(message);
        }


        public static void Send(HttpRequest request, MessageAction action)
        {
            SendRequestMessage(request, null, action);
        }

        public static void Send(HttpRequest request, MessageAction action, string d1)
        {
            SendRequestMessage(request, null, action, d1);
        }

        public static void Send(HttpRequest request, MessageAction action, string d1, string d2)
        {
            SendRequestMessage(request, null, action, d1, d2);
        }

        public static void Send(HttpRequest request, MessageAction action, IEnumerable<string> d1)
        {
            SendRequestMessage(request, null, action, string.Join(", ", d1));
        }

        public static void Send(HttpRequest request, string loginName, MessageAction action)
        {
            SendRequestMessage(request, loginName, action);
        }

        public static void Send(HttpRequest request, string loginName, MessageAction action, string d1)
        {
            SendRequestMessage(request, loginName, action, d1);
        }

        private static void SendRequestMessage(HttpRequest request, string loginName, MessageAction action, params string[] description)
        {
            if (sender == null) return;

            if (request == null)
            {
                log.Debug(string.Format("Empty Http Request for \"{0}\" type of event", action));
                return;
            }

            var message = MessageFactory.Create(request, loginName, action, description);
            if (!MessagePolicy.Check(message)) return;

            sender.Send(message);
        }


        public static void Send(Dictionary<string, string> httpHeaders, MessageAction action, string d1)
        {
            SendHeadersMessage(httpHeaders, action, d1);
        }

        private static void SendHeadersMessage(Dictionary<string, string> httpHeaders, MessageAction action, params string[] description)
        {
            if (sender == null) return;

            if (httpHeaders == null)
            {
                log.Debug(string.Format("Empty Request Headers for \"{0}\" type of event", action));
                return;
            }

            var message = MessageFactory.Create(httpHeaders, action, description);
            if (!MessagePolicy.Check(message)) return;

            sender.Send(message);
        }


        public static void Send(HttpRequest request, MessageInitiator initiator, MessageAction action, params string[] description)
        {
            SendInitiatorMessage(request, initiator.ToString(), action, description);
        }

        public static void Send(MessageInitiator initiator, MessageAction action, params string[] description)
        {
            SendInitiatorMessage(null, initiator.ToString(), action, description);
        }

        private static void SendInitiatorMessage(HttpRequest request, string initiator, MessageAction action, params string[] description)
        {
            if (sender == null) return;

            var message = MessageFactory.Create(request, initiator, action, description);
            if (!MessagePolicy.Check(message)) return;

            sender.Send(message);
        }
    }
}