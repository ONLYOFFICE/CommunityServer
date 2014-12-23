/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
                context.Response.ContentType = "application/json";
                var id = context.Request.QueryString[UploadProgressStatistic.UploadIdField];
                var us = UploadProgressStatistic.GetStatistic(id);

                if (!string.IsNullOrEmpty(context.Request["limit"]))
                {
                    var limit = long.Parse(context.Request["limit"]);
                    if (us.TotalBytes > limit)
                    {
                        us.ReturnCode = 1;
                        us.IsFinished = true;
                    }
                }

                context.Response.Write(us.ToJson());
            }
        }
    }
}