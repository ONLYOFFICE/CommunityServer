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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ASC.Common.Logging;
using ASC.CRM.Core.Dao;
using ASC.Data.Storage;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM.Configuration;
using ASC.Web.CRM.Core;
using Autofac;

namespace ASC.Web.CRM.Classes
{
    public static class OrganisationLogoManager
    {
        #region Members

        public static readonly String OrganisationLogoBaseDirName = "organisationlogo";
        public static readonly String OrganisationLogoImgName = "logo";

        public static readonly String OrganisationLogoSrcFormat = "data:image/jpeg;base64,{0}";

        public static readonly Size OrganisationLogoSize = new Size(200, 150);

        private static readonly Object _synchronizedObj = new Object();

        #endregion

        #region Private Methods

        private static String BuildFileDirectory()
        {
            return String.Concat(OrganisationLogoBaseDirName, "/");
        }

        private static String BuildFilePath(String imageExtension)
        {
            return String.Concat(BuildFileDirectory(), OrganisationLogoImgName, imageExtension);
        }


        private static String ExecResizeImage(byte[] imageData, Size fotoSize, IDataStore dataStore, String photoPath)
        {
            var data = imageData;
            using (var stream = new MemoryStream(data))
            using (var img = new Bitmap(stream))
            {
                var imgFormat = img.RawFormat;
                if (fotoSize != img.Size)
                {
                    using (var img2 = CommonPhotoManager.DoThumbnail(img, fotoSize, false, false, false))
                    {
                        data = CommonPhotoManager.SaveToBytes(img2, Global.GetImgFormatName(imgFormat));
                    }
                }
                else
                {
                    data = Global.SaveToBytes(img);
                }

                using (var fileStream = new MemoryStream(data))
                {
                    var photoUri = dataStore.Save(photoPath, fileStream).ToString();
                    photoUri = String.Format("{0}?cd={1}", photoUri, DateTime.UtcNow.Ticks);
                    return photoUri;
                }
            }
        }

      

        #endregion

        public static String GetDefaultLogoUrl()
        {
            return WebImageSupplier.GetAbsoluteWebPath("org_logo_default.png", ProductEntryPoint.ID);
        }

        public static String GetOrganisationLogoBase64(int logoID)
        {
            if (logoID <= 0) { return ""; }
            using (var scope = DIHelper.Resolve())
            {
                return scope.Resolve<DaoFactory>().InvoiceDao.GetOrganisationLogoBase64(logoID);
            }
        }

        public static String GetOrganisationLogoSrc(int logoID)
        {
            var bytestring = GetOrganisationLogoBase64(logoID);
            return String.IsNullOrEmpty(bytestring) ? "" : String.Format(OrganisationLogoSrcFormat, bytestring);
        }

        public static void DeletePhoto(bool recursive)
        {
            var photoDirectory = BuildFileDirectory();
            var store = Global.GetStore();

            lock (_synchronizedObj)
            {
                if (store.IsDirectory(photoDirectory))
                {
                    store.DeleteFiles(photoDirectory, "*", recursive);
                    if (recursive)
                    {
                        store.DeleteDirectory(photoDirectory);
                    }
                }
            }
        }

        public static int TryUploadOrganisationLogoFromTmp(DaoFactory factory)
        {
            var directoryPath = BuildFileDirectory();
            var dataStore = Global.GetStore();

            if (!dataStore.IsDirectory(directoryPath))
                return 0;

            try
            {
                var photoPaths = Global.GetStore().ListFilesRelative("", directoryPath, OrganisationLogoImgName + "*", false);
                if (photoPaths.Length == 0)
                    return 0;

                byte[] bytes;
                using (var photoTmpStream = dataStore.GetReadStream(Path.Combine(directoryPath, photoPaths[0])))
                {
                    bytes = Global.ToByteArray(photoTmpStream);
                }

                var logoID = factory.InvoiceDao.SaveOrganisationLogo(bytes);
                dataStore.DeleteFiles(directoryPath, "*", false);
                return logoID;
            }

            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.CRM").ErrorFormat("TryUploadOrganisationLogoFromTmp failed with error: {0}", ex);
                return 0;
            }
        }

        public static String UploadLogo(byte[] imageData, ImageFormat imageFormat)
        {
            var photoPath = BuildFilePath("." + Global.GetImgFormatName(imageFormat));
            return ExecResizeImage(imageData, OrganisationLogoSize, Global.GetStore(), photoPath);
        }
    }
}