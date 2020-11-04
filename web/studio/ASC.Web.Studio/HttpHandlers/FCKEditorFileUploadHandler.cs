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


using System.Text.RegularExpressions;
using ASC.Common.Web;
using ASC.Data.Storage;
using ASC.Web.Core.Files;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using System;
using System.Web;
using Resources;

namespace ASC.Web.Studio.HttpHandlers
{
    public class FCKEditorFileUploadHandler : AbstractHttpAsyncHandler
    {
        public override void OnProcessRequest(HttpContext context)
        {
            if (string.IsNullOrEmpty(context.Request["newEditor"]))
            {
                OldProcessRequest(context);
            }
            else
            {
                NewProcessRequest(context);
            }
        }

        private static void OldProcessRequest(HttpContext context)
        {
            try
            {
                var storeDomain = context.Request["esid"];
                var itemID = context.Request["iid"] ?? "";

                var file = context.Request.Files["NewFile"];

                if (file.ContentLength > SetupInfo.MaxImageUploadSize)
                {
                    OldSendFileUploadResponse(context, 1, true, string.Empty, string.Empty, FileSizeComment.FileImageSizeExceptionString);
                    return;
                }

                var filename = file.FileName.Replace("%", string.Empty);
                var ind = file.FileName.LastIndexOf("\\");
                if (ind >= 0)
                {
                    filename = file.FileName.Substring(ind + 1);
                }
                if (FileUtility.GetFileTypeByFileName(filename) != FileType.Image)
                {
                    OldSendFileUploadResponse(context, 1, true, string.Empty, filename, Resource.ErrorUnknownFileImageType);
                    return;
                }

                var folderID = CommonControlsConfigurer.FCKAddTempUploads(storeDomain, filename, itemID);

                var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "fckuploaders");
                var saveUri = store.Save(storeDomain, folderID + "/" + filename, file.InputStream).ToString();

                OldSendFileUploadResponse(context, 0, true, saveUri, filename, string.Empty);
            }
            catch (Exception e)
            {
                OldSendFileUploadResponse(context, 1, true, string.Empty, string.Empty, e.Message.HtmlEncode());
            }
        }

        private static void NewProcessRequest(HttpContext context)
        {
            try
            {
                var funcNum = context.Request["CKEditorFuncNum"];
                var storeDomain = context.Request["esid"];
                var itemID = context.Request["iid"] ?? "";
                var file = context.Request.Files["upload"];

                if (file.ContentLength > SetupInfo.MaxImageUploadSize)
                {
                    NewSendFileUploadResponse(context, string.Empty, funcNum, FileSizeComment.FileImageSizeExceptionString);
                    return;
                }

                var filename = file.FileName;

                var ind = filename.LastIndexOf("\\", StringComparison.Ordinal);
                if (ind >= 0)
                {
                    filename = filename.Substring(ind + 1);
                }

                filename = new Regex("[\t*\\+:\"<>#%&?|\\\\/]").Replace(filename, "_");

                if (FileUtility.GetFileTypeByFileName(filename) != FileType.Image)
                {
                    NewSendFileUploadResponse(context, string.Empty, funcNum, Resource.ErrorUnknownFileImageType);
                    return;
                }

                var folderID = CommonControlsConfigurer.FCKAddTempUploads(storeDomain, filename, itemID);

                var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "fckuploaders");
                var saveUri = store.Save(storeDomain, folderID + "/" + filename, file.InputStream).ToString();

                NewSendFileUploadResponse(context, saveUri, funcNum, string.Empty);
            }
            catch (Exception e)
            {
                NewSendFileUploadResponse(context, string.Empty, string.Empty, e.Message.HtmlEncode());
            }
        }

        private static void OldSendFileUploadResponse(HttpContext context, int errorNumber, bool isQuickUpload, string fileUrl, string fileName, string customMsg)
        {
            context.Response.Clear();

            context.Response.Write("<script type=\"text/javascript\">");
            // Minified version of the document.domain automatic fix script.
            // The original script can be found at _dev/domain_fix_template.js
            context.Response.Write(@"(function(){var d=document.domain; while (true){try{var A=window.top.opener.document.domain;break;}catch(e) {};d=d.replace(/.*?(?:\.|$)/g,'');if (d.length==0) break;try{document.domain=d;}catch (e){break;}}})();");

            if (!string.IsNullOrEmpty(fileUrl))
            {
                fileUrl = HttpUtility.UrlPathEncode(fileUrl).Replace("'", "\\'");
            }
            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = HttpUtility.UrlEncode(fileName);
            }

            if (isQuickUpload)
                context.Response.Write("window.parent.OnUploadCompleted(" + errorNumber + ",'" + fileUrl.Replace("'", "\\'") + "','" + fileName.Replace("'", "\\'") + "','" + customMsg.Replace("'", "\\'") + "') ;");
            else
                context.Response.Write("window.parent.frames['frmUpload'].OnUploadCompleted(" + errorNumber + ",'" + fileName.Replace("'", "\\'") + "') ;");

            context.Response.Write("</script>");
        }

        private static void NewSendFileUploadResponse(HttpContext context, string fileUrl, string funcNum, string errorMsg)
        {
            context.Response.Clear();

            context.Response.Write("<script type=\"text/javascript\">");

            if (string.IsNullOrEmpty(errorMsg))
            {
                if (!string.IsNullOrEmpty(fileUrl))
                {
                    fileUrl = HttpUtility.UrlPathEncode(fileUrl).Replace("'", "\\'");
                    context.Response.Write("window.parent.CKEDITOR.tools.callFunction(" + funcNum + ",'" + fileUrl + "', '');");
                }
                else
                {
                    context.Response.Write("window.parent.CKEDITOR.tools.callFunction(" + funcNum + ",'', 'empty url');");
                }
            }
            else
            {
                context.Response.Write("window.parent.CKEDITOR.tools.callFunction(" + funcNum + ",'', '" + errorMsg.Replace("'", "\\'") + "');");
            }

            context.Response.Write("</script>");
        }
    }
}