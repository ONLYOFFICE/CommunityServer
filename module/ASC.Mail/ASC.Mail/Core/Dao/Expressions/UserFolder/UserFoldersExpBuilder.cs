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

namespace ASC.Mail.Core.Dao.Expressions.UserFolder
{
    public class UserFoldersExpBuilder
    {
        private readonly SimpleUserFoldersExp _exp;

        public UserFoldersExpBuilder(int tenant, string user)
        {
            _exp = new SimpleUserFoldersExp(tenant, user);
        }

        public UserFoldersExpBuilder SetIds(List<uint> ids)
        {
            _exp.Ids = ids;
            return this;
        }

        public UserFoldersExpBuilder SetParent(uint parentId)
        {
            _exp.ParentId = parentId;
            return this;
        }

        public UserFoldersExpBuilder SetName(string name)
        {
            _exp.Name = name;
            return this;
        }

        public UserFoldersExpBuilder SetHasFolders(bool hasFolders)
        {
            _exp.HasFolders = hasFolders;
            return this;
        }

        public UserFoldersExpBuilder SetHasMails(bool hasMails)
        {
            _exp.HasMails = hasMails;
            return this;
        }

        public UserFoldersExpBuilder SetOrderBy(string orderBy)
        {
            _exp.OrderBy = orderBy;
            return this;
        }

        public UserFoldersExpBuilder SetOrderAsc(bool orderAsc)
        {
            _exp.OrderAsc = orderAsc;
            return this;
        }

        public UserFoldersExpBuilder SetStartIndex(int startIndex)
        {
            _exp.StartIndex = startIndex;
            return this;
        }

        public UserFoldersExpBuilder SetLimit(int limit)
        {
            _exp.Limit = limit;
            return this;
        }

        public SimpleUserFoldersExp Build()
        {
            return _exp;
        }

        public static implicit operator SimpleUserFoldersExp(UserFoldersExpBuilder builder)
        {
            return builder._exp;
        }
    }
}
