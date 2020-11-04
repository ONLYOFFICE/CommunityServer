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
using System.Configuration;
using System.Linq;
using System.Security;
using ASC.Core.Billing;
using ASC.Core.Data;
using ASC.Core.Security.Authentication;
using ASC.Core.Tenants;
using ASC.Core.Users;

namespace ASC.Core
{
    public class HostedSolution
    {
        private readonly ITenantService tenantService;
        private readonly IUserService userService;
        private readonly IQuotaService quotaService;
        private readonly ITariffService tariffService;
        private readonly TenantManager clientTenantManager;
        private readonly DbSettingsManager settingsManager;

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
            clientTenantManager = new TenantManager(tenantService, quotaService, tariffService);
            settingsManager = new DbSettingsManager(connectionString);
            Region = region ?? string.Empty;
            DbId = connectionString.Name;
        }

        public List<Tenant> GetTenants(DateTime from)
        {
            return tenantService.GetTenants(from).Select(AddRegion).ToList();
        }

        public List<Tenant> FindTenants(string login)
        {
            return FindTenants(login, null);
        }

        public List<Tenant> FindTenants(string login, string passwordHash)
        {
            if (!string.IsNullOrEmpty(passwordHash) && userService.GetUserByPasswordHash(Tenant.DEFAULT_TENANT, login, passwordHash) == null)
            {
                throw new SecurityException("Invalid login or password.");
            }
            return tenantService.GetTenants(login, passwordHash).Select(AddRegion).ToList();
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
            if (ri.FirstName == null) throw new Exception("Account firstname can not be empty");
            if (ri.LastName == null) throw new Exception("Account lastname can not be empty");
            if (!UserFormatter.IsValidUserName(ri.FirstName, ri.LastName)) throw new Exception("Incorrect firstname or lastname");

            if (string.IsNullOrEmpty(ri.PasswordHash)) ri.PasswordHash = Guid.NewGuid().ToString();

            // create tenant
            tenant = new Tenant(ri.Address.ToLowerInvariant())
            {
                Name = ri.Name,
                Language = ri.Culture.Name,
                TimeZone = ri.TimeZoneInfo,
                HostedRegion = ri.HostedRegion,
                PartnerId = ri.PartnerId,
                AffiliateId = ri.AffiliateId,
                Campaign = ri.Campaign,
                Industry = ri.Industry,
                Spam = ri.Spam,
                Calls = ri.Calls
            };

            tenant = tenantService.SaveTenant(tenant);

            // create user
            var user = new UserInfo
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
            userService.SetUserPasswordHash(tenant.TenantId, user.ID, ri.PasswordHash);
            userService.SaveUserGroupRef(tenant.TenantId, new UserGroupRef(user.ID, Constants.GroupAdmin.ID, UserGroupRefType.Contains));

            // save tenant owner
            tenant.OwnerId = user.ID;
            tenant = tenantService.SaveTenant(tenant);

            settingsManager.SaveSettings(new TenantAnalyticsSettings { Analytics = ri.Analytics }, tenant.TenantId);
            settingsManager.SaveSettings(new TenantControlPanelSettings { LimitedAccess = ri.LimitedControlPanel }, tenant.TenantId);
        }

        public Tenant SaveTenant(Tenant tenant)
        {
            return tenantService.SaveTenant(tenant);
        }

        public void RemoveTenant(Tenant tenant)
        {
            tenantService.RemoveTenant(tenant.TenantId);
        }

        public string CreateAuthenticationCookie(int tenantId, Guid userId)
        {
            var u = userService.GetUser(tenantId, userId);
            return CreateAuthenticationCookie(tenantId, u);
        }

        private string CreateAuthenticationCookie(int tenantId, UserInfo user)
        {
            if (user == null) return null;

            var tenantSettings = settingsManager.LoadSettingsFor<TenantCookieSettings>(tenantId, Guid.Empty);
            var expires = tenantSettings.IsDefault() ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddMinutes(tenantSettings.LifeTime);
            var userSettings = settingsManager.LoadSettingsFor<TenantCookieSettings>(tenantId, user.ID);
            return CookieStorage.EncryptCookie(tenantId, user.ID, tenantSettings.Index, expires, userSettings.Index);
        }

        public Tariff GetTariff(int tenant, bool withRequestToPaymentSystem = true)
        {
            return tariffService.GetTariff(tenant, withRequestToPaymentSystem);
        }

        public TenantQuota GetTenantQuota(int tenant)
        {
            return clientTenantManager.GetTenantQuota(tenant);
        }

        public IEnumerable<TenantQuota> GetTenantQuotas()
        {
            return clientTenantManager.GetTenantQuotas();
        }

        public TenantQuota SaveTenantQuota(TenantQuota quota)
        {
            return clientTenantManager.SaveTenantQuota(quota);
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