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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Common.Logging;
using ASC.Core.Common.Settings;
using ASC.Core.Tenants;
using ASC.Data.Storage;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;
using TMResourceData;

namespace ASC.Web.Core.WhiteLabel
{
    [Serializable]
    [DataContract]
    public class TenantWhiteLabelSettings : BaseSettings<TenantWhiteLabelSettings>
    {
        public const string DefaultLogoText = "ONLYOFFICE";

        #region Logos information: extension, isDefault, text for img auto generating

        [DataMember(Name = "LogoLightSmallExt")]
        private string _logoLightSmallExt;
        [DataMember(Name = "DefaultLogoLightSmall")]
        private bool _isDefaultLogoLightSmall { get; set; }

        [DataMember(Name = "LogoDarkExt")]
        private string _logoDarkExt;
        [DataMember(Name = "DefaultLogoDark")]
        private bool _isDefaultLogoDark { get; set; }

        [DataMember(Name = "LogoFaviconExt")]
        private string _logoFaviconExt;
        [DataMember(Name = "DefaultLogoFavicon")]
        private bool _isDefaultLogoFavicon { get; set; }

        [DataMember(Name = "LogoDocsEditorExt")]
        private string _logoDocsEditorExt;
        [DataMember(Name = "DefaultLogoDocsEditor")]
        private bool _isDefaultLogoDocsEditor { get; set; }


        [DataMember(Name = "LogoText")]
        public string _logoText { get; set; }

        public string LogoText
        {
            get
            {
                if (!String.IsNullOrEmpty(_logoText) && _logoText != DefaultLogoText)
                    return _logoText;

                var partnerSettings = LoadForDefaultTenant();
                return String.IsNullOrEmpty(partnerSettings._logoText) ? DefaultLogoText : partnerSettings._logoText;
            }
            set { _logoText = value; }
        }

        private const String moduleName = "whitelabel";

        #endregion

        #region Logo available sizes

        public static readonly Size logoLightSmallSize = new Size(284, 46);
        public static readonly Size logoDarkSize = new Size(432, 70);
        public static readonly Size logoFaviconSize = new Size(32, 32);
        public static readonly Size logoDocsEditorSize = new Size(172, 40);

        #endregion

        #region ISettings Members

        public override ISettings GetDefault()
        {
            return new TenantWhiteLabelSettings
                {
                    _logoLightSmallExt = null,
                    _logoDarkExt = null,
                    _logoFaviconExt = null,
                    _logoDocsEditorExt = null,

                    _isDefaultLogoLightSmall = true,
                    _isDefaultLogoDark = true,
                    _isDefaultLogoFavicon = true,
                    _isDefaultLogoDocsEditor = true,

                    LogoText = null
                };
        }
        #endregion

        #region Restore default

        public void RestoreDefault(int tenantId, IDataStore storage = null)
        {
            _logoLightSmallExt = null;
            _logoDarkExt = null;
            _logoFaviconExt = null;
            _logoDocsEditorExt = null;

            _isDefaultLogoLightSmall = true;
            _isDefaultLogoDark = true;
            _isDefaultLogoFavicon = true;
            _isDefaultLogoDocsEditor = true;

            LogoText = null;

            var store = storage ?? StorageFactory.GetStorage(tenantId.ToString(), moduleName);
            try
            {
                store.DeleteFiles("", "*", false);
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC").Error(e);
            }

            Save(tenantId, true);
        }

