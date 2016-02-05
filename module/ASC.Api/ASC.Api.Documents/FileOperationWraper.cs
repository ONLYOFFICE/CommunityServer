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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Services.WCFService.FileOperations;

namespace ASC.Api.Documents
{
    /// <summary>
    /// </summary>
    [DataContract(Name = "operation_result", Namespace = "")]
    public class FileOperationWraper
    {
        /// <summary>
        /// </summary>
        [DataMember(Name = "id", IsRequired = false)]
        public string Id { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "operation", IsRequired = false)]
        public FileOperationType OperationType { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "progress", IsRequired = false)]
        public int Progress { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "error", IsRequired = false)]
        public string Error { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "processed", IsRequired = false)]
        public string Processed { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "files", IsRequired = true, EmitDefaultValue = true)]
        public List<FileWrapper> Files { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "folders", IsRequired = true, EmitDefaultValue = true)]
        public List<FolderWrapper> Folders { get; set; }

        /// <summary>
        /// </summary>
        public FileOperationWraper()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="o"></param>
        public FileOperationWraper(FileOperationResult o)
        {
            Id = o.Id;
            OperationType = o.OperationType;
            Progress = o.Progress;
            Error = o.Error;
            Processed = o.Processed;

            if (!string.IsNullOrEmpty(o.Result))
            {
                var arr = o.Result.Split(':');
                var folders = arr.Where(s => s.StartsWith("folder_")).Select(s => s.Substring(7));
                if (folders.Any())
                {
                    using (var folderDao = Global.DaoFactory.GetFolderDao())
                    {
                        Folders = folderDao.GetFolders(folders.ToArray()).Select(r => new FolderWrapper(r)).ToList();
                    }
                }
                var files = arr.Where(s => s.StartsWith("file_")).Select(s => s.Substring(5));
                if (files.Any())
                {
                    using (var fileDao = Global.DaoFactory.GetFileDao())
                    {
                        Files = fileDao.GetFiles(files.ToArray()).Select(r => new FileWrapper(r)).ToList();
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static FileOperationWraper GetSample()
        {
            return new FileOperationWraper
                {
                    Id = Guid.NewGuid().ToString(),
                    OperationType = FileOperationType.Move,
                    Progress = 100,
                    //Source = "folder_1,file_1",
                    //Result = "folder_1,file_1",
                    Error = "",
                    Processed = "1",
                    Files = new List<FileWrapper> { FileWrapper.GetSample() },
                    Folders = new List<FolderWrapper> { FolderWrapper.GetSample() }
                };
        }
    }
}