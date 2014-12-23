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

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/monitoring/js/portschecker.js"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/monitoring/css/monitoring.less"));
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