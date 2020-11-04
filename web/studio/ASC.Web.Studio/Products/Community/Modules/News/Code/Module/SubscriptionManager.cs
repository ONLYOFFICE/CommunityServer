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

            var objIDs = SubscriptionProvider.GetSubscriptions(type.NotifyAction, new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""), false);
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
                        new DirectRecipient(currentAccount.ID.ToString(), currentAccount.Name), false)
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
                        new DirectRecipient(currentAccount.ID.ToString(), currentAccount.Name), false)
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
                GetSubscriptionObjects = GetNewFeedSubscriptionObjects,
                CanSubscribe = true
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