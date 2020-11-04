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
using ASC.CRM.Core;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.CRM.Configuration;
using ASC.Web.Studio.Controls.FileUploader;
using System;
using System.Web;


namespace ASC.Web.CRM.Classes
{
    public class ImportFileHandler : IFileUploadHandler
    {
        public FileUploadResult ProcessUpload(HttpContext context)
        {
            if (!WebItemSecurity.IsAvailableForMe(ProductEntryPoint.ID))
                throw CRMSecurity.CreateSecurityException();

            var fileUploadResult = new FileUploadResult();

            if (!FileToUpload.HasFilesToUpload(context)) return fileUploadResult;

            var file = new FileToUpload(context);

            String assignedPath;

            Global.GetStore().SaveTemp("temp", out assignedPath, file.InputStream);

            file.InputStream.Position = 0;

            var jObject = ImportFromCSV.GetInfo(file.InputStream, context.Request["importSettings"]);

            jObject.Add("assignedPath", assignedPath);

            fileUploadResult.Success = true;
            fileUploadResult.Data = Global.EncodeTo64(jObject.ToString());

            return fileUploadResult;
        }
    }
}