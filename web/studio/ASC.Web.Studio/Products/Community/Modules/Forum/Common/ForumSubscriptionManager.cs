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
        private Guid _userID = Guid.Empty;

        private Guid _threadSubscriptionTypeID = new Guid("{61230AB9-96A0-4da7-9F45-6DAB04A95F29}");
        private Guid _topicSubscriptionTypeID = new Guid("{59B90C57-747B-4b2b-97DC-8AD73D8B0829}");
        private Guid _tagSubscriptionTypeID = new Guid("{9DFFB68F-1D07-4b38-9218-412B9219C1D8}");
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
                                URL = VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/topics.aspx") + "?f=" + thread.ID.ToString(),
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
                                URL = VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/posts.aspx") + "?t=" + topic.ID.ToString(),
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
                IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType)

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
