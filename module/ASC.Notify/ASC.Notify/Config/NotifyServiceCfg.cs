/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
            var section = ConfigurationManagerExtension.GetSection("notify") as NotifyServiceCfgSectionHandler;
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
