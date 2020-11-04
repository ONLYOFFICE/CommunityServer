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
using System.Linq;
using System.Web;

using ASC.Core;
using ASC.Web.Studio.Utility;
using ASC.Web.Talk.Addon;

namespace ASC.Web.Talk
{
    class TalkConfiguration
    {
        public string ServerAddress
        {
            get;
            private set;
        }

        public string UpdateInterval
        {
            get;
            private set;
        }

        public string OverdueInterval
        {
            get;
            private set;
        }

        public string ServerName
        {
            get;
            private set;
        }

        public string ServerPort
        {
            get;
            private set;
        }

        public string BoshUri
        {
            get;
            private set;
        }

        public string UserName
        {
            get;
            private set;
        }

        public string Jid
        {
            get;
            private set;
        }

        public string FileTransportType
        {
            get;
            private set;
        }

        public string RequestTransportType
        {
            get;
            private set;
        }

        public bool EnabledFirebugLite
        {
            get;
            private set;
        }

        public bool EnabledHistory
        {
            get;
            private set;
        }

        public bool EnabledConferences
        {
            get;
            private set;
        }

        public bool EnabledMassend
        {
            get;
            private set;
        }

        public String ValidSymbols
        {
            get;
            private set;
        }

        public String HistoryLength
        {
            get;
            private set;
        }

        public String ResourcePriority
        {
            get;
            private set;
        }

        public String ClientInactivity
        {
            get;
            private set;
        }

        // for migration from teamlab.com to onlyoffice.com
        public bool ReplaceDomain
        {
            get;
            private set;
        }

        public String ReplaceFromDomain
        {
            get;
            private set;
        }

        public String ReplaceToDomain
        {
            get;
            private set;
        }

        public TalkConfiguration()
        {
            RequestTransportType = ConfigurationManagerExtension.AppSettings["RequestTransportType"] ?? "flash";

            // for migration from teamlab.com to onlyoffice.com
            var replaceSetting = ConfigurationManagerExtension.AppSettings["jabber.replace-domain"];
            if (!string.IsNullOrEmpty(replaceSetting))
            {
                ReplaceDomain = true;
                var q = replaceSetting.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLowerInvariant());
                ReplaceFromDomain = q.ElementAt(0);
                ReplaceToDomain = q.ElementAt(1);
            }

            ServerAddress = new Uri(CommonLinkUtility.ServerRootPath).Host;
            ServerAddress = ReplaceToOldDomain(ServerAddress);
            ServerName = CoreContext.TenantManager.GetCurrentTenant().TenantDomain;
            ServerName = ReplaceToOldDomain(ServerName);
            ServerPort = ConfigurationManagerExtension.AppSettings["JabberPort"] ?? "5222";
            BoshUri = ConfigurationManagerExtension.AppSettings["BoshPath"] ?? "http://localhost:5280/http-poll/";
            if (RequestTransportType == "handler")
            {
                BoshUri = VirtualPathUtility.ToAbsolute(TalkAddon.BaseVirtualPath + "/http-poll/httppoll.ashx");
            }
            else
            {
                BoshUri = string.Format(BoshUri, ServerAddress);
            }

            try
            {
                UserName = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).UserName.ToLowerInvariant();
            }
            catch
            {
                UserName = string.Empty;
            }
            Jid = string.Format("{0}@{1}", UserName, ServerName).ToLowerInvariant();
            FileTransportType = ConfigurationManagerExtension.AppSettings["FileTransportType"] ?? "flash";
            // in seconds
            UpdateInterval = ConfigurationManagerExtension.AppSettings["UpdateInterval"] ?? "3600";
            OverdueInterval = ConfigurationManagerExtension.AppSettings["OverdueInterval"] ?? "60";

            EnabledHistory = (ConfigurationManagerExtension.AppSettings["History"] ?? "on") == "on";
            EnabledMassend = (ConfigurationManagerExtension.AppSettings["Massend"] ?? "on") == "on";
            EnabledConferences = (ConfigurationManagerExtension.AppSettings["Conferences"] ?? "on") == "on";
            EnabledFirebugLite = (ConfigurationManagerExtension.AppSettings["FirebugLite"] ?? "off") == "on";
            ValidSymbols = ConfigurationManagerExtension.AppSettings["ValidSymbols"] ?? "äöüßña-žа-яё";
            HistoryLength = ConfigurationManagerExtension.AppSettings["HistoryLength"] ?? "20";
            ResourcePriority = ConfigurationManagerExtension.AppSettings["ResourcePriority"] ?? "60";
            ClientInactivity = ConfigurationManagerExtension.AppSettings["ClientInactivity"] ?? "30";
        }


        private string ReplaceToOldDomain(string orig)
        {
            if (ReplaceDomain && orig != null && orig.EndsWith(ReplaceToDomain))
            {
                var place = orig.LastIndexOf(ReplaceToDomain);
                if (place >= 0)
                {
                    return orig.Remove(place, ReplaceToDomain.Length).Insert(place, ReplaceFromDomain);
                }
            }
            return orig;
        }
    }
}