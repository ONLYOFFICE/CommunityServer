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

using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;

namespace ASC.Xmpp.Server
{
    public class ReverseJabberServiceClient
    {
        private static readonly TimeSpan timeout = TimeSpan.FromSeconds(1);
        private static ILog _log = LogManager.GetLogger(typeof(ReverseJabberServiceClient));
        private static DateTime lastErrorTime;
        private static readonly string _enableSignalr = ConfigurationManager.AppSettings["web.enable-signalr"] ?? "false";
        private static readonly string fromTeamlabToOnlyOffice = ConfigurationManager.AppSettings["jabber.from-teamlab-to-onlyoffice"] ?? "true";
        private static readonly string fromServerInJid = ConfigurationManager.AppSettings["jabber.from-server-in-jid"] ?? "teamlab.com";
        private static readonly string toServerInJid = ConfigurationManager.AppSettings["jabber.to-server-in-jid"] ?? "onlyoffice.com";

        public void SendMessage(string callerUserName, string calleeUserName, string messageText, int tenantId, string domain)
        {
            if (_enableSignalr != "true" || !IsAvailable()) return;

            using (var service = GetService())
            {
                try
                {
                    if (fromTeamlabToOnlyOffice == "true" && domain.EndsWith(fromServerInJid))
                    {
                        int place = domain.LastIndexOf(fromServerInJid);
                        if (place >= 0)
                        {
                            domain = domain.Remove(place, fromServerInJid.Length).Insert(place, toServerInJid);
                        }
                    }
                    _log.DebugFormat("Send Message callerUserName={0}, calleeUserName={1}, messageText={2}, tenantId={3}, domain={4}",
                        callerUserName, calleeUserName, messageText, tenantId, domain);
                    service.SendMessage(callerUserName, calleeUserName, messageText, tenantId, domain);
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
            }
        }

        public void SendInvite(string chatRoomName, string calleeUserName, string domain)
        {
            if (_enableSignalr != "true" || !IsAvailable()) return;

            using (var service = GetService())
            {
                try
                {
                    if (fromTeamlabToOnlyOffice == "true" && domain.EndsWith(fromServerInJid))
                    {
                        int place = domain.LastIndexOf(fromServerInJid);
                        if (place >= 0)
                        {
                            domain = domain.Remove(place, fromServerInJid.Length).Insert(place, toServerInJid);
                        }
                    }
                    _log.DebugFormat("Send Invite chatRoomName={0}, calleeUserName={1}, domain={2}",
                        chatRoomName, calleeUserName, domain);
                    service.SendInvite(chatRoomName, calleeUserName, domain);
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
            }
        }

        public void SendState(string from, byte state, int tenantId, string domain)
        {
            if (_enableSignalr != "true" || !IsAvailable()) return;

            using (var service = GetService())
            {
                try
                {
                    if (fromTeamlabToOnlyOffice == "true" && domain.EndsWith(fromServerInJid))
                    {
                        int place = domain.LastIndexOf(fromServerInJid);
                        if (place >= 0)
                        {
                            domain = domain.Remove(place, fromServerInJid.Length).Insert(place, toServerInJid);
                        }
                    }
                    _log.DebugFormat("Send State from={0}, state={1}, tenantId={2}, domain{3}", from, state, tenantId, domain);
                    service.SendState(from, state, tenantId, domain);
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
            }
        }

        public void SendOfflineMessages(string callerUserName, List<string> users, int tenantId)
        {
            if (_enableSignalr != "true" || !IsAvailable()) return;

            using (var service = GetService())
            {
                _log.DebugFormat("SendOfflineMessages callerUserName={0}, tenantId={1}", callerUserName, tenantId);
                try
                {
                    service.SendOfflineMessages(callerUserName, users, tenantId);
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
            }
        }

        private bool IsAvailable()
        {
            return lastErrorTime + timeout < DateTime.Now;
        }

        private ReverseJabberServiceClientWcf GetService()
        {
            var service = new ReverseJabberServiceClientWcf();
            try
            {
                service.Open();
            }
            catch (Exception error)
            {
                 ProcessError(error);
            }
            return service;
        }

        private void ProcessError(Exception e)
        {
            _log.ErrorFormat("Service Error: {0}, {1}, {2}", e.Message, e.StackTrace, 
                (e.InnerException != null) ? e.InnerException.Message : string.Empty);
            if (e is CommunicationException || e is TimeoutException)
            {
                lastErrorTime = DateTime.Now;
            }
        }
    }
}
