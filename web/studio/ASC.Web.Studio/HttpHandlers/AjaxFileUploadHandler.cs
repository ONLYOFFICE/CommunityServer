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


using ASC.Common.Web;
using ASC.Core;
using ASC.Web.Core.Utility;
using System;
using System.Web;

namespace ASC.Web.Studio.HttpHandlers
{
    public class AjaxFileUploadHandler : AbstractHttpAsyncHandler
    {
        public override void OnProcessRequest(HttpContext context)
        {
            var result = new FileUploadResult()
            {
                Success = false,
                Message = "type not found"
            };

            if(!SecurityContext.IsAuthenticated)
                Global.Authenticate();

            if (!String.IsNullOrEmpty(context.Request["type"]))
            {
                try
                {
                    var uploadHandler = (IFileUploadHandler)Activator.CreateInstance(Type.GetType(context.Request["type"], true));
                    result = uploadHandler.ProcessUpload(context);
                }
                catch { }
            }

            context.Response.StatusCode = 200;
            context.Response.Write(AjaxPro.JavaScriptSerializer.Serialize(result));
        }
    }
}
