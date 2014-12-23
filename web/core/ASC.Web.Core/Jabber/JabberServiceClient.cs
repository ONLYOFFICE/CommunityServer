/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using ASC.Core;
using ASC.Core.Common.Notify.Jabber;
using ASC.Core.Notify.Jabber;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

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