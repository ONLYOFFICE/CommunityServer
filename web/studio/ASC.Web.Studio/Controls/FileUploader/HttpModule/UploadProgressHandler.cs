/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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