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
using ASC.Core;
using ASC.Xmpp.Core;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.utils.Idn;
using ASC.Xmpp.Server;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Services;
using ASC.Xmpp.Server.Streams;
using Stream = ASC.Xmpp.Core.protocol.Stream;
using Uri = ASC.Xmpp.Core.protocol.Uri;
using System.Configuration;

namespace ASC.Xmpp.Host
{
	[XmppHandler(typeof(Stanza))]
	class CreatorStartStreamHandler : IXmppStreamStartHandler
	{
		private readonly Dictionary<string, Type> templates = new Dictionary<string, Type>();

		private XmppServiceManager serviceManager;

		private XmppHandlerManager handlerManager;


		public string Namespace
		{
			get { return Uri.CLIENT; }
		}


		public CreatorStartStreamHandler(Dictionary<string, Type> instanceTemplate)
		{
			this.templates = instanceTemplate;
		}

		public void StreamStartHandle(XmppStream xmppStream, Stream stream, XmppHandlerContext context)
		{
			lock (this)
			{
				//Check tennats here
				if (ValidateHost(stream.To))
				{
					//Create new services
					foreach (var template in templates)
					{
						var service = (IXmppService)Activator.CreateInstance(template.Value);
						service.Jid = new Jid(Stringprep.NamePrep(string.Format("{0}.{1}", template.Key, stream.To.Server).Trim('.')));

						if (serviceManager.GetService(service.Jid) != null)
						{
							continue;
						}

						service.Name = service.Jid.ToString();
						if (!string.IsNullOrEmpty(template.Key))
						{
							service.ParentService = serviceManager.GetService(new Jid(Stringprep.NamePrep(stream.To.Server)));
						}
						service.Configure(new Dictionary<string, string>());
						serviceManager.RegisterService(service);
					}
					//Reroute
					handlerManager.ProcessStreamStart(stream, Uri.CLIENT, xmppStream);
				}
				else
				{
					context.Sender.SendToAndClose(xmppStream, XmppStreamError.HostUnknown);
				}
			}
		}

		public void OnRegister(IServiceProvider serviceProvider)
		{
			serviceManager = (XmppServiceManager)serviceProvider.GetService(typeof(XmppServiceManager));
			handlerManager = (XmppHandlerManager)serviceProvider.GetService(typeof(XmppHandlerManager));
		}

		public void OnUnregister(IServiceProvider serviceProvider)
		{

		}

		private bool ValidateHost(Jid jid)
		{
			if (jid != null && jid.IsServer)
			{
                // for migration from teamlab.com to onlyoffice.com
                if ((ConfigurationManager.AppSettings["jabber.from-teamlab-to-onlyoffice"] ?? "false") == "true")
                {
                    string fromServerInJid = ConfigurationManager.AppSettings["jabber.from-server-in-jid"] ?? "teamlab.com";
                    if (jid.Server.EndsWith(fromServerInJid) &&
                        CoreContext.TenantManager.GetTenant(jid.Server.Replace(fromServerInJid,
                            ConfigurationManager.AppSettings["jabber.to-server-in-jid"] ?? "onlyoffice.com")) != null)
                    {
                        return true;
                    }
                }
                else
                {
                    if (CoreContext.TenantManager.GetTenant(jid.Server) != null)
                    {
                        return true;
                    }
                }
			}
			return false;
		}
	}


	public class CreatorService : XmppServiceBase
	{
		public override void Configure(IDictionary<string, string> properties)
		{
			var template = new Dictionary<string, Type>();
			foreach (var pair in properties)
			{
				template.Add(pair.Key, Type.GetType(pair.Value, true));
			}

			Handlers.Add(new CreatorStartStreamHandler(template));
		}
	}
}