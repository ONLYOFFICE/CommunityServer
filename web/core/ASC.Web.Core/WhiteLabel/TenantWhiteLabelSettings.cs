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


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

using ASC.Data.Storage;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;
using TMResourceData;


namespace ASC.Web.Core.WhiteLabel
{
    [Serializable]
    [DataContract]
    public class TenantWhiteLabelSettings : ISettings
    {
        #region Logos information: extension, isDefault, text for img auto generating

        [DataMember(Name = "LogoLightExt")]
        private string _logoLightExt;
        [DataMember(Name = "DefaultLogoLight")]
        private bool _isDefaultLogoLight { get; set; }

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


        [DataMember(Name = "LogoDocsEditorEmbeddedExt")]
        private string _logoDocsEditorEmbeddedExt;
        [DataMember(Name = "DefaultLogoDocsEditorEmbedded")]
        private bool _isDefaultLogoDocsEditorEmbedded { get; set; }


        [DataMember(Name = "LogoText")]
        public string LogoText { get; set; }

        private const String moduleName = "whitelabel";

        #endregion

        #region Logo available sizes

        public static readonly Size logoLightSize = new Size(432, 70);
        public static readonly Size logoLightSmallSize = new Size(284, 46);

        public static readonly Size logoDarkSize = new Size(432, 70);

        public static readonly Size logoFaviconSize = new Size(32, 32);
        public static readonly Size logoDocsEditorSize = new Size(172, 40);

        public static readonly Size logoDocsEditorEmbeddedSize = new Size(248, 40);

        #endregion

        #region ISettings Members

        public ISettings GetDefault()
        {
            return new TenantWhiteLabelSettings()
            {
                        _isDefaultLogoLight = true,
                        _isDefaultLogoLightSmall = true,
                        _isDefaultLogoDark = true,
                        _isDefaultLogoFavicon = true,
                        _isDefaultLogoDocsEditor = true,
                        LogoText = null
                    };
        }
        #endregion

        #region Restore default

        public void RestoreDefault()
        {
            _isDefaultLogoLight = true;
            _isDefaultLogoLightSmall = true;

            _isDefaultLogoDark = true;

            _isDefaultLogoFavicon = true;
            _isDefaultLogoDocsEditor = true;
            _isDefaultLogoDocsEditorEmbedded = true;

            LogoText = null;

            var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), moduleName);
            try
            {
                store.DeleteFiles("", "*", false);
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger("ASC").Error(e);
            }

