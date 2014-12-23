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
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Core;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Specific;
using ASC.Web.Files.Classes;

namespace ASC.Api.Documents
{
    /// <summary>
    /// </summary>
    [DataContract(Name = "folder", Namespace = "")]
    public class FolderWrapper : FileEntryWrapper
    {
        /// <summary>
        /// </summary>
        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public object ParentId { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public int FilesCount { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public int FoldersCount { get; set; }

        /// <summary>
        /// </summary>
        [DataMember]
        public bool IsShareable { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="folder"></param>
        public FolderWrapper(Folder folder)
            : base(folder)
        {
            ParentId = folder.ParentFolderID;
            if (folder.RootFolderType == FolderType.USER
                && !Equals(folder.RootFolderCreator, SecurityContext.CurrentAccount.ID))
            {
                RootFolderType = FolderType.SHARE;

                using (var folderDao = Global.DaoFactory.GetFolderDao())
                {
                    var parentFolder = folderDao.GetFolder(folder.ParentFolderID);
                    if (!Global.GetFilesSecurity().CanRead(parentFolder))
                        ParentId = Global.FolderShare;
                }
            }

            FilesCount = folder.TotalFiles;
            FoldersCount = folder.TotalSubFolders;
            IsShareable = folder.Shareable;
        }

        private FolderWrapper()
            : base()
        {

        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static FolderWrapper GetSample()
        {
            return new FolderWrapper
                {
                    Access = FileShare.ReadWrite,
                    Updated = ApiDateTime.GetSample(),
                    Created = ApiDateTime.GetSample(),
                    CreatedBy = EmployeeWraper.GetSample(),
                    Id = new Random().Next(),
                    RootFolderType = FolderType.BUNCH,
                    SharedByMe = false,
                    Title = "Some titile",
                    UpdatedBy = EmployeeWraper.GetSample(),
                    FilesCount = new Random().Next(),
                    FoldersCount = new Random().Next(),
                    ParentId = new Random().Next(),
                    IsShareable = false
                };
        }
    }
}