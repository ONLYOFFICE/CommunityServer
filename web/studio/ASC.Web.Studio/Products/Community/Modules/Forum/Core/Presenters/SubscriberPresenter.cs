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
using ASC.Notify.Model;
using ASC.Notify.Recipients;

namespace ASC.Forum
{
    internal class SubscriberPresenter : PresenterTemplate<ISubscriberView>
    {
        public static void UnsubscribeAllOnTopic(int topicID)
        {
            ForumNotifySource.Instance.GetSubscriptionProvider().UnSubscribe(Constants.NewPostInTopic, topicID.ToString());
        }

        public static void UnsubscribeAllOnThread(int threadID)
        {
            ForumNotifySource.Instance.GetSubscriptionProvider().UnSubscribe(Constants.NewPostInThread, threadID.ToString());
        }


        protected override void RegisterView()
        {
            _view.Subscribe += new EventHandler<SubscribeEventArgs>(SubscribeHandler);
            _view.UnSubscribe += new EventHandler<SubscribeEventArgs>(UnSubscribeHandler);
            _view.GetSubscriptionState += new EventHandler<SubscribeEventArgs>(GetSubscriptionStateHandler);
            
        }
        
        void UnSubscribeHandler(object sender, SubscribeEventArgs e)
        {
            var recipient = (IDirectRecipient)ForumNotifySource.Instance.GetRecipientsProvider().GetRecipient(e.UserID.ToString());
            if(recipient!=null)
                ForumNotifySource.Instance.GetSubscriptionProvider().UnSubscribe(e.NotifyAction, e.ObjectID, recipient);
        }

        void GetSubscriptionStateHandler(object sender, SubscribeEventArgs e)
        {
            var recipient = (IDirectRecipient)ForumNotifySource.Instance.GetRecipientsProvider().GetRecipient(e.UserID.ToString());
            if (recipient == null)
                return;

            ISubscriptionProvider subscriptionProvider = ForumNotifySource.Instance.GetSubscriptionProvider();
            List<string> objectIDs = new List<string>(subscriptionProvider.GetSubscriptions(e.NotifyAction,recipient,false));

            if (e.ObjectID == null && objectIDs.Count == 1 && objectIDs[0] == null)
            {
                _view.IsSubscribe = true;
                return;
            }

            _view.IsSubscribe = (objectIDs.Find(id => String.Compare(id, e.ObjectID, true) == 0) != null);

        }

        private void SubscribeHandler(object sender, SubscribeEventArgs e)
        {            
            var recipient = (IDirectRecipient)ForumNotifySource.Instance.GetRecipientsProvider().GetRecipient(e.UserID.ToString());
            if(recipient!=null)
                ForumNotifySource.Instance.GetSubscriptionProvider().Subscribe(e.NotifyAction, e.ObjectID, recipient);
                
        }
    }
}
