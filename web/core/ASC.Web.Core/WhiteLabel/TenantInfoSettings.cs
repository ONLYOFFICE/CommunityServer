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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Core.WhiteLabel
{
    [Serializable]
    [DataContract]
    public class TenantInfoSettings : BaseSettings<TenantInfoSettings>
    {
        [DataMember(Name = "LogoSize")]
        public Size CompanyLogoSize { get; private set; }

        [DataMember(Name = "LogoFileName")] private string _companyLogoFileName;

        [DataMember(Name = "Default")]
        private bool _isDefault { get; set; }

        #region ISettings Members

        public override ISettings GetDefault()
        {
            return new TenantInfoSettings
                       {
                           _isDefault = true
                       };
        }

        public void RestoreDefault()
        {
            RestoreDefaultTenantName();
            RestoreDefaultLogo();
        }

        public void RestoreDefaultTenantName()
        {
            var currentTenant = CoreContext.TenantManager.GetCurrentTenant();
            currentTenant.Name = ConfigurationManagerExtension.AppSettings["web.portal-name"] ?? "Cloud Office Applications";
            CoreContext.TenantManager.SaveTenant(currentTenant);
        }

        public void RestoreDefaultLogo()
        {
            _isDefault = true;

            var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "logo");
            try
            {
                store.DeleteFiles("", "*", false);
            }
            catch
            {
            }
            CompanyLogoSize = default(Size);

            TenantLogoManager.RemoveMailLogoDataFromCache();
        }

        public void SetCompanyLogo(string companyLogoFileName, byte[] data)
        {
            var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "logo");

            if (!_isDefault)
            {
                try
                {
                    store.DeleteFiles("", "*", false);
                }
                catch
                {
                }
            }
            using (var memory = new MemoryStream(data))
            using (var image = Image.FromStream(memory))
            {
                CompanyLogoSize = image.Size;
                memory.Seek(0, SeekOrigin.Begin);
                store.Save(companyLogoFileName, memory);
                _companyLogoFileName = companyLogoFileName;
            }
            _isDefault = false;

            TenantLogoManager.RemoveMailLogoDataFromCache();
        }

        public string GetAbsoluteCompanyLogoPath()
        {
            if (_isDefault)
            {
                return WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/dark_general.png");
            }

            var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "logo");
            return store.GetUri(_companyLogoFileName ?? "").ToString();
        }

        /// <summary>
        /// Get logo stream or null in case of default logo
        /// </summary>
        public Stream GetStorageLogoData()
        {
            if (_isDefault) return null;

            var storage = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(CultureInfo.InvariantCulture), "logo");

            if (storage == null) return null;

            var fileName = _companyLogoFileName ?? "";

            return storage.IsFile(fileName) ? storage.GetReadStream(fileName) : null;
        }

        public override Guid ID
        {
            get { return new Guid("{5116B892-CCDD-4406-98CD-4F18297C0C0A}"); }
        }

        #endregion
    }
}