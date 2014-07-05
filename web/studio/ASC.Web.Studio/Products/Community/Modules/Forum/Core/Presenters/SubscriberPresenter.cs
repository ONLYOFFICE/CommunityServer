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
            List<string> objectIDs = new List<string>(subscriptionProvider.GetSubscriptions(e.NotifyAction,recipient));

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
