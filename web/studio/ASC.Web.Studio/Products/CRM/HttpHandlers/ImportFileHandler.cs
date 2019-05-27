/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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