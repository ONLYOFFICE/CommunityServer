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
using System.Linq;
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
                var senderNames = Enumerable.Empty<string>();
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
                        log4net.LogManager.GetLogger("ASC.Notify").ErrorFormat("Source: {0}, action: {1}, sender: {2}, error: {3}", source.ID, a.ID, s, error);
                    }
                }
            }
        }
    }
}
