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


using ASC.HealthCheck.Classes;
using ASC.HealthCheck.Models;
using ASC.HealthCheck.Resources;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web.Http;

namespace ASC.HealthCheck.Controllers
{
    public class PortsCheckApiController : ApiController
    {
        private static readonly object syncRoot = new object();
        private static string host;
        private readonly ILog log = LogManager.GetLogger(typeof(PortsCheckApiController));
        private const int PortsCount = 9;

        private readonly Port[] ports = {
            new Port { Name = "HTTP", Number = 80, AllowClosedForIncomingRequests = false, Description = HealthCheckResource.HttpDescription },
            new Port { Name = "HTTPS", Number = 443, AllowClosedForIncomingRequests = false, Description = HealthCheckResource.HttpsDescription },
            new Port { Name = "SMTP", Number = 25, Description = HealthCheckResource.SmtpDescription },
            new Port { Name = "SMTPS", Number = 465, Description = HealthCheckResource.SmtpsDescription },
            new Port { Name = "IMAP", Number = 143,  Description = HealthCheckResource.ImapDescription },
            new Port { Name = "IMAPS", Number = 993,  Description = HealthCheckResource.ImapsDescription },
            new Port { Name = "POP3", Number = 110, Description = HealthCheckResource.Pop3Description },
            new Port { Name = "POP3S", Number = 995, Description = HealthCheckResource.Pop3sDescription },
            new Port { Name = "XMPP", Number = 5222, AllowClosedForIncomingRequests = false, Description = HealthCheckResource.XmppDescription }
        };

        private readonly Task[] tasks = new Task[PortsCount];

        static PortsCheckApiController()
        {
            try
            {
                host = new ShellExe().ExecuteCommand("dig", "+short myip.opendns.com @resolver1.opendns.com").
                    Replace(Environment.NewLine, string.Empty);
            }
            catch
            {
                host = string.Empty;
            }
        }

        [HttpGet]
        public IList<Port> GetPortList()
        {
            try
            {
                log.DebugFormat("GetPortList host = {0}", host);
                lock (syncRoot)
                {
                    return ports;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unexpected error on GetPortList: {0} {1}",
                    ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return null;
            }
        }

        [HttpGet]
        public IList<JObject> GetPortStatus()
        {
            try
            {
                log.Debug("GetPortStatus");
                lock (syncRoot)
                {
                    for (int i = 0; i < ports.Length; i++)
                    {
                        int j = i;
                        tasks[i] = Task.Run(() =>
                        {
                            CheckPortStatus(j);
                        });
                    }
                    Task.WaitAll(tasks);
                    var portStatuses = new List<JObject>();
                    foreach (var port in ports)
                    {
                        dynamic jsonObject = new JObject();
                        jsonObject.Number = port.Number;
                        jsonObject.PortStatus = port.PortStatus;
                        jsonObject.Status = port.Status;
                        jsonObject.StatusDescription = port.StatusDescription;
                        portStatuses.Add(jsonObject);
                    }
                    return portStatuses;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unexpected error on GetPortStatus: {0} {1}",
                    ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return null;
            }
        }

        private void CheckPortStatus(int j)
        {
            var tuple = GetPortStatus(ports[j]);
            ports[j].PortStatus = tuple.Item1;
            ports[j].StatusDescription = tuple.Item2;
            ports[j].Status = ports[j].PortStatus == PortStatus.Open ?
                HealthCheckResource.PortStatusOpen : HealthCheckResource.PortStatusClose;
        }

        private Tuple<PortStatus, string> GetPortStatus(Port port)
        {
            PortStatus portStatus = PortStatus.Open;
            string statusDescription = HealthCheckResource.PortStatusOpen;
            if (!port.AllowClosedForOutgoingRequests && IsClosedForOutgoingRequests(port.Number))
            {
                portStatus = PortStatus.Closed;
                statusDescription = HealthCheckResource.PortStatusClosedForOutgoingRequests;
            }
            if (!port.AllowClosedForIncomingRequests && IsClosedForIncomingRequests(port.Number))
            {
                portStatus = PortStatus.Closed;
                statusDescription = HealthCheckResource.PortStatusClosedForIncomingRequests;
            }
            return Tuple.Create(portStatus, statusDescription);
        }

        private bool IsClosedForOutgoingRequests(int portNumber)
        {
            return !TryConnect("portquiz.net", portNumber);
        }

        private bool IsClosedForIncomingRequests(int portNumber)
        {
            return !TryConnect(host, portNumber);
        }

        private bool TryConnect(string host, int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                IAsyncResult result = socket.BeginConnect(host, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(7000, true);
                if (success)
                {
                    socket.EndConnect(result);
                    return true;
                }
                else
                {
                    throw new SocketException((int)SocketError.TimedOut); // Connection timed out.
                }
            }
            catch (SocketException ex)
            {
                var socketErrorCode = (SocketError)ex.ErrorCode;
                if (socketErrorCode == SocketError.AccessDenied ||
                    socketErrorCode == SocketError.Fault ||
                    socketErrorCode == SocketError.HostNotFound ||
                    socketErrorCode == SocketError.NetworkUnreachable ||
                    socketErrorCode == SocketError.NotConnected ||
                    socketErrorCode == SocketError.TimedOut ||
                    socketErrorCode == SocketError.AddressNotAvailable)
                {
                    log.DebugFormat("Can't connect to host: {0}, port:{1}. {2}, socketErrorCode = {3}",
                        host, port, ex.ToString(), socketErrorCode);
                    return false;
                }
                log.DebugFormat("Can't connect to host: {0}, port:{1}. {2}, socketErrorCode = {3}, not problem",
                        host, port, ex.ToString(), socketErrorCode);
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unexpected Error: {0}", ex.ToString());
                return false;
            }
            finally
            {
                socket.Close();
            }
        }
    }
}