/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


using System.Collections.Generic;

using ASC.Mail.Core.Entities;
using ASC.Mail.Enums;

namespace ASC.Mail.Core.Dao.Interfaces
{
    public interface IFolderDao
    {
        /// <summary>
        ///     Get folder by id.
        /// </summary>
        Folder GetFolder(FolderType folder);

        /// <summary>
        ///     Get a list of folders.
        /// </summary>
        List<Folder> GetFolders();

        /// <summary>
        ///     Save folder
        /// </summary>
        int Save(Folder folder);

        /// <summary>
        ///     Update folder counters
        /// </summary>
        int ChangeFolderCounters(FolderType folder, int? unreadMessDiff = null, int? totalMessDiff = null,
            int? unreadConvDiff = null, int? totalConvDiff = null);

        int Delete();
    }
}
