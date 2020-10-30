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

using ASC.ElasticSearch;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Configuration;

namespace ASC.Web.Projects.Core.Search
{
    public sealed class CommentsWrapper : Wrapper
    {
        [ColumnId("comment_id")]
        public override int Id { get; set; }

        [Column("content", 1, charFilter: CharFilter.html)]
        public string Content { get; set; }

        [Column("target_uniq_id", 2)]
        public string TargetUniqID { get; set; }

        [ColumnCondition("inactive", 3, false)]
        public bool InActive { get; set; }

        [ColumnLastModified("create_on")]
        public override DateTime LastModifiedOn { get; set; }

        protected override string Table { get { return "projects_comments"; } }

        public static implicit operator CommentsWrapper(Comment comment)
        {
            return ProductEntryPoint.Mapper.Map<CommentsWrapper>(comment);
        }
    }
}