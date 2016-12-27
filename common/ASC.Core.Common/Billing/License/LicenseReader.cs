/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Configuration;
using System.IO;
using System.Linq;

namespace ASC.Core.Billing
{
    public static class LicenseReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LicenseReader));
        private static readonly string licensePath;
        private static readonly string licensePathTemp;

        public const string CustomerIdKey = "CustomerId";
        public const int MaxUserCount = 10000;


        static LicenseReader()
        {
            licensePath = ConfigurationManager.AppSettings["license.file.path"];
            licensePathTemp = licensePath + ".tmp";
        }


        public static string CustomerId
        {
            get { return CoreContext.Configuration.GetSetting(CustomerIdKey); }
            private set { CoreContext.Configuration.SaveSetting(CustomerIdKey, value); }
        }

        public static Stream GetLicenseStream(bool temp = false)
        {
            var path = temp ? licensePathTemp : licensePath;
            if (!File.Exists(path)) throw new BillingNotFoundException("License not found");

            return File.OpenRead(path);
        }

        public static void RejectLicense()
        {
            if (File.Exists(licensePathTemp))
                File.Delete(licensePathTemp);
            if (File.Exists(licensePath))
                File.Delete(licensePath);

            CoreContext.PaymentManager.DeleteDefaultTariff();
        }

        public static void RefreshLicense()
        {
            try
            {
                var temp = true;
                if (!File.Exists(licensePathTemp))
                {
                    Log.Debug("Temp license not found");

                    if (!File.Exists(licensePath))
                    {
                        throw new BillingNotFoundException("License not found");
                    }
                    temp = false;
                }

                using (var licenseStream = GetLicenseStream(temp))
                using (var reader = new StreamReader(licenseStream))
                {
                    var licenseJsonString = reader.ReadToEnd();

                    LicenseToDB(licenseJsonString);

                    if (temp)
                    {
                        SaveLicense(licenseStream, licensePath);
                    }
                }

                if (temp)
                    File.Delete(licensePathTemp);
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public static void SaveLicenseTemp(Stream licenseStream)
        {
            try
            {
                using (var reader = new StreamReader(licenseStream))
                {
                    var licenseJsonString = reader.ReadToEnd();

                    string customerId;
                    DateTime dueDate;
                    int activeUsers;
                    int countPortals;
                    Validate(licenseJsonString, out customerId, out dueDate, out activeUsers, out countPortals);

                    SaveLicense(licenseStream, licensePathTemp);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        private static void SaveLicense(Stream licenseStream, string path)
        {
            if (licenseStream == null) throw new ArgumentNullException("licenseStream");

            if (licenseStream.CanSeek)
            {
                licenseStream.Seek(0, SeekOrigin.Begin);
            }

            const int bufferSize = 4096;
            using (var fs = File.Open(path, FileMode.Create))
            {
                var buffer = new byte[bufferSize];
                int readed;
                while ((readed = licenseStream.Read(buffer, 0, bufferSize)) != 0)
                {
                    fs.Write(buffer, 0, readed);
                }
            }
        }

        private static JObject Validate(string licenseJsonString, out string customerId, out DateTime dueDate, out int activeUsers, out int countPortals)
        {
            var licenseJson = JObject.Parse(licenseJsonString);

            if (string.IsNullOrEmpty(customerId = licenseJson.Value<string>("customer_id"))
                || string.IsNullOrEmpty(licenseJson.Value<string>("signature")))
            {
                throw new BillingNotConfiguredException("License not correct", licenseJsonString);
            }

            dueDate = licenseJson.Value<DateTime>("end_date");
            if (dueDate.Date < DateTime.UtcNow.Date)
            {
                throw new BillingException("License expired", licenseJsonString);
            }

            activeUsers = licenseJson.Value<int>("user_quota");
            if (activeUsers.Equals(default(int)) || activeUsers < 1)
                activeUsers = MaxUserCount;

            if (activeUsers < CoreContext.UserManager.GetUsers(EmployeeStatus.Default, EmployeeType.User).Length)
            {
                throw new BillingException("License quota", licenseJsonString);
            }

            countPortals = licenseJson.Value<int>("portal_count");
            if (countPortals <= 0)
            {
                countPortals = CoreContext.TenantManager.GetTenantQuota(Tenant.DEFAULT_TENANT).CountPortals;
            }
            var activePortals = CoreContext.TenantManager.GetTenants().Count(t => t.Status == TenantStatus.Active);
            if (activePortals > 1 && countPortals < activePortals)
            {
                throw new BillingException("License portal count", licenseJsonString);
            }

            return licenseJson;
        }

        private static void LicenseToDB(string licenseJsonString)
        {
            string customerId;
            DateTime dueDate;
            int activeUsers;
            int countPortals;
            var licenseJson = Validate(licenseJsonString, out customerId, out dueDate, out activeUsers, out countPortals);

            CustomerId = customerId;

            var defaultQuota = CoreContext.TenantManager.GetTenantQuota(Tenant.DEFAULT_TENANT);

            var quota = new TenantQuota(-1000)
                {
                    ActiveUsers = activeUsers,
                    MaxFileSize = defaultQuota.MaxFileSize,
                    MaxTotalSize = defaultQuota.MaxTotalSize,
                    Name = "license",
                    HasDomain = true,
                    Audit = true,
                    ControlPanel = true,
                    HealthCheck = true,
                    Ldap = true,
                    WhiteLabel = true,
                    Update = true,
                    Support = true,
                    Trial = licenseJson.Value<bool>("trial"),
                    CountPortals = countPortals,
                };
            CoreContext.TenantManager.SaveTenantQuota(quota);

            var tariff = new Tariff
                {
                    QuotaId = quota.Id,
                    DueDate = dueDate,
                };

            CoreContext.PaymentManager.SetTariff(-1, tariff);

            var affiliateId = licenseJson.Value<string>("affiliate_id");
            if (!string.IsNullOrEmpty(affiliateId))
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant();
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