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
using System.Linq;
using ASC.Common.Logging;
using ASC.Notify.Channels;
using ASC.Notify.Engine;
using ASC.Notify.Model;
using ASC.Notify.Sinks;

namespace ASC.Notify
{
    public sealed class Context : INotifyRegistry
    {
        public const string SYS_RECIPIENT_ID = "_#" + _SYS_RECIPIENT_ID + "#_";
        internal const string _SYS_RECIPIENT_ID = "SYS_RECIPIENT_ID";
        internal const string _SYS_RECIPIENT_NAME = "SYS_RECIPIENT_NAME";
        internal const string _SYS_RECIPIENT_ADDRESS = "SYS_RECIPIENT_ADDRESS";

        private readonly Dictionary<string, ISenderChannel> channels = new Dictionary<string, ISenderChannel>(2);


        public NotifyEngine NotifyEngine
        {
            get;
            private set;
        }

        public INotifyRegistry NotifyService
        {
            get { return this; }
        }

        public DispatchEngine DispatchEngine
        {
            get;
            private set;
        }


        public event Action<Context, INotifyClient> NotifyClientRegistration;


        public Context()
        {
            NotifyEngine = new NotifyEngine(this);
            DispatchEngine = new DispatchEngine(this);
        }


        void INotifyRegistry.RegisterSender(string senderName, ISink senderSink)
        {
            lock (channels)
            {
                channels[senderName] = new SenderChannel(this, senderName, null, senderSink);
            }
        }

        void INotifyRegistry.UnregisterSender(string senderName)
        {
            lock (channels)
            {
                channels.Remove(senderName);
            }
        }

        ISenderChannel INotifyRegistry.GetSender(string senderName)
        {
            lock (channels)
            {
                ISenderChannel channel;
                channels.TryGetValue(senderName, out channel);
                return channel;
            }
        }

        INotifyClient INotifyRegistry.RegisterClient(INotifySource source)
        {
            //ValidateNotifySource(source);
            var client = new NotifyClientImpl(this, source);
            if (NotifyClientRegistration != null)
            {
                NotifyClientRegistration(this, client);
            }
            return client;
        }


        private void ValidateNotifySource(INotifySource source)
        {
            foreach (var a in source.GetActionProvider().GetActions())
            {
                IEnumerable<string> senderNames;
                lock (channels)
                {
                    senderNames = channels.Values.Select(s => s.SenderName);
                }
                foreach (var s in senderNames)
                {
                    try
                    {
                        var pattern = source.GetPatternProvider().GetPattern(a, s);
                        if (pattern == null)
                        {
                            throw new NotifyException(string.Format("In notify source {0} pattern not found for action {1} and sender {2}", source.ID, a.ID, s));
                        }
                    }
                    catch (Exception error)
                    {
                        LogManager.GetLogger("ASC.Notify").ErrorFormat("Source: {0}, action: {1}, sender: {2}, error: {3}", source.ID, a.ID, s, error);
                    }
                }
            }
        }
    }
}
