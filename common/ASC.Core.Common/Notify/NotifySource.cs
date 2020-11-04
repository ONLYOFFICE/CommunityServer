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
using System.Globalization;
using System.Threading;
using ASC.Common.Module;
using ASC.Notify;
using ASC.Notify.Messages;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;

namespace ASC.Core.Notify
{
    public abstract class NotifySource : INotifySource
    {
        private readonly object syncRoot = new object();
        private bool initialized;

        private readonly IDictionary<CultureInfo, IActionProvider> actions = new Dictionary<CultureInfo, IActionProvider>();

        private readonly IDictionary<CultureInfo, IPatternProvider> patterns = new Dictionary<CultureInfo, IPatternProvider>();


        protected ISubscriptionProvider SubscriprionProvider;

        protected IRecipientProvider RecipientsProvider;


        protected IActionProvider ActionProvider
        {
            get { return GetActionProvider(); }
        }

        protected IPatternProvider PatternProvider
        {
            get { return GetPatternProvider(); }
        }


        public string ID
        {
            get;
            private set;
        }


        public NotifySource(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");

            ID = id;
        }

        public NotifySource(Guid id)
            : this(id.ToString())
        {
        }

        public IActionProvider GetActionProvider()
        {
            lock (actions)
            {
                var culture = Thread.CurrentThread.CurrentCulture;
                if (!actions.ContainsKey(culture))
                {
                    actions[culture] = CreateActionProvider();
                }
                return actions[culture];
            }
        }

        public IPatternProvider GetPatternProvider()
        {
            lock (patterns)
            {
                var culture = Thread.CurrentThread.CurrentCulture;
                if (Thread.CurrentThread.CurrentUICulture != culture)
                {
                    Thread.CurrentThread.CurrentUICulture = culture;
                }
                if (!patterns.ContainsKey(culture))
                {
                    patterns[culture] = CreatePatternsProvider();
                }
                return patterns[culture];
            }
        }

        public IRecipientProvider GetRecipientsProvider()
        {
            LazyInitializeProviders();
            return RecipientsProvider;
        }

        public ISubscriptionProvider GetSubscriptionProvider()
        {
            LazyInitializeProviders();
            return SubscriprionProvider;
        }

        protected void LazyInitializeProviders()
        {
            if (!initialized)
            {
                lock (syncRoot)
                {
                    if (!initialized)
                    {
                        RecipientsProvider = CreateRecipientsProvider();
                        if (RecipientsProvider == null)
                        {
                            throw new NotifyException(String.Format("Provider {0} not instanced.", "IRecipientsProvider"));
                        }

                        SubscriprionProvider = CreateSubscriptionProvider();
                        if (SubscriprionProvider == null)
                        {
                            throw new NotifyException(String.Format("Provider {0} not instanced.", "ISubscriprionProvider"));
                        }

                        initialized = true;
                    }
                }
            }
        }


        protected abstract IPatternProvider CreatePatternsProvider();

        protected abstract IActionProvider CreateActionProvider();


        protected virtual ISubscriptionProvider CreateSubscriptionProvider()
        {
            var subscriptionProvider = new DirectSubscriptionProvider(ID, CoreContext.SubscriptionManager, RecipientsProvider);
            return new TopSubscriptionProvider(RecipientsProvider, subscriptionProvider, WorkContext.DefaultClientSenders);
        }

        protected virtual IRecipientProvider CreateRecipientsProvider()
        {
            return new RecipientProviderImpl();
        }
    }
}