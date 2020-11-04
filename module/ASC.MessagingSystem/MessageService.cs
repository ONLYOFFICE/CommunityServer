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


using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using ASC.Common.Logging;
using ASC.MessagingSystem.DbSender;


namespace ASC.MessagingSystem
{
    public static class MessageService
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Messaging");
        private static readonly IMessageSender sender;


        static MessageService()
        {
            if (ConfigurationManagerExtension.AppSettings["messaging.enabled"] != "true")
            {
                return;
            }

            sender = new DbMessageSender();
        }

        #region HttpRequest

        public static void Send(HttpRequest request, MessageAction action)
        {
            SendRequestMessage(request, null, action, null);
        }

        public static void Send(HttpRequest request, MessageAction action, string d1)
        {
            SendRequestMessage(request, null, action, null, d1);
        }

        public static void Send(HttpRequest request, MessageAction action, string d1, string d2)
        {
            SendRequestMessage(request, null, action, null, d1, d2);
        }

        public static void Send(HttpRequest request, MessageAction action, string d1, string d2, string d3)
        {
            SendRequestMessage(request, null, action, null, d1, d2, d3);
        }

        public static void Send(HttpRequest request, MessageAction action, string d1, string d2, string d3, string d4)
        {
            SendRequestMessage(request, null, action, null, d1, d2, d3, d4);
        }

        public static void Send(HttpRequest request, MessageAction action, IEnumerable<string> d1, string d2)
        {
            SendRequestMessage(request, null, action, null, string.Join(", ", d1), d2);
        }

        public static void Send(HttpRequest request, MessageAction action, string d1, IEnumerable<string> d2)
        {
            SendRequestMessage(request, null, action, null, d1, string.Join(", ", d2));
        }

        public static void Send(HttpRequest request, MessageAction action, string d1, string d2, IEnumerable<string> d3)
        {
            SendRequestMessage(request, null, action, null, d1, d2, string.Join(", ", d3));
        }

        public static void Send(HttpRequest request, MessageAction action, IEnumerable<string> d1)
        {
            SendRequestMessage(request, null, action, null, string.Join(", ", d1));
        }

        public static void Send(HttpRequest request, string loginName, MessageAction action)
        {
            SendRequestMessage(request, loginName, action, null);
        }

        public static void Send(HttpRequest request, string loginName, MessageAction action, string d1)
        {
            SendRequestMessage(request, loginName, action, null, d1);
        }

        #endregion

        #region HttpRequest & Target

        public static void Send(HttpRequest request, MessageAction action, MessageTarget target)
        {
            SendRequestMessage(request, null, action, target);
        }

        public static void Send(HttpRequest request, MessageAction action, MessageTarget target, string d1)
        {
            SendRequestMessage(request, null, action, target, d1);
        }

        public static void Send(HttpRequest request, MessageAction action, MessageTarget target, string d1, string d2)
        {
            SendRequestMessage(request, null, action, target, d1, d2);
        }

        public static void Send(HttpRequest request, MessageAction action, MessageTarget target, string d1, string d2, string d3)
        {
            SendRequestMessage(request, null, action, target, d1, d2, d3);
        }

        public static void Send(HttpRequest request, MessageAction action, MessageTarget target, string d1, string d2, string d3, string d4)
        {
            SendRequestMessage(request, null, action, target, d1, d2, d3, d4);
        }

        public static void Send(HttpRequest request, MessageAction action, MessageTarget target, IEnumerable<string> d1, string d2)
        {
            SendRequestMessage(request, null, action, target, string.Join(", ", d1), d2);
        }

        public static void Send(HttpRequest request, MessageAction action, MessageTarget target, string d1, IEnumerable<string> d2)
        {
            SendRequestMessage(request, null, action, target, d1, string.Join(", ", d2));
        }

        public static void Send(HttpRequest request, MessageAction action, MessageTarget target, string d1, string d2, IEnumerable<string> d3)
        {
            SendRequestMessage(request, null, action, target, d1, d2, string.Join(", ", d3));
        }

