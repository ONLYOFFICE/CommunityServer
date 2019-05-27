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
