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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace ASC.Xmpp.Server.Gateway
{
	class TcpXmppListener : XmppListenerBase
	{
		private IPEndPoint bindEndPoint = new IPEndPoint(IPAddress.Any, 5222);
		private TcpListener tcpListener;

		private static readonly ILog log = LogManager.GetLogger(typeof(TcpXmppListener));
        private long maxPacket = 1048576; //1024 kb

        public X509Certificate2 Certificate { get; private set; }
        public XmppStartTlsOption StartTls { get; private set; }

	    public override void Configure(IDictionary<string, string> properties)
		{
			try
			{
				if (properties.ContainsKey("bindPort"))
				{
					bindEndPoint = new IPEndPoint(IPAddress.Any, int.Parse(properties["bindPort"]));
				}
				if (properties.ContainsKey("certificate"))
				{
					if (File.Exists(properties["certificate"]))
					{
                        Certificate = !properties.ContainsKey("certificatePassword") ?
                            new X509Certificate2(properties["certificate"]) :
                            new X509Certificate2(properties["certificate"], properties["certificatePassword"]);
                    }
				}
                StartTls = XmppStartTlsOption.None;
                if (properties.ContainsKey("startTls"))
                {
                    StartTls = String.Equals(properties["startTls"], "optional", StringComparison.OrdinalIgnoreCase) ?
                        XmppStartTlsOption.Optional : (String.Equals(properties["startTls"], "required", StringComparison.OrdinalIgnoreCase) ?
                        XmppStartTlsOption.Required : XmppStartTlsOption.None);
                }
                if (StartTls != XmppStartTlsOption.None && Certificate == null)
                {
                    throw new ConfigurationErrorsException(
                        "Wrong configuration of TcpXmppListener! StartTls is Optional or Required but Certificate is null.");
                }
                if (properties.ContainsKey("maxpacket"))
                {
                    long.TryParse(properties["maxpacket"], out maxPacket);
                }
				log.InfoFormat("Configure listener '{0}' on {1}", Name, bindEndPoint);
			}
			catch (Exception e)
			{
				log.ErrorFormat("Error configure listener '{0}': {1}", Name, e);
				throw;
			}
		}

		protected override void DoStart()
		{
			tcpListener = new TcpListener(bindEndPoint);
			tcpListener.Start();
			tcpListener.BeginAcceptSocket(BeginAcceptCallback, null);
		}

		protected override void DoStop()
		{
			tcpListener.Stop();
		}

		private void BeginAcceptCallback(IAsyncResult asyncResult)
        {
            try
            {
                var socket = tcpListener.EndAcceptSocket(asyncResult);
                if (Started)
                {
                    var nossl = Certificate == null || StartTls != XmppStartTlsOption.None;
                    AddNewXmppConnection(nossl ? new TcpXmppConnection(socket, maxPacket) : new TcpSslXmppConnection(socket, maxPacket, Certificate));
                }
            }
            catch (ObjectDisposedException) { return; }
            catch (Exception e)
            {
                log.ErrorFormat("Error listener '{0}' on AcceptCallback: {1}", Name, e);
            }
            finally
            {
                if (Started)
                {
                    try
                    {
                        tcpListener.BeginAcceptSocket(BeginAcceptCallback, null);
                    }
                    catch(Exception e)
                    {
                        log.ErrorFormat("Error listener '{0}' on AcceptCallback: {1}", Name, e);
                    }
                }
            }
        }
    }
}
