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
using System.Collections;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility;
using Newtonsoft.Json;
using Resources;

namespace ASC.Web.People.Core.Import
{
    public class ContactsUploader : IFileUploadHandler
    {
        public FileUploadResult ProcessUpload(HttpContext context)
        {
            var result = new ContactsUploaderResult();
            try
            {
                SecurityContext.CheckPermissions(Constants.Action_AddRemoveUser);

                if (context.Request.Files.Count == 0)
                {
                    result.Success = false;
                    result.Message = Resource.ErrorEmptyUploadFileSelected;
                    return result;
                }

                var file = context.Request.Files[0];

                const int maxFileSize = 512000;
                if (file.ContentLength > maxFileSize)
                {
                    result.Success = false;
                    result.Message = String.Format(Resource.ImportContactsFromFileErrorTooLarge, (maxFileSize/1024));
                    return result;
                }

                var ext = FileUtility.GetFileExtension(file.FileName);
                if (ext != ".csv")
                {
                    result.Success = false;
                    result.Message = Resource.ErrorEmptyUploadFileSelected;
                    return result;
                }

                var param = new FileParameters
                    {
                        Encode = Convert.ToInt32(context.Request["enc"]),
                        Separator = Convert.ToInt32(context.Request["sep"]),
                        Delimiter = Convert.ToInt32(context.Request["del"]),
                        Position = Convert.ToInt32(context.Request["pos"]),
                        IsRaw = Convert.ToBoolean(context.Request["raw"]),
                        UserHeader = context.Request["head"]
                    };

                IUserImporter importer = new TextFileUserImporter(file.InputStream, param);

                var users = (param.IsRaw)
                                ? (IEnumerable)importer.GetRawUsers()
                                : importer.GetDiscoveredUsers();

                result.Message = JsonConvert.SerializeObject(users);
                result.Columns = JsonConvert.SerializeObject(ContactInfo.GetColumns());
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message.HtmlEncode();
            }

            return result;
        }
    }
}