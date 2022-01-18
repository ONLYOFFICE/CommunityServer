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


using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using System.Linq;

namespace ASC.Files.Core.Data
{
    internal class LinkDao : AbstractDao, ILinkDao
    {
        public LinkDao(int tenant, string dbid)
            : base(tenant, dbid)
        {
        }
        
        public void AddLink(object sourceId, object linkedId)
        {
            var sql = Insert("files_link")
                        .InColumnValue("source_id", sourceId)
                        .InColumnValue("linked_id", linkedId)
                        .InColumnValue("linked_for", SecurityContext.CurrentAccount.ID.ToString());
            dbManager.ExecuteNonQuery(sql);
        }

        public object GetSource(object linkedId)
        {
            var query =
                Query("files_link")
                    .Select("source_id")
                    .Where("linked_id", linkedId)
                    .Where("linked_for", SecurityContext.CurrentAccount.ID.ToString());

            return dbManager.ExecuteList(query)
                            .ConvertAll(r => r[0])
                            .SingleOrDefault();
        }

        public object GetLinked(object sourceId)
        {
            var query =
                Query("files_link")
                    .Select("linked_id")
                    .Where("source_id", sourceId)
                    .Where("linked_for", SecurityContext.CurrentAccount.ID.ToString());

            return dbManager.ExecuteList(query)
                            .ConvertAll(r => r[0])
                            .SingleOrDefault();
        }

        public void DeleteLink(object sourceId)
        {
            var query =
                Delete("files_link")
                    .Where("source_id", sourceId)
                    .Where("linked_for", SecurityContext.CurrentAccount.ID.ToString());

            dbManager.ExecuteNonQuery(query);
        }

        public void DeleteAllLink(object fileId)
        {
            var query =
                Delete("files_link")
                    .Where(Exp.Eq("source_id", fileId) | Exp.Eq("linked_id", fileId));

            dbManager.ExecuteNonQuery(query);
        }
    }
}