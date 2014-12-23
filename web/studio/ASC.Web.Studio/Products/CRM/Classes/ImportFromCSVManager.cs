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

            var jObject = new JObject();

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