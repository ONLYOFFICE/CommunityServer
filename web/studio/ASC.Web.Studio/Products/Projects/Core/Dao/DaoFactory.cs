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


#region Usings

using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Data.DAO;

#endregion

namespace ASC.Projects.Data
{
    public class DaoFactory : IDaoFactory
    {
        private readonly string dbId;
        private readonly int tenant;


        public DaoFactory(string dbId, int tenant)
        {
            this.dbId = dbId;
            this.tenant = tenant;
        }


        public IProjectDao GetProjectDao()
        {
            return new CachedProjectDao(dbId, tenant);
        }

        public IParticipantDao GetParticipantDao()
        {
            return new ParticipantDao(dbId, tenant);
        }

        public IMilestoneDao GetMilestoneDao()
        {
            return new CachedMilestoneDao(dbId, tenant);
        }

        public ITaskDao GetTaskDao()
        {
            return new CachedTaskDao(dbId, tenant);
        }

        public ISubtaskDao GetSubtaskDao()
        {
            return new CachedSubtaskDao(dbId, tenant);
        }

        public IMessageDao GetMessageDao()
        {
            return new CachedMessageDao(dbId, tenant);
        }

        public ICommentDao GetCommentDao()
        {
            return new CommentDao(dbId, tenant);
        }

        public ITemplateDao GetTemplateDao()
        {
            return new TemplateDao(dbId, tenant);
        }

        public ITimeSpendDao GetTimeSpendDao()
        {
            return new TimeSpendDao(dbId, tenant);
        }

        public IReportDao GetReportDao()
        {
            return new ReportDao(dbId, tenant);
        }

        public ISearchDao GetSearchDao()
        {
            return new SearchDao(dbId, tenant);
        }

        public ITagDao GetTagDao()
        {
            return new TagDao(dbId, tenant);
        }
    }
}
