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
using System.Security;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Engine
{
    public class TimeTrackingEngine
    {
        public IDaoFactory DaoFactory { get; set; }

        public List<TimeSpend> GetByFilter(TaskFilter filter)
        {
            var listTimeSpend = new List<TimeSpend>();
            var isAdmin = ProjectSecurity.CurrentUserAdministrator;
            var anyOne = ProjectSecurity.IsPrivateDisabled;

            while (true)
            {
                var timeSpend = DaoFactory.TimeSpendDao.GetByFilter(filter, isAdmin, anyOne);
                timeSpend = GetTasks(timeSpend).Where(r=> r.Task != null).ToList();

                if (filter.LastId != 0)
                {
                    var lastTimeSpendIndex = timeSpend.FindIndex(r => r.ID == filter.LastId);

                    if (lastTimeSpendIndex >= 0)
                    {
                        timeSpend = timeSpend.SkipWhile((r, index) => index <= lastTimeSpendIndex).ToList();
                    }
                }

                listTimeSpend.AddRange(timeSpend);

                if (filter.Max <= 0 || filter.Max > 150000) break;

                listTimeSpend = listTimeSpend.Take((int)filter.Max).ToList();

                if (listTimeSpend.Count == filter.Max || timeSpend.Count == 0) break;

                if (listTimeSpend.Count != 0)
                    filter.LastId = listTimeSpend.Last().ID;

                filter.Offset += filter.Max;
            }

            return listTimeSpend;
        }

        public int GetByFilterCount(TaskFilter filter)
        {
            return DaoFactory.TimeSpendDao.GetByFilterCount(filter, ProjectSecurity.CurrentUserAdministrator, ProjectSecurity.IsPrivateDisabled);
        }

        public float GetByFilterTotal(TaskFilter filter)
        {
            return DaoFactory.TimeSpendDao.GetByFilterTotal(filter, ProjectSecurity.CurrentUserAdministrator, ProjectSecurity.IsPrivateDisabled);
        }


        public List<TimeSpend> GetByTask(int taskId)
        {
            var timeSpend = DaoFactory.TimeSpendDao.GetByTask(taskId);
            return GetTasks(timeSpend).FindAll(r => ProjectSecurity.CanRead(r.Task));
        }

        public List<TimeSpend> GetByProject(int projectId)
        {
            var timeSpend = DaoFactory.TimeSpendDao.GetByProject(projectId);
            return GetTasks(timeSpend).FindAll(r => ProjectSecurity.CanRead(r.Task));
        }

        public string GetTotalByProject(int projectId)
        {
            var time = GetByFilterTotal(new TaskFilter { ProjectIds = new List<int> { projectId } });
            var hours = (int)time;
            var minutes = (int)(Math.Round((time - hours) * 60));
            var result = hours + ":" + minutes.ToString("D2");

            return !result.Equals("0:00", StringComparison.InvariantCulture) ? result : "";
        }

        public TimeSpend GetByID(int id)
        {
            var timeSpend = DaoFactory.TimeSpendDao.GetById(id);
            if (timeSpend != null)
            {
                timeSpend.Task = DaoFactory.TaskDao.GetById(timeSpend.Task.ID);
            }
            return timeSpend;
        }

        public TimeSpend SaveOrUpdate(TimeSpend timeSpend)
        {
            ProjectSecurity.DemandEdit(timeSpend);

            // check guest responsible
            if (ProjectSecurity.IsVisitor(timeSpend.Person))
            {
                ProjectSecurity.CreateGuestSecurityException();
            }

            if (timeSpend.ID == 0)
            {
                timeSpend.CreateOn = DateTime.UtcNow;
            }

            return DaoFactory.TimeSpendDao.Save(timeSpend);
        }

        public TimeSpend ChangePaymentStatus(TimeSpend timeSpend, PaymentStatus newStatus)
        {
            if (!ProjectSecurity.CanEditPaymentStatus(timeSpend)) throw new SecurityException("Access denied.");

            if (timeSpend == null) throw new ArgumentNullException("timeSpend");

            var task = DaoFactory.TaskDao.GetById(timeSpend.Task.ID);

            if (task == null) throw new Exception("Task can't be null.");

            ProjectSecurity.DemandEdit(timeSpend);

            if (timeSpend.PaymentStatus == newStatus) return timeSpend;

            timeSpend.PaymentStatus = newStatus;

            timeSpend.StatusChangedOn = DateTime.UtcNow;

            return DaoFactory.TimeSpendDao.Save(timeSpend);
        }

        public void Delete(TimeSpend timeSpend)
        {
            ProjectSecurity.DemandDelete(timeSpend);
            DaoFactory.TimeSpendDao.Delete(timeSpend.ID);
        }

        private List<TimeSpend> GetTasks(List<TimeSpend> listTimeSpend)
        {
            var listTasks = DaoFactory.TaskDao.GetById(listTimeSpend.Select(r => r.Task.ID).ToList());

            listTimeSpend.ForEach(t => t.Task = listTasks.Find(task => task.ID == t.Task.ID));

            return listTimeSpend;
        }
    }
}