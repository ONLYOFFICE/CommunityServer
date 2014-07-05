/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using ASC.Core.Billing;
using ASC.Core.Caching;
using ASC.Core.Data;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System.Configuration;

namespace ASC.Core
{
    public static class CoreContext
    {
        static CoreContext()
        {
            ConfigureCoreContextByDefault();
            
            var section = ConfigurationManager.GetSection("unity");
            if (section != null)
            {
                ConfigureCoreContextByUnity(section);
            }
        }


        public static IConfigurationClient Configuration { get; private set; }

        public static ITenantManagerClient TenantManager { get; private set; }

        public static IUserManagerClient UserManager { get; private set; }

        public static IGroupManagerClient GroupManager { get; private set; }

        public static IAuthManagerClient Authentication { get; private set; }

        public static IAzManagerClient AuthorizationManager { get; private set; }

        public static IPaymentManagerClient PaymentManager { get; private set; }

        internal static ISubscriptionManagerClient SubscriptionManager { get; private set; }


        private static void ConfigureCoreContextByDefault()
        {
            var cs = ConfigurationManager.ConnectionStrings["core"];
            if (cs == null)
            {
                throw new ConfigurationErrorsException("Can not configure CoreContext: connection string with name core not found.");
            }

            var tenantService = new CachedTenantService(new DbTenantService(cs));
            var userService = new CachedUserService(new DbUserService(cs));
            var azService = new CachedAzService(new DbAzService(cs));
            var quotaService = new CachedQuotaService(new DbQuotaService(cs));
            var subService = new CachedSubscriptionService(new DbSubscriptionService(cs));
            var tariffService = new TariffService(cs, quotaService, tenantService);

            Configuration = new ClientConfiguration(tenantService);
            TenantManager = new ClientTenantManager(tenantService, quotaService, tariffService);
            PaymentManager = new ClientPaymentManager(Configuration, quotaService, tariffService);
            UserManager = new ClientUserManager(userService);
            GroupManager = new ClientUserManager(userService);
            Authentication = new ClientAuthManager(userService);
            AuthorizationManager = new ClientAzManager(azService);
            SubscriptionManager = new ClientSubscriptionManager(subService);
        }

        private static void ConfigureCoreContextByUnity(object section)
        {
            if (((UnityConfigurationSection)section).Containers["Core"] != null)
            {
                var unity = new UnityContainer().LoadConfiguration("Core");
                if (unity.IsRegistered<IConfigurationClient>())
                {
                    Configuration = unity.Resolve<IConfigurationClient>();
                }
                if (unity.IsRegistered<ITenantManagerClient>())
                {
                    TenantManager = unity.Resolve<ITenantManagerClient>();
                }
                if (unity.IsRegistered<IUserManagerClient>())
                {
                    UserManager = unity.Resolve<IUserManagerClient>();
                }
                if (unity.IsRegistered<IGroupManagerClient>())
                {
                    GroupManager = unity.Resolve<IGroupManagerClient>();
                }
                if (unity.IsRegistered<IAuthManagerClient>())
                {
                    Authentication = unity.Resolve<IAuthManagerClient>();
                }
                if (unity.IsRegistered<IAzManagerClient>())
                {
                    AuthorizationManager = unity.Resolve<IAzManagerClient>();
                }
                if (unity.IsRegistered<ISubscriptionManagerClient>())
                {
                    SubscriptionManager = unity.Resolve<ISubscriptionManagerClient>();
                }
            }
        }
    }
}