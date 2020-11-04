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
using ASC.Notify.Model;

namespace ASC.Forum
{
    public interface ISubscriberView
    {
        bool IsSubscribe { get; set; }

        event EventHandler<SubscribeEventArgs> Subscribe;

        event EventHandler<SubscribeEventArgs> UnSubscribe;

        event EventHandler<SubscribeEventArgs> GetSubscriptionState;
    }

    public class SubscribeEventArgs : EventArgs
    {
        public INotifyAction NotifyAction { get; private set; }

        public string ObjectID { get; private set; }

        public Guid UserID { get; private set; }       
        
        public SubscribeEventArgs(INotifyAction notifyAction, string objectID, Guid userID)
        {
            this.NotifyAction = notifyAction;
            this.ObjectID = objectID;
            this.UserID = userID;            
        }
    }
}
