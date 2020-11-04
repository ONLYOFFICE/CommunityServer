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
using System.Security;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Engine
{
    public class TimeTrackingEngine
    {
        public IDaoFactory DaoFactory { get; set; }
        public ProjectSecurity ProjectSecurity { get; set; }

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