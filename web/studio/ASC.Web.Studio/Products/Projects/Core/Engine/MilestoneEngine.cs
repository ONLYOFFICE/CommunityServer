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
using ASC.Core;
using ASC.Core.Tenants;
using ASC.ElasticSearch;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Projects.Core.Search;

namespace ASC.Projects.Engine
{
    public class MilestoneEngine
    {
        public IDaoFactory DaoFactory { get; set; }
        public ProjectSecurity ProjectSecurity { get; set; }
        public bool DisableNotifications { get; set; }

        #region Get Milestones

        public MilestoneEngine(bool disableNotifications)
        {
            DisableNotifications = disableNotifications;
        }

        public IEnumerable<Milestone> GetAll()
        {
            return DaoFactory.MilestoneDao.GetAll().Where(CanRead);
        }

        public List<Milestone> GetByFilter(TaskFilter filter)
        {
            var listMilestones = new List<Milestone>();
            var anyOne = ProjectSecurity.IsPrivateDisabled;
            var isAdmin = ProjectSecurity.CurrentUserAdministrator;

            while (true)
            {
                var milestones = DaoFactory.MilestoneDao.GetByFilter(filter, isAdmin, anyOne);

                if (filter.LastId != 0)
                {
                    var lastMilestoneIndex = milestones.FindIndex(r => r.ID == filter.LastId);

                    if (lastMilestoneIndex >= 0)
                    {
                        milestones = milestones.SkipWhile((r, index) => index <= lastMilestoneIndex).ToList();
                    }
                }

                listMilestones.AddRange(milestones);

                if (filter.Max <= 0 || filter.Max > 150000) break;

                listMilestones = listMilestones.Take((int) filter.Max).ToList();

                if (listMilestones.Count == filter.Max || milestones.Count == 0) break;

                if (listMilestones.Count != 0)
                    filter.LastId = listMilestones.Last().ID;

                filter.Offset += filter.Max;
            }

            return listMilestones;
        }

        public int GetByFilterCount(TaskFilter filter)
        {
            return DaoFactory.MilestoneDao.GetByFilterCount(filter, ProjectSecurity.CurrentUserAdministrator, ProjectSecurity.IsPrivateDisabled);
        }

        public List<Tuple<Guid, int, int>> GetByFilterCountForReport(TaskFilter filter)
        {
            return DaoFactory.MilestoneDao.GetByFilterCountForReport(filter, ProjectSecurity.CurrentUserAdministrator, ProjectSecurity.IsPrivateDisabled);
        }

        public List<Milestone> GetByProject(int projectId)
        {
            var milestones = DaoFactory.MilestoneDao.GetByProject(projectId).Where(CanRead).ToList();
            milestones.Sort((x, y) =>
            {
                if (x.Status != y.Status) return x.Status.CompareTo(y.Status);
                if (x.Status == MilestoneStatus.Open) return x.DeadLine.CompareTo(y.DeadLine);
                return y.DeadLine.CompareTo(x.DeadLine);
            });
            return milestones;
        }

        public List<Milestone> GetByStatus(int projectId, MilestoneStatus milestoneStatus)
        {
            var milestones = DaoFactory.MilestoneDao.GetByStatus(projectId, milestoneStatus).Where(CanRead).ToList();
            milestones.Sort((x, y) =>
            {
                if (x.Status != y.Status) return x.Status.CompareTo(y.Status);
                if (x.Status == MilestoneStatus.Open) return x.DeadLine.CompareTo(y.DeadLine);
                return y.DeadLine.CompareTo(x.DeadLine);
            });
            return milestones;
        }

        public List<Milestone> GetUpcomingMilestones(int max, params int[] projects)
        {
            var offset = 0;
            var milestones = new List<Milestone>();
            while (true)
            {
                var packet = DaoFactory.MilestoneDao.GetUpcomingMilestones(offset, 2 * max, projects);
                milestones.AddRange(packet.Where(CanRead));
                if (max <= milestones.Count || packet.Count() < 2 * max)
                {
                    break;
                }
                offset += 2 * max;
            }
            return milestones.Count <= max ? milestones : milestones.GetRange(0, max);
        }

        public List<Milestone> GetLateMilestones(int max)
        {
            var offset = 0;
            var milestones = new List<Milestone>();
            while (true)
            {
                var packet = DaoFactory.MilestoneDao.GetLateMilestones(offset, 2 * max);
                milestones.AddRange(packet.Where(CanRead));
                if (max <= milestones.Count || packet.Count() < 2 * max)
                {
                    break;
                }
                offset += 2 * max;
            }
            return milestones.Count <= max ? milestones : milestones.GetRange(0, max);
        }

        public List<Milestone> GetByDeadLine(DateTime deadline)
        {
            return DaoFactory.MilestoneDao.GetByDeadLine(deadline).Where(CanRead).ToList();
        }

        public Milestone GetByID(int id)
        {
            return GetByID(id, true);
        }

        public Milestone GetByID(int id, bool checkSecurity)
        {
            var m = DaoFactory.MilestoneDao.GetById(id);

            if (!checkSecurity)
                return m;

            return CanRead(m) ? m : null;
        }

