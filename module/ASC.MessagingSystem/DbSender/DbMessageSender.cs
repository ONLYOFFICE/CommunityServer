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

namespace ASC.MessagingSystem.DbSender
{
    public class DbMessageSender : IMessageSender
    {
        private readonly ILog log = LogManager.GetLogger("ASC.Messaging");

        private static bool MessagingEnabled
        {
            get
            {
                var setting = ConfigurationManagerExtension.AppSettings["messaging.enabled"];
                return !string.IsNullOrEmpty(setting) && setting == "true";
            }
        }


        public void Send(EventMessage message)
        {
            try
            {
                if (!MessagingEnabled) return;
                
                if (message == null) return;
                
                MessagesRepository.Add(message);
            }
            catch(Exception ex)
            {
                log.Error("Failed to send a message", ex);
            }
        }
    }
}