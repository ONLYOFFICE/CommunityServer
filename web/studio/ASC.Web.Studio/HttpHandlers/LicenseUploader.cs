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
using System.Security;
using System.Web;
using ASC.Common.Logging;
using ASC.Core.Billing;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using Resources;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Studio.HttpHandlers
{
    internal class LicenseUploader : IFileUploadHandler
    {
        public FileUploadResult ProcessUpload(HttpContext context)
        {
            var result = new FileUploadResult();
            try
            {
                if (!SecurityContext.IsAuthenticated && WizardSettings.Load().Completed) throw new SecurityException(Resource.PortalSecurity);
                if (context.Request.Files.Count == 0) throw new Exception(Resource.ErrorEmptyUploadFileSelected);

                var licenseFile = context.Request.Files[0];
                var dueDate = LicenseReader.SaveLicenseTemp(licenseFile.InputStream);

                result.Message = dueDate >= DateTime.UtcNow.Date
                                     ? Resource.LicenseUploaded
                                     : string.Format(
                                          (TenantExtra.GetTenantQuota().Update
                                              ? Resource.LicenseUploadedOverdueSupport
                                              : Resource.LicenseUploadedOverdue),
                                                     "<span class='tariff-marked'>",
                                                     "</span>",
                                                     dueDate.Date.ToLongDateString());
                result.Success = true;
            }
            catch (LicenseExpiredException ex)
            {
                LogManager.GetLogger("ASC").Error("License upload", ex);
                result.Message = Resource.LicenseErrorExpired;
            }
            catch (LicenseQuotaException ex)
            {
                LogManager.GetLogger("ASC").Error("License upload", ex);
                result.Message = Resource.LicenseErrorQuota;
            }
            catch (LicensePortalException ex)
            {
                LogManager.GetLogger("ASC").Error("License upload", ex);
                result.Message = Resource.LicenseErrorPortal;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC").Error("License upload", ex);
                result.Message = Resource.LicenseError;
            }

            return result;
        }
    }
}