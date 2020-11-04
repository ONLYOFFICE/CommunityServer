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
using System.Web;
using ASC.Core;
using ASC.Forum;
using ASC.Notify.Model;
using ASC.Web.Core.Subscriptions;
using ASC.Web.Studio.Utility;
using ASC.Web.Community.Product;
using ASC.Notify.Recipients;

namespace ASC.Web.Community.Forum
{
    public class ForumSubscriptionManager : ISubscriptionManager,
                                          ISubscriptionGetcherView
    {
        private Guid _threadSubscriptionTypeID = new Guid("{61230AB9-96A0-4da7-9F45-6DAB04A95F29}");
        private Guid _topicSubscriptionTypeID = new Guid("{59B90C57-747B-4b2b-97DC-8AD73D8B0829}");
        private Guid _topicOnForumSubscriptionTypeID = new Guid("{7730BE18-4733-427d-BC3B-6ABF82F88559}");


        private List<SubscriptionObject> GetSubscriptionObjectsByType(Guid productID, Guid moduleID, Guid typeID)
        {
            List<SubscriptionObject> subscriptionObjects = new List<SubscriptionObject>();

            InitSubscriptionGetcherPresenter();
            if (GetSubscriptionObjects != null)
            {
                if (typeID.Equals(_threadSubscriptionTypeID))
                {

                    GetSubscriptionObjects(this, new SubscriptionEventArgs(SubscriptionConstants.NewPostInThread, SecurityContext.CurrentAccount.ID, TenantProvider.CurrentTenantID));
                    if (this.SubscriptionObjects != null && this.SubscriptionObjects.Count > 0)
                    {
                        foreach (Thread thread in this.SubscriptionObjects)
                        {
                            subscriptionObjects.Add(new SubscriptionObject()
                            {
                                ID = thread.ID.ToString(),
                                Name = thread.Title,
                                URL = VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/Topics.aspx") + "?f=" + thread.ID.ToString(),
                                SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_threadSubscriptionTypeID))
                            });
                        }
                    }
                }

                else if (typeID.Equals(_topicSubscriptionTypeID))
                {
                    GetSubscriptionObjects(this, new SubscriptionEventArgs(SubscriptionConstants.NewPostInTopic, SecurityContext.CurrentAccount.ID, TenantProvider.CurrentTenantID));
                    if (this.SubscriptionObjects != null && this.SubscriptionObjects.Count > 0)
                    {
                        foreach (Topic topic in this.SubscriptionObjects)
                        {
                            subscriptionObjects.Add(new SubscriptionObject()
                            {
                                ID = topic.ID.ToString(),
                                Name = topic.Title,
                                URL = VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/Posts.aspx") + "?t=" + topic.ID.ToString(),
                                SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_topicSubscriptionTypeID))
                            });
                        }
                    }
                }

                else if (typeID.Equals(_topicOnForumSubscriptionTypeID))
                {
                    GetSubscriptionObjects(this, new SubscriptionEventArgs(SubscriptionConstants.NewTopicInForum, SecurityContext.CurrentAccount.ID, TenantProvider.CurrentTenantID));
                    if (this.SubscriptionObjects != null && this.SubscriptionObjects.Count > 0)
                    {
                        subscriptionObjects.Add(new SubscriptionObject()
                        {
                            ID = _topicOnForumSubscriptionTypeID.ToString(),
                            Name = Resources.ForumResource.NewTopicOnForumSubscriptionTitle,
                            URL = string.Empty,
                            SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_topicOnForumSubscriptionTypeID))
                        });
                    }
                }


            }
            return subscriptionObjects;
        }

        private bool IsEmptySubscriptionType(Guid productID, Guid moduleID, Guid typeID)
        {
            var type = GetSubscriptionTypes().Find(t => t.ID.Equals(typeID));

            var objIDs = SubscriptionProvider.GetSubscriptions(type.NotifyAction, new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""), false);
            if (objIDs != null && objIDs.Length > 0)
                return false;

            return true;
        }

        public ForumSubscriptionManager()
        {

        }

        private bool _isInitSubscriptionGetcher = false;
        private void InitSubscriptionGetcherPresenter()
        {
            if (_isInitSubscriptionGetcher == false)
            {
                ForumManager.Instance.PresenterFactory.GetPresenter<ISubscriptionGetcherView>().SetView(this);
                _isInitSubscriptionGetcher = true;
            }
        }

        #region ISubscriptionManager Members

        List<SubscriptionObject> ISubscriptionManager.GetSubscriptionObjects(Guid subItem)
        {
            List<SubscriptionObject> subscriptionObjects = new List<SubscriptionObject>();
            foreach (var type in GetSubscriptionTypes())
                subscriptionObjects.AddRange(GetSubscriptionObjectsByType(CommunityProduct.ID, ForumManager.ModuleID, type.ID));

            return subscriptionObjects;
        }

        public List<SubscriptionType> GetSubscriptionTypes()
        {
            var subscriptionTypes = new List<SubscriptionType>();

            subscriptionTypes.Add(new SubscriptionType()
            {
                ID = _topicOnForumSubscriptionTypeID,
                Name = Resources.ForumResource.NewTopicOnForumSubscriptionTitle,
                NotifyAction = SubscriptionConstants.NewTopicInForum,
                Single = true,
                IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType),
                CanSubscribe = true
            });

            subscriptionTypes.Add(new SubscriptionType()
            {
                ID = _threadSubscriptionTypeID,
                Name = Resources.ForumResource.ThreadSubscriptionTitle,
                NotifyAction = SubscriptionConstants.NewPostInThread,
                GetSubscriptionObjects = new GetSubscriptionObjectsDelegate(GetSubscriptionObjectsByType),
                IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType)
            });

            subscriptionTypes.Add(new SubscriptionType()
            {
                ID = _topicSubscriptionTypeID,
                Name = Resources.ForumResource.TopicSubscriptionTitle,
                NotifyAction = SubscriptionConstants.NewPostInTopic,
                GetSubscriptionObjects = new GetSubscriptionObjectsDelegate(GetSubscriptionObjectsByType),
                IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType)
            });


            return subscriptionTypes;
        }

        public ISubscriptionProvider SubscriptionProvider
        {
            get { return SubscriptionConstants.SubscriptionProvider; }
        }

        #endregion

        #region ISubscriptionGetcherView Members

        public event EventHandler<SubscriptionEventArgs> GetSubscriptionObjects;

        public IList<object> SubscriptionObjects { get; set; }

        #endregion
    }
}
