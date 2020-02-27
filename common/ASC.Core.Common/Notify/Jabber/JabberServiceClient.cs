/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
