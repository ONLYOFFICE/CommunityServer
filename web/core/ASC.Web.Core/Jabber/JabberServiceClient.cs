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


using ASC.Core;
using ASC.Core.Notify.Jabber;
using System;
using System.ServiceModel;

namespace ASC.Web.Core.Jabber
{
    public class JabberServiceClient
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);
        private static DateTime lastErrorTime;

        public bool IsAvailable()
        {
            return lastErrorTime + Timeout < DateTime.Now;
        }
        
        public int GetNewMessagesCount(int tenantId, string userName)
        {
            var result = 0;
            if (string.IsNullOrEmpty(userName) || !IsAvailable())
            {
                return result;
            }

            using (var service = GetService())
            {
                try
                {
                    result = service.GetNewMessagesCount(tenantId, userName);
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
            }
            return result;
        }

        public string GetAuthToken(int tenantId)
        {
            var result = string.Empty;
            if (!IsAvailable())
            {
                return result;
            }

            var userName = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).UserName ?? string.Empty;
            if (string.IsNullOrEmpty(userName))
            {
                return result;
            }

            using (var service = GetService())
            {
                try
                {
                    result = Attempt(() => service.GetUserToken(tenantId, userName), 3);
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
            }
            return result;
        }

        public void SendCommand(int tenantId, string from, string to, string command, bool fromTenant)
        {
            if (!IsAvailable()) return;

            using (var service = GetService())
            {
                try
                {
                    service.SendCommand(tenantId, from, to, command, fromTenant);
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
            }
        }

        public void SendMessage(int tenantId, string from, string to, string text)
        {
            try
            {
                if (!IsAvailable()) throw new Exception();
                using (var service = GetService())
                {
                    service.SendMessage(tenantId, from, to, text, null);
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }


        private JabberServiceClientWcf GetService()
        {
            var service =  new JabberServiceClientWcf();
            try
            {
                service.Open();
            }
            catch(Exception error)
            {
                ProcessError(error);
            }
            return service;
        }

        private void ProcessError(Exception error)
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

        private T Attempt<T>(Func<T> f, int count)
        {
            var i = 0;
            while (true)
            {
                try
                {
                    return f();
                }
                catch
                {
                    if (count < ++i)
                    {
                        throw;
                    }
                }
            }
        }
    }
}