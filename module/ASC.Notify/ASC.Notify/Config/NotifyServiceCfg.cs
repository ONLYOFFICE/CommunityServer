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
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using ASC.Core.Notify.Senders;

namespace ASC.Notify.Config
{
    public static class NotifyServiceCfg
    {
        public static string ConnectionStringName
        {
            get;
            private set;
        }

        public static IDictionary<string, INotifySender> Senders
        {
            get;
            private set;
        }

        public static int MaxThreads
        {
            get;
            private set;
        }

        public static int BufferSize
        {
            get;
            private set;
        }

        public static int MaxAttempts
        {
            get;
            private set;
        }

        public static TimeSpan AttemptsInterval
        {
            get;
            private set;
        }

        public static IDictionary<string, MethodInfo> Schedulers
        {
            get;
            private set;
        }

        public static string ServerRoot
        {
            get;
            private set;
        }

        public static int StoreMessagesDays
        {
            get;
            private set;
        }

        static NotifyServiceCfg()
        {
            var section = ConfigurationManager.GetSection("notify") as NotifyServiceCfgSectionHandler;
            if (section == null)
            {
                throw new ConfigurationErrorsException("Section notify not found.");
            }

            ConnectionStringName = section.ConnectionStringName;
            Senders = new Dictionary<string, INotifySender>();
            foreach (NotifyServiceCfgSenderElement element in section.Senders)
            {
                var sender = (INotifySender)Activator.CreateInstance(Type.GetType(element.Type, true));
                sender.Init(element.Properties);
                Senders.Add(element.Name, sender);
            }
            MaxThreads = section.Process.MaxThreads == 0 ? Environment.ProcessorCount : section.Process.MaxThreads;
            BufferSize = section.Process.BufferSize;
            MaxAttempts = section.Process.MaxAttempts;
            AttemptsInterval = section.Process.AttemptsInterval;

            Schedulers = new Dictionary<string, MethodInfo>();
            foreach (NotifyServiceCfgSchedulerElement element in section.Schedulers)
            {
                var typeName = element.Register.Substring(0, element.Register.IndexOf(','));
                var assemblyName = element.Register.Substring(element.Register.IndexOf(','));
                var type = Type.GetType(typeName.Substring(0, typeName.LastIndexOf('.')) + assemblyName, true);
                Schedulers[element.Name] = type.GetMethod(typeName.Substring(typeName.LastIndexOf('.') + 1), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            }
            ServerRoot = section.Schedulers.ServerRoot;
            StoreMessagesDays = section.StoreMessagesDays;
        }
    }
}
