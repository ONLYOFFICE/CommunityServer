/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System;
using System.Collections.Generic;
using System.Linq;

using ASC.Common.Data.Sql.Expressions;
using ASC.FullTextIndex;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Data.DAO
{
    class SearchDao : BaseDao, ISearchDao
    {
        public SearchDao(string dbId, int tenant)
            : base(dbId, tenant)
        {
        }

        public IEnumerable<DomainObject<int>> Search(String text, int projectId)
        {
            var result = new List<DomainObject<int>>();
            result.AddRange(GetProjects(text, projectId));
            result.AddRange(GetTasks(text, projectId));
            result.AddRange(GetSubtasks(text));
            result.AddRange(GetMilestones(text, projectId));
            result.AddRange(GetMessages(text, projectId));
            result.AddRange(GetComments(text));
            return result;
        }

        private IEnumerable<DomainObject<int>> GetProjects(String text, int projectId)
        {
            Exp projWhere;

            if (FullTextSearch.SupportModule(FullTextSearch.ProjectsModule))
            {
                var projIds = FullTextSearch.Search(FullTextSearch.ProjectsModule.Match(text));
                projWhere = Exp.In("id", projIds);
            }
            else
            {
                projWhere = BuildLike(new[] { "title", "description" }, text, projectId);
            }

            return new ProjectDao(DatabaseId, Tenant).GetProjects(projWhere);
        }

        private IEnumerable<DomainObject<int>> GetMilestones(String text, int projectId)
        {
            Exp mileWhere;

            if (FullTextSearch.SupportModule(FullTextSearch.ProjectsMilestonesModule))
            {
                var mileIds = FullTextSearch.Search(FullTextSearch.ProjectsMilestonesModule.Match(text));
                mileWhere = Exp.In("t.id", mileIds);
            }
            else
            {
                mileWhere = BuildLike(new[] { "t.title" }, text, projectId);
            }

            return new MilestoneDao(DatabaseId, Tenant).GetMilestones(mileWhere);
        }

        private IEnumerable<DomainObject<int>> GetTasks(String text, int projectId)
        {
            Exp taskWhere;

            if (FullTextSearch.SupportModule(FullTextSearch.ProjectsTasksModule))
            {
                var taskIds = FullTextSearch.Search(FullTextSearch.ProjectsTasksModule.Match(text));

                taskWhere = Exp.In("t.id", taskIds);
            }
            else
            {
                taskWhere = BuildLike(new[] { "t.title", "t.description" }, text, projectId);
            }

            return new TaskDao(DatabaseId, Tenant).GetTasks(taskWhere);
        }

        private IEnumerable<DomainObject<int>> GetMessages(String text, int projectId)
        {
            Exp messWhere;

            if (FullTextSearch.SupportModule(FullTextSearch.ProjectsMessagesModule))
            {
                var messIds = FullTextSearch.Search(FullTextSearch.ProjectsMessagesModule.Match(text));

                messWhere = Exp.In("t.id", messIds);
            }
            else
            {
                messWhere = BuildLike(new[] { "t.title", "t.content" }, text, projectId);
            }

            return new MessageDao(DatabaseId, Tenant).GetMessages(messWhere);
        }

        private IEnumerable<DomainObject<int>> GetComments(String text)
        {
            Exp commentsWhere;

            if (FullTextSearch.SupportModule(FullTextSearch.ProjectsCommentsModule))
            {
                var commentIds = FullTextSearch.Search(FullTextSearch.ProjectsCommentsModule.Match(text));

                commentsWhere = Exp.In("comment_id", commentIds);
            }
            else
            {
                commentsWhere = BuildLike(new[] { "content" }, text);
            }

            return new CommentDao(DatabaseId, Tenant).GetComments(commentsWhere);
        }

        private IEnumerable<DomainObject<int>> GetSubtasks(String text)
        {
            Exp subtasksWhere;

            if (FullTextSearch.SupportModule(FullTextSearch.ProjectsSubtasksModule))
            {
                var subtaskIds = FullTextSearch.Search(FullTextSearch.ProjectsSubtasksModule.Match(text));

                subtasksWhere = Exp.In("id", subtaskIds);
            }
            else
            {
                subtasksWhere = BuildLike(new[] { "title" }, text);
            }

            return new SubtaskDao(DatabaseId, Tenant).GetSubtasks(subtasksWhere);
        }

        private static Exp BuildLike(string[] columns, string text, int projectId = 0)
        {
            var projIdWhere = 0 < projectId ? Exp.Eq("p.id", projectId) : Exp.Empty;
            var keywords = GetKeywords(text);

            var like = Exp.Empty;
            foreach (var keyword in keywords)
            {
                var keywordLike = Exp.Empty;
                foreach (var column in columns)
                {
                    keywordLike = keywordLike | Exp.Like(column, keyword, SqlLike.AnyWhere);
                }
                like = like & keywordLike;
            }
            return like & projIdWhere;
        }

        private static IEnumerable<string> GetKeywords(string text)
        {
            return text.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                .Where(k => 3 <= k.Trim().Length)
                .ToArray();

        }
    }
}
