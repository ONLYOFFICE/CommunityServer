/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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


        public IList Search(String text, int projectId)
        {
            var projWhere = Exp.Empty;
            var mileWhere = Exp.Empty;
            var messWhere = Exp.Empty;
            var taskWhere = Exp.Empty;

            if (FullTextSearch.SupportModule(FullTextSearch.ProjectsModule))
            {
                var searched = FullTextSearch.Search(text, FullTextSearch.ProjectsModule);

                var projIds = GetIdentifiers(searched, projectId, EntityType.Project);
                if (0 < projIds.Length) projWhere = Exp.In("id", projIds);
                
                var mileIds = GetIdentifiers(searched, projectId, EntityType.Milestone);
                if (0 < mileIds.Length) mileWhere = Exp.In("t.id", mileIds);
                
                var messIds = GetIdentifiers(searched, projectId, EntityType.Message);
                if (0 < messIds.Length) messWhere = Exp.In("t.id", messIds);
                
                var taskIds = GetIdentifiers(searched, projectId, EntityType.Task);
                if (0 < taskIds.Length) taskWhere = Exp.In("t.id", taskIds);
            }
            else
            {
                var keywords = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(k => 3 <= k.Trim().Length)
                    .ToArray();
                if (keywords.Length == 0) return new ArrayList();

                var projIdWhere = 0 < projectId ? Exp.Eq("p.id", projectId) : Exp.Empty;
                projWhere = BuildLike(new[] { "title", "description" }, keywords, true) & projIdWhere;
                mileWhere = BuildLike(new[] { "t.title" }, keywords, true) & projIdWhere;
                messWhere = BuildLike(new[] { "t.title", "t.content" }, keywords, true) & projIdWhere;
                taskWhere = BuildLike(new[] { "t.title", "t.description" }, keywords, true) & projIdWhere;
            }

            var result = new ArrayList();

            if (projWhere != Exp.Empty)
            {
                var projDao = new ProjectDao(DatabaseId, Tenant);
                result.AddRange(projDao.GetProjects(projWhere));
            }
            if (mileWhere != Exp.Empty)
            {
                var mileDao = new MilestoneDao(DatabaseId, Tenant);
                result.AddRange(mileDao.GetMilestones(mileWhere));
            }
            if (messWhere != Exp.Empty)
            {
                var messDao = new MessageDao(DatabaseId, Tenant);
                result.AddRange(messDao.GetMessages(messWhere));
            }
            if (taskWhere != Exp.Empty)
            {
                var taskDao = new TaskDao(DatabaseId, Tenant);
                result.AddRange(taskDao.GetTasks(taskWhere));
            }

            return result;
        }

        private Exp BuildLike(string[] columns, string[] keywords, bool startWith)
        {
            var like = Exp.Empty;
            foreach (var keyword in keywords)
            {
                var keywordLike = Exp.Empty;
                foreach (var column in columns)
                {
                    keywordLike = keywordLike | Exp.Like(column, keyword, startWith ? SqlLike.StartWith : SqlLike.EndWith) | Exp.Like(column, ' ' + keyword);
                }
                like = like & keywordLike;
            }
            return like;
        }

        private static object[] GetIdentifiers(TextSearchResult searchResult, int projectId, EntityType entryType)
        {
            var result = new List<object> { -1 };
            var ids = new List<string>();

            if (projectId != 0)
            {
                ids.AddRange(searchResult.GetIdentifierDetails(projectId.ToString(CultureInfo.InvariantCulture)));
            }
            else
            {
                ids.AddRange(searchResult.GetIdentifiers());
                foreach (var id in searchResult.GetIdentifiers())
                {
                    ids.AddRange(searchResult.GetIdentifierDetails(id));
                }
            }

            foreach (var id in ids)
            {
                if (entryType == EntityType.Project && char.IsDigit(id, id.Length - 1))
                {
                    result.Add(int.Parse(id));
                }
                else if (entryType == EntityType.Milestone && id.EndsWith("s"))
                {
                    result.Add(int.Parse(id.TrimEnd('s')));
                }
                else if (entryType == EntityType.Task && id.EndsWith("t"))
                {
                    result.Add(int.Parse(id.TrimEnd('t')));
                }
                else if (entryType == EntityType.Message && id.EndsWith("m"))
                {
                    result.Add(int.Parse(id.TrimEnd('m')));
                }
                else if (entryType == EntityType.File && id.EndsWith("f"))
                {
                    result.Add(id.TrimEnd('f'));
                }
            }

            return result.ToArray();
        }
    }
}
