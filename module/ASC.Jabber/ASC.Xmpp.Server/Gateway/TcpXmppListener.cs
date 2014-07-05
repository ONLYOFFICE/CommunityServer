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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using log4net;

namespace ASC.Xmpp.Server.Gateway
{
	class TcpXmppListener : XmppListenerBase
	{
		private IPEndPoint bindEndPoint = new IPEndPoint(IPAddress.Any, 5222);
		private X509Certificate certificate;
		private TcpListener tcpListener;

		private static readonly ILog log = LogManager.GetLogger(typeof(TcpXmppListener));
        private long maxPacket = 524288;//512 kb

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
							certificate = X509Certificate.CreateFromCertFile(properties["certificate"]);
						}
						catch { }
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
			tcpListener.BeginAcceptSocket(BeginAcceptCallback, tcpListener);
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

				var tcpListener = (TcpListener)asyncResult.AsyncState;

				tcpListener.BeginAcceptSocket(BeginAcceptCallback, tcpListener);

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