        public void RestoreDefault(WhiteLabelLogoTypeEnum type)
        {
            if (!GetIsDefault(type))
            {
                try
                {
                    SetIsDefault(type, true);
                    var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), moduleName);
                    DeleteLogoFromStore(store, type);
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("ASC").Error(e);
                }
            }
        }

        #endregion

        #region Set logo

        public void SetLogo(WhiteLabelLogoTypeEnum type, string logoFileExt, byte[] data, IDataStore storage = null)
        {
            var store = storage ?? StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), moduleName);

            #region delete from storage if already exists

            var isAlreadyHaveBeenChanged = !GetIsDefault(type);

            if (isAlreadyHaveBeenChanged)
            {
                try
                {
                    DeleteLogoFromStore(store, type);
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("ASC").Error(e);
                }
            }
            #endregion

            using (var memory = new MemoryStream(data))
            using (var image = Image.FromStream(memory))
            {
                var logoSize = image.Size;
                var logoFileName = BuildLogoFileName(type, logoFileExt, false);

                memory.Seek(0, SeekOrigin.Begin);
                store.Save(logoFileName, memory);
            }

            SetExt(type, logoFileExt);
            SetIsDefault(type, false);

            var generalSize = GetSize(type, true);
            var generalFileName = BuildLogoFileName(type, logoFileExt, true);
            ResizeLogo(type, generalFileName, data, -1, generalSize, store);
        }

        public void SetLogo(Dictionary<int, string> logo, IDataStore storage = null)
        {
            var xStart = @"data:image/png;base64,";

            foreach (var currentLogo in logo)
            {
                var currentLogoType = (WhiteLabelLogoTypeEnum)(currentLogo.Key);
                var currentLogoPath = currentLogo.Value;

                if (!String.IsNullOrEmpty(currentLogoPath))
                {
                    var fileExt = "png";
                    byte[] data = null;

                    if (!currentLogoPath.StartsWith(xStart))
                    {
                        var fileName = Path.GetFileName(currentLogoPath);
                        fileExt = fileName.Split('.').Last();
                        data = UserPhotoManager.GetTempPhotoData(fileName);
                        try
                        {
                            UserPhotoManager.RemoveTempPhoto(fileName);
                        }
                        catch (Exception ex)
                        {
                            LogManager.GetLogger("ASC").Error(ex);
                        }
                    }
                    else
                    {
                        var xB64 = currentLogoPath.Substring(xStart.Length); // Get the Base64 string
                        data = System.Convert.FromBase64String(xB64); // Convert the Base64 string to binary data
                    }

                    if (data != null)
                    {
                        SetLogo(currentLogoType, fileExt, data, storage);
                    }
                }
            }
        }

        public void SetLogoFromStream(WhiteLabelLogoTypeEnum type, string fileExt, Stream fileStream, IDataStore storage = null)
        {
            byte[] data = null;
            using(var memoryStream = new MemoryStream())
            {
              fileStream.CopyTo(memoryStream);
              data = memoryStream.ToArray();
            }

            if (data != null)
            {
                SetLogo(type, fileExt, data, storage);
            }
        }

        #endregion

        #region Get/Set IsDefault and Extension

        private bool GetIsDefault(WhiteLabelLogoTypeEnum type)
        {
            switch (type)
            {
                case WhiteLabelLogoTypeEnum.LightSmall:
                    return _isDefaultLogoLightSmall;
                case WhiteLabelLogoTypeEnum.Dark:
                    return _isDefaultLogoDark;
                case WhiteLabelLogoTypeEnum.Favicon:
                    return _isDefaultLogoFavicon;
                case WhiteLabelLogoTypeEnum.DocsEditor:
                    return _isDefaultLogoDocsEditor;
            }
            return true;
        }

        private void SetIsDefault(WhiteLabelLogoTypeEnum type, bool value)
        {
            switch (type)
            {
                case WhiteLabelLogoTypeEnum.LightSmall:
                    _isDefaultLogoLightSmall = value;
                    break;
                case WhiteLabelLogoTypeEnum.Dark:
                    _isDefaultLogoDark = value;
                    break;
                case WhiteLabelLogoTypeEnum.Favicon:
                    _isDefaultLogoFavicon = value;
                    break;
                case WhiteLabelLogoTypeEnum.DocsEditor:
                    _isDefaultLogoDocsEditor = value;
                    break;
            }
        }

        private string GetExt(WhiteLabelLogoTypeEnum type)
        {
            switch (type)
            {
                case WhiteLabelLogoTypeEnum.LightSmall:
                    return _logoLightSmallExt;
                case WhiteLabelLogoTypeEnum.Dark:
                    return _logoDarkExt;
                case WhiteLabelLogoTypeEnum.Favicon:
                    return _logoFaviconExt;
                case WhiteLabelLogoTypeEnum.DocsEditor:
                    return _logoDocsEditorExt;
            }
            return "";
        }

        private void SetExt(WhiteLabelLogoTypeEnum type, String fileExt)
        {
            switch (type)
            {
                case WhiteLabelLogoTypeEnum.LightSmall:
                    _logoLightSmallExt = fileExt;
                    break;
                case WhiteLabelLogoTypeEnum.Dark:
                    _logoDarkExt = fileExt;
                    break;
                case WhiteLabelLogoTypeEnum.Favicon:
                    _logoFaviconExt = fileExt;
                    break;
                case WhiteLabelLogoTypeEnum.DocsEditor:
                    _logoDocsEditorExt = fileExt;
                    break;
            }
        }

        #endregion

        #region Get logo path

        public string GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum type, bool general = true)
        {
            if (GetIsDefault(type))
            {
                return GetAbsoluteDefaultLogoPath(type, general);
            }

            return GetAbsoluteStorageLogoPath(type, general);
        }

        private string GetAbsoluteStorageLogoPath(WhiteLabelLogoTypeEnum type, bool general)
        {
            var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), moduleName);
            var fileName = BuildLogoFileName(type, GetExt(type), general);

            if (store.IsFile(fileName))
            {
                return store.GetUri(fileName).ToString();
            }
            return GetAbsoluteDefaultLogoPath(type, general);
        }

        public static string GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum type, bool general)
        {
            var partnerLogoPath = GetPartnerStorageLogoPath(type, general);
            if (!String.IsNullOrEmpty(partnerLogoPath))
                return partnerLogoPath;

            switch (type)
            {
                case WhiteLabelLogoTypeEnum.LightSmall:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/light_small_general.svg") : WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/light_small.svg");
                case WhiteLabelLogoTypeEnum.Dark:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/dark_general.png") : WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/dark.png");
                case WhiteLabelLogoTypeEnum.DocsEditor:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/editor_logo_general.png") : WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/editor_logo.png");
                case WhiteLabelLogoTypeEnum.Favicon:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/favicon_general.ico") : WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/favicon.ico");
            }
            return "";
        }

        private static string GetPartnerStorageLogoPath(WhiteLabelLogoTypeEnum type, bool general)
        {
            var partnerSettings = LoadForDefaultTenant();

            if (partnerSettings.GetIsDefault(type)) return null;

            var partnerStorage = StorageFactory.GetStorage(string.Empty, "static_partnerdata");

            if (partnerStorage == null) return null;

            var logoPath = BuildLogoFileName(type, partnerSettings.GetExt(type), general);

            return partnerStorage.IsFile(logoPath) ? partnerStorage.GetUri(logoPath).ToString() : null;
        }

        #endregion

        #region Get Whitelabel Logo Stream

        /// <summary>
        /// Get logo stream or null in case of default whitelabel
        /// </summary>
        public Stream GetWhitelabelLogoData(WhiteLabelLogoTypeEnum type, bool general)
        {
            if (GetIsDefault(type))
                return GetPartnerStorageLogoData(type, general);

            return GetStorageLogoData(type, general);
        }

        private Stream GetStorageLogoData(WhiteLabelLogoTypeEnum type, bool general)
        {
            var storage = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(CultureInfo.InvariantCulture), moduleName);

            if (storage == null) return null;

            var fileName = BuildLogoFileName(type, GetExt(type), general);

            return storage.IsFile(fileName) ? storage.GetReadStream(fileName) : null;
        }

        private Stream GetPartnerStorageLogoData(WhiteLabelLogoTypeEnum type, bool general)
        {
            var partnerSettings = LoadForDefaultTenant();

            if (partnerSettings.GetIsDefault(type)) return null;

            var partnerStorage = StorageFactory.GetStorage(string.Empty, "static_partnerdata");

            if (partnerStorage == null) return null;

            var fileName = BuildLogoFileName(type, partnerSettings.GetExt(type), general);

            return partnerStorage.IsFile(fileName) ? partnerStorage.GetReadStream(fileName) : null;
        }

        #endregion

        public static string BuildLogoFileName(WhiteLabelLogoTypeEnum type, String fileExt, bool general)
        {
            return String.Format("logo_{0}{2}.{1}", type.ToString().ToLowerInvariant(), fileExt, general ? "_general" : "");
        }

        public static Size GetSize(WhiteLabelLogoTypeEnum type, bool general)
        {
            switch (type)
            {
                case WhiteLabelLogoTypeEnum.LightSmall:
                    return new Size(
                        general ? TenantWhiteLabelSettings.logoLightSmallSize.Width / 2 : TenantWhiteLabelSettings.logoLightSmallSize.Width,
                        general ? TenantWhiteLabelSettings.logoLightSmallSize.Height / 2 : TenantWhiteLabelSettings.logoLightSmallSize.Height);
                case WhiteLabelLogoTypeEnum.Dark:
                    return new Size(
                        general ? TenantWhiteLabelSettings.logoDarkSize.Width / 2 : TenantWhiteLabelSettings.logoDarkSize.Width,
                        general ? TenantWhiteLabelSettings.logoDarkSize.Height / 2 : TenantWhiteLabelSettings.logoDarkSize.Height);
                case WhiteLabelLogoTypeEnum.Favicon:
                    return new Size(
                        general ? TenantWhiteLabelSettings.logoFaviconSize.Width / 2 : TenantWhiteLabelSettings.logoFaviconSize.Width,
                        general ? TenantWhiteLabelSettings.logoFaviconSize.Height / 2 : TenantWhiteLabelSettings.logoFaviconSize.Height);
                case WhiteLabelLogoTypeEnum.DocsEditor:
                    return new Size(
                        general ? TenantWhiteLabelSettings.logoDocsEditorSize.Width / 2 : TenantWhiteLabelSettings.logoDocsEditorSize.Width,
                        general ? TenantWhiteLabelSettings.logoDocsEditorSize.Height / 2 : TenantWhiteLabelSettings.logoDocsEditorSize.Height);
            }
            return new Size(0, 0);
        }

        private static void ResizeLogo(WhiteLabelLogoTypeEnum type, String fileName, byte[] data, long maxFileSize, Size size, IDataStore store)
        {
            //Resize synchronously
            if (data == null || data.Length <= 0) throw new UnknownImageFormatException();
            if (maxFileSize != -1 && data.Length > maxFileSize) throw new ImageWeightLimitException();

            try
            {
                using (var stream = new MemoryStream(data))
                using (var img = Image.FromStream(stream))
                {
                    var imgFormat = img.RawFormat;
                    if (size != img.Size)
                    {
                        using (var img2 = CommonPhotoManager.DoThumbnail(img, size, false, true, false))
                        {
                            data = CommonPhotoManager.SaveToBytes(img2);
                        }
                    }
                    else
                    {
                        data = CommonPhotoManager.SaveToBytes(img);
                    }

                    //fileExt = CommonPhotoManager.GetImgFormatName(imgFormat);

                    using (var stream2 = new MemoryStream(data))
                    {
                        store.Save(fileName, stream2);
                        //NOTE: Update cache here
                        //var filePath = Path.GetFileName(photoUrl);

                        //AddToCache(item.UserId, item.Size, fileNPath, true);
                    }
                }
            }
            catch (ArgumentException error)
            {
                throw new UnknownImageFormatException(error);
            }
        }

        public override Guid ID
        {
            get { return new Guid("{05d35540-c80b-4b17-9277-abd9e543bf93}"); }
        }

        #region Save for Resource replacement

        private static readonly List<int> AppliedTenants = new List<int>();

        public static void Apply(int tenantId)
        {
            if (!DBResourceManager.ResourcesFromDataBase) return;

            if (AppliedTenants.Contains(tenantId)) return;

            var whiteLabelSettings = LoadForTenant(tenantId);
            whiteLabelSettings.SetNewLogoText(tenantId);

            if (!AppliedTenants.Contains(tenantId)) AppliedTenants.Add(tenantId);
        }

        public void Save(int tenantId, bool restore = false)
        {
            SaveForTenant(tenantId);

            if (tenantId == Tenant.DEFAULT_TENANT)
            {
                AppliedTenants.Clear();
            }
            else
            {
                SetNewLogoText(tenantId, restore);
                TenantLogoManager.RemoveMailLogoDataFromCache();
            }
        }

        private void SetNewLogoText(int tenantId, bool restore = false)
        {
            WhiteLabelHelper.DefaultLogoText = DefaultLogoText;

            var partnerSettings = LoadForDefaultTenant();

            if (restore && String.IsNullOrEmpty(partnerSettings._logoText))
            {
                WhiteLabelHelper.RestoreOldText(tenantId);
            }
            else
            {
                WhiteLabelHelper.SetNewText(tenantId, LogoText);
            }
        }

        #endregion

        #region Delete from Store

        private void DeleteLogoFromStore(IDataStore store, WhiteLabelLogoTypeEnum type)
        {
            DeleteLogoFromStoreByGeneral(store, type, false);
            DeleteLogoFromStoreByGeneral(store, type, true);
        }

        private void DeleteLogoFromStoreByGeneral(IDataStore store, WhiteLabelLogoTypeEnum type, bool general)
        {
            var fileExt = GetExt(type);
            var logo = BuildLogoFileName(type, fileExt, general);
            if (store.IsFile(logo))
            {
                store.Delete(logo);
            }
        }

        #endregion
    }
}