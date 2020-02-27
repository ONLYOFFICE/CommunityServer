/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


#region Import

using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using ASC.CRM.Core;
using ASC.MessagingSystem;
using ASC.Web.Core.Utility;
using System.IO;
using Newtonsoft.Json.Linq;

#endregion

namespace ASC.Web.CRM.Classes
{
    public class ImportFromCSVManager
    {
        public void StartImport(EntityType entityType, String CSVFileURI, String importSettingsJSON)
        {
            ImportFromCSV.Start(entityType, CSVFileURI, importSettingsJSON);

            var action = GetMessageAction(entityType);
            MessageService.Send(HttpContext.Current.Request, action);
        }

        public FileUploadResult ProcessUploadFake(string fileTemp, string importSettingsJSON)
        {
            var fileUploadResult = new FileUploadResult();

            if (String.IsNullOrEmpty(fileTemp) || String.IsNullOrEmpty(importSettingsJSON)) return fileUploadResult;

            if (!Global.GetStore().IsFile("temp", fileTemp)) return fileUploadResult;

            JObject jObject;

            //Read contents
            using (Stream storeStream = Global.GetStore().GetReadStream("temp", fileTemp))
            {
                using (var CSVFileStream = new MemoryStream())
                {
                    //Copy
                    var buffer = new byte[4096];
                    int readed;
                    while ((readed = storeStream.Read(buffer, 0, 4096)) != 0)
                    {
                        CSVFileStream.Write(buffer, 0, readed);
                    }
                    CSVFileStream.Position = 0;

                    jObject = ImportFromCSV.GetInfo(CSVFileStream, importSettingsJSON);
                }
            }

            jObject.Add("assignedPath", fileTemp);

            fileUploadResult.Success = true;
            fileUploadResult.Data = Global.EncodeTo64(jObject.ToString());

            return fileUploadResult;
        }

        private static MessageAction GetMessageAction(EntityType entityType)
        {
            switch (entityType)
            {
                case EntityType.Contact:
                    return MessageAction.ContactsImportedFromCSV;
                case EntityType.Task:
                    return MessageAction.CrmTasksImportedFromCSV;
                case EntityType.Opportunity:
                    return MessageAction.OpportunitiesImportedFromCSV;
                case EntityType.Case:
                    return MessageAction.CasesImportedFromCSV;
                default:
                    throw new ArgumentException("entityType");
            }
        }
    }
}