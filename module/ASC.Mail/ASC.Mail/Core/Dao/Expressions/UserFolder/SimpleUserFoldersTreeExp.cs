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
using System.Linq;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.DbSchema.Tables;

namespace ASC.Mail.Core.Dao.Expressions.UserFolder
{
    public class SimpleUserFoldersTreeExp : IUserFoldersTreeExp
    {
        public List<uint> Ids { get; set; }
        public uint? ParentId { get; set; }
        public uint? Level { get; set; }

        public string OrderBy { get; set; }
        public bool? OrderAsc { get; set; }
        public int? StartIndex { get; set; }
        public int? Limit { get; set; }

        public static UserFoldersTreeExpBuilder CreateBuilder()
        {
            return new UserFoldersTreeExpBuilder();
        }

        public Exp GetExpression()
        {
            var exp = Exp.Empty;

            if (Ids != null && Ids.Any())
            {
                exp &= Exp.In(UserFolderTreeTable.Columns.FolderId, Ids);
            }

            if (ParentId.HasValue)
            {
                exp &= Exp.Eq(UserFolderTreeTable.Columns.ParentId, ParentId.Value);
            }

            if (Level.HasValue)
            {
                exp &= Exp.Eq(UserFolderTreeTable.Columns.Level, Level.Value);
            }

            return exp;
        }
    }
}