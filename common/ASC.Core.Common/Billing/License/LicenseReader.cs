/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using ASC.Core.Tenants;
using ASC.Core.Users;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace ASC.Core.Billing
{
    public static class LicenseReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LicenseReader));

        private static string LicensePath
        {
            get { return ConfigurationManager.AppSettings["license.file.path"]; }
        }

        public static Stream GetLicenseStream()
        {
            if (!File.Exists(LicensePath)) throw new BillingNotFoundException("License not found");

            return File.OpenRead(LicensePath);
        }

        internal static void UpdateLicense(Stream licenseStream, string customerId = null)
        {
            try
            {
                SaveLicense(licenseStream);

                if (!File.Exists(LicensePath)) throw new BillingNotFoundException("License not found");

                var licenseJsonString = File.ReadAllText(LicensePath);

                LicenseToDB(licenseJsonString, customerId);
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        private static void SaveLicense(Stream licenseStream)
        {
            if (licenseStream == null) throw new ArgumentNullException("licenseStream");

            if (licenseStream.CanSeek)
            {
                licenseStream.Seek(0, SeekOrigin.Begin);
            }

            const int bufferSize = 4096;
            using (var fs = File.Open(LicensePath, FileMode.Create))
            {
                var buffer = new byte[bufferSize];
                int readed;
                while ((readed = licenseStream.Read(buffer, 0, bufferSize)) != 0)
                {
                    fs.Write(buffer, 0, readed);
                }
            }
        }

        private static void LicenseToDB(string licenseJsonString, string customerId = null)
        {
            var licenseJson = JObject.Parse(licenseJsonString);

            if (!licenseJson.Value<string>("customer_id").Equals(customerId ?? LicenseClient.CustomerId)
                || string.IsNullOrEmpty(licenseJson.Value<string>("signature")))
            {
                throw new BillingNotConfiguredException("License not correct", licenseJsonString);
            }

            var dueDate = licenseJson.Value<DateTime>("end_date");
            if (dueDate < DateTime.UtcNow)
            {
                throw new BillingException("License expired", licenseJsonString);
            }

            var features = new List<string>
                {
                    //free
                    "domain", "docs",

                    //pay
                    "audit", "backup", "controlpanel", "healthcheck", "ldap", "whitelabel",
                };

            if (licenseJson.Value<bool>("trial")) features.Add("trial");

            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            var activeUsers = licenseJson.Value<int>("user_quota");
            if (activeUsers.Equals(default(int)) || activeUsers < 1)
                activeUsers = 10000;

            if (activeUsers < CoreContext.UserManager.GetUsers(EmployeeStatus.Default, EmployeeType.User).Length)
            {
                throw new BillingException("License quota", licenseJsonString);
            }

            var quota = new TenantQuota(-1000)
                {
                    ActiveUsers = activeUsers,
                    MaxFileSize = 1024 * 1024 * 1024,
                    MaxTotalSize = 1024L * 1024 * 1024 * 1024 - 1,
                    Name = "license",
                    Features = string.Join(",", features),
                };
            CoreContext.TenantManager.SaveTenantQuota(quota);

            var tariff = new Tariff
                {
                    QuotaId = quota.Id,
                    DueDate = dueDate,
                };

            CoreContext.PaymentManager.SetTariff(tenant.TenantId, tariff);

            var affiliateId = licenseJson.Value<string>("affiliate_id");
            if (!string.IsNullOrEmpty(affiliateId))
            {
                tenant.AffiliateId = affiliateId;
                CoreContext.TenantManager.SaveTenant(tenant);
            }
        }

        private static void LogError(Exception error)
        {
            if (error is BillingNotFoundException)
            {
                Log.DebugFormat("License not found: {0}", error.Message);
            }
            else
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Error(error);
                }
                else
                {
                    Log.Error(error.Message);
                }
            }
        }
    }
}