/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.Common.Web;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Core.Files;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.HttpHandlers
{
    public class FCKEditorFileUploadHandler : AbstractHttpAsyncHandler
    {
        public override void OnProcessRequest(HttpContext context)
        {
            try
            {
                if (!SecurityContext.IsAuthenticated)
                {
                    throw new HttpException(403, "Access denied.");
                }

                var storeDomain = context.Request["esid"];
                var itemID = context.Request["iid"] ?? "";
                var file = context.Request.Files["upload"];

                if (file.ContentLength > SetupInfo.MaxImageUploadSize)
                {
                    SendFileUploadResponse(context, string.Empty, FileSizeComment.FileImageSizeExceptionString);
                    return;
                }

                var filename = string.IsNullOrEmpty(file.FileName) ? string.Empty : Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);

                if (FileUtility.GetFileTypeByFileName(filename) != FileType.Image)
                {
                    SendFileUploadResponse(context, string.Empty, Resource.ErrorUnknownFileImageType);
                    return;
                }

                var folderID = CommonControlsConfigurer.FCKAddTempUploads(storeDomain, filename, itemID);

                var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "fckuploaders");
                var saveUri = store.Save(storeDomain, folderID + "/" + filename, file.InputStream).ToString();

                SendFileUploadResponse(context, saveUri, string.Empty, filename);
            }
            catch (Exception e)
            {
                SendFileUploadResponse(context, string.Empty, e.Message.HtmlEncode());
            }
        }

        private static void SendFileUploadResponse(HttpContext context, string fileUrl, string errorMsg, string filename = null)
        {
            context.Response.Clear();

            if (string.IsNullOrEmpty(errorMsg))
            {
                if (!string.IsNullOrEmpty(fileUrl))
                {
                    context.Response.Write(
                        Newtonsoft.Json.JsonConvert.SerializeObject(new
                        {
                            fileName = filename,
                            url = HttpUtility.UrlPathEncode(fileUrl),
                            uploaded = 1,
                        })
                    );
                }
                else
                {
                    context.Response.Write(
                        Newtonsoft.Json.JsonConvert.SerializeObject(new
                        {
                            fileName = filename,
                            url = fileUrl,
                            uploaded = 0,
                            error = new
                            {
                                message = "Empty URL"
                            }
                        })
                    );
                }
            }
            else
            {
                context.Response.Write(
                        Newtonsoft.Json.JsonConvert.SerializeObject(new
                        {
                            fileName = filename,
                            url = fileUrl,
                            uploaded = 0,
                            error = new
                            {
                                message = errorMsg
                            }
                        })
                    );
            }
        }
    }
}