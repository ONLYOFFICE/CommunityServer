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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

using ASC.Common.Logging;
using ASC.Core;
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

        [DataMember(Name = "LogoDocsEditorEmbedExt")]
        private string _logoDocsEditorEmbedExt;
        [DataMember(Name = "DefaultLogoDocsEditorEmbed")]
        private bool _isDefaultLogoDocsEditorEmbed { get; set; }

        [DataMember(Name = "LogoLigthExt")]
        private string _logoLightExt;
        [DataMember(Name ="DefaultLogoLight")]
        private bool _isDefaultLogoLight { get; set; }

        [DataMember(Name = "LogoAboutDarkExt")]
        private string _logoAboutDarkExt;
        [DataMember(Name = "DefaultLogoAboutDark")]
        private bool _isDefaultLogoAboutDark { get; set; }

        [DataMember(Name = "LogAboutLightExt")]
        private string _logoAboutLightExt;
        [DataMember(Name = "DefaultLogoAboutLight")]
        private bool _isDefaultLogoAboutLight { get; set; }

        [DataMember(Name = "LogoText")]
        public string _logoText { get; set; }

        public string LogoText
        {
            get
            {
                if (!string.IsNullOrEmpty(_logoText) && _logoText != DefaultLogoText)
                    return _logoText;

                var partnerSettings = LoadForDefaultTenant();
                return string.IsNullOrEmpty(partnerSettings._logoText) ? DefaultLogoText : partnerSettings._logoText;
            }
            set { _logoText = value; }
        }

        private const string moduleName = "whitelabel";

        #endregion

        #region Logo available sizes

        public static readonly Size logoLightSmallSize = new Size(284, 46);
        public static readonly Size logoDarkSize = new Size(432, 70);
        public static readonly Size logoFaviconSize = new Size(32, 32);
        public static readonly Size logoDocsEditorSize = new Size(172, 40);
        public static readonly Size logoDocsEditorEmbedSize = new Size(172, 40);
        public static readonly Size logoLightSize = new Size(432, 70);
        public static readonly Size logoAboutDarkSize = new Size(432, 70);
        public static readonly Size logoAboutLightSize = new Size(432, 70);

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
                _logoDocsEditorEmbedExt = null,
                _logoLightExt = null,
                _logoAboutDarkExt = null,
                _logoAboutLightExt = null,

                _isDefaultLogoLightSmall = true,
                _isDefaultLogoDark = true,
                _isDefaultLogoFavicon = true,
                _isDefaultLogoDocsEditor = true,
                _isDefaultLogoDocsEditorEmbed = true,
                _isDefaultLogoLight = true,
                _isDefaultLogoAboutDark = true,
                _isDefaultLogoAboutLight = true,

                LogoText = null
            };
        }
        #endregion

        #region Restore default

        public bool IsDefault
        {
            get
            {
                var defaultSettings = GetDefault() as TenantWhiteLabelSettings;

                if (defaultSettings == null) return false;

                return _logoLightSmallExt == defaultSettings._logoLightSmallExt &&
                       _logoDarkExt == defaultSettings._logoDarkExt &&
                       _logoFaviconExt == defaultSettings._logoFaviconExt &&
                       _logoDocsEditorExt == defaultSettings._logoDocsEditorExt &&
                       _logoDocsEditorEmbedExt == defaultSettings._logoDocsEditorEmbedExt &&
                       _logoLightExt == defaultSettings._logoLightExt &&
                       _logoAboutDarkExt == defaultSettings._logoAboutDarkExt &&
                       _logoAboutLightExt == defaultSettings._logoAboutLightExt &&

                       _isDefaultLogoLightSmall == defaultSettings._isDefaultLogoLightSmall &&
                       _isDefaultLogoDark == defaultSettings._isDefaultLogoDark &&
                       _isDefaultLogoFavicon == defaultSettings._isDefaultLogoFavicon &&
                       _isDefaultLogoDocsEditor == defaultSettings._isDefaultLogoDocsEditor &&
                       _isDefaultLogoDocsEditorEmbed == defaultSettings._isDefaultLogoDocsEditorEmbed &&
                       _isDefaultLogoLight == defaultSettings._isDefaultLogoLight &&
                       _isDefaultLogoAboutDark == defaultSettings._isDefaultLogoAboutDark &&
                       _isDefaultLogoAboutLight == defaultSettings._isDefaultLogoAboutLight &&

                       LogoText == defaultSettings.LogoText;
            }
        }

        public void RestoreDefault(int tenantId, IDataStore storage = null)
        {
            _logoLightSmallExt = null;
            _logoDarkExt = null;
            _logoFaviconExt = null;
            _logoDocsEditorExt = null;
            _logoDocsEditorEmbedExt = null;
            _logoLightExt = null;
            _logoAboutDarkExt = null;
            _logoAboutLightExt = null;

            _isDefaultLogoLightSmall = true;
            _isDefaultLogoDark = true;
            _isDefaultLogoFavicon = true;
            _isDefaultLogoDocsEditor = true;
            _isDefaultLogoDocsEditorEmbed = true;
            _isDefaultLogoLight = true;
            _isDefaultLogoAboutDark = true;
            _isDefaultLogoAboutLight = true;

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

        public void RestoreDefaultLogo(WhiteLabelLogoTypeEnum type, int tenantId, IDataStore storage = null)
        {
            if (!GetIsDefault(type))
            {
                try
                {
                    SetExt(type, null);
                    SetIsDefault(type, true);
                    var store = storage ?? StorageFactory.GetStorage(tenantId.ToString(), moduleName);
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

                if (!string.IsNullOrEmpty(currentLogoPath))
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
                        data = Convert.FromBase64String(xB64); // Convert the Base64 string to binary data
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
            using (var memoryStream = new MemoryStream())
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
                case WhiteLabelLogoTypeEnum.DocsEditorEmbed:
                    return _isDefaultLogoDocsEditorEmbed;
                case WhiteLabelLogoTypeEnum.Light:
                    return _isDefaultLogoLight;
                case WhiteLabelLogoTypeEnum.AboutDark:
                    return _isDefaultLogoAboutDark;
                case WhiteLabelLogoTypeEnum.AboutLight:
                    return _isDefaultLogoAboutLight;
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
                case WhiteLabelLogoTypeEnum.DocsEditorEmbed:
                    _isDefaultLogoDocsEditorEmbed = value;
                    break;
                case WhiteLabelLogoTypeEnum.Light:
                    _isDefaultLogoLight = value;
                    break;
                case WhiteLabelLogoTypeEnum.AboutDark:
                    _isDefaultLogoAboutDark = value;
                    break;
                case WhiteLabelLogoTypeEnum.AboutLight:
                    _isDefaultLogoAboutLight = value;
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
                case WhiteLabelLogoTypeEnum.DocsEditorEmbed:
                    return _logoDocsEditorEmbedExt;
                case WhiteLabelLogoTypeEnum.Light:
                    return _logoLightExt;
                case WhiteLabelLogoTypeEnum.AboutDark:
                    return _logoAboutDarkExt;
                case WhiteLabelLogoTypeEnum.AboutLight:
                    return _logoAboutLightExt;
            }
            return "";
        }

        private void SetExt(WhiteLabelLogoTypeEnum type, string fileExt)
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
                case WhiteLabelLogoTypeEnum.DocsEditorEmbed:
                    _logoDocsEditorEmbedExt = fileExt;
                    break;
                case WhiteLabelLogoTypeEnum.Light:
                    _logoLightExt = fileExt;
                    break;
                case WhiteLabelLogoTypeEnum.AboutDark:
                    _logoAboutDarkExt = fileExt;
                    break;
                case WhiteLabelLogoTypeEnum.AboutLight:
                    _logoAboutLightExt = fileExt;
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
            if (!string.IsNullOrEmpty(partnerLogoPath))
                return partnerLogoPath;

            switch (type)
            {
                case WhiteLabelLogoTypeEnum.LightSmall:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("logo/light_small_general.svg") : WebImageSupplier.GetAbsoluteWebPath("logo/light_small.svg");
                case WhiteLabelLogoTypeEnum.Dark:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("logo/dark_general.png") : WebImageSupplier.GetAbsoluteWebPath("logo/dark.png");
                case WhiteLabelLogoTypeEnum.DocsEditor:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("logo/editor_logo_general.png") : WebImageSupplier.GetAbsoluteWebPath("logo/editor_logo.png");
                case WhiteLabelLogoTypeEnum.DocsEditorEmbed:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("logo/editor_logo_embed_general.png") : WebImageSupplier.GetAbsoluteWebPath("logo/editor_logo_embed.png");
                case WhiteLabelLogoTypeEnum.Favicon:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("logo/favicon_general.ico") : WebImageSupplier.GetAbsoluteWebPath("logo/favicon.ico");
                case WhiteLabelLogoTypeEnum.Light:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("logo/light_general.png") : WebImageSupplier.GetAbsoluteWebPath("logo/light.png");
                case WhiteLabelLogoTypeEnum.AboutLight:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("logo/about_light_general.png") : WebImageSupplier.GetAbsoluteWebPath("logo/about_light.png");
                case WhiteLabelLogoTypeEnum.AboutDark:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("logo/about_dark_general.png") : WebImageSupplier.GetAbsoluteWebPath("logo/about_dark.png");
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

        public static Stream GetPartnerStorageLogoData(WhiteLabelLogoTypeEnum type, bool general)
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
            return string.Format("logo_{0}{2}.{1}", type.ToString().ToLowerInvariant(), fileExt, general ? "_general" : "");
        }

        public static Size GetSize(WhiteLabelLogoTypeEnum type, bool general)
        {
            switch (type)
            {
                case WhiteLabelLogoTypeEnum.LightSmall:
                    return new Size(
                        general ? logoLightSmallSize.Width / 2 : logoLightSmallSize.Width,
                        general ? logoLightSmallSize.Height / 2 : logoLightSmallSize.Height);
                case WhiteLabelLogoTypeEnum.Dark:
                    return new Size(
                        general ? logoDarkSize.Width / 2 : logoDarkSize.Width,
                        general ? logoDarkSize.Height / 2 : logoDarkSize.Height);
                case WhiteLabelLogoTypeEnum.Favicon:
                    return new Size(
                        general ? logoFaviconSize.Width / 2 : logoFaviconSize.Width,
                        general ? logoFaviconSize.Height / 2 : logoFaviconSize.Height);
                case WhiteLabelLogoTypeEnum.DocsEditor:
                    return new Size(
                        general ? logoDocsEditorSize.Width / 2 : logoDocsEditorSize.Width,
                        general ? logoDocsEditorSize.Height / 2 : logoDocsEditorSize.Height);
                case WhiteLabelLogoTypeEnum.DocsEditorEmbed:
                    return new Size(
                        general ? logoDocsEditorEmbedSize.Width / 2 : logoDocsEditorEmbedSize.Width,
                        general ? logoDocsEditorEmbedSize.Height / 2 : logoDocsEditorEmbedSize.Height);
                case WhiteLabelLogoTypeEnum.Light:
                    return new Size(
                        general ? logoLightSize.Width / 2 : logoLightSize.Width,
                        general ? logoLightSize.Height / 2 : logoLightSize.Height);
                case WhiteLabelLogoTypeEnum.AboutDark:
                    return new Size(
                        general ? logoAboutDarkSize.Width / 2 : logoAboutDarkSize.Width,
                        general ? logoAboutDarkSize.Height / 2 : logoAboutDarkSize.Height);
                case WhiteLabelLogoTypeEnum.AboutLight:
                    return new Size(
                        general ? logoAboutLightSize.Width / 2 : logoAboutLightSize.Width,
                        general ? logoAboutLightSize.Height / 2 : logoAboutLightSize.Height);
            }
            return new Size(0, 0);
        }

        private static void ResizeLogo(WhiteLabelLogoTypeEnum type, string fileName, byte[] data, long maxFileSize, Size size, IDataStore store)
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
            if (AppliedTenants.Contains(tenantId)) return;

            var whiteLabelSettings = LoadForTenant(tenantId);
            whiteLabelSettings.SetNewLogoText(tenantId);

            if (!AppliedTenants.Contains(tenantId)) AppliedTenants.Add(tenantId);
        }

        public void ClearAppliedTenants()
        {
            AppliedTenants.Clear();
        }

        public void Save(int tenantId, bool restore = false)
        {
            SaveForTenant(tenantId);

            if (tenantId == Tenant.DEFAULT_TENANT)
            {
                ClearAppliedTenants();
            }
            else
            {
                SetNewLogoText(tenantId, restore);
                TenantLogoManager.RemoveMailLogoDataFromCache();
            }
        }

        private void SetNewLogoText(int tenantId, bool restore = false)
        {
            var partnerSettings = LoadForDefaultTenant();

            var logoText = partnerSettings._logoText;

            WhiteLabelHelper.DefaultLogoText = CoreContext.Configuration.CustomMode && !string.IsNullOrEmpty(logoText)
                ? logoText
                : DefaultLogoText;

            if (restore && string.IsNullOrEmpty(logoText))
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