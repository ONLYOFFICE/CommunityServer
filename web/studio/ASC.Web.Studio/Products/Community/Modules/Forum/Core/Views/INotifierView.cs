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
using ASC.Core.Users;

namespace ASC.Forum
{
    public interface INotifierView
    {
        event EventHandler<NotifyEventArgs> SendNotify;
    }

    public class NotifyEventArgs : EventArgs
    {
        public INotifyAction NotifyAction { get; set; }

        public string ObjectID { get; private set; }

        public string ThreadURL { get; set; }
        public string TopicURL { get; set; }
        public string PostURL { get; set; }
        public string TagURL { get; set; }
        public string UserURL { get; set; }
        public string Date { get; set; }

        public string ThreadTitle { get; set; }
        public string TopicTitle { get; set; }

        public UserInfo Poster { get; set; }

        public string PostText { get; set; }
        public string TagName { get; set; }
        public int TopicId { get; set; }
        public int PostId { get; set; }
        public int TenantId { get; set; }


        public NotifyEventArgs(INotifyAction notifyAction, string objectID)
        {
            this.NotifyAction = notifyAction;
            this.ObjectID = objectID;           
        }
    }
}
