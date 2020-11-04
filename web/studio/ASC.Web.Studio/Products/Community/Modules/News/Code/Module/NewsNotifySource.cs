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
using ASC.Core;
using ASC.Core.Notify;
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Web.Community.News.Resources;

namespace ASC.Web.Community.News.Code.Module
{
    public enum FeedSubscriptionType
    {
        NewFeed = 0,
        NewOrder = 1
    }

    public class NewsNotifyClient
    {
        public static INotifyClient NotifyClient { get; private set; }

        static NewsNotifyClient()
        {
            NotifyClient = WorkContext.NotifyContext.NotifyService.RegisterClient(NewsNotifySource.Instance);
        }
    }

    public class NewsNotifySource : NotifySource
    {
        public static NewsNotifySource Instance
        {
            get;
            private set;
        }

        private NewsNotifySource()
            : base(new Guid("{6504977C-75AF-4691-9099-084D3DDEEA04}"))
        {

        }

        static NewsNotifySource()
        {
            Instance = new NewsNotifySource();
        }

        protected override IActionProvider CreateActionProvider()
        {
            return new ConstActionProvider(NewsConst.NewFeed, NewsConst.NewComment);
        }

        protected override IPatternProvider CreatePatternsProvider()
        {
            return new XmlPatternProvider2(NewsPatternsResource.news_patterns);
        }
    }
}