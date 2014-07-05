/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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