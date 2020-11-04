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
using System.Linq;
using System.Net.Sockets;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.Utility;
using AjaxPro;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.Monitoring, Location)]
    [AjaxNamespace("PortsCheckerAjax")]
    public partial class PortsChecker : UserControl
    {
        public const string Location = "~/UserControls/Management/Monitoring/PortsChecker.ascx";

        protected static readonly List<Port> Ports = new List<Port>
            {
                new Port {Name = "HTTP", Number = 80, AllowClosedForIncomingRequests = false},
                new Port {Name = "HTTPS", Number = 443, AllowClosedForIncomingRequests = false},
                new Port {Name = "SMTP", Number = 25},
                new Port {Name = "SMTPS", Number = 465},
                new Port {Name = "IMAP", Number = 143},
                new Port {Name = "IMAPS", Number = 993},
                new Port {Name = "POP3", Number = 110},
                new Port {Name = "POP3S", Number = 995}
            };

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts("~/UserControls/Management/Monitoring/js/portschecker.js")
                .RegisterStyle("~/UserControls/Management/Monitoring/css/monitoring.less");

            HelpLink = CommonLinkUtility.GetHelpLink();
        }

        [AjaxMethod]
        public List<object> GetPortStatusList()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            return Ports
                .Select(port =>
                    {
                        PortStatus status = GetPortStatus(port);
                        return new {name = port.Name, number = port.Number, status = status, statusDescription = GetStatusDescription(status)};
                    })
                .Cast<object>()
                .ToList();
        }

        private PortStatus GetPortStatus(Port port)
        {
            if (!port.AllowClosedForOutgoingRequests && IsClosedForOutgoingRequests(port.Number))
                return PortStatus.Closed;

            if (!port.AllowClosedForIncomingRequests && IsClosedForIncomingRequests(port.Number))
                return PortStatus.Closed;

            return PortStatus.Open;
        }

        private bool IsClosedForOutgoingRequests(int portNumber)
        {
            return !TryConnect("portquiz.net", portNumber);
        }

        private bool IsClosedForIncomingRequests(int portNumber)
        {
            return !TryConnect(CoreContext.TenantManager.GetCurrentTenant().TenantDomain, portNumber);
        }

        private bool TryConnect(string host, int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(host, port);
                return socket.Connected;
            }
            catch
            {
                return false;
            }
            finally
            {
                socket.Close();
            }
        }

        protected string GetStatusDescription(PortStatus status)
        {
            return MonitoringResource.ResourceManager.GetString("PortStatus" + status) ?? status.ToString();
        }

        protected enum PortStatus
        {
            Open,
            Closed
        }

        protected class Port
        {
            public string Name { get; set; }
            public int Number { get; set; }
            public bool AllowClosedForIncomingRequests { get; set; }
            public bool AllowClosedForOutgoingRequests { get; set; }

            public Port()
            {
                AllowClosedForIncomingRequests = true;
                AllowClosedForOutgoingRequests = false;
            }
        }
    }
}