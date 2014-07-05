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