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


using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core.Tenants;

namespace ASC.Web.UserControls.Wiki.Data
{
    class CommentDao : BaseDao
    {
        public CommentDao(string dbid, int tenant)
            : base(dbid, tenant)
        {
        }


        public List<Comment> GetComments(string pageName)
        {
            var q = Query("wiki_comments")
                .Select("Id", "ParentId", "PageName", "Body", "UserId", "Date", "Inactive")
                .Where("PageName", pageName)
                .OrderBy("Date", true);

            return db.ExecuteList(q)
                .ConvertAll(r => ToComment(r));
        }

        public Comment GetComment(Guid id)
        {
            var q = Query("wiki_comments")
                .Select("Id", "ParentId", "PageName", "Body", "UserId", "Date", "Inactive")
                .Where("Id", id.ToString());

            return db.ExecuteList(q)
                .ConvertAll(r => ToComment(r))
                .SingleOrDefault();
        }

        public Comment SaveComment(Comment comment)
        {
            if (comment == null) throw new NotImplementedException("comment");

            if (comment.Id == Guid.Empty) comment.Id = Guid.NewGuid();
            var i = Insert("wiki_comments")
                .InColumnValue("id", comment.Id.ToString())
                .InColumnValue("ParentId", comment.ParentId.ToString())
                .InColumnValue("PageName", comment.PageName)
                .InColumnValue("Body", comment.Body)
                .InColumnValue("UserId", comment.UserId.ToString())
                .InColumnValue("Date", TenantUtil.DateTimeToUtc(comment.Date))
                .InColumnValue("Inactive", comment.Inactive);

            db.ExecuteNonQuery(i);

            return comment;
        }

        public void RemoveComment(Guid id)
        {
            var d = Delete("wiki_comments").Where("id", id.ToString());
            db.ExecuteNonQuery(d);
        }


        private Comment ToComment(object[] r)
        {
            return new Comment
            {
                Id = new Guid((string)r[0]),
                ParentId = new Guid((string)r[1]),
                PageName = (string)r[2],
                Body = (string)r[3],
                UserId = new Guid((string)r[4]),
                Date = TenantUtil.DateTimeFromUtc((DateTime)r[5]),
                Inactive = Convert.ToBoolean(r[6]),
            };
        }
    }
}