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
