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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Projects.Core.DataInterfaces;
using System.Collections.Generic;

namespace ASC.Projects.Data.DAO
{
    class ParticipantDao : BaseDao, IParticipantDao
    {
        public ParticipantDao(string dbId, int tenant) : base(dbId, tenant) { }


        public int[] GetFollowingProjects(Guid participant)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(new SqlQuery(FollowingProjectTable).Select("project_id").Where("participant_id", participant.ToString()))
                    .ConvertAll(r => Convert.ToInt32(r[0]))
                    .ToArray();
            }
        }

        public int[] GetMyProjects(Guid participant)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(new SqlQuery(ParticipantTable).Select("project_id").Where("participant_id", participant.ToString()))
                    .ConvertAll(r => Convert.ToInt32(r[0]))
                    .ToArray();
            }
        }
        public List<int> GetInterestedProjects(Guid participant)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var unionQ = new SqlQuery(FollowingProjectTable).Select("project_id")
                                                                .Where("participant_id", participant.ToString())
                                                                .Union(
                                                                new SqlQuery(ParticipantTable)
                                                                .Select("project_id")
                                                                .Where("participant_id",participant.ToString()));

                return db.ExecuteList(unionQ).ConvertAll(r => Convert.ToInt32(r[0]));
            }
        }

        public void AddToFollowingProjects(int project, Guid participant)
        {
            using (var db = new DbManager(DatabaseId))
            {
                db.ExecuteNonQuery(
                    new SqlInsert(FollowingProjectTable, true)
                        .InColumnValue("project_id", project)
                        .InColumnValue("participant_id", participant.ToString()));

                var projDao = new ProjectDao(db.DatabaseId, Tenant);
                projDao.UpdateLastModified(project);
            }
        }

        public void RemoveFromFollowingProjects(int project, Guid participant)
        {
            using (var db = new DbManager(DatabaseId))
            {
                db.ExecuteNonQuery(
                    new SqlDelete(FollowingProjectTable)
                        .Where("project_id", project)
                        .Where("participant_id", participant.ToString()));

                var projDao = new ProjectDao(db.DatabaseId, Tenant);
                projDao.UpdateLastModified(project);
            }
        }
    }
}
