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
using System.Linq;
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Core.Entities;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Engine.Operations
{
    public class MailCheckMailserverDomainsDnsOperation : MailOperation
    {
        private readonly string _domainName;
        private readonly ServerDns _dns;

        public override MailOperationType OperationType
        {
            get { return MailOperationType.CheckDomainDns; }
        }

        public MailCheckMailserverDomainsDnsOperation(Tenant tenant, IAccount user, string domainName, ServerDns dns)
            : base(tenant, user)
        {
            _domainName = domainName;
            _dns = dns;

            SetSource(_domainName);
        }

        protected override void Do()
        {
            try
            {
                SetProgress((int?) MailOperationCheckDomainDnsProgress.Init, "Setup tenant and user");

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

                ServerDomain domain;

                using (var daoFactory = new DaoFactory())
                {
                    var serverDomainDao = daoFactory.CreateServerDomainDao(CurrentTenant.TenantId);

                    var domains = serverDomainDao.GetDomains();

                    domain =
                        domains.FirstOrDefault(
                            d => d.Name.Equals(_domainName, StringComparison.InvariantCultureIgnoreCase));

                    if (domain == null)
                        throw new Exception(string.Format("Domain '{0}' not found", _domainName));

                }

                var hasChanges = false;

                SetProgress((int?) MailOperationCheckDomainDnsProgress.CheckMx, "Check DNS MX record");

                if (_dns.UpdateMx(domain.Name))
                {
                    hasChanges = true;
                }

                SetProgress((int?) MailOperationCheckDomainDnsProgress.CheckSpf, "Check DNS SPF record");

                if (_dns.UpdateSpf(domain.Name))
                {
                    hasChanges = true;
                }

                SetProgress((int?) MailOperationCheckDomainDnsProgress.CheckDkim, "Check DNS DKIM record");

                if (_dns.UpdateDkim(domain.Name))
                {
                    hasChanges = true;
                }

                if (!hasChanges)
                    return;

                SetProgress((int?) MailOperationCheckDomainDnsProgress.UpdateResults,
                    "Update domain dns check results");

                using (var daoFactory = new DaoFactory())
                {
                    var serverDnsDao = daoFactory.CreateServerDnsDao(CurrentTenant.TenantId, CurrentUser.ID.ToString());
                    serverDnsDao.Save(_dns);
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Mail operation error -> Domain '{0}' dns check failed. Error: {1}", _domainName, e);
                Error = "InternalServerError";
            }
        }
    }
}
