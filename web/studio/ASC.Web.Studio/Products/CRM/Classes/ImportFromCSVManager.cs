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