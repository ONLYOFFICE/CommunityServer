/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

            return license.DueDate.Date;
        }

        private static void LicenseToDB(License license)
        {
            Validate(license);

            CustomerId = license.CustomerId;

            var defaultQuota = CoreContext.TenantManager.GetTenantQuota(Tenant.DEFAULT_TENANT);

            var quota = new TenantQuota(-1000)
            {
                ActiveUsers = Constants.MaxEveryoneCount,
                MaxFileSize = defaultQuota.MaxFileSize,
                MaxTotalSize = defaultQuota.MaxTotalSize,
                Name = "license",
                DocsEdition = true,
                Customization = license.Customization,
                Update = true,
                Trial = license.Trial,
            };

            CoreContext.TenantManager.SaveTenantQuota(quota);

            var tariff = new Tariff
            {
                QuotaId = quota.Id,
                DueDate = license.DueDate,
            };

            CoreContext.PaymentManager.SetTariff(-1, tariff);
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