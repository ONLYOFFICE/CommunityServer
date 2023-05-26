/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Configuration;
using System.Linq;

using ASC.Common.Caching;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Engine
{
    public class UserActionEngine
    {
        public const string RedisClientPrefix = "ASC.MailAction:";
        public const string RedisClientQueuesKey = RedisClientPrefix + "Queues";

        public readonly string key;

        public int TenantId { get; }
        public string Username { get; }
        public EngineFactory MailEngineFactory { get; }

        private bool SendUserData { get; }

        public UserActionEngine(int tenantId, string username, EngineFactory mailEngineFactory)
        {
            TenantId = tenantId;
            Username = username;

            MailEngineFactory = mailEngineFactory;

            SendUserData = (ConfigurationManagerExtension.AppSettings["mail.send-user-activity"] == null) ? false :
                Convert.ToBoolean(ConfigurationManagerExtension.AppSettings["mail.send-user-activity"]);

            key = RedisClientPrefix + Username;

            if (SendUserData)
            {
                var exp = new UserMailboxesExp(TenantId, Username, onlyTeamlab: true);

                var mailboxes = MailEngineFactory.MailboxEngine.GetMailboxDataList(exp);

                SendUserData = mailboxes.Any();
            }
        }

        public void SendUserAlive(int folder, IEnumerable<int> tags)
        {
            if (!SendUserData) return;

            if (!(AscCache.Default is RedisCache cache)) return;

            CachedTenantUserMailBox cashedTenantUserMailBox = new CachedTenantUserMailBox()
            {
                Tenant = TenantId,
                UserName = Username,
                Tags = tags,
                Folder = folder
            };

            cache.Publish(cashedTenantUserMailBox, CacheNotifyAction.Insert);
        }

        public void SendUserActivity(List<int> ids, MailUserAction action = MailUserAction.StartImapClient, int destinationFolder = -1, uint? userFolderId = null)
        {
            if (!SendUserData) return;

            if (!(AscCache.Default is RedisCache cache)) return;

            CashedMailUserAction cashedMailUserAction = new CashedMailUserAction()
            {
                Tenant = TenantId,
                UserName = Username,
                Uds = ids,
                Action = action,
                Destination = destinationFolder,
                UserFolderId = userFolderId
            };

            cache.PushMailAction(key, cashedMailUserAction);

            SendUserAlive(-1, null);
        }

        public void SendUserActivity(string data, MailUserAction action = MailUserAction.StartImapClient)
        {
            if (!SendUserData) return;

            if (!(AscCache.Default is RedisCache cache)) return;

            CashedMailUserAction cashedMailUserAction = new CashedMailUserAction()
            {
                Tenant = TenantId,
                UserName = Username,
                Action = action,
                Data = data
            };

            cache.PushMailAction(key, cashedMailUserAction);

            SendUserAlive(-1, null);
        }

        public void SendActivityUserFolderUpdate(uint id, string name, uint? parentId = null)
        {
            if (!SendUserData) return;

            if (!(AscCache.Default is RedisCache cache)) return;

            CashedMailUserAction cashedMailUserAction = new CashedMailUserAction()
            {
                Tenant = TenantId,
                UserName = Username,
                Action = MailUserAction.UpdateUserFolder,
                Uds = new List<int>() { (int)id },
                Data = name,
                UserFolderId = parentId
            };

            cache.PushMailAction(key, cashedMailUserAction);

            SendUserAlive(-1, null);
        }
    }
}
