/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Web;
using System.Web.Configuration;
using ASC.Core;
using ASC.Web.Studio.Utility;
using ASC.Web.Talk.Addon;
using log4net;

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
        public String FromTeamlabToOnlyOffice
        {
            get;
            private set;
        }

        public String FromServerInJid
        {
            get;
            private set;
        }

        public String ToServerInJid
        {
            get;
            private set;
        }

        public TalkConfiguration()
        {
            RequestTransportType = WebConfigurationManager.AppSettings["RequestTransportType"] ?? "flash";

            // for migration from teamlab.com to onlyoffice.com
            FromTeamlabToOnlyOffice = WebConfigurationManager.AppSettings["FromTeamlabToOnlyOffice"] ?? "true";
            FromServerInJid = WebConfigurationManager.AppSettings["FromServerInJid"] ?? "teamlab.com";
            ToServerInJid = WebConfigurationManager.AppSettings["ToServerInJid"] ??"onlyoffice.com";
            ServerAddress = CoreContext.TenantManager.GetCurrentTenant().TenantDomain;
            if (FromTeamlabToOnlyOffice == "true" && ServerAddress != null && ServerAddress.EndsWith(ToServerInJid))
            {
                int place = ServerAddress.LastIndexOf(ToServerInJid);
                if (place >= 0)
                {
                    ServerAddress = ServerAddress.Remove(place, ToServerInJid.Length).Insert(place, FromServerInJid);
                }
            }
            ServerName = ServerAddress;
            ServerPort = WebConfigurationManager.AppSettings["JabberPort"] ?? 5222.ToString();
            BoshUri = WebConfigurationManager.AppSettings["BoshPath"] ?? "http://localhost:5280/http-poll/";
            if (RequestTransportType == "handler")
            {
                BoshUri = VirtualPathUtility.ToAbsolute(TalkAddon.BaseVirtualPath + "/http-poll/default.aspx");
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
            FileTransportType = WebConfigurationManager.AppSettings["FileTransportType"] ?? "flash";
            // in seconds
            UpdateInterval = WebConfigurationManager.AppSettings["UpdateInterval"] ?? "3600";
            OverdueInterval = WebConfigurationManager.AppSettings["OverdueInterval"] ?? "60";

            EnabledHistory = (WebConfigurationManager.AppSettings["History"] ?? "on") == "on";
            EnabledMassend = (WebConfigurationManager.AppSettings["Massend"] ?? "on") == "on";
            EnabledConferences = (WebConfigurationManager.AppSettings["Conferences"] ?? "on") == "on";
            EnabledFirebugLite = (WebConfigurationManager.AppSettings["FirebugLite"] ?? "off") == "on";
            ValidSymbols = WebConfigurationManager.AppSettings["ValidSymbols"] ?? "äöüßña-žа-я";
            HistoryLength = WebConfigurationManager.AppSettings["HistoryLength"] ?? "10";
            ResourcePriority = WebConfigurationManager.AppSettings["ResourcePriority"] ?? "60";
            ClientInactivity = WebConfigurationManager.AppSettings["ClientInactivity"] ?? "90";
        }
    }
}