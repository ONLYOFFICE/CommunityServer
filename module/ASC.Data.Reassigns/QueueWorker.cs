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

        public static ReassignProgressItem StartReassign(HttpContext context, int tenantId, Guid fromUserId, Guid toUserId, Guid currentUserId)
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
                    task = new ReassignProgressItem(context, tenantId, fromUserId, toUserId, currentUserId);
                    Queue.Add(task);
                }

                if (!Queue.IsStarted)
                    Queue.Start(x => x.RunJob());

                return task;
            }
        }

        public static RemoveProgressItem StartRemove(HttpContext context, int tenantId, UserInfo user, Guid currentUserId)
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
                    task = new RemoveProgressItem(context, tenantId, user, currentUserId);
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
