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
using System.Data;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Data.Contracts;

namespace ASC.Mail.Core.Engine.Operations
{
    public class MailRemoveMailserverDomainOperation : MailOperation
    {
        private readonly ServerDomainData _domain;

        public override MailOperationType OperationType
        {
            get { return MailOperationType.RemoveDomain; }
        }

        public MailRemoveMailserverDomainOperation(Tenant tenant, IAccount user, ServerDomainData domain)
            : base(tenant, user)
        {
            _domain = domain;

            SetSource(_domain.Id.ToString());
        }

        protected override void Do()
        {
            try
            {
                SetProgress((int?) MailOperationRemoveDomainProgress.Init, "Setup tenant and user");

                CoreContext.TenantManager.SetCurrentTenant(CurrentTenant);

                try
                {
                    SecurityContext.AuthenticateMe(CurrentUser);
                }
                catch
                {
                    // User was removed
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                }

                SetProgress((int?) MailOperationRemoveDomainProgress.RemoveFromDb, "Remove domain from Db");

                var tenant = CurrentTenant.TenantId;

                var mailboxes = new List<MailBoxData>();

                var engine = new EngineFactory(tenant);

                using (var db = new DbManager(Defines.CONNECTION_STRING_NAME, Defines.RemoveDomainTimeout))
                {
                    var daoFactory = new DaoFactory(db);

                    using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        var serverGroupDao = daoFactory.CreateServerGroupDao(tenant);

                        var serverAddressDao = daoFactory.CreateServerAddressDao(tenant);

                        var groups = serverGroupDao.GetList(_domain.Id);

                        foreach (var serverGroup in groups)
                        {
                            serverAddressDao.DeleteAddressesFromMailGroup(serverGroup.Id);
                            serverAddressDao.Delete(serverGroup.AddressId);
                            serverGroupDao.Delete(serverGroup.Id);
                        }

                        var serverAddresses = serverAddressDao.GetDomainAddresses(_domain.Id);

                        var serverMailboxAddresses = serverAddresses.Where(a => a.MailboxId > -1 && !a.IsAlias);

                        foreach (var serverMailboxAddress in serverMailboxAddresses)
                        {
                            var mailbox =
                                engine.MailboxEngine.GetMailboxData(
                                    new ConcreteTenantServerMailboxExp(serverMailboxAddress.MailboxId, tenant, false));

                            if (mailbox == null)
                                continue;

                            mailboxes.Add(mailbox);

                            engine.MailboxEngine.RemoveMailBox(daoFactory, mailbox, false);
                        }

                        serverAddressDao.Delete(serverAddresses.Select(a => a.Id).ToList());

                        var serverDomainDao = daoFactory.CreateServerDomainDao(tenant);

                        serverDomainDao.Delete(_domain.Id);

                        var serverDao = daoFactory.CreateServerDao();
                        var server = serverDao.Get(tenant);

                        var serverEngine = new Server.Core.ServerEngine(server.Id, server.ConnectionString);

                        serverEngine.RemoveDomain(_domain.Name);

                        tx.Commit();
                    }
                }

                SetProgress((int?) MailOperationRemoveDomainProgress.ClearCache, "Clear accounts cache");

                CacheEngine.ClearAll();

                SetProgress((int?)MailOperationRemoveDomainProgress.RemoveIndex, "Remove Elastic Search index by messages");

                foreach (var mailbox in mailboxes)
                {
                    engine.IndexEngine.Remove(mailbox);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Mail operation error -> Remove mailbox: {0}", e);
                Error = "InternalServerError";
            }
        }
    }
}
