/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