        public static void Send(HttpRequest request, MessageAction action, MessageTarget target, IEnumerable<string> d1)
        {
            SendRequestMessage(request, null, action, target, string.Join(", ", d1));
        }

        public static void Send(HttpRequest request, string loginName, MessageAction action, MessageTarget target)
        {
            SendRequestMessage(request, loginName, action, target);
        }

        public static void Send(HttpRequest request, string loginName, MessageAction action, MessageTarget target, string d1)
        {
            SendRequestMessage(request, loginName, action, target, d1);
        }

        #endregion

        private static void SendRequestMessage(HttpRequest request, string loginName, MessageAction action, MessageTarget target, params string[] description)
        {
            if (sender == null) return;

            if (request == null)
            {
                log.Debug(string.Format("Empty Http Request for \"{0}\" type of event", action));
                return;
            }

            var message = MessageFactory.Create(request, loginName, action, target, description);
            if (!MessagePolicy.Check(message)) return;

            sender.Send(message);
        }

        #region HttpHeaders

        public static void Send(MessageUserData userData, Dictionary<string, string> httpHeaders, MessageAction action)
        {
            SendHeadersMessage(userData, httpHeaders, action, null);
        }

        public static void Send(Dictionary<string, string> httpHeaders, MessageAction action)
        {
            SendHeadersMessage(null, httpHeaders, action, null);
        }

        public static void Send(Dictionary<string, string> httpHeaders, MessageAction action, string d1)
        {
            SendHeadersMessage(null, httpHeaders, action, null, d1);
        }

        public static void Send(Dictionary<string, string> httpHeaders, MessageAction action, IEnumerable<string> d1)
        {
            SendHeadersMessage(null, httpHeaders, action, null, d1 != null ? d1.ToArray() : null);
        }

        public static void Send(MessageUserData userData, Dictionary<string, string> httpHeaders, MessageAction action, MessageTarget target)
        {
            SendHeadersMessage(userData, httpHeaders, action, target);
        }

        #endregion

        #region HttpHeaders & Target

        public static void Send(Dictionary<string, string> httpHeaders, MessageAction action, MessageTarget target)
        {
            SendHeadersMessage(null, httpHeaders, action, target);
        }

        public static void Send(Dictionary<string, string> httpHeaders, MessageAction action, MessageTarget target, string d1)
        {
            SendHeadersMessage(null, httpHeaders, action, target, d1);
        }

        public static void Send(Dictionary<string, string> httpHeaders, MessageAction action, MessageTarget target, IEnumerable<string> d1)
        {
            SendHeadersMessage(null, httpHeaders, action, target, d1 != null ? d1.ToArray() : null);
        }

        #endregion

        private static void SendHeadersMessage(MessageUserData userData, Dictionary<string, string> httpHeaders, MessageAction action, MessageTarget target, params string[] description)
        {
            if (sender == null) return;

            var message = MessageFactory.Create(userData, httpHeaders, action, target, description);
            if (!MessagePolicy.Check(message)) return;

            sender.Send(message);
        }

        #region Initiator

        public static void Send(HttpRequest request, MessageInitiator initiator, MessageAction action, params string[] description)
        {
            SendInitiatorMessage(request, initiator.ToString(), action, null, description);
        }

        public static void Send(MessageInitiator initiator, MessageAction action, params string[] description)
        {
            SendInitiatorMessage(null, initiator.ToString(), action, null, description);
        }

        #endregion

        #region Initiator & Target

        public static void Send(HttpRequest request, MessageInitiator initiator, MessageAction action, MessageTarget target, params string[] description)
        {
            SendInitiatorMessage(request, initiator.ToString(), action, target, description);
        }

        public static void Send(MessageInitiator initiator, MessageAction action, MessageTarget target, params string[] description)
        {
            SendInitiatorMessage(null, initiator.ToString(), action, target, description);
        }

        #endregion

        private static void SendInitiatorMessage(HttpRequest request, string initiator, MessageAction action, MessageTarget target, params string[] description)
        {
            if (sender == null) return;

            var message = MessageFactory.Create(request, initiator, action, target, description);
            if (!MessagePolicy.Check(message)) return;

            sender.Send(message);
        }
    }
}