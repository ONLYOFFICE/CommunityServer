/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace ASC.Xmpp.Server.Gateway
{
	class TcpXmppListener : XmppListenerBase
	{
		private IPEndPoint bindEndPoint = new IPEndPoint(IPAddress.Any, 5222);
		private X509Certificate2 certificate;
		private TcpListener tcpListener;

		private static readonly ILog log = LogManager.GetLogger(typeof(TcpXmppListener));
        private long maxPacket = 1048576; //1024 kb

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
						try
						{
                            certificate = new X509Certificate2(properties["certificate"], "111111");
						}
						catch
                        {
                        }
					}
				}
                if (properties.ContainsKey("maxpacket"))
                    long.TryParse(properties["maxpacket"], out maxPacket);

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
			tcpListener = null;
		}

		private void BeginAcceptCallback(IAsyncResult asyncResult)
		{
			try
			{
				if (!Started) return;

				tcpListener.BeginAcceptSocket(BeginAcceptCallback, null);

				var socket = tcpListener.EndAcceptSocket(asyncResult);
				AddNewXmppConnection(certificate == null ? new TcpXmppConnection(socket,maxPacket) : new TcpSslXmppConnection(socket, maxPacket, certificate));
			}
			catch (ObjectDisposedException) { return; }
			catch (Exception e)
			{
				log.ErrorFormat("Error listener '{0}' on AcceptCallback: {1}", Name, e);
			}
		}
	}
}
