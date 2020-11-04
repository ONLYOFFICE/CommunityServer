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
using ASC.Notify.Model;
using System.Collections.Generic;

namespace ASC.Forum
{
    public interface ISubscriptionGetcherView
    {
        IList<object> SubscriptionObjects { get; set; }

        event EventHandler<SubscriptionEventArgs> GetSubscriptionObjects;
    }

    public class SubscriptionEventArgs: EventArgs
    {
        public Guid UserID { get; private set; }

        public int TenantID { get; private set; }

        public INotifyAction NotifyAction { get; private set; }

        public SubscriptionEventArgs(INotifyAction notifyAction, Guid userID, int tenantID)
        {
            this.NotifyAction = notifyAction;         
            this.UserID = userID;
            this.TenantID = tenantID;
        }
    }
}
