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

        public int TenantId { get; }
        public string Username { get; }
        public EngineFactory MailEngineFactory { get; }

        private bool IsSendUserActivity { get; }

        public UserActionEngine(int tenantId, string username, EngineFactory mailEngineFactory)
        {
            TenantId = tenantId;
            Username = username;
            MailEngineFactory = mailEngineFactory;
            IsSendUserActivity = (ConfigurationManagerExtension.AppSettings["mail.send-user-activity"] == null) ? false :
                Convert.ToBoolean(ConfigurationManagerExtension.AppSettings["mail.send-user-activity"]);
        }

        public void SendUserAlive(int folder, IEnumerable<int> tags)
        {
            if (!IsSendUserActivity) return;

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
            if (!IsSendUserActivity) return;

            if (!(AscCache.Default is RedisCache cache)) return;

            var exp = new UserMailboxesExp(TenantId, Username, onlyTeamlab: true);

            var mailboxes = MailEngineFactory.MailboxEngine.GetMailboxDataList(exp);

            var mailboxesOnlyoffice = mailboxes.ToList();

            if (!mailboxesOnlyoffice.Any()) return;

            string key = RedisClientPrefix + Username;

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
            if (!IsSendUserActivity) return;

            if (!(AscCache.Default is RedisCache cache)) return;

            var exp = new UserMailboxesExp(TenantId, Username, onlyTeamlab: true);

            var mailboxes = MailEngineFactory.MailboxEngine.GetMailboxDataList(exp);

            var mailboxesOnlyoffice = mailboxes.ToList();

            if (!mailboxesOnlyoffice.Any()) return;

            string key = RedisClientPrefix + Username;

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
    }
}
