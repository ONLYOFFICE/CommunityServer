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
using System.Configuration;
using System.IO;
using System.Linq;

using ASC.Common.Logging;
using ASC.Core.Tenants;
using ASC.Core.Users;

namespace ASC.Core.Billing
{
    public static class LicenseReader
    {
        private static readonly ILog Log = LogManager.GetLogger("ASC");
        public static readonly string LicensePath;
        private static readonly string LicensePathTemp;

        public const string CustomerIdKey = "CustomerId";
        public const int MaxUserCount = 10000;


        static LicenseReader()
        {
            LicensePath = (ConfigurationManagerExtension.AppSettings["license.file.path"] ?? "").Trim();
            LicensePathTemp = LicensePath + ".tmp";
        }


        public static string CustomerId
        {
            get { return CoreContext.Configuration.GetSetting(CustomerIdKey); }
            private set { CoreContext.Configuration.SaveSetting(CustomerIdKey, value); }
        }

        private static Stream GetLicenseStream(bool temp = false)
        {
            var path = temp ? LicensePathTemp : LicensePath;
            if (!File.Exists(path)) throw new BillingNotFoundException("License not found");

            return File.OpenRead(path);
        }

        public static void RejectLicense()
        {
            if (File.Exists(LicensePathTemp))
                File.Delete(LicensePathTemp);
            if (File.Exists(LicensePath))
                File.Delete(LicensePath);

            CoreContext.PaymentManager.DeleteDefaultTariff();
        }

        public static void RefreshLicense()
        {
            try
            {
                var temp = true;
                if (!File.Exists(LicensePathTemp))
                {
                    Log.Debug("Temp license not found");

                    if (!File.Exists(LicensePath))
                    {
                        throw new BillingNotFoundException("License not found");
                    }
                    temp = false;
                }

                using (var licenseStream = GetLicenseStream(temp))
                using (var reader = new StreamReader(licenseStream))
                {
                    var licenseJsonString = reader.ReadToEnd();
                    var license = License.Parse(licenseJsonString);

                    LicenseToDB(license);

                    if (temp)
                    {
                        SaveLicense(licenseStream, LicensePath);
                    }
                }

                if (temp)
                    File.Delete(LicensePathTemp);
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public static DateTime SaveLicenseTemp(Stream licenseStream)
        {
            try
            {
                using (var reader = new StreamReader(licenseStream))
                {
                    var licenseJsonString = reader.ReadToEnd();
                    var license = License.Parse(licenseJsonString);

                    var dueDate = Validate(license);

                    SaveLicense(licenseStream, LicensePathTemp);

                    return dueDate;
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

        private static DateTime Validate(License license)
        {
            if (string.IsNullOrEmpty(license.CustomerId)
                || string.IsNullOrEmpty(license.Signature))
            {
                throw new BillingNotConfiguredException("License not correct", license.OriginalLicense);
            }

            if (license.DueDate.Date < VersionReleaseDate)
            {
                throw new LicenseExpiredException("License expired", license.OriginalLicense);
            }

            if (license.ActiveUsers.Equals(default(int)) || license.ActiveUsers < 1)
                license.ActiveUsers = MaxUserCount;

            if (license.ActiveUsers < CoreContext.UserManager.GetUsers(EmployeeStatus.Default, EmployeeType.User).Length)
            {
                throw new LicenseQuotaException("License quota", license.OriginalLicense);
            }

            if (license.PortalCount <= 0)
            {
                license.PortalCount = CoreContext.TenantManager.GetTenantQuota(Tenant.DEFAULT_TENANT).CountPortals;
            }
            var activePortals = CoreContext.TenantManager.GetTenants().Count();
            if (activePortals > 1 && license.PortalCount < activePortals)
            {
                throw new LicensePortalException("License portal count", license.OriginalLicense);
            }

            return license.DueDate.Date;
        }

        private static void LicenseToDB(License license)
        {
            Validate(license);

            CustomerId = license.CustomerId;

            var defaultQuota = CoreContext.TenantManager.GetTenantQuota(Tenant.DEFAULT_TENANT);

            var quota = new TenantQuota(-1000)
            {
                ActiveUsers = license.ActiveUsers,
                MaxFileSize = defaultQuota.MaxFileSize,
                MaxTotalSize = defaultQuota.MaxTotalSize,
                Name = "license",
                DocsEdition = true,
                HasDomain = true,
                Audit = true,
                ControlPanel = true,
                HealthCheck = true,
                Ldap = true,
                Sso = true,
                Customization = license.Customization,
                WhiteLabel = license.WhiteLabel || license.Customization,
                Branding = license.Branding,
                SSBranding = license.SSBranding,
                Update = true,
                Support = true,
                Trial = license.Trial,
                CountPortals = license.PortalCount,
                DiscEncryption = true,
                PrivacyRoom = true,
            };

            if (defaultQuota.Name != "overdue" && !defaultQuota.Trial)
            {
                quota.WhiteLabel |= defaultQuota.WhiteLabel;
                quota.Branding |= defaultQuota.Branding;
                quota.SSBranding |= defaultQuota.SSBranding;

                quota.CountPortals = Math.Max(defaultQuota.CountPortals, quota.CountPortals);
            }

            CoreContext.TenantManager.SaveTenantQuota(quota);

            var tariff = new Tariff
            {
                QuotaId = quota.Id,
                DueDate = license.DueDate,
            };

            CoreContext.PaymentManager.SetTariff(-1, tariff);

            if (!string.IsNullOrEmpty(license.AffiliateId))
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                tenant.AffiliateId = license.AffiliateId;
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

        private static DateTime _date = DateTime.MinValue;

        public static DateTime VersionReleaseDate
        {
            get
            {
                // release sign is not longer requered
                return _date;

                /*
                if (_date != DateTime.MinValue) return _date;

                _date = DateTime.MaxValue;
                try
                {
                    var versionDate = ConfigurationManagerExtension.AppSettings["version.release-date"];
                    var sign = ConfigurationManagerExtension.AppSettings["version.release-date.sign"];

                    if (!sign.StartsWith("ASC "))
                    {
                        throw new Exception("sign without ASC");
                    }

                    var splitted = sign.Substring(4).Split(':');
                    var pkey = splitted[0];
                    if (pkey != versionDate)
                    {
                        throw new Exception("sign with different date");
                    }

                    var date = splitted[1];
                    var orighash = splitted[2];

                    var skey = MachinePseudoKeys.GetMachineConstant();

                    using (var hasher = new HMACSHA1(skey))
                    {
                        var data = string.Join("\n", date, pkey);
                        var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(data));
                        if (HttpServerUtility.UrlTokenEncode(hash) != orighash && Convert.ToBase64String(hash) != orighash)
                        {
                            throw new Exception("incorrect hash");
                        }
                    }

                    var year = Int32.Parse(versionDate.Substring(0, 4));
                    var month = Int32.Parse(versionDate.Substring(4, 2));
                    var day = Int32.Parse(versionDate.Substring(6, 2));
                    _date = new DateTime(year, month, day);
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("ASC").Error("VersionReleaseDate", ex);
                }
                return _date;*/
            }
        }
    }
}