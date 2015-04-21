/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
