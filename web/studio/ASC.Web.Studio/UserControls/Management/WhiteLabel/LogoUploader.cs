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


using ASC.Core;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.PublicResources;
using Resources;
using System;
using System.Drawing;
using System.IO;
using System.Web;

namespace ASC.Web.Studio.UserControls.WhiteLabel
{
    internal class LogoUploader : IFileUploadHandler
    {
        #region IFileUploadHandler Members

        public FileUploadResult ProcessUpload(HttpContext context)
        {
            var result = new FileUploadResult();
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var type = Convert.ToInt32(context.Request["logotype"]);
                var whiteLabelType = (WhiteLabelLogoTypeEnum)type;

                if (context.Request.Files.Count != 0)
                {
                    var imgContentType = @"image";

                    var logo = context.Request.Files[0];
                    if (!logo.ContentType.StartsWith(imgContentType))
                    {
                        throw new Exception(WhiteLabelResource.ErrorFileNotImage);
                    }

                    var data = new byte[logo.InputStream.Length];

                    var br = new BinaryReader(logo.InputStream);
                    br.Read(data, 0, (int)logo.InputStream.Length);
                    br.Close();


                    var size = TenantWhiteLabelSettings.GetSize(whiteLabelType, false);

                    //get size
                    using (var memory = new MemoryStream(data))
                    using (var image = Image.FromStream(memory))
                    {
                        var actualSize = image.Size;
                        if (actualSize.Height != size.Height || actualSize.Width != size.Width)
                        {
                            throw new ImageSizeLimitException();
                        }
                    }

                    var ap = UserPhotoManager.SaveTempPhoto(data, SetupInfo.MaxImageUploadSize, size.Width, size.Height);

                    result.Success = true;
                    result.Message = ap;
                }
                else
                {
                    result.Success = false;
                    result.Message = Resource.ErrorEmptyUploadFileSelected;
                }
            }
            catch(ImageWeightLimitException)
            {
                result.Success = false;
                result.Message = Resource.ErrorImageWeightLimit;
            }
            catch(ImageSizeLimitException)
            {
                result.Success = false;
                result.Message = WhiteLabelResource.ErrorImageSize;
            }
            catch(Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message.HtmlEncode();
            }

            return result;
        }

        #endregion
    }
}