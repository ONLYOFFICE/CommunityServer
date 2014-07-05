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
using ASC.Core;
using ASC.Notify.Model;
using ASC.Web.Community.News.Resources;
using ASC.Web.Core.Subscriptions;
using ASC.Notify.Recipients;
using ASC.Web.Community.News.Code.DAO;
using System.Web;

namespace ASC.Web.Community.News.Code.Module
{
    public class SubscriptionManager : ISubscriptionManager
    {

        private Guid _newsSubscriptionTypeID = new Guid("{C27F9ADE-81C9-4103-B617-607811897C17}");
        private static Guid SubscribeToNewsCommentsTypeID = Guid.NewGuid();

        private bool IsEmptySubscriptionType(Guid productID, Guid moduleID, Guid typeID)
        {
            var type = GetSubscriptionTypes().Find(t => t.ID.Equals(typeID));

            var objIDs = SubscriptionProvider.GetSubscriptions(type.NotifyAction, new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            if (objIDs != null && objIDs.Length > 0)
                return false;

            return true;
        }


        private static INotifyAction GetNotifyActionBySubscriptionType(FeedSubscriptionType subscriptionType)
        {
            switch (subscriptionType)
            {
                case FeedSubscriptionType.NewFeed: return NewsConst.NewFeed;
            }
            return null;
        }


        #region ISubscriptionManager Members

        public ISubscriptionProvider SubscriptionProvider
        {
            get
            {
                return NewsNotifySource.Instance.GetSubscriptionProvider();
            }
        }

        #region Subscription Objects
        private List<SubscriptionObject> GetNewFeedSubscriptionObjects(Guid productID, Guid moduleOrGroupID, Guid typeID)
        {
            return GetSubscriptionObjects(true, false);
        }

        private List<SubscriptionObject> GetNewCommentSubscriptionObjects(Guid productID, Guid moduleOrGroupID, Guid typeID)
        {
            return GetSubscriptionObjects(false, true);
        }

        private List<SubscriptionObject> GetSubscriptionObjects(bool includeFeed, bool includeComments)
        {
            List<SubscriptionObject> subscriptionObjects = new List<SubscriptionObject>();

            IList<string> list = new List<string>();

            var currentAccount = SecurityContext.CurrentAccount;

            if (includeFeed)
            {

                list = new List<string>(
                    SubscriptionProvider.GetSubscriptions(
                        NewsConst.NewFeed,
                        new DirectRecipient(currentAccount.ID.ToString(), currentAccount.Name))
                        );
                if (list.Count > 0)
                {
                    foreach (string id in list)
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            subscriptionObjects.Add(new SubscriptionObject()
                            {
                                ID = id,
                                Name = NewsResource.NotifyOnNewFeed,
                                URL = string.Empty,
                                SubscriptionType = GetSubscriptionTypes()[0]
                            });
                        }
                    }
                }
            }

            if (includeComments)
            {

                list = new List<string>(
                    SubscriptionProvider.GetSubscriptions(
                        NewsConst.NewComment,
                        new DirectRecipient(currentAccount.ID.ToString(), currentAccount.Name))
                        );
                if (list.Count > 0)
                {
                    var storage = FeedStorageFactory.Create();
                    foreach (string id in list)
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            try
                            {
                                var feedID = Int32.Parse(id);
                                var feed = storage.GetFeed(feedID);
                                subscriptionObjects.Add(new SubscriptionObject()
                                {
                                    ID = id,
                                    Name = feed.Caption,
                                    URL = FeedUrls.GetFeedAbsolutePath(feed.Id),
                                    SubscriptionType = GetSubscriptionTypes()[1]
                                });
                            }
                            catch { }
                        }
                    }
                }
            }
            return subscriptionObjects;
        }



        public List<SubscriptionObject> GetSubscriptionObjects(Guid subItem)
        {
            return GetSubscriptionObjects(true, true);

        }
        #endregion



        public List<SubscriptionType> GetSubscriptionTypes()
        {

            var subscriptionTypes = new List<SubscriptionType>();
            subscriptionTypes.Add(new SubscriptionType()
            {
                ID = _newsSubscriptionTypeID,
                Name = NewsResource.NotifyOnNewFeed,
                NotifyAction = GetNotifyActionBySubscriptionType(FeedSubscriptionType.NewFeed),
                Single = true,
                IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType),
                GetSubscriptionObjects = GetNewFeedSubscriptionObjects
            });

            subscriptionTypes.Add(new SubscriptionType()
            {
                ID = SubscribeToNewsCommentsTypeID,
                Name = NewsResource.NotificationOnNewComments,
                NotifyAction = NewsConst.NewComment,
                Single = false,
                IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType),
                GetSubscriptionObjects = GetNewCommentSubscriptionObjects
            });

            return subscriptionTypes;
        }

        public void UnsubscribeForObject(Guid subscriptionTypeId)
        {
            ISubscriptionProvider subscriptionProvider = NewsNotifySource.Instance.GetSubscriptionProvider();
            if (subscriptionTypeId == _newsSubscriptionTypeID)
            {

                subscriptionProvider.UnSubscribe(
                    NewsConst.NewFeed,
                    null,
                    NewsNotifySource.Instance.GetRecipientsProvider().
                        GetRecipient(SecurityContext.CurrentAccount.
                                         ID.ToString())

                    );
            }
        }

        #endregion
    }
}