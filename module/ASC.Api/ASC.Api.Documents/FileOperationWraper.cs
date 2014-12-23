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

        //[DataMember(Name = "source", IsRequired = false)]
        //public string Source { get; set; }

        //[DataMember(Name = "result", IsRequired = false)]
        //public object Result { get; set; }

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
            //Source = o.Source;
            //Result = o.Result;
            Error = o.Error;
            Processed = o.Processed;

            if (o.FileIds != null)
            {
                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    Files = fileDao.GetFiles(o.FileIds).Select(r => new FileWrapper(r)).ToList();
                }
            }
            if (o.FolderIds != null)
            {
                using (var folderDao = Global.DaoFactory.GetFolderDao())
                {
                    Folders = folderDao.GetFolders(o.FolderIds).Select(r => new FolderWrapper(r)).ToList();
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
                    Files = new List<FileWrapper> {FileWrapper.GetSample()},
                    Folders = new List<FolderWrapper> {FolderWrapper.GetSample()}
                };
        }
    }
}