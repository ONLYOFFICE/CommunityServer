/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
