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


#region Usings

using System;
using System.Data;
using ASC.Common.Data;
using ASC.Blogs.Core.Domain;

#endregion

namespace ASC.Blogs.Core.Data
{
    static class RowMappers
    {
        public static Blog ToBlog(IDataRecord row)
        {
            return new Blog
                       {
                           BlogID = row.Get<int>("id"),
                           Name = row.Get<string>("name"),
                           UserID = row.Get<Guid>("user_id"),
                           GroupID = row.Get<Guid>("group_id"),
                       };
        }

        public static string ToString(IDataRecord row)
        {
            return row.Get<string>("name");
        }

        public static Tag ToTag(IDataRecord row)
        {
            return new Tag
                       {
                           Content = row.Get<string>("name"),
                           PostId = row.Get<Guid>("post_id"),
                       };
        }

        public static Comment ToComment(IDataRecord row)
        {
            return new Comment
                       {
                           ID = row.Get<Guid>("id"),
                           PostId = row.Get<Guid>("post_id"),
                           Content = row.Get<string>("content"),
                           UserID = row.Get<Guid>("created_by"),
                           Datetime = ASC.Core.Tenants.TenantUtil.DateTimeFromUtc(row.Get<DateTime>("created_when")),
                           ParentId = row.Get<Guid>("parent_id"),
                           Inactive = row.Get<int>("inactive") > 0
                       };
        }

        public static Post ToPost(IDataRecord row, bool withContent)
        {
            return new Post
                       {
                           ID = row.Get<Guid>("id"),
                           Title = row.Get<string>("title"),
                           Content = withContent ? row.Get<string>("content") : null,
                           UserID = row.Get<Guid>("created_by"),
                           Datetime = ASC.Core.Tenants.TenantUtil.DateTimeFromUtc(row.Get<DateTime>("created_when")),
                           BlogId = row.Get<int>("blog_id"),
                           AutoIncrementID = row.Get<int>("post_id")
                       };
        }
    }
}
