/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;

namespace ASC.Core.Notify.Signalr
{
    public class SignalrServiceClient
    {
        private static readonly TimeSpan timeout = TimeSpan.FromSeconds(1);
        private static ILog log = LogManager.GetLogger(typeof(SignalrServiceClient));
        private static DateTime lastErrorTime;
        private static string enableSignalr = ConfigurationManager.AppSettings["web.enable-signalr"] ?? "false";
        private static readonly string fromTeamlabToOnlyOffice = ConfigurationManager.AppSettings["jabber.from-teamlab-to-onlyoffice"] ?? "true";
        private static readonly string fromServerInJid = ConfigurationManager.AppSettings["jabber.from-server-in-jid"] ?? "teamlab.com";
        private static readonly string toServerInJid = ConfigurationManager.AppSettings["jabber.to-server-in-jid"] ?? "onlyoffice.com";

        public SignalrServiceClient()
        {
        }

        public SignalrServiceClient(string enSignalr)
        {
            enableSignalr = enSignalr;
        }

        public void SendMessage(string callerUserName, string calleeUserName, string messageText, int tenantId, string domain)
        {
            try
            {
                if (enableSignalr != "true" || !IsAvailable()) return;

                using (var service = new SignalrServiceClientWcf())
                {
                    if (service != null)
                    {
                        if (fromTeamlabToOnlyOffice == "true" && domain.EndsWith(fromServerInJid))
                        {
                            int place = domain.LastIndexOf(fromServerInJid);
                            if (place >= 0)
                            {
                                domain = domain.Remove(place, fromServerInJid.Length).Insert(place, toServerInJid);
                            }
                        }
                        log.DebugFormat("Send Message callerUserName={0}, calleeUserName={1}, messageText={2}, tenantId={3}, domain={4}",
                            callerUserName, calleeUserName, messageText, tenantId, domain);
                        service.SendMessage(callerUserName, calleeUserName, messageText, tenantId, domain);
                    }
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        public void SendInvite(string chatRoomName, string calleeUserName, string domain)
        {
            try
            {
                if (enableSignalr != "true" || !IsAvailable()) return;

                using (var service = new SignalrServiceClientWcf())
                {
                    if (service != null)
                    {
                   
                        if (fromTeamlabToOnlyOffice == "true" && domain.EndsWith(fromServerInJid))
                        {
                            int place = domain.LastIndexOf(fromServerInJid);
                            if (place >= 0)
                            {
                                domain = domain.Remove(place, fromServerInJid.Length).Insert(place, toServerInJid);
                            }
                        }
                        log.DebugFormat("Send Invite chatRoomName={0}, calleeUserName={1}, domain={2}",
                            chatRoomName, calleeUserName, domain);
                        service.SendInvite(chatRoomName, calleeUserName, domain);
                    }
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        public void SendState(string from, byte state, int tenantId, string domain)
        {
            try
            {
                if (enableSignalr != "true" || !IsAvailable()) return;

                using (var service = new SignalrServiceClientWcf())
                {
                    if (service != null)
                    {
                            if (fromTeamlabToOnlyOffice == "true" && domain.EndsWith(fromServerInJid))
                            {
                                int place = domain.LastIndexOf(fromServerInJid);
                                if (place >= 0)
                                {
                                    domain = domain.Remove(place, fromServerInJid.Length).Insert(place, toServerInJid);
                                }
                            }
                            log.DebugFormat("Send State from={0}, state={1}, tenantId={2}, domain={3}", from, state, tenantId, domain);
                            service.SendState(from, state, tenantId, domain);
                    }
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        public void SendOfflineMessages(string callerUserName, List<string> users, int tenantId)
        {
            try
            {
                if (enableSignalr != "true" || !IsAvailable()) return;

                using (var service = new SignalrServiceClientWcf())
                {
                    if (service != null)
                    {
                        log.DebugFormat("SendOfflineMessages callerUserName={0}, tenantId={1}", callerUserName, tenantId);
                    
                            service.SendOfflineMessages(callerUserName, users, tenantId);
                    
                    }
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        public void SendUnreadCounts(Dictionary<string, int> unreadCounts, string domain)
        {
            try
            {
                if (enableSignalr != "true" || !IsAvailable()) return;

                using (var service = new SignalrServiceClientWcf())
                {
                    if (service != null)
                    {
                    
                            if (fromTeamlabToOnlyOffice == "true" && domain.EndsWith(fromServerInJid))
                            {
                                int place = domain.LastIndexOf(fromServerInJid);
                                if (place >= 0)
                                {
                                    domain = domain.Remove(place, fromServerInJid.Length).Insert(place, toServerInJid);
                                }
                            }
                            log.DebugFormat("SendUnreadCounts domain={0}", domain);
                            service.SendUnreadCounts(unreadCounts, domain);
                    }
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        public void SendUnreadUsers(Dictionary<int, HashSet<Guid>> unreadUsers)
        {
            try
            {
                if (enableSignalr != "true" || !IsAvailable()) return;

                using (var service = new SignalrServiceClientWcf())
                {
                    if (service != null)
                    {
                    
                            log.Debug("Send Unread Users");
                            service.SendUnreadUsers(unreadUsers);
                    
                    }
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        public void SendUnreadUser(int tenant, string userId)
        {
            try
            {
                if (enableSignalr != "true" || !IsAvailable()) return;

                using (var service = new SignalrServiceClientWcf())
                {
                    if (service != null)
                    {
                    
                            log.Debug("Send Unread User");
                            service.SendUnreadUser(tenant, userId);
                    
                    }
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        public void SendMailNotification(int tenant, string userId, int state)
        {
            try
            {
                if (enableSignalr != "true" || !IsAvailable()) return;

                using (var service = new SignalrServiceClientWcf())
                {
                    if (service != null)
                    {
                    
                            log.Debug("Send Mail Notification");
                            service.SendMailNotification(tenant, userId, state);
                    }
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        private bool IsAvailable()
        {
            return lastErrorTime + timeout < DateTime.Now;
        }

        private void ProcessError(Exception e)
        {
            log.ErrorFormat("Service Error: {0}, {1}, {2}", e.Message, e.StackTrace, 
                (e.InnerException != null) ? e.InnerException.Message : string.Empty);
            if (e is CommunicationException || e is TimeoutException)
            {
                lastErrorTime = DateTime.Now;
            }
        }
    }
}
