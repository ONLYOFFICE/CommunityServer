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


using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.iq.bind;
using ASC.Xmpp.Core.protocol.sasl;
using ASC.Xmpp.Core.protocol.stream;
using ASC.Xmpp.Core.protocol.stream.feature;
using ASC.Xmpp.Core.protocol.tls;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Storage;
using System;
using System.Text;
using Uri = ASC.Xmpp.Core.protocol.Uri;

namespace ASC.Xmpp.Server.Streams
{
	class ClientNamespaceHandler : IXmppStreamStartHandler
	{
		public string Namespace
		{
			get { return Uri.CLIENT; }
		}

		public void StreamStartHandle(XmppStream xmppStream, Stream stream, XmppHandlerContext context)
		{
			var streamHeader = new StringBuilder();
			streamHeader.AppendLine("<?xml version='1.0' encoding='UTF-8'?>");
			streamHeader.AppendFormat("<stream:{0} xmlns:{0}='{1}' xmlns='{2}' from='{3}' id='{4}' version='1.0'>",
				Uri.PREFIX, Uri.STREAM, Uri.CLIENT, stream.To, xmppStream.Id);
            context.Sender.SendTo(xmppStream, streamHeader.ToString());

			var features = new Features();
			features.Prefix = Uri.PREFIX;
			if (xmppStream.Authenticated)
			{
				features.AddChild(new Bind());
				features.AddChild(new Core.protocol.iq.session.Session());
			}
			else
			{
				features.Mechanisms = new Mechanisms();
                var connection = context.Sender.GetXmppConnection(xmppStream.ConnectionId);
                var storage = new DbLdapSettingsStore();
                storage.GetLdapSettings(xmppStream.Domain);
                if (!storage.EnableLdapAuthentication || connection is BoshXmppConnection)
                {
                    features.Mechanisms.AddChild(new Mechanism(MechanismType.DIGEST_MD5));
                }
                else
                {
                    features.Mechanisms.AddChild(new Mechanism(MechanismType.PLAIN));
                }
                features.Mechanisms.AddChild(new Element("required"));
				features.Register = new Register();
                var auth = new Auth();
                auth.Namespace = Uri.FEATURE_IQ_AUTH;
                features.ChildNodes.Add(auth);
                if (connection is TcpXmppConnection)
                {
                    var tcpXmppListener = (TcpXmppListener)(context.XmppGateway.GetXmppListener("Jabber Listener"));
                    if (tcpXmppListener.StartTls != XmppStartTlsOption.None && !((TcpXmppConnection)connection).TlsStarted)
                    {
                        features.StartTls = new StartTls();
                        if (tcpXmppListener.StartTls == XmppStartTlsOption.Required)
                        {
                            features.StartTls.Required = true;
                        }
                    }
                }
			}
            context.Sender.SendTo(xmppStream, features);
		}

		public void OnRegister(IServiceProvider serviceProvider)
		{

		}

		public void OnUnregister(IServiceProvider serviceProvider)
		{

		}
	}
}