            Save(true);
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
                    log4net.LogManager.GetLogger("ASC").Error(e);
                }
            }
        }

        #endregion

        #region Set logo

        public void SetLogo(WhiteLabelLogoTypeEnum type, string logoFileExt, byte[] data)
        {
            var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), moduleName);

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
                    log4net.LogManager.GetLogger("ASC").Error(e);
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

            #region docs embedded logo from logo for about/login page

            if (type == WhiteLabelLogoTypeEnum.Dark) {

                isAlreadyHaveBeenChanged = !GetIsDefault(WhiteLabelLogoTypeEnum.DocsEditorEmbedded);

                if (isAlreadyHaveBeenChanged)
                {
                    try
                    {
                        DeleteLogoFromStore(store, WhiteLabelLogoTypeEnum.DocsEditorEmbedded);
                    }
                    catch (Exception e)
                    {
                        log4net.LogManager.GetLogger("ASC").Error(e);
                    }
                }


                var tmpSize = GetSize(WhiteLabelLogoTypeEnum.DocsEditorEmbedded, false);
                var tmpFileName = BuildLogoFileName(WhiteLabelLogoTypeEnum.DocsEditorEmbedded, logoFileExt, false);

                ResizeLogo(WhiteLabelLogoTypeEnum.DocsEditorEmbedded, tmpFileName, data, -1, tmpSize, store);

                tmpSize = GetSize(WhiteLabelLogoTypeEnum.DocsEditorEmbedded, true);
                tmpFileName = BuildLogoFileName(WhiteLabelLogoTypeEnum.DocsEditorEmbedded, logoFileExt, true);

                ResizeLogo(WhiteLabelLogoTypeEnum.DocsEditorEmbedded, tmpFileName, data, -1, tmpSize, store);

                SetExt(WhiteLabelLogoTypeEnum.DocsEditorEmbedded, logoFileExt);
                SetIsDefault(WhiteLabelLogoTypeEnum.DocsEditorEmbedded, false);

            }
            #endregion
        }

        public void SetLogo(Dictionary<int, string> logo)
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
                            log4net.LogManager.GetLogger("ASC").Error(ex);
                        }
                    }
                    else
                    {
                        var xB64 = currentLogoPath.Substring(xStart.Length); // Get the Base64 string
                        data = System.Convert.FromBase64String(xB64); // Convert the Base64 string to binary data
                    }

                    if (data != null)
                    {
                        SetLogo(currentLogoType, fileExt, data);
                    }
                }
            }
        }

        public void SetLogoFromStream(WhiteLabelLogoTypeEnum type, string fileExt, Stream fileStream)
        {
            byte[] data = null;
            using(var memoryStream = new MemoryStream())
            {
              fileStream.CopyTo(memoryStream);
              data = memoryStream.ToArray();
            }

            if (data != null)
            {
                SetLogo(type, fileExt, data);
            }
        }

        #endregion

        #region Get/Set IsDefault and Extension

        private bool GetIsDefault(WhiteLabelLogoTypeEnum type)
        {
            switch (type)
            {
                case WhiteLabelLogoTypeEnum.Light:
                    return _isDefaultLogoLight;
                case WhiteLabelLogoTypeEnum.LightSmall:
                    return _isDefaultLogoLightSmall;
                case WhiteLabelLogoTypeEnum.Dark:
                    return _isDefaultLogoDark;
                case WhiteLabelLogoTypeEnum.Favicon:
                    return _isDefaultLogoFavicon;
                case WhiteLabelLogoTypeEnum.DocsEditor:
                    return _isDefaultLogoDocsEditor;
                case WhiteLabelLogoTypeEnum.DocsEditorEmbedded:
                    return _isDefaultLogoDocsEditorEmbedded;
            }
            return true;
        }

        private void SetIsDefault(WhiteLabelLogoTypeEnum type, bool value)
        {
            switch (type)
            {
                case WhiteLabelLogoTypeEnum.Light:
                    _isDefaultLogoLight = value;
                    break;
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
                case WhiteLabelLogoTypeEnum.DocsEditorEmbedded:
                    _isDefaultLogoDocsEditorEmbedded = value;
                    break;
            }
        }

        private string GetExt(WhiteLabelLogoTypeEnum type)
        {
            switch (type)
            {
                case WhiteLabelLogoTypeEnum.Light:
                    return _logoLightExt;
                case WhiteLabelLogoTypeEnum.LightSmall:
                    return _logoLightSmallExt;
                case WhiteLabelLogoTypeEnum.Dark:
                    return _logoDarkExt;
                case WhiteLabelLogoTypeEnum.Favicon:
                    return _logoFaviconExt;
                case WhiteLabelLogoTypeEnum.DocsEditor:
                    return _logoDocsEditorExt;
                case WhiteLabelLogoTypeEnum.DocsEditorEmbedded:
                    return _logoDocsEditorEmbeddedExt;
            }
            return "";
        }

        private void SetExt(WhiteLabelLogoTypeEnum type, String fileExt)
        {
            switch (type)
            {
                case WhiteLabelLogoTypeEnum.Light:
                    _logoLightExt = fileExt;
                    break;
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
                case WhiteLabelLogoTypeEnum.DocsEditorEmbedded:
                    _logoDocsEditorEmbeddedExt = fileExt;
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
            switch (type)
            {
                case WhiteLabelLogoTypeEnum.Light:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/light_general.png") : WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/light.png");
                case WhiteLabelLogoTypeEnum.LightSmall:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/light_small_general.png") : WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/light_small.png");
                case WhiteLabelLogoTypeEnum.Dark:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/dark_general.png") : WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/dark.png");
                case WhiteLabelLogoTypeEnum.DocsEditor:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/editor_logo_general.png") : WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/editor_logo.png");
                case WhiteLabelLogoTypeEnum.Favicon:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/favicon_general.ico") : WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/favicon.ico");
                case WhiteLabelLogoTypeEnum.DocsEditorEmbedded:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/editor_embedded_logo_general.png") : WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/editor_embedded_logo.png");

            }
            return "";
        }

        #endregion

        public static string BuildLogoFileName(WhiteLabelLogoTypeEnum type, String fileExt, bool general)
        {
            if (type == WhiteLabelLogoTypeEnum.Favicon) { fileExt = "ico"; }

            return String.Format("logo_{0}{2}.{1}", type.ToString().ToLowerInvariant(), fileExt, general ? "_general" : "");
        }

        public static Size GetSize(WhiteLabelLogoTypeEnum type, bool general)
        {
            switch (type)
            {
                case WhiteLabelLogoTypeEnum.Light:
                    return new Size(
                        general ? TenantWhiteLabelSettings.logoLightSize.Width / 2 : TenantWhiteLabelSettings.logoLightSize.Width,
                        general ? TenantWhiteLabelSettings.logoLightSize.Height / 2 : TenantWhiteLabelSettings.logoLightSize.Height);
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
                case WhiteLabelLogoTypeEnum.DocsEditorEmbedded:
                    return new Size(
                        general ? TenantWhiteLabelSettings.logoDocsEditorEmbeddedSize.Width / 2 : TenantWhiteLabelSettings.logoDocsEditorEmbeddedSize.Width,
                        general ? TenantWhiteLabelSettings.logoDocsEditorEmbeddedSize.Height / 2 : TenantWhiteLabelSettings.logoDocsEditorEmbeddedSize.Height);
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

        public Guid ID
        {
            get { return new Guid("{05d35540-c80b-4b17-9277-abd9e543bf93}"); }
        }

        #region Save for Resource replacement

        public void Save(bool restore = false)
        {
            SettingsManager.Instance.SaveSettings(this, TenantProvider.CurrentTenantID);
            SetNewLogoText(restore);
        }

        public const string DefaultLogo = "OnlyOffice";

        public void SetNewLogoText(bool restore = false)
        {
            WhiteLabelHelper.DefaultLogo = DefaultLogo;
            if (restore)
            {
                WhiteLabelHelper.RestoreOldText();
            }
            else if(!string.IsNullOrEmpty(LogoText))
            {
                WhiteLabelHelper.SetNewText(LogoText);
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