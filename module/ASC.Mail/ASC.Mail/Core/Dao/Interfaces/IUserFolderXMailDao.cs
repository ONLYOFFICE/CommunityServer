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
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao.Interfaces
{
    public interface IUserFolderXMailDao
    {
        UserFolderXMail Get(int mailId);
        List<UserFolderXMail> GetList(uint? folderId = null, List<int> mailIds = null);
        List<int> GetMailIds(uint folderId);
        void SetMessagesFolder(IEnumerable<int> messageIds, uint folderId);
        int Save(UserFolderXMail item);
        int Remove(int? mailId = null, uint? folderId = null);
        int Remove(List<int> mailIds);
        int RemoveByMailbox(int mailboxId);
    }
}
