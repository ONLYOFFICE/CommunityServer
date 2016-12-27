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


using ASC.Core;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.utils.Idn;
using ASC.Xmpp.Server;
using ASC.Xmpp.Server.Configuration;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Services;
using ASC.Xmpp.Server.Streams;
using System;
using System.Collections.Generic;
using System.Configuration;
using Stream = ASC.Xmpp.Core.protocol.Stream;
using Uri = ASC.Xmpp.Core.protocol.Uri;

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
                if (JabberConfiguration.ReplaceDomain)
                {
                    if (jid.Server.EndsWith(JabberConfiguration.ReplaceFromDomain) &&
                        CoreContext.TenantManager.GetTenant(jid.Server.Replace(JabberConfiguration.ReplaceFromDomain, JabberConfiguration.ReplaceToDomain)) != null)
                    {
                        return true;
                    }
                    if (!jid.Server.EndsWith(JabberConfiguration.ReplaceToDomain) && CoreContext.TenantManager.GetTenant(jid.Server) != null)
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