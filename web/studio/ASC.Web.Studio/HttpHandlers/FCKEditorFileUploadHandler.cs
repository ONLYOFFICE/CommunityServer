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

                var filename = file.FileName.Replace("%", string.Empty);
                var ind = file.FileName.LastIndexOf("\\");
                if (ind >= 0)
                {
                    filename = file.FileName.Substring(ind + 1);
                }
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