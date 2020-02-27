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