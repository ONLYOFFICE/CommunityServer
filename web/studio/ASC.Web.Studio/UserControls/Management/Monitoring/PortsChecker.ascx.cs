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
            Page.RegisterBodyScripts("~/usercontrols/management/monitoring/js/portschecker.js");
            Page.RegisterStyle("~/usercontrols/management/monitoring/css/monitoring.less");

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