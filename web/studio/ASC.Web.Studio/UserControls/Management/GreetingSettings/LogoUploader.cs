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
using ASC.Web.Studio.Core;
using Resources;
using System;
using System.IO;
using System.Web;

namespace ASC.Web.Studio.UserControls.Management
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

                if (context.Request.Files.Count != 0)
                {
                    var logo = context.Request.Files[0];
                    var data = new byte[logo.InputStream.Length];

                    var br = new BinaryReader(logo.InputStream);
                    br.Read(data, 0, (int) logo.InputStream.Length);
                    br.Close();

                    var ap = UserPhotoManager.SaveTempPhoto(data, SetupInfo.MaxImageUploadSize, 250, 100);

                    result.Success = true;
                    result.Message = ap;
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
                result.Message = Resource.ErrorImageSizetLimit;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message.HtmlEncode();
            }

            return result;
        }

        #endregion
    }
}