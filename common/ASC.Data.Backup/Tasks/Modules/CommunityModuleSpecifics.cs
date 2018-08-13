/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using ASC.Data.Backup.Tasks.Data;
using ASC.Data.Backup.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ASC.Data.Backup.Tasks.Modules
{
    internal class CommunityModuleSpecifics : ModuleSpecificsBase
    {
        private readonly TableInfo[] _tables = new[]
            {
                new TableInfo("bookmarking_bookmark", "Tenant", "ID")
                    {
                        UserIDColumns = new[] {"UserCreatorID"},
                        DateColumns = new Dictionary<string, bool> {{"Date", false}}
                    },
                new TableInfo("bookmarking_bookmarktag", "tenant"),
                new TableInfo("bookmarking_comment", "tenant", "ID", IdType.Guid)
                    {
                        UserIDColumns = new[] {"UserID"},
                        DateColumns = new Dictionary<string, bool> {{"Datetime", false}}
                    },
                new TableInfo("bookmarking_tag", "tenant", "TagID"),
                new TableInfo("bookmarking_userbookmark", "tenant", "UserBookmarkID")
                    {
                        UserIDColumns = new[] {"UserID"},
                        DateColumns = new Dictionary<string, bool> {{"DateAdded", false}, {"LastModified", false}}
                    },
                new TableInfo("bookmarking_userbookmarktag", "tenant"),
                new TableInfo("blogs_comments", "Tenant", "id", IdType.Guid)
                    {
                        UserIDColumns = new[] {"created_by"},
                        DateColumns = new Dictionary<string, bool> {{"created_when", false}}
                    },
                new TableInfo("blogs_posts", "Tenant", "post_id", IdType.Autoincrement)
                    {
                        UserIDColumns = new[] {"created_by"},
                        DateColumns = new Dictionary<string, bool> {{"created_when", false}, {"LastModified", false}}
                    },
                new TableInfo("blogs_reviewposts", "Tenant")
                    {                        
                        UserIDColumns = new[] {"reviewed_by"},
                        DateColumns = new Dictionary<string, bool> {{"timestamp", false}}
                    },
                new TableInfo("blogs_tags", "Tenant"),
                new TableInfo("events_comment", "Tenant", "id")
                    {
                        UserIDColumns = new[] {"Creator"},
                        DateColumns = new Dictionary<string, bool> {{"Date", false}}
                    },
                new TableInfo("events_feed", "Tenant", "id")
                    {
                        UserIDColumns = new[] {"Creator"},
                        DateColumns = new Dictionary<string, bool> {{"Date", false}, {"LastModified", false}}
                    },
                new TableInfo("events_poll", "Tenant")
                    {
                        DateColumns = new Dictionary<string, bool> {{"StartDate", true}, {"EndDate", true}}
                    },
                new TableInfo("events_pollanswer", "Tenant") {UserIDColumns = new[] {"User"}}, //todo: check //varchar(64)?
                new TableInfo("events_pollvariant", "Tenant", "Id"),
                new TableInfo("events_reader", "Tenant") {UserIDColumns = new[] {"Reader"}}, //todo: check
                new TableInfo("forum_answer", "TenantID", "id")
                    {
                        UserIDColumns = new[] {"user_id"},
                        DateColumns = new Dictionary<string, bool> {{"create_date", false}}
                    },
                new TableInfo("forum_answer_variant"),
                new TableInfo("forum_attachment", "TenantID", "id")
                    {
                        DateColumns = new Dictionary<string, bool> {{"create_date", false}}
                    },
                new TableInfo("forum_category", "TenantID", "id")
                    {
                        UserIDColumns = new[] {"poster_id"},
                        DateColumns = new Dictionary<string, bool> {{"create_date", false}}
                    },
                new TableInfo("forum_lastvisit", "tenantid")
                    {
                        UserIDColumns = new[] {"user_id"},
                        DateColumns = new Dictionary<string, bool> {{"last_visit", false}}
                    },
                new TableInfo("forum_post", "TenantID", "id")
                    {
                        UserIDColumns = new[] {"poster_id", "editor_id"},
                        DateColumns = new Dictionary<string, bool> {{"create_date", false}, {"edit_date", false}, {"LastModified", false}}
                    },
                new TableInfo("forum_question", "TenantID", "id") {DateColumns = new Dictionary<string, bool> {{"create_date", false}}},
                new TableInfo("forum_tag", "TenantID", "id"),
                new TableInfo("forum_thread", "TenantID", "id")
                    {
                        UserIDColumns = new[] {"recent_poster_id"},
                        DateColumns = new Dictionary<string, bool> {{"recent_post_date", false}}
                    },
                new TableInfo("forum_topic", "TenantID", "id")
                    {
                        UserIDColumns = new[] {"poster_id"},
                        DateColumns = new Dictionary<string, bool> {{"create_date", false}, {"LastModified", false}}
                    },
                new TableInfo("forum_topicwatch", "TenantID") {UserIDColumns = new[] {"UserID"}},
                new TableInfo("forum_topic_tag"),
                new TableInfo("forum_variant", idColumn: "id"),
                new TableInfo("wiki_categories", "Tenant"),
                new TableInfo("wiki_comments", "Tenant", "Id", IdType.Guid)
                    {
                        UserIDColumns = new[] {"UserId"},
                        DateColumns = new Dictionary<string, bool> {{"Date", false}}
                    },
                new TableInfo("wiki_files", "Tenant")
                    {
                        UserIDColumns = new[] {"UserID"},
                        DateColumns = new Dictionary<string, bool> {{"Date", false}}
                    },
                new TableInfo("wiki_pages", "tenant", "id", IdType.Autoincrement)
                    {
                        UserIDColumns = new[] {"modified_by"},
                        DateColumns = new Dictionary<string, bool> {{"modified_on", false}}
                    },
                new TableInfo("wiki_pages_history", "Tenant")
                    {
                        UserIDColumns = new[] {"create_by"},
                        DateColumns = new Dictionary<string, bool> {{"create_on", false}}
                    },
            };

        private readonly RelationInfo[] _tableRelations = new[]
            {
                new RelationInfo("bookmarking_bookmark", "ID", "bookmarking_bookmarktag", "BookmarkID"),
                new RelationInfo("bookmarking_tag", "TagID", "bookmarking_bookmarktag", "TagID"),
                new RelationInfo("bookmarking_bookmark", "ID", "bookmarking_comment", "BookmarkID"),
                new RelationInfo("bookmarking_comment", "ID", "bookmarking_comment", "Parent"), 
                new RelationInfo("bookmarking_bookmark", "ID", "bookmarking_userbookmark", "BookmarkID"),
                new RelationInfo("bookmarking_tag", "TagID", "bookmarking_userbookmarktag", "TagID"),
                new RelationInfo("bookmarking_userbookmark", "UserBookmarkID", "bookmarking_userbookmarktag", "UserBookmarkID"),
                new RelationInfo("blogs_comments", "id", "blogs_comments", "parent_id"),
                new RelationInfo("blogs_posts", "id", "blogs_comments", "post_id"),
                new RelationInfo("blogs_comments", "id", "blogs_posts", "LastCommentId", null, null, RelationImportance.Low),
                new RelationInfo("blogs_posts", "id", "blogs_reviewposts", "post_id"),
                new RelationInfo("blogs_posts", "id", "blogs_tags", "post_id"),
                new RelationInfo("events_feed", "Id", "events_comment", "Feed"),
                new RelationInfo("events_comment", "Id", "events_comment", "Parent"),
                new RelationInfo("events_feed", "Id", "events_poll", "Id"), 
                new RelationInfo("events_pollvariant", "Id", "events_pollanswer", "Variant"),
                new RelationInfo("events_feed", "Id", "events_pollvariant", "Poll"),
                new RelationInfo("events_feed", "Id", "events_reader", "Feed"),
                new RelationInfo("forum_question", "id", "forum_answer", "question_id"),
                new RelationInfo("forum_answer", "id", "forum_answer_variant", "answer_id"),
                new RelationInfo("forum_variant", "id", "forum_answer_variant", "variant_id"),
                new RelationInfo("forum_post", "id", "forum_attachment", "post_id"),
                new RelationInfo("forum_category", "id", "forum_attachment", "path"),
                new RelationInfo("forum_thread", "id", "forum_attachment", "path"), 
                new RelationInfo("forum_thread", "id", "forum_lastvisit", "thread_id"),
                new RelationInfo("forum_topic", "id", "forum_post", "topic_id"),
                new RelationInfo("forum_post", "id", "forum_post", "parent_post_id"),
                new RelationInfo("forum_topic", "id", "forum_question", "topic_id"),
                new RelationInfo("forum_category", "id", "forum_thread", "category_id"),
                new RelationInfo("forum_post", "id", "forum_thread", "recent_post_id", null, null, RelationImportance.Low),
                new RelationInfo("forum_topic", "id", "forum_thread", "recent_topic_id", null, null, RelationImportance.Low),
                new RelationInfo("forum_thread", "id", "forum_topic", "thread_id"),
                new RelationInfo("forum_question", "id", "forum_topic", "question_id", null, null, RelationImportance.Low),
                new RelationInfo("forum_post", "id", "forum_topic", "recent_post_id", null, null, RelationImportance.Low),
                new RelationInfo("forum_topic", "id", "forum_topicwatch", "TopicID"),
                new RelationInfo("forum_topic", "id", "forum_topic_tag", "topic_id"),
                new RelationInfo("forum_tag", "id", "forum_topic_tag", "tag_id"),
                new RelationInfo("forum_question", "id", "forum_variant", "question_id"),
                new RelationInfo("wiki_comments", "Id", "wiki_comments", "ParentId") 
            };

        public override ModuleName ModuleName
        {
            get { return ModuleName.Community; }
        }

        public override IEnumerable<TableInfo> Tables
        {
            get { return _tables; }
        }

        public override IEnumerable<RelationInfo> TableRelations
        {
            get { return _tableRelations; }
        }

        public override bool TryAdjustFilePath(ColumnMapper columnMapper, ref string filePath)
        {
            filePath = PreparePath(columnMapper, "/", filePath);
            return filePath != null;
        }

        protected override bool TryPrepareValue(IDbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, IEnumerable<RelationInfo> relations, ref object value)
        {
            relations = relations.ToList();

            if (relations.All(x => x.ChildTable == "forum_attachment" && x.ChildColumn == "path"))
            {
                value = PreparePath(columnMapper, "\\", Convert.ToString(value));
                return value != null;
            }

            return base.TryPrepareValue(connection, columnMapper, table, columnName, relations, ref value);
        }

        protected override bool TryPrepareValue(IDbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, ref object value)
        {
            var column = columnName.ToLowerInvariant();
            if (table.Name == "forum_post" && column == "text" ||
                table.Name == "events_feed" && column == "text" ||
                table.Name == "events_comment" && column == "comment" ||
                table.Name == "blogs_posts" && column == "content" ||
                table.Name == "blogs_comments" && column == "content" ||
                table.Name == "bookmarking_comment" && column == "content" ||
                table.Name == "wiki_comments" && column == "body")
            {
                value = FCKEditorPathUtility.CorrectStoragePath(value as string, columnMapper.GetTenantMapping());
                return true;
            }
            return base.TryPrepareValue(connection, columnMapper, table, columnName, ref value);
        }

        protected override string GetSelectCommandConditionText(int tenantId, TableInfo table)
        {
            if (table.Name == "forum_answer_variant")
                return "inner join forum_answer as t1 on t1.id = t.answer_id where t1.TenantID = " + tenantId;

            if (table.Name == "forum_variant")
                return "inner join forum_question as t1 on t1.id = t.question_id where t1.TenantID = " + tenantId;

            if (table.Name == "forum_topic_tag")
                return "inner join forum_topic as t1 on t1.id = t.topic_id where t1.TenantID = " + tenantId;

            return base.GetSelectCommandConditionText(tenantId, table);
        }


        private static string PreparePath(ColumnMapper columnMapper, string partsSeparator, string path)
        {
            string[] parts = path.Split(new[] { partsSeparator }, StringSplitOptions.None);

            if (parts.Length != 4)
                return null;

            var categoryId = columnMapper.GetMapping("forum_category", "id", parts[0]);
            if (categoryId == null)
                return null;

            var threadId = columnMapper.GetMapping("forum_thread", "id", parts[1]);
            if (threadId == null)
                return null;

            parts[0] = categoryId.ToString();
            parts[1] = threadId.ToString();

            return string.Join(partsSeparator, parts);
        }
    }
}
