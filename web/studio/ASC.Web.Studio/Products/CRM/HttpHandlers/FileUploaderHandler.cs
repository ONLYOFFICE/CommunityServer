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
using System.Web;

using ASC.Files.Core;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Controls.FileUploader;
using ASC.Web.Studio.Controls.FileUploader.HttpModule;
using ASC.Web.Studio.Core;
using ASC.CRM.Core.Dao;
using ASC.Web.CRM.Core;
using Autofac;

namespace ASC.Web.CRM.Classes
{
    public class FileUploaderHandler : FileUploadHandler
    {
        public override FileUploadResult ProcessUpload(HttpContext context)
        {
            var fileUploadResult = new FileUploadResult();

            if (!FileToUpload.HasFilesToUpload(context)) return fileUploadResult;

            var file = new FileToUpload(context);

            if (String.IsNullOrEmpty(file.FileName) || file.ContentLength == 0)
                throw new InvalidOperationException(CRMErrorsResource.InvalidFile);

            if (0 < SetupInfo.MaxUploadSize && SetupInfo.MaxUploadSize < file.ContentLength)
                throw FileSizeComment.FileSizeException;

            var fileName = file.FileName.LastIndexOf('\\') != -1
                ? file.FileName.Substring(file.FileName.LastIndexOf('\\') + 1)
                : file.FileName;

            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                var document = new File
                {
                    Title = fileName,
                    FolderID = daoFactory.FileDao.GetRoot(),
                    ContentLength = file.ContentLength
                };

                document = daoFactory.FileDao.SaveFile(document, file.InputStream);

                fileUploadResult.Data = document.ID;
                fileUploadResult.FileName = document.Title;
                fileUploadResult.FileURL = document.DownloadUrl;
                fileUploadResult.Success = true;


                return fileUploadResult;
            }
        }
    }
}