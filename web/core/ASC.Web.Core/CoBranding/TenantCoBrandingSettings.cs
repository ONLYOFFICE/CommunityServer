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
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;
using System.Collections.Generic;
using System.Reflection;
using ASC.Web.Core.Users;
using ASC.Web.Core;

namespace ASC.Web.Core.CoBranding
{

    [Serializable]
    [DataContract]
    public class TenantCoBrandingSettings : ISettings
    {
        //public IEnumerable<TenantCoBrandingLogoSettings> LogoSettings;

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


        private const String moduleName = "cobranding";

        public static readonly Size logoLightSize = new Size(432, 70);
        public static readonly Size logoLightSmallSize = new Size(284, 46);

        public static readonly Size logoDarkSize = new Size(432, 70);

        public static readonly Size logoFaviconSize = new Size(32, 32);
        public static readonly Size logoDocsEditorSize = new Size(172, 40);

        #region ISettings Members

        public ISettings GetDefault()
        {
            return new TenantCoBrandingSettings() {
                        _isDefaultLogoLight = true,
                        _isDefaultLogoLightSmall = true,
                        _isDefaultLogoDark = true,
                        _isDefaultLogoFavicon = true,
                        _isDefaultLogoDocsEditor = true,
                    };
        }

        public void RestoreDefault()
        {
            _isDefaultLogoLight = true;
            _isDefaultLogoLightSmall = true;

            _isDefaultLogoDark = true;

            _isDefaultLogoFavicon = true;
            _isDefaultLogoDocsEditor = true;

            var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), moduleName);
            try
            {
                store.DeleteFiles("", "*", false);
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger("ASC").Error(e);
            }
        }

        public void RestoreDefault(CoBrandingLogoTypeEnum type)
        {
            if (!GetIsDefault(type))
            {
                try
                {
                    SetIsDefault(type, true);
                    var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), moduleName);
                    store.Delete(BuildLogoFileName(type, GetExt(type), false));
                    store.Delete(BuildLogoFileName(type, GetExt(type), true));
                }
                catch (Exception e)
                {
                    log4net.LogManager.GetLogger("ASC").Error(e);
                }
            }
        }

        public void SetLogo(CoBrandingLogoTypeEnum type, string logoFileExt, byte[] data)
        {
            var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), moduleName);
            var isAlreadyHaveBeenChanged = !GetIsDefault(type);

            if (isAlreadyHaveBeenChanged)
            {
                try
                {
                    store.Delete(BuildLogoFileName(type, GetExt(type), false));
                    store.Delete(BuildLogoFileName(type, GetExt(type), true));
                }
                catch (Exception e)
                {
                    log4net.LogManager.GetLogger("ASC").Error(e);
                }
            }
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

        private bool GetIsDefault(CoBrandingLogoTypeEnum type){
            switch (type)
            {
                case CoBrandingLogoTypeEnum.Light:
                    return _isDefaultLogoLight;
                case CoBrandingLogoTypeEnum.LightSmall:
                    return _isDefaultLogoLightSmall;
                case CoBrandingLogoTypeEnum.Dark:
                    return _isDefaultLogoDark;
                case CoBrandingLogoTypeEnum.Favicon:
                    return _isDefaultLogoFavicon;
                case CoBrandingLogoTypeEnum.DocsEditor:
                    return _isDefaultLogoDocsEditor;
            }
            return true;
        }

        private void SetIsDefault(CoBrandingLogoTypeEnum type, bool value)
        {
            switch (type)
            {
                case CoBrandingLogoTypeEnum.Light:
                    _isDefaultLogoLight = value;
                    break;
                case CoBrandingLogoTypeEnum.LightSmall:
                    _isDefaultLogoLightSmall = value;
                    break;
                case CoBrandingLogoTypeEnum.Dark:
                    _isDefaultLogoDark = value;
                    break;
                case CoBrandingLogoTypeEnum.Favicon:
                    _isDefaultLogoFavicon = value;
                    break;
                case CoBrandingLogoTypeEnum.DocsEditor:
                    _isDefaultLogoDocsEditor = value;
                    break;
            }
        }

        private string GetExt(CoBrandingLogoTypeEnum type)
        {
            switch (type)
            {
                case CoBrandingLogoTypeEnum.Light:
                    return _logoLightExt;
                case CoBrandingLogoTypeEnum.LightSmall:
                    return _logoLightSmallExt;
                case CoBrandingLogoTypeEnum.Dark:
                    return _logoDarkExt;
                case CoBrandingLogoTypeEnum.Favicon:
                    return _logoFaviconExt;
                case CoBrandingLogoTypeEnum.DocsEditor:
                    return _logoDocsEditorExt;
            }
            return "";
        }

        private void SetExt(CoBrandingLogoTypeEnum type, String fileExt)
        {
            switch (type)
            {
                case CoBrandingLogoTypeEnum.Light:
                    _logoLightExt = fileExt;
                    break;
                case CoBrandingLogoTypeEnum.LightSmall:
                    _logoLightSmallExt = fileExt;
                    break;
                case CoBrandingLogoTypeEnum.Dark:
                    _logoDarkExt = fileExt;
                    break;
                case CoBrandingLogoTypeEnum.Favicon:
                    _logoFaviconExt = fileExt;
                    break;
                case CoBrandingLogoTypeEnum.DocsEditor:
                    _logoDocsEditorExt = fileExt;
                    break;
            }
        }


        public string GetAbsoluteLogoPath(CoBrandingLogoTypeEnum type, bool general = true)
        {
            if (GetIsDefault(type))
            {
                return GetAbsoluteDefaultLogoPath(type, general);
            }

            return GetAbsoluteStorageLogoPath(type, general);
        }

        private string GetAbsoluteStorageLogoPath(CoBrandingLogoTypeEnum type, bool general)
        {
            var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), moduleName);
            var fileName = BuildLogoFileName(type, GetExt(type), general);

            if (store.IsFile(fileName))
            {
                return store.GetUri(fileName).ToString();
            }
            return GetAbsoluteDefaultLogoPath(type, general);
        }

        public static string GetAbsoluteDefaultLogoPath(CoBrandingLogoTypeEnum type, bool general)
        {
            switch (type)
            {
                case CoBrandingLogoTypeEnum.Light:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/light_general.png") : WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/light.png");
                case CoBrandingLogoTypeEnum.LightSmall:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/light_small_general.png") : WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/light_small.png");
                case CoBrandingLogoTypeEnum.Dark:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/dark_general.png") : WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/dark.png");
                case CoBrandingLogoTypeEnum.DocsEditor:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/editor_logo_general.png") : WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/editor_logo.png");
                case CoBrandingLogoTypeEnum.Favicon:
                    return general ? WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/favicon_general.ico") : WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/favicon.ico");
            }
            return "";
        }

        public static string BuildLogoFileName(CoBrandingLogoTypeEnum type, String fileExt, bool general)
        {
            if (type == CoBrandingLogoTypeEnum.Favicon) { fileExt = "ico"; }

            return String.Format("logo_{0}{2}.{1}", type.ToString().ToLower(), fileExt, general ? "_general" : "");
        }

        public static Size GetSize(CoBrandingLogoTypeEnum type, bool general)
        {
            switch (type)
            {
                case CoBrandingLogoTypeEnum.Light:
                    return new Size(
                        general ? TenantCoBrandingSettings.logoLightSize.Width / 2 : TenantCoBrandingSettings.logoLightSize.Width,
                        general ? TenantCoBrandingSettings.logoLightSize.Height / 2 : TenantCoBrandingSettings.logoLightSize.Height);
                case CoBrandingLogoTypeEnum.LightSmall:
                    return new Size(
                        general ? TenantCoBrandingSettings.logoLightSmallSize.Width / 2 : TenantCoBrandingSettings.logoLightSmallSize.Width,
                        general ? TenantCoBrandingSettings.logoLightSmallSize.Height / 2 : TenantCoBrandingSettings.logoLightSmallSize.Height);
                case CoBrandingLogoTypeEnum.Dark:
                    return new Size(
                        general ? TenantCoBrandingSettings.logoDarkSize.Width / 2 : TenantCoBrandingSettings.logoDarkSize.Width,
                        general ? TenantCoBrandingSettings.logoDarkSize.Height / 2 : TenantCoBrandingSettings.logoDarkSize.Height);
                case CoBrandingLogoTypeEnum.Favicon:
                    return new Size(
                        general ? TenantCoBrandingSettings.logoFaviconSize.Width / 2 : TenantCoBrandingSettings.logoFaviconSize.Width,
                        general ? TenantCoBrandingSettings.logoFaviconSize.Height / 2 : TenantCoBrandingSettings.logoFaviconSize.Height);
                case CoBrandingLogoTypeEnum.DocsEditor:
                    return new Size(
                        general ? TenantCoBrandingSettings.logoDocsEditorSize.Width / 2 : TenantCoBrandingSettings.logoDocsEditorSize.Width,
                        general ? TenantCoBrandingSettings.logoDocsEditorSize.Height / 2 : TenantCoBrandingSettings.logoDocsEditorSize.Height);
            }
            return new Size(0, 0);
        }

        private static void ResizeLogo(CoBrandingLogoTypeEnum type, String fileName, byte[] data, long maxFileSize, Size size, IDataStore store)
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

        #endregion
    }
}