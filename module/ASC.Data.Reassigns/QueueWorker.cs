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
using System.Linq;
using System.Web;
using ASC.Common.Threading.Progress;
using ASC.Core.Users;

namespace ASC.Data.Reassigns
{
    public class QueueWorker
    {
        private static readonly ProgressQueue Queue = new ProgressQueue(1, TimeSpan.FromMinutes(5), true);

        public static string GetProgressItemId(int tenantId, Guid userId, Type progressItemType)
        {
            return string.Format("{0}_{1}_{2}", tenantId, userId, progressItemType.Name);
        }

        public static IProgressItem GetProgressItemStatus(int tenantId, Guid userId, Type progressItemType)
        {
            var id = GetProgressItemId(tenantId, userId, progressItemType);
            return Queue.GetStatus(id);
        }

        public static void Terminate(int tenantId, Guid userId, Type progressItemType)
        {
            var item = GetProgressItemStatus(tenantId, userId, progressItemType);

            if (item != null)
                Queue.Remove(item);
        }

        public static ReassignProgressItem StartReassign(HttpContext context, int tenantId, Guid fromUserId, Guid toUserId, Guid currentUserId, bool deleteProfile)
        {
            lock (Queue.SynchRoot)
            {
                var task = GetProgressItemStatus(tenantId, fromUserId, typeof(ReassignProgressItem)) as ReassignProgressItem;

                if (task != null && task.IsCompleted)
                {
                    Queue.Remove(task);
                    task = null;
                }

                if (task == null)
                {
                    task = new ReassignProgressItem(context, tenantId, fromUserId, toUserId, currentUserId, deleteProfile);
                    Queue.Add(task);
                }

                if (!Queue.IsStarted)
                    Queue.Start(x => x.RunJob());

                return task;
            }
        }

        public static RemoveProgressItem StartRemove(HttpContext context, int tenantId, UserInfo user, Guid currentUserId, bool notify)
        {
            lock (Queue.SynchRoot)
            {
                var task = GetProgressItemStatus(tenantId, user.ID, typeof(RemoveProgressItem)) as RemoveProgressItem;

                if (task != null && task.IsCompleted)
                {
                    Queue.Remove(task);
                    task = null;
                }

                if (task == null)
                {
                    task = new RemoveProgressItem(context, tenantId, user, currentUserId, notify);
                    Queue.Add(task);
                }

                if (!Queue.IsStarted)
                    Queue.Start(x => x.RunJob());

                return task;
            }
        }

        public static Dictionary<string, string> GetHttpHeaders(HttpRequest httpRequest)
        {
            return httpRequest == null
                       ? null
                       : httpRequest.Headers.AllKeys.ToDictionary(key => key, key => httpRequest.Headers[key]);
        }
    }
}
