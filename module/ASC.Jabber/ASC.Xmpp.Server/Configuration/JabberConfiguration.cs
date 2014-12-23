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
using System.Configuration;
using ASC.Xmpp.Core;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.utils.Idn;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Services;
using log4net.Config;

namespace ASC.Xmpp.Server.Configuration
{
	public static class JabberConfiguration
	{
		public static void Configure(XmppServer server)
		{
			Configure(server, null);
		}

		public static void Configure(XmppServer server, string configFile)
		{
			XmlConfigurator.Configure();

			var jabberSection = GetSection(configFile);

			ConfigureListeners(jabberSection, server);
			ConfigureStorages(jabberSection, server);
			ConfigureServices(jabberSection, server);
		}

		private static void ConfigureServices(JabberConfigurationSection jabberSection, XmppServer server)
		{
			foreach (ServiceConfigurationElement se in jabberSection.Services)
			{
				var service = (IXmppService)Activator.CreateInstance(Type.GetType(se.TypeName, true));
				service.Jid = new Jid(Stringprep.NamePrep(se.Jid));
				service.Name = se.Name;
				if (!string.IsNullOrEmpty(se.Parent))
				{
					service.ParentService = server.GetXmppService(new Jid(Stringprep.NamePrep(se.Parent)));
				}
				service.Configure(se.GetProperties());
				server.RegisterXmppService(service);
			}
		}

		private static void ConfigureStorages(JabberConfigurationSection jabberSection, XmppServer server)
		{
			foreach (JabberConfigurationElement se in jabberSection.Storages)
			{
				var storage = Activator.CreateInstance(Type.GetType(se.TypeName, true));
				if (storage is IConfigurable) ((IConfigurable)storage).Configure(se.GetProperties());
				server.StorageManager.SetStorage(se.Name, storage);
			}
		}

		private static void ConfigureListeners(JabberConfigurationSection jabberSection, XmppServer server)
		{
			foreach (JabberConfigurationElement le in jabberSection.Listeners)
			{
				var listener = (IXmppListener)Activator.CreateInstance(Type.GetType(le.TypeName, true));
				listener.Name = le.Name;
				listener.Configure(le.GetProperties());
				server.AddXmppListener(listener);
			}
		}


		private static JabberConfigurationSection GetSection(string configFile)
		{
			if (string.IsNullOrEmpty(configFile))
			{
				return (JabberConfigurationSection)ConfigurationManager.GetSection(Schema.SECTION_NAME);
			}

			var cfg = ConfigurationManager.OpenMappedExeConfiguration(
				new ExeConfigurationFileMap() { ExeConfigFilename = configFile },
				ConfigurationUserLevel.None
			);
			return (JabberConfigurationSection)cfg.GetSection(Schema.SECTION_NAME);
		}
	}
}