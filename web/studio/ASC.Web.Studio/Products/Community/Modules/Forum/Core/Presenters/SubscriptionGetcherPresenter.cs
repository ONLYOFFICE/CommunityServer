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
using ASC.Forum.Module;
using ASC.Notify.Recipients;

namespace ASC.Forum
{
    internal class SubscriptionGetcherPresenter : PresenterTemplate<ISubscriptionGetcherView>
    {
        protected override void RegisterView()
        {
            _view.GetSubscriptionObjects += new EventHandler<SubscriptionEventArgs>(GetSubscriptionObjectsHandler);            
        }

        private void GetSubscriptionObjectsHandler(object sender, SubscriptionEventArgs e)
        {
            var recipient = (IDirectRecipient)ForumNotifySource.Instance.GetRecipientsProvider().GetRecipient(e.UserID.ToString());
            if (recipient == null)
            {
                _view.SubscriptionObjects = new List<object>(0);
                return;
            }

            List<string> objIDs = new List<string>(ForumNotifySource.Instance.GetSubscriptionProvider().GetSubscriptions(e.NotifyAction, recipient, false));

            if (objIDs == null || objIDs.Count == 0)
            {
                _view.SubscriptionObjects = new List<object>(0);
                return;
            }

            if (String.Equals(e.NotifyAction.ID, SubscriptionConstants.NewPostInTopic.ID, StringComparison.InvariantCultureIgnoreCase))
            {
                _view.SubscriptionObjects = ForumDataProvider.GetTopicsByIDs(e.TenantID, objIDs.ConvertAll<int>(id => Convert.ToInt32(id)), false)
                                            .ConvertAll<object>(topic => (object)topic);
            }

            else if (String.Equals(e.NotifyAction.ID, SubscriptionConstants.NewPostInThread.ID, StringComparison.InvariantCultureIgnoreCase))
            {
                List<ThreadCategory> categories = null;
                List<Thread> threads = null;

                ForumDataProvider.GetThreadCategories(e.TenantID, false, out categories, out threads);
                threads.RemoveAll(tid => (objIDs.Find(id => id == tid.ID.ToString()) == null));

                _view.SubscriptionObjects = threads.ConvertAll<object>(thread => (object)thread);
            }

            else if (String.Equals(e.NotifyAction.ID, SubscriptionConstants.NewPostByTag.ID, StringComparison.InvariantCultureIgnoreCase))
            {
                _view.SubscriptionObjects = ForumDataProvider.GetTagByIDs(e.TenantID, objIDs.ConvertAll<int>(id => Convert.ToInt32(id)))
                                         .ConvertAll<object>(tag => (object)tag);
            }

            else if (String.Equals(e.NotifyAction.ID, SubscriptionConstants.NewTopicInForum.ID, StringComparison.InvariantCultureIgnoreCase))
            {
                if (objIDs != null && objIDs.Count == 1 && objIDs[0] == null)
                {
                    var objList = new List<object>();
                    objList.Add(null);
                    _view.SubscriptionObjects = objList;
                }
            }

        }
    }
}
