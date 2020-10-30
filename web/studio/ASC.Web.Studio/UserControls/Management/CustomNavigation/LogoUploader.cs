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


using System.Globalization;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.Drawing;
using System.IO;
using System.Web;
using ASC.Common.Logging;

namespace ASC.Web.Studio.UserControls.CustomNavigation
{
    internal class LogoUploader : IFileUploadHandler
    {
        public FileUploadResult ProcessUpload(HttpContext context)
        {
            var result = new FileUploadResult();
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var width = Convert.ToInt32(context.Request["size"]);
                var size = new Size(width, width);

                if (context.Request.Files.Count != 0)
                {
                    const string imgContentType = @"image";

                    var logo = context.Request.Files[0];
                    if (!logo.ContentType.StartsWith(imgContentType))
                    {
                        throw new Exception(WhiteLabelResource.ErrorFileNotImage);
                    }

                    var data = new byte[logo.InputStream.Length];

                    var reader = new BinaryReader(logo.InputStream);
                    reader.Read(data, 0, (int) logo.InputStream.Length);
                    reader.Close();

                    using (var stream = new MemoryStream(data))
                    using (var image = Image.FromStream(stream))
                    {
                        var actualSize = image.Size;
                        if (actualSize.Height != size.Height || actualSize.Width != size.Width)
                        {
                            throw new ImageSizeLimitException();
                        }
                    }

                    result.Success = true;
                    result.Message = UserPhotoManager.SaveTempPhoto(data, SetupInfo.MaxImageUploadSize, size.Width,
                        size.Height);
                }
                else
                {
                    result.Success = false;
                    result.Message = Resource.ErrorEmptyUploadFileSelected;
                }
            }
            catch (ImageWeightLimitException)
            {
                result.Success = false;
                result.Message = Resource.ErrorImageWeightLimit;
            }
            catch (ImageSizeLimitException)
            {
                result.Success = false;
                result.Message = WhiteLabelResource.ErrorImageSize;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message.HtmlEncode();
            }

            return result;
        }
    }

    public class StorageHelper
    {
        private const string StorageName = "customnavigation";

        private const string Base64Start = "data:image/png;base64,";

        public static string SaveTmpLogo(string tmpLogoPath)
        {
            if (String.IsNullOrEmpty(tmpLogoPath)) return null;

            try
            {
                byte[] data;

                if (tmpLogoPath.StartsWith(Base64Start))
                {
                    data = Convert.FromBase64String(tmpLogoPath.Substring(Base64Start.Length));

                    return SaveLogo(Guid.NewGuid() + ".png", data);
                }

                var fileName = Path.GetFileName(tmpLogoPath);

                data = UserPhotoManager.GetTempPhotoData(fileName);

                UserPhotoManager.RemoveTempPhoto(fileName);

                return SaveLogo(fileName, data);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC").Error(ex);
                return null;
            }
        }

        public static void DeleteLogo(string logoPath)
        {
            if (String.IsNullOrEmpty(logoPath)) return;

            try
            {
                var store = StorageFactory.GetStorage(
                    TenantProvider.CurrentTenantID.ToString(CultureInfo.InvariantCulture), StorageName);

                var fileName = Path.GetFileName(logoPath);

                if (store.IsFile(fileName))
                {
                    store.Delete(fileName);
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC").Error(e);
            }
        }

        private static string SaveLogo(string fileName, byte[] data)
        {
            var store = StorageFactory.GetStorage(
                TenantProvider.CurrentTenantID.ToString(CultureInfo.InvariantCulture), StorageName);

            using (var stream = new MemoryStream(data))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return store.Save(fileName, stream).ToString();
            }
        }
    }
}