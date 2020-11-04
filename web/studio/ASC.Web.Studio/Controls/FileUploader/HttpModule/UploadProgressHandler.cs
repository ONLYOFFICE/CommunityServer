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
using AjaxPro;
using ASC.Common.Web;

namespace ASC.Web.Studio.Controls.FileUploader.HttpModule
{
    public abstract class FileUploadHandler
    {
        public class FileUploadResult
        {
            public bool Success { get; set; }
            public string FileName { get; set; }
            public string FileURL { get; set; }
            public object Data { get; set; }
            public string Message { get; set; }
        }

        public virtual string GetFileName(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            var ind = path.LastIndexOf('\\');
            return ind != -1 ? path.Substring(ind + 1) : path;
        }

        public abstract FileUploadResult ProcessUpload(HttpContext context);
    }

    public class UploadProgressHandler : AbstractHttpAsyncHandler
    {
        public override void OnProcessRequest(HttpContext context)
        {
            if (!string.IsNullOrEmpty(context.Request["submit"]))
            {
                FileUploadHandler.FileUploadResult result;
                try
                {
                    var uploadHandler = (FileUploadHandler)Activator.CreateInstance(Type.GetType(context.Request["submit"], true));
                    result = uploadHandler.ProcessUpload(context);
                }
                catch (Exception ex)
                {
                    result = new FileUploadHandler.FileUploadResult
                        {
                            Success = false,
                            Message = ex.Message.HtmlEncode(),
                        };
                }

                //NOTE: Don't set content type. ie cant parse it
                context.Response.StatusCode = 200;
                context.Response.Write(JavaScriptSerializer.Serialize(result));
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Write("Not found submit parameter.");
            }
        }
    }
}