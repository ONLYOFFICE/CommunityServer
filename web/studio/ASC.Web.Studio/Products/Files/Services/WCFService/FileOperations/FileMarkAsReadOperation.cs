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

using System.Collections.Generic;
using System.Linq;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Web.Files.Utils;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    internal class FileMarkAsReadOperation : FileOperation
    {
        public FileMarkAsReadOperation(Tenant tenant, List<object> folders, List<object> files)
            : base(tenant, folders, files)
        {
            CountWithoutSubitems = true;
        }

        protected override FileOperationType OperationType
        {
            get { return FileOperationType.MarkAsRead; }
        }

        protected override void Do()
        {
            Percentage = 0;

            var entries = Enumerable.Empty<FileEntry>();

            if (Folders.Any())
                entries = entries.Concat(FolderDao.GetFolders(Folders.ToArray()));

            if (Files.Any())
                entries = entries.Concat(FileDao.GetFiles(Files.ToArray()));

            entries.ToList().ForEach(x =>
                {
                    if (Canceled) return;

                    FileMarker.RemoveMarkAsNew(x, Owner);

                    if (x is File)
                    {
                        ProcessedFile(x.ID.ToString());
                        ResultedFile(x.ID.ToString());
                    }
                    else
                    {
                        ProcessedFolder(x.ID.ToString());
                        ResultedFolder(x.ID.ToString());
                    }

                    ProgressStep();
                });

            var rootFolderIdAsNew =
                FileMarker.GetRootFoldersIdMarkedAsNew()
                          .Select(item => string.Format("new_{{\"key\"? \"{0}\", \"value\"? \"{1}\"}}", item.Key, item.Value));

            Status += string.Join(SplitCharacter, rootFolderIdAsNew.ToArray());

            IsCompleted = true;
        }
    }
}