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
    public class UserFoldersTreeExpBuilder
    {
        private readonly SimpleUserFoldersTreeExp _exp;

        public UserFoldersTreeExpBuilder()
        {
            _exp = new SimpleUserFoldersTreeExp();
        }

        public UserFoldersTreeExpBuilder SetIds(List<uint> ids)
        {
            _exp.Ids = ids;
            return this;
        }

        public UserFoldersTreeExpBuilder SetParent(uint parentId)
        {
            _exp.ParentId = parentId;
            return this;
        }

        public UserFoldersTreeExpBuilder SetLevel(uint level)
        {
            _exp.Level = level;
            return this;
        }

        public UserFoldersTreeExpBuilder SetOrderBy(string orderBy)
        {
            _exp.OrderBy = orderBy;
            return this;
        }

        public UserFoldersTreeExpBuilder SetOrderAsc(bool orderAsc)
        {
            _exp.OrderAsc = orderAsc;
            return this;
        }

        public UserFoldersTreeExpBuilder SetStartIndex(int startIndex)
        {
            _exp.StartIndex = startIndex;
            return this;
        }

        public UserFoldersTreeExpBuilder SetLimit(int limit)
        {
            _exp.Limit = limit;
            return this;
        }

        public SimpleUserFoldersTreeExp Build()
        {
            return _exp;
        }

        public static implicit operator SimpleUserFoldersTreeExp(UserFoldersTreeExpBuilder builder)
        {
            return builder._exp;
        }
    }
}