        public bool IsExists(int id)
        {
            return GetByID(id) != null;
        }

        public string GetLastModified()
        {
            return DaoFactory.MilestoneDao.GetLastModified();
        }

        private bool CanRead(Milestone m)
        {
            return ProjectSecurity.CanRead(m);
        }

        #endregion

        #region Save, Delete, Notify

        public Milestone SaveOrUpdate(Milestone milestone)
        {
            return SaveOrUpdate(milestone, false, false);
        }

        public Milestone SaveOrUpdate(Milestone milestone, bool notifyResponsible)
        {
            return SaveOrUpdate(milestone, notifyResponsible, false);
        }

        public Milestone SaveOrUpdate(Milestone milestone, bool notifyResponsible, bool import)
        {
            if (milestone == null) throw new ArgumentNullException("milestone");
            if (milestone.Project == null) throw new Exception("milestone.project is null");
            if (milestone.Responsible.Equals(Guid.Empty)) throw new Exception("milestone.responsible is empty");

            // check guest responsible
            if (ProjectSecurity.IsVisitor(milestone.Responsible))
            {
                ProjectSecurity.CreateGuestSecurityException();
            }

            milestone.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            milestone.LastModifiedOn = TenantUtil.DateTimeNow();

            var isNew = milestone.ID == default(int);//Task is new
            var oldResponsible = Guid.Empty;

            if (isNew)
            {
                if (milestone.CreateBy == default(Guid)) milestone.CreateBy = SecurityContext.CurrentAccount.ID;
                if (milestone.CreateOn == default(DateTime)) milestone.CreateOn = TenantUtil.DateTimeNow();

                ProjectSecurity.DemandCreate<Milestone>(milestone.Project);
                milestone = DaoFactory.MilestoneDao.Save(milestone);
            }
            else
            {
                var oldMilestone = DaoFactory.MilestoneDao.GetById(new[] { milestone.ID }).FirstOrDefault();

                if (oldMilestone == null) throw new ArgumentNullException("milestone");

                oldResponsible = oldMilestone.Responsible;

                ProjectSecurity.DemandEdit(milestone);
                milestone = DaoFactory.MilestoneDao.Save(milestone);

            }


            if (!milestone.Responsible.Equals(Guid.Empty))
                NotifyMilestone(milestone, notifyResponsible, isNew, oldResponsible);

            FactoryIndexer<MilestonesWrapper>.IndexAsync(milestone);

            return milestone;
        }

        public Milestone ChangeStatus(Milestone milestone, MilestoneStatus newStatus)
        {
            ProjectSecurity.DemandEdit(milestone);

            if (milestone == null) throw new ArgumentNullException("milestone");
            if (milestone.Project == null) throw new Exception("Project can be null.");
            if (milestone.Status == newStatus) return milestone;
            if (milestone.Project.Status == ProjectStatus.Closed) throw new Exception(EngineResource.ProjectClosedError);
            if (milestone.ActiveTaskCount != 0 && newStatus == MilestoneStatus.Closed) throw new Exception("Can not close a milestone with open tasks");

            milestone.Status = newStatus;
            milestone.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            milestone.LastModifiedOn = TenantUtil.DateTimeNow();
            milestone.StatusChangedOn = TenantUtil.DateTimeNow();

            var senders = new HashSet<Guid> { milestone.Project.Responsible, milestone.CreateBy, milestone.Responsible };

            if (newStatus == MilestoneStatus.Closed && !false && senders.Count != 0)
            {
                NotifyClient.Instance.SendAboutMilestoneClosing(senders, milestone);
            }

            if (newStatus == MilestoneStatus.Open && !false && senders.Count != 0)
            {
                NotifyClient.Instance.SendAboutMilestoneResumed(senders, milestone);
            }

            return DaoFactory.MilestoneDao.Save(milestone);
        }

        private void NotifyMilestone(Milestone milestone, bool notifyResponsible, bool isNew, Guid oldResponsible)
        {
            //Don't send anything if notifications are disabled
            if (DisableNotifications) return;

            if (isNew && milestone.Project.Responsible != SecurityContext.CurrentAccount.ID && !milestone.Project.Responsible.Equals(milestone.Responsible))
            {
                NotifyClient.Instance.SendAboutMilestoneCreating(new List<Guid> { milestone.Project.Responsible }, milestone);
            }

            if (notifyResponsible && milestone.Responsible != SecurityContext.CurrentAccount.ID)
            {
                if (isNew || !oldResponsible.Equals(milestone.Responsible))
                    NotifyClient.Instance.SendAboutResponsibleByMilestone(milestone);
                else
                    NotifyClient.Instance.SendAboutMilestoneEditing(milestone);
            }
        }


        public void Delete(Milestone milestone)
        {
            if (milestone == null) throw new ArgumentNullException("milestone");

            ProjectSecurity.DemandDelete(milestone);
            DaoFactory.MilestoneDao.Delete(milestone.ID);

            var users = new HashSet<Guid> { milestone.Project.Responsible, milestone.Responsible };

            NotifyClient.Instance.SendAboutMilestoneDeleting(users, milestone);

            FactoryIndexer<MilestonesWrapper>.DeleteAsync(milestone);
        }

        #endregion
    }
}