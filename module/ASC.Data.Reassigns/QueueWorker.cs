/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
