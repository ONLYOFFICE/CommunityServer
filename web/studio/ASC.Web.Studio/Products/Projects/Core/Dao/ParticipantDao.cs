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
using ASC.Common.Data.Sql;
using ASC.Projects.Core.DataInterfaces;
using System.Collections.Generic;

namespace ASC.Projects.Data.DAO
{
    class ParticipantDao : BaseDao, IParticipantDao
    {
        public IDaoFactory DaoFactory { get; set; }
        public IProjectDao ProjectDao { get { return DaoFactory.ProjectDao; } }

        public ParticipantDao(int tenant) : base(tenant)
        {
        }

        public int[] GetFollowingProjects(Guid participant)
        {
            return Db.ExecuteList(new SqlQuery(FollowingProjectTable).Select("project_id").Where("participant_id", participant.ToString()))
                .ConvertAll(r => Convert.ToInt32(r[0]))
                .ToArray();
        }

        public int[] GetMyProjects(Guid participant)
        {
            return Db.ExecuteList(new SqlQuery(ParticipantTable).Select("project_id").Where("participant_id", participant.ToString()))
                .ConvertAll(r => Convert.ToInt32(r[0]))
                .ToArray();
        }
        public List<int> GetInterestedProjects(Guid participant)
        {
            var unionQ = new SqlQuery(FollowingProjectTable).Select("project_id")
                                                            .Where("participant_id", participant.ToString())
                                                            .Union(
                                                            new SqlQuery(ParticipantTable)
                                                            .Select("project_id")
                                                            .Where("participant_id",participant.ToString()));

            return Db.ExecuteList(unionQ).ConvertAll(r => Convert.ToInt32(r[0]));
        }

        public void AddToFollowingProjects(int project, Guid participant)
        {
            Db.ExecuteNonQuery(
                new SqlInsert(FollowingProjectTable, true)
                    .InColumnValue("project_id", project)
                    .InColumnValue("participant_id", participant.ToString()));

            ProjectDao.UpdateLastModified(project);
        }

        public void RemoveFromFollowingProjects(int project, Guid participant)
        {
            Db.ExecuteNonQuery(
                new SqlDelete(FollowingProjectTable)
                    .Where("project_id", project)
                    .Where("participant_id", participant.ToString()));

            ProjectDao.UpdateLastModified(project);
        }
    }
}
