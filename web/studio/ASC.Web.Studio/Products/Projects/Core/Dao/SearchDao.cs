/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.ElasticSearch;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Core.Search;

namespace ASC.Projects.Data.DAO
{
    class SearchDao : BaseDao, ISearchDao
    {
        public IDaoFactory DaoFactory { get; set; }

        public IProjectDao ProjectDao { get { return DaoFactory.ProjectDao; } }
        public IMilestoneDao MilestoneDao { get { return DaoFactory.MilestoneDao; } }
        public ITaskDao TaskDao { get { return DaoFactory.TaskDao; } }
        public IMessageDao MessageDao { get { return DaoFactory.MessageDao; } }
        public ICommentDao CommentDao { get { return DaoFactory.CommentDao; } }
        public ISubtaskDao SubtaskDao { get { return DaoFactory.SubtaskDao; } }

        public SearchDao(int tenant) : base(tenant)
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

            List<int> projIds;
            if (FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(text), out projIds))
            {
                projWhere = Exp.In("id", projIds);
            }
            else
            {
                projWhere = BuildLike(new[] { "title", "description" }, text, projectId);
            }

            return ProjectDao.GetProjects(projWhere);
        }

        private IEnumerable<DomainObject<int>> GetMilestones(String text, int projectId)
        {
            Exp mileWhere;

            List<int> mileIds;
            if (FactoryIndexer<MilestonesWrapper>.TrySelectIds(s => s.MatchAll(text), out mileIds))
            {
                mileWhere = Exp.In("t.id", mileIds);
            }
            else
            {
                mileWhere = BuildLike(new[] { "t.title" }, text, projectId);
            }

            return MilestoneDao.GetMilestones(mileWhere);
        }

        private IEnumerable<DomainObject<int>> GetTasks(String text, int projectId)
        {
            Exp taskWhere;

            List<int> taskIds;
            if (FactoryIndexer<TasksWrapper>.TrySelectIds(s => s.MatchAll(text), out taskIds))
            {
                taskWhere = Exp.In("t.id", taskIds);
            }
            else
            {
                taskWhere = BuildLike(new[] { "t.title", "t.description" }, text, projectId);
            }

            return TaskDao.GetTasks(taskWhere);
        }

        private IEnumerable<DomainObject<int>> GetMessages(String text, int projectId)
        {
            Exp messWhere;

            List<int> messIds;
            if (FactoryIndexer<DiscussionsWrapper>.TrySelectIds(s => s.MatchAll(text), out messIds))
            {
                messWhere = Exp.In("t.id", messIds);
            }
            else
            {
                messWhere = BuildLike(new[] { "t.title", "t.content" }, text, projectId);
            }

            return MessageDao.GetMessages(messWhere);
        }

        private IEnumerable<DomainObject<int>> GetComments(String text)
        {
            Exp commentsWhere;

            List<int> commentIds;
            if (FactoryIndexer<CommentsWrapper>.TrySelectIds(s => s.MatchAll(text).Where(r=> r.InActive, false), out commentIds))
            {
                commentsWhere = Exp.In("comment_id", commentIds);
            }
            else
            {
                commentsWhere = BuildLike(new[] { "content" }, text);
            }

            return CommentDao.GetComments(commentsWhere);
        }

        private IEnumerable<DomainObject<int>> GetSubtasks(String text)
        {
            Exp subtasksWhere;

            List<int> subtaskIds;
            if (FactoryIndexer<SubtasksWrapper>.TrySelectIds(s => s.MatchAll(text), out subtaskIds))
            {
                subtasksWhere = Exp.In("id", subtaskIds);
            }
            else
            {
                subtasksWhere = BuildLike(new[] { "title" }, text);
            }

            return SubtaskDao.GetSubtasks(subtasksWhere);
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
