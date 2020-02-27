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
using System.Threading;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Data.Storage;
using ASC.Mail.Enums;
using ASC.Mail.Utils;
using Newtonsoft.Json.Linq;

namespace ASC.Mail.Core.Engine
{
    public class SpamEngine
    {
        public int Tenant { get; private set; }

        public string User { get; private set; }

        public ILog Log { get; private set; }

        public SpamEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.CrmLinkEngine");
        }

        public void SendConversationsToSpamTrainer(int tenant, string user, List<int> ids, bool isSpam, string httpContextScheme)
        {
            var userCulture = Thread.CurrentThread.CurrentCulture;
            var userUiCulture = Thread.CurrentThread.CurrentUICulture;

            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    Thread.CurrentThread.CurrentCulture = userCulture;
                    Thread.CurrentThread.CurrentUICulture = userUiCulture;

                    CoreContext.TenantManager.SetCurrentTenant(tenant);

                    var tlMails = GetTlMailStreamList(tenant, user, ids);
                    SendEmlUrlsToSpamTrainer(tenant, user, tlMails, isSpam, httpContextScheme);
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("SendConversationsToSpamTrainer() failed with exception:\r\n{0}", ex.ToString());
                }
            });

        }

        private Dictionary<int, string> GetTlMailStreamList(int tenant, string user, List<int> ids)
        {
            var streamList = new Dictionary<int, string>();

            var engine = new EngineFactory(tenant, user);

            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                var tlMailboxes =
                    daoMailbox.GetMailBoxes(new UserMailboxesExp(tenant, user, false, true));

                var tlMailboxesIds = tlMailboxes.ConvertAll(mb => mb.Id);

                if (!tlMailboxesIds.Any())
                    return streamList;

                streamList = engine.ChainEngine.GetChainedMessagesInfo(daoFactory, ids)
                    .Where(r => r.FolderRestore != FolderType.Sent)
                    .Where(r => tlMailboxesIds.Contains(r.MailboxId))
                    .ToDictionary(r => r.Id, r => r.Stream);
            }

            return streamList;
        }

        private void SendEmlUrlsToSpamTrainer(int tenant, string user, Dictionary<int, string> tlMails,
            bool isSpam, string httpContextScheme)
        {
            if (!tlMails.Any())
                return;

            using (var daoFactory = new DaoFactory())
            {
                var db = daoFactory.DbManager;

                var serverInformationQuery = new SqlQuery(ServerTable.TABLE_NAME)
                    .InnerJoin(TenantXServerTable.TABLE_NAME,
                        Exp.EqColumns(TenantXServerTable.Columns.ServerId, ServerTable.Columns.Id))
                    .Select(ServerTable.Columns.ConnectionString)
                    .Where(TenantXServerTable.Columns.Tenant, tenant);

                var serverInfo = db.ExecuteList(serverInformationQuery)
                    .ConvertAll(r =>
                    {
                        var connectionString = Convert.ToString(r[0]);
                        var json = JObject.Parse(connectionString);

                        if (json["Api"] != null)
                        {
                            return new
                            {
                                server_ip = json["Api"]["Server"].ToString(),
                                port = Convert.ToInt32(json["Api"]["Port"].ToString()),
                                protocol = json["Api"]["Protocol"].ToString(),
                                version = json["Api"]["Version"].ToString(),
                                token = json["Api"]["Token"].ToString()
                            };
                        }

                        return null;
                    }
                    ).SingleOrDefault(info => info != null);

                if (serverInfo == null)
                {
                    Log.Error(
                        "SendEmlUrlsToSpamTrainer: Can't sent task to spam trainer. Empty server api info.");
                    return;
                }

                var apiHelper = new ApiHelper(httpContextScheme);

                foreach (var tlSpamMail in tlMails)
                {
                    try
                    {
                        var emlUrl = GetMailEmlUrl(tenant, user, tlSpamMail.Value);

                        apiHelper.SendEmlToSpamTrainer(serverInfo.server_ip, serverInfo.protocol, serverInfo.port,
                            serverInfo.version, serverInfo.token, emlUrl, isSpam);
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorFormat("SendEmlUrlsToSpamTrainer() Exception: \r\n {0}", ex.ToString());
                    }
                }
            }
        }

        public string GetMailEmlUrl(int tenant, string user, string streamId)
        {
            // Using id_user as domain in S3 Storage - allows not to add quota to tenant.
            var emlPath = MailStoragePathCombiner.GetEmlKey(user, streamId);
            var dataStore = MailDataStore.GetDataStore(tenant);

            try
            {
                var emlUri = dataStore.GetUri(string.Empty, emlPath);
                var url = MailStoragePathCombiner.GetStoredUrl(emlUri);

                return url;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("GetMailEmlUrl() tenant='{0}', user_id='{1}', save_eml_path='{2}' Exception: {3}",
                    tenant, user, emlPath, ex.ToString());
            }

            return "";
        }
    }
}
