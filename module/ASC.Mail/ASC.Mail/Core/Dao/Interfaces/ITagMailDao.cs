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

namespace ASC.Mail.Core.Dao.Interfaces
{
    public interface ITagMailDao
    {
        /// <summary>
        ///     Set tag on messages
        /// </summary>
        /// <param name="messageIds"></param>
        /// <param name="tagId"></param>
        void SetMessagesTag(IEnumerable<int> messageIds, int tagId);

        int CalculateTagCount(int id);

        Dictionary<int, List<int>> GetMailTagsDictionary(List<int> mailIds);

        List<int> GetTagIds(List<int> mailIds);

        List<int> GetTagIds(int mailboxId);

        int Delete(int tagId, List<int> mailIds);

        int DeleteByTagId(int tagId);

        int DeleteByMailboxId(int mailboxId);

        int DeleteByMailIds(List<int> mailIds);
    }
}
