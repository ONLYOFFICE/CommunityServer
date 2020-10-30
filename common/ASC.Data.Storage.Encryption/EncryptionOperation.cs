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
using System.Linq;
using System.Runtime.Serialization;
using ASC.Common.Logging;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Data.Storage.DiscStorage;
using System.IO;
using System.Threading.Tasks;
using Resources;
using System.Configuration;

namespace ASC.Data.Storage.Encryption
{
    [DataContract]
    public class EncryptionOperation : ProgressBase
    {
        private static readonly ILog Log;
        private static readonly string ConfigPath;
        private static readonly IEnumerable<string> Modules;
        private static readonly bool UseProgressFile;

        private readonly IEnumerable<Tenant> Tenants;
        private readonly EncryptionSettings Settings;
        private readonly NotifyHelper NotifyHelper;
        private readonly bool IsEncryption;

        private bool HasErrors;

        private const string ProgressFileName = "EncryptionProgress.tmp";


        static EncryptionOperation()
        {
            Log = LogManager.GetLogger("ASC");
            ConfigPath = "";
            Modules = StorageFactory.GetModuleList(ConfigPath, "disc", true);
            UseProgressFile = Convert.ToBoolean(ConfigurationManagerExtension.AppSettings["storage.encryption.progressfile"] ?? "true");
        }

        public EncryptionOperation(EncryptionSettings encryptionSettings, string serverRootPath)
        {
            Settings = encryptionSettings;
            Tenants = CoreContext.TenantManager.GetTenants(false);
            StepCount = Modules.Count() * Tenants.Count();
            NotifyHelper = new NotifyHelper(serverRootPath);
            IsEncryption = Settings.Status == EncryprtionStatus.EncryptionStarted;
            HasErrors = false;
        }


        protected override void DoJob()
        {
            try
            {
                if (!CoreContext.Configuration.Standalone)
                {
                    throw new NotSupportedException(Resource.ErrorServerEditionMethod);
                }

                if (Settings.Status == EncryprtionStatus.Encrypted || Settings.Status == EncryprtionStatus.Decrypted)
                {
                    Log.Debug("Storage already " + Settings.Status);
                    return;
                }

                foreach (var tenant in Tenants)
                {
                    Parallel.ForEach(Modules, (module) =>
                    {
                        EncryptStore(tenant, module);
                    });
                }

                if (!HasErrors)
                {
                    DeleteProgressFiles();
                    SaveNewSettings();
                }

                ActivateTenants();
            }
            catch (Exception e)
            {
                Error = e;
                Log.Error(e);
            }
        }


        private void EncryptStore(Tenant tenant, string module)
        {
            var store = (DiscDataStore)StorageFactory.GetStorage(ConfigPath, tenant.TenantId.ToString(), module);

            var domains = StorageFactory.GetDomainList(ConfigPath, module).ToList();

            domains.Add(string.Empty);

            var progress = ReadProgress(store);

            foreach (var domain in domains)
            {
                var logParent = string.Format("Tenant: {0}, Module: {1}, Domain: {2}", tenant.TenantAlias, module, domain);

                var files = GetFiles(domains, progress, store, domain);

                EncryptFiles(store, domain, files, logParent);
            }

            StepDone();

            Log.DebugFormat("Percentage: {0}", Percentage);
        }

        private List<string> ReadProgress(DiscDataStore store)
        {
            var encryptedFiles = new List<string>();

            if (!UseProgressFile)
            {
                return encryptedFiles;
            }

            if (store.IsFile(string.Empty, ProgressFileName))
            {
                using (var stream = store.GetReadStream(string.Empty, ProgressFileName, false))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            encryptedFiles.Add(line);
                        }
                    }
                }
            }
            else
            {
                store.GetWriteStream(string.Empty, ProgressFileName, FileMode.Create).Close();
            }

            return encryptedFiles;
        }

        private IEnumerable<string> GetFiles(List<string> domains, List<string> progress, DiscDataStore targetStore, string targetDomain)
        {
            IEnumerable<string> files = targetStore.ListFilesRelative(targetDomain, "\\", "*.*", true);

            if (progress.Any())
            {
                files = files.Where(path => !progress.Contains(path));
            }

            if (!string.IsNullOrEmpty(targetDomain))
            {
                return files;
            }

            var notEmptyDomains = domains.Where(domain => !string.IsNullOrEmpty(domain));

            if (notEmptyDomains.Any())
            {
                files = files.Where(path => notEmptyDomains.All(domain => !path.Contains(domain + Path.DirectorySeparatorChar)));
            }

            files = files.Where(path => !path.EndsWith(ProgressFileName));

            return files;
        }

        private void EncryptFiles(DiscDataStore store, string domain, IEnumerable<string> files, string logParent)
        {
            foreach (var file in files)
            {
                var logItem = string.Format("{0}, File: {1}", logParent, file);

                Log.Debug(logItem);

                try
                {
                    if (IsEncryption)
                    {
                        store.Encrypt(domain, file);
                    }
                    else
                    {
                        store.Decrypt(domain, file);
                    }

                    WriteProgress(store, file);
                }
                catch (Exception e)
                {
                    HasErrors = true;
                    Log.Error(logItem + " " + e.Message, e);

                    // ERROR_DISK_FULL: There is not enough space on the disk.
                    // if (e is IOException && e.HResult == unchecked((int)0x80070070)) break;
                }
            }
        }

        private void WriteProgress(DiscDataStore store, string file)
        {
            if (!UseProgressFile)
            {
                return;
            }

            using (var stream = store.GetWriteStream(string.Empty, ProgressFileName, FileMode.Append))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine(file);
                }
            }
        }

        private void DeleteProgressFiles()
        {
            foreach (var tenant in Tenants)
            {
                foreach (var module in Modules)
                {
                    var store = (DiscDataStore)StorageFactory.GetStorage(ConfigPath, tenant.TenantId.ToString(), module);

                    if (store.IsFile(string.Empty, ProgressFileName))
                    {
                        store.Delete(string.Empty, ProgressFileName);
                    }
                }
            }
        }

        private void SaveNewSettings()
        {
            if (IsEncryption)
            {
                Settings.Status = EncryprtionStatus.Encrypted;
            }
            else
            {
                Settings.Status = EncryprtionStatus.Decrypted;
                Settings.Password = string.Empty;
            }

            Settings.Save();

            Log.Debug("Save new EncryptionSettings");
        }

        private void ActivateTenants()
        {
            foreach (var tenant in Tenants)
            {
                if (tenant.Status == TenantStatus.Encryption)
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant);

                    tenant.SetStatus(TenantStatus.Active);
                    CoreContext.TenantManager.SaveTenant(tenant);
                    Log.DebugFormat("Tenant {0} SetStatus Active", tenant.TenantAlias);

                    if (!HasErrors)
                    {
                        if (Settings.NotifyUsers)
                        {
                            if (IsEncryption)
                            {
                                NotifyHelper.SendStorageEncryptionSuccess(tenant.TenantId);
                            }
                            else
                            {
                                NotifyHelper.SendStorageDecryptionSuccess(tenant.TenantId);
                            }
                            Log.DebugFormat("Tenant {0} SendStorageEncryptionSuccess", tenant.TenantAlias);
                        }
                    }
                    else
                    {
                        if (IsEncryption)
                        {
                            NotifyHelper.SendStorageEncryptionError(tenant.TenantId);
                        }
                        else
                        {
                            NotifyHelper.SendStorageDecryptionError(tenant.TenantId);
                        }
                        Log.DebugFormat("Tenant {0} SendStorageEncryptionError", tenant.TenantAlias);
                    }
                }
            }
        }
    }
}
