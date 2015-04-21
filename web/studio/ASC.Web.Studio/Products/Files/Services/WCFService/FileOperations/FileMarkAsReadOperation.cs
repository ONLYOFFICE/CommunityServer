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