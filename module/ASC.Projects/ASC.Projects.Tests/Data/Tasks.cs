/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using ASC.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Domain.Reports;
using ASC.Projects.Data;
using ASC.Projects.Engine;
using NUnit.Framework;

namespace ASC.Projects.Tests.Data
{

    [TestFixture]
    public class Tasks : TestBase
    {
        private TaskEngine engine = new TaskEngine(new DaoFactory(DbId, 0), new EngineFactory(DbId, 0, null));


        public Tasks()
        {
            CoreContext.TenantManager.SetCurrentTenant(0);
        }

        [Test]
        public void SaveOrUpdateTest()
        {
            var daoFactory = new DaoFactory("projects", 0);
            var task = daoFactory.GetTaskDao().GetById(187);
            var participant = new Participant(SecurityContext.CurrentAccount.ID);

            Console.WriteLine(task.UniqID);
            daoFactory.GetParticipantDao().Read(participant.ID, task.UniqID, DateTime.Now);
            Console.WriteLine(daoFactory.GetParticipantDao().WhenReaded(participant.ID, task.UniqID));
        }

        [Test]
        public void BuildTaskListReport()
        {
            var daoFactory = new DaoFactory("projects", 0);
            var result = daoFactory.GetReportDao().BuildTaskListReport(new ReportFilter());
            Console.WriteLine(result.Count);
        }

        [Test]
        public void BuildUsersWorkReport1()
        {
            var daoFactory = new DaoFactory("projects", 0);
            var result = daoFactory.GetReportDao().BuildUsersWorkReport(new ReportFilter());
            Console.WriteLine(result.Count);

        }

        [Test]
        public void GetByResponsibleTest()
        {
            var tasks = engine.GetByResponsible(Guid.Empty);
        }
    }
}
