/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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