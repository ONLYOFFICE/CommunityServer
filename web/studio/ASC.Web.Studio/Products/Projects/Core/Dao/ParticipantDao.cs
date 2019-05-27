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
