/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using ASC.Core.Billing;
using ASC.Core.Data;
using ASC.Core.Security.Authentication;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Security.Cryptography;

namespace ASC.Core
{
    public class HostedSolution
    {
        private readonly ITenantService tenantService;
        private readonly IUserService userService;
        private readonly IQuotaService quotaService;
        private readonly ITariffService tariffService;
        private readonly ClientTenantManager clientTenantManager;

        public string Region
        {
            get;
            private set;
        }

        public string DbId
        {
            get;
            private set;
        }


        public HostedSolution(ConnectionStringSettings connectionString)
            : this(connectionString, null)
        {
        }

        public HostedSolution(ConnectionStringSettings connectionString, string region)
        {
            tenantService = new DbTenantService(connectionString);
            userService = new DbUserService(connectionString);
            quotaService = new DbQuotaService(connectionString);
            tariffService = new TariffService(connectionString, quotaService, tenantService);
            clientTenantManager = new ClientTenantManager(tenantService, quotaService, tariffService);
            Region = region ?? string.Empty;
            DbId = connectionString.Name;
        }

        public List<Tenant> GetTenants(DateTime from)
        {
            return tenantService.GetTenants(from).Select(t => AddRegion(t)).ToList();
        }

        public List<Tenant> FindTenants(string login)
        {
            return FindTenants(login, null);
        }

        public List<Tenant> FindTenants(string login, string password)
        {
            var hash = !string.IsNullOrEmpty(password) ? Hasher.Base64Hash(password, HashAlg.SHA256) : null;
            if (hash != null && userService.GetUser(Tenant.DEFAULT_TENANT, login, hash) == null)
            {
                throw new SecurityException("Invalid login or password.");
            }
            return tenantService.GetTenants(login, hash).Select(t => AddRegion(t)).ToList();
        }

        public Tenant GetTenant(String domain)
        {
            return AddRegion(tenantService.GetTenant(domain));
        }

        public Tenant GetTenant(int id)
        {
            return AddRegion(tenantService.GetTenant(id));
        }

        public void CheckTenantAddress(string address)
        {
            tenantService.ValidateDomain(address);
        }

        public void RegisterTenant(TenantRegistrationInfo ri, out Tenant tenant)
        {
            tenant = null;

            if (ri == null) throw new ArgumentNullException("registrationInfo");
            if (string.IsNullOrEmpty(ri.Address)) throw new Exception("Address can not be empty");

            if (string.IsNullOrEmpty(ri.Email)) throw new Exception("Account email can not be empty");
            if (string.IsNullOrEmpty(ri.FirstName)) throw new Exception("Account firstname can not be empty");
            if (string.IsNullOrEmpty(ri.LastName)) throw new Exception("Account lastname can not be empty");
            if (string.IsNullOrEmpty(ri.Password)) ri.Password = Crypto.GeneratePassword(6);

            // create tenant
            tenant = new Tenant(ri.Address.ToLowerInvariant())
            {
                Name = ri.Name,
                Language = ri.Culture.Name,
                TimeZone = ri.TimeZoneInfo,
                HostedRegion = ri.HostedRegion,
                PartnerId = ri.PartnerId,
                Industry = ri.Industry
            };

            tenant = tenantService.SaveTenant(tenant);

            // create user
            var user = new UserInfo()
            {
                UserName = ri.Email.Substring(0, ri.Email.IndexOf('@')),
                LastName = ri.LastName,
                FirstName = ri.FirstName,
                Email = ri.Email,
                MobilePhone = ri.MobilePhone,
                WorkFromDate = TenantUtil.DateTimeNow(tenant.TimeZone),
                ActivationStatus = ri.ActivationStatus
            };
            user = userService.SaveUser(tenant.TenantId, user);
            userService.SetUserPassword(tenant.TenantId, user.ID, ri.Password);
            userService.SaveUserGroupRef(tenant.TenantId, new UserGroupRef(user.ID, Constants.GroupAdmin.ID, UserGroupRefType.Contains));

            // save tenant owner
            tenant.OwnerId = user.ID;
            tenant = tenantService.SaveTenant(tenant);
        }

        public Tenant SaveTenant(Tenant tenant)
        {
            return tenantService.SaveTenant(tenant);
        }

        public string CreateAuthenticationCookie(int tenantId, string login, string password)
        {
            var passwordhash = Hasher.Base64Hash(password, HashAlg.SHA256);
            var u = userService.GetUser(tenantId, login, passwordhash);
            return u != null ? CookieStorage.EncryptCookie(tenantId, u.ID, login, passwordhash) : null;
        }

        public string CreateAuthenticationCookie(int tenantId, Guid userId)
        {
            var u = userService.GetUser(tenantId, userId);
            var password = userService.GetUserPassword(tenantId, userId);
            var passwordhash = Hasher.Base64Hash(password, HashAlg.SHA256);
            return u != null ? CookieStorage.EncryptCookie(tenantId, userId, u.Email, passwordhash) : null;
        }

        public Tariff GetTariff(int tenant, bool withRequestToPaymentSystem = true)
        {
            return tariffService.GetTariff(tenant, withRequestToPaymentSystem);
        }

        public TenantQuota GetTenantQuota(int tenant)
        {
            return clientTenantManager.GetTenantQuota(tenant);
        }

        public void SetTariff(int tenant, bool paid)
        {
            var quota = quotaService.GetTenantQuotas().FirstOrDefault(q => paid ? q.NonProfit : q.Trial);
            if (quota != null)
            {
                tariffService.SetTariff(tenant, new Tariff { QuotaId = quota.Id, DueDate = DateTime.MaxValue, });
            }
        }

        public void SetTariff(int tenant, Tariff tariff)
        {
            tariffService.SetTariff(tenant, tariff);
        }

        public void SaveButton(int tariffId, string partnerId, string buttonUrl)
        {
            tariffService.SaveButton(tariffId, partnerId, buttonUrl);
        }


        private Tenant AddRegion(Tenant tenant)
        {
            if (tenant != null)
            {
                tenant.HostedRegion = Region;
            }
            return tenant;
        }
    }
}