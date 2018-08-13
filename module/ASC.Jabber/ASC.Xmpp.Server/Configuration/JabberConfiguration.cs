/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Configuration;
using System.Linq;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.utils.Idn;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Services;
using log4net.Config;

namespace ASC.Xmpp.Server.Configuration
{
    public static class JabberConfiguration
    {
        public static bool ReplaceDomain
        {
            get;
            private set;
        }

        public static string ReplaceToDomain
        {
            get;
            private set;
        }

        public static string ReplaceFromDomain
        {
            get;
            private set;
        }

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



            var replaceSetting = ConfigurationManager.AppSettings["jabber.replace-domain"];
            if (!string.IsNullOrEmpty(replaceSetting))
            {
                ReplaceDomain = true;
                var q = replaceSetting.Split(new []{"->"}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLowerInvariant());
                ReplaceFromDomain = q.ElementAt(0);
                ReplaceToDomain = q.ElementAt(1);
            }
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