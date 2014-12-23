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
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using ASC.Core.Notify.Senders;

namespace ASC.Notify.Config
{
    static class NotifyServiceCfg
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

        public static bool DeleteSendedMessages
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
            DeleteSendedMessages = section.DeleteSendedMessages;
        }
    }
}
