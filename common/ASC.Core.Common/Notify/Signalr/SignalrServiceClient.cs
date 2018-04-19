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


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using ASC.Core.Common.Notify.Jabber;
using log4net;
using Newtonsoft.Json;

namespace ASC.Core.Notify.Signalr
{
    public class SignalrServiceClient
    {
        private static readonly TimeSpan Timeout;
        private static readonly ILog Log;
        private static DateTime lastErrorTime;
        public static readonly bool EnableSignalr;
        private static readonly string CoreMachineKey;
        private static readonly string Url;
        private static readonly bool JabberReplaceDomain;
        private static readonly string JabberReplaceFromDomain;
        private static readonly string JabberReplaceToDomain;

        private readonly string hub;

        static SignalrServiceClient()
        {
            Timeout = TimeSpan.FromSeconds(1);
            Log = LogManager.GetLogger(typeof(SignalrServiceClient));
            CoreMachineKey = ConfigurationManager.AppSettings["core.machinekey"];
            Url = ConfigurationManager.AppSettings["web.hub.internal"];
            EnableSignalr = !string.IsNullOrEmpty(Url);

            try
            {
                var replaceSetting = ConfigurationManager.AppSettings["jabber.replace-domain"];
                if (!string.IsNullOrEmpty(replaceSetting))
                {
                    JabberReplaceDomain = true;
                    var q =
                        replaceSetting.Split(new[] {"->"}, StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim().ToLowerInvariant())
                            .ToList();
                    JabberReplaceFromDomain = q.ElementAt(0);
                    JabberReplaceToDomain = q.ElementAt(1);
                }
            }
            catch (Exception)
            {
            }
        }

        public SignalrServiceClient(string hub)
        {
            this.hub = hub.Trim('/');
        }

        public void SendMessage(string callerUserName, string calleeUserName, string messageText, int tenantId,
            string domain)
        {
            try
            {
                domain = ReplaceDomain(domain);
                var tenant = tenantId == -1
                    ? CoreContext.TenantManager.GetTenant(domain)
                    : CoreContext.TenantManager.GetTenant(tenantId);
                var isTenantUser = callerUserName == string.Empty;
                var message = new MessageClass
                {
                    UserName = isTenantUser ? tenant.GetTenantDomain() : callerUserName,
                    Text = messageText
                };

                MakeRequest("send", new {tenantId = tenant.TenantId, callerUserName, calleeUserName, message, isTenantUser});
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
                domain = ReplaceDomain(domain);

                var tenant = CoreContext.TenantManager.GetTenant(domain);

                var message = new MessageClass
                {
                    UserName = tenant.GetTenantDomain(),
                    Text = chatRoomName
                };

                MakeRequest("sendInvite", new {tenantId = tenant.TenantId, calleeUserName, message});
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
                domain = ReplaceDomain(domain);

                if (tenantId == -1)
                {
                    tenantId = CoreContext.TenantManager.GetTenant(domain).TenantId;
                }

                MakeRequest("setState", new {tenantId, from, state});
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
                MakeRequest("sendOfflineMessages", new {tenantId, callerUserName, users});
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
                domain = ReplaceDomain(domain);

                var tenant = CoreContext.TenantManager.GetTenant(domain);

                MakeRequest("sendUnreadCounts", new {tenantId = tenant.TenantId, unreadCounts});
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        public void SendUnreadUsers(Dictionary<int, Dictionary<Guid, int>> unreadUsers)
        {
            try
            {
                MakeRequest("sendUnreadUsers", unreadUsers);
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        public void SendUnreadUser(int tenant, string userId, int count)
        {
            try
            {
                MakeRequest("updateFolders", new {tenant, userId, count});
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
                MakeRequest("sendMailNotification", new {tenant, userId, state});
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        public void EnqueueCall(string numberId, string callId, string agent)
        {
            try
            {
                MakeRequest("enqueue", new { numberId, callId, agent });
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        public void IncomingCall(string callId, string agent)
        {
            try
            {
                MakeRequest("incoming", new { callId, agent });
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        public void MissCall(string numberId, string callId, string agent)
        {
            try
            {
                MakeRequest("miss", new { numberId, callId, agent });
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        public void FilesChangeEditors(int tenantId, string fileId, bool finish)
        {
            try
            {
                MakeRequest("changeEditors", new { tenantId, fileId, finish });
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        public T GetAgent<T>(string numberId, List<Guid> contactsResponsibles)
        {
            try
            {
                return MakeRequest<T>("GetAgent", new { numberId, contactsResponsibles });
            }
            catch (Exception error)
            {
                ProcessError(error);
            }

            return default(T);
        }

        private string ReplaceDomain(string domain)
        {
            if (JabberReplaceDomain && domain.EndsWith(JabberReplaceFromDomain))
            {
                var place = domain.LastIndexOf(JabberReplaceFromDomain);
                if (place >= 0)
                {
                    return domain.Remove(place, JabberReplaceFromDomain.Length).Insert(place, JabberReplaceToDomain);
                }
            }

            return domain;
        }

        private void ProcessError(Exception e)
        {
            Log.ErrorFormat("Service Error: {0}, {1}, {2}", e.Message, e.StackTrace,
                (e.InnerException != null) ? e.InnerException.Message : string.Empty);
            if (e is CommunicationException || e is TimeoutException)
            {
                lastErrorTime = DateTime.Now;
            }
        }

        private string MakeRequest(string method, object data)
        {
            if (!IsAvailable()) return "";

            using (var webClient = new WebClient())
            {
                var jsonData = JsonConvert.SerializeObject(data);
                Log.DebugFormat("Method:{0}, Data:{1}", method, jsonData);
                webClient.Encoding = Encoding.UTF8;
                webClient.Headers.Add("Authorization", CreateAuthToken());
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                return webClient.UploadString(GetMethod(method), jsonData);
            }
        }

        private T MakeRequest<T>(string method, object data)
        {
            var resultMakeRequest = MakeRequest(method, data);
            return JsonConvert.DeserializeObject<T>(resultMakeRequest);
        }

        private bool IsAvailable()
        {
            return EnableSignalr || lastErrorTime + Timeout < DateTime.Now;
        }

        private string GetMethod(string method)
        {
            return string.Format("{0}/controller/{1}/{2}", Url.TrimEnd('/') , hub, method);
        }

        public static string CreateAuthToken(string pkey = "socketio")
        {
            using (var hasher = new HMACSHA1(Encoding.UTF8.GetBytes(CoreMachineKey)))
            {
                var now = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var hash = Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", now, pkey))));
                return string.Format("ASC {0}:{1}:{2}", pkey, now, hash);
            }
        }
    }
}