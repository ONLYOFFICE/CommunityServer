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
    public class SimpleUserFoldersExp : IUserFoldersExp
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public List<uint> Ids { get; set; }
        public uint? ParentId { get; set; }
        public string Name { get; set; }
        public bool? HasMails { get; set; }
        public bool? HasFolders { get; set; }

        public string OrderBy { get; set; }
        public bool? OrderAsc { get; set; }
        public int? StartIndex { get; set; }
        public int? Limit { get; set; }

        public SimpleUserFoldersExp(int tenant, string user)
        {
            Tenant = tenant;
            User = user;
        }

        public static UserFoldersExpBuilder CreateBuilder(int tenant, string user)
        {
            return new UserFoldersExpBuilder(tenant, user);
        }

        public Exp GetExpression()
        {
            var exp = Exp.Eq(UserFolderTable.Columns.Tenant, Tenant)
                      & Exp.Eq(UserFolderTable.Columns.User, User);

            if (Ids != null && Ids.Any())
            {
                exp &= Exp.In(UserFolderTable.Columns.Id, Ids);
            }

            if (ParentId.HasValue)
            {
                exp &= Exp.Eq(UserFolderTable.Columns.ParentId, ParentId.Value);
            }

            if (!string.IsNullOrEmpty(Name))
            {
                exp &= Exp.Eq(UserFolderTable.Columns.Name, Name);
            }

            if (HasMails.HasValue)
            {
                exp &= HasMails.Value
                    ? Exp.Gt(UserFolderTable.Columns.TotalMessagesCount, 0)
                    : Exp.Eq(UserFolderTable.Columns.TotalMessagesCount, 0);
            }

            if (HasFolders.HasValue)
            {
                exp &= HasFolders.Value 
                    ? Exp.Gt(UserFolderTable.Columns.FolderCount, 0)
                    : Exp.Eq(UserFolderTable.Columns.FolderCount, 0);
            }

            return exp;
        }
    }
}