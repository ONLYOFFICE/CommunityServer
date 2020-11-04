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
using ASC.Core.Common.Notify.Jabber;

namespace ASC.Core.Notify.Jabber
{
    public class JabberServiceClient
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(2);

        private static DateTime lastErrorTime = default(DateTime);

        private static bool IsServiceProbablyNotAvailable()
        {
            return lastErrorTime != default(DateTime) && lastErrorTime + Timeout > DateTime.Now;
        }

        public bool SendMessage(int tenantId, string from, string to, string text, string subject)
        {
            if (IsServiceProbablyNotAvailable()) return false;

            using (var service = GetService())
            {
                try
                {
                    service.SendMessage(tenantId, from, to, text, subject);
                    return true;
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
            }

            return false;
        }
        public string GetVersion()
        {
            using (var service = GetService())
            {
                try
                {
                    return service.GetVersion();
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
            }
            return null;
        }
        public int GetNewMessagesCount()
        {
            var result = 0;
            if (IsServiceProbablyNotAvailable()) return result;

            using (var service = GetService())
            {
                try
                {
                    return service.GetNewMessagesCount(GetCurrentTenantId(), GetCurrentUserName());
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
            }

            return result;
        }

        public byte AddXmppConnection(string connectionId, byte state)
        {
            byte result = 4;
            if (IsServiceProbablyNotAvailable()) throw new Exception();

            using (var service = GetService())
            {
                try
                {
                    result = service.AddXmppConnection(connectionId, GetCurrentUserName(), state, GetCurrentTenantId());
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
                return result;
            }
        }

        public byte RemoveXmppConnection(string connectionId)
        {
            const byte result = 4;
            if (IsServiceProbablyNotAvailable()) return result;

            using (var service = GetService())
            {
                try
                {
                    return service.RemoveXmppConnection(connectionId, GetCurrentUserName(), GetCurrentTenantId());
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
            }

            return result;
        }

        public byte GetState(string userName)
        {
            const byte defaultState = 0;

            try
            {
                if (IsServiceProbablyNotAvailable()) return defaultState;
                using (var service = GetService())
                {
                    return service.GetState(GetCurrentTenantId(), userName);
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }

            return defaultState;
        }

        public byte SendState(byte state)
        {
            try
            {
                if (IsServiceProbablyNotAvailable()) throw new Exception();
                using (var service = GetService())
                {
                    return service.SendState(GetCurrentTenantId(), GetCurrentUserName(), state);
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
            return 4;
        }

        public Dictionary<string, byte> GetAllStates()
        {
            Dictionary<string, byte> states = null;
            try
            {
                if (IsServiceProbablyNotAvailable()) throw new Exception();
                using (var service = GetService())
                {
                    states = service.GetAllStates(GetCurrentTenantId(), GetCurrentUserName());
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
            return states;
        }

        public MessageClass[] GetRecentMessages(string to, int id)
        {
            MessageClass[] messages = null;
            try
            {
                if (IsServiceProbablyNotAvailable()) throw new Exception();
                using (var service = GetService())
                {
                    messages = service.GetRecentMessages(GetCurrentTenantId(), GetCurrentUserName(), to, id);
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
            return messages;
        }

        public void Ping(byte state)
        {
            try
            {
                if (IsServiceProbablyNotAvailable()) throw new Exception();
                using (var service = GetService())
                {
                    service.Ping(SecurityContext.CurrentAccount.ID.ToString(), GetCurrentTenantId(), GetCurrentUserName(), state);
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        private static int GetCurrentTenantId()
        {
            return CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }

        private static string GetCurrentUserName()
        {
            return CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).UserName;
        }

        private static void ProcessError(Exception error)
        {
            if (error is FaultException)
            {
                throw error;
            }
            if (error is CommunicationException || error is TimeoutException)
            {
                lastErrorTime = DateTime.Now;
            }
            throw error;
        }

        private JabberServiceClientWcf GetService()
        {
            return new JabberServiceClientWcf();
        }
    }
}
