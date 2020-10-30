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


using System.Collections.Generic;
using ASC.Mail.Core.Dao.Expressions.UserFolder;
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao.Interfaces
{
    public interface IUserFolderDao
    {
        UserFolder Get(uint id);

        UserFolder GetByMail(uint mailId);

        List<UserFolder> GetList(IUserFoldersExp exp);

        uint Save(UserFolder folder);

        int Remove(uint id);

        int Remove(IUserFoldersExp exp);

        void RecalculateFoldersCount(uint id);

        int SetFolderCounters(uint folderId, int? unreadMess = null, int? totalMess = null,
            int? unreadConv = null, int? totalConv = null);

        /// <summary>
        ///     Update folder counters
        /// </summary>
        int ChangeFolderCounters(uint folderId, int? unreadMessDiff = null, int? totalMessDiff = null,
            int? unreadConvDiff = null, int? totalConvDiff = null);
    }
}
