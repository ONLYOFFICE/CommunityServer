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
using System.Linq;
using ASC.Core;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Data;
using ASC.Web.Core;
using System.Web;
using System.IO;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;

namespace ASC.Projects.Engine
{
    public class ProjectSecurity
    {
        private static bool? ganttJsExists;


        #region Properties

        private static Guid CurrentUserId
        {
            get { return SecurityContext.CurrentAccount.ID; }
        }

        public static bool CurrentUserAdministrator
        {
            get { return IsAdministrator(CurrentUserId); }
        }

        private static bool CurrentUserIsVisitor
        {
            get { return IsVisitor(CurrentUserId); }
        }

        private static bool CurrentUserIsOutsider
        {
            get { return IsOutsider(CurrentUserId); }
        }

        public static bool IsPrivateDisabled
        {
            get { return SettingsManager.Instance.LoadSettings<TenantAccessSettings>(TenantProvider.CurrentTenantID).Anyone; }
        }

        public static bool IsProjectsEnabled(Guid userID)
        {
            var projects = WebItemManager.Instance[WebItemManager.ProjectsProductID];

            if (projects != null)
            {
                return !projects.IsDisabled(userID);
            }

            return false;
        }

        public static bool IsCrmEnabled(Guid userID)
        {
            var projects = WebItemManager.Instance[WebItemManager.CRMProductID];

            if (projects != null)
            {
                return !projects.IsDisabled(userID);
            }

            return false;
        }

        #endregion

        #region Can Go To Feed

        public static bool CanGoToFeed(Project project, Guid userId)
        {
            if (project == null || !IsProjectsEnabled(userId))
            {
                return false;
            }
            return WebItemSecurity.IsProductAdministrator(EngineFactory.ProductId, userId)
                   || IsInTeam(project, userId, false)
                   || IsFollow(project, userId);
        }

        public static bool CanGoToFeed(ParticipantFull participant, Guid userId)
        {
            if (participant == null || !IsProjectsEnabled(userId))
            {
                return false;
            }
            return IsInTeam(participant.Project, userId, false) || IsFollow(participant.Project, userId);
        }

        public static bool CanGoToFeed(Milestone milestone, Guid userId)
        {
            if (milestone == null || !IsProjectsEnabled(userId))
            {
                return false;
            }
            if (!IsInTeam(milestone.Project, userId, false) && !IsFollow(milestone.Project, userId))
            {
                return false;
            }
            return milestone.Responsible == userId || GetTeamSecurityForParticipants(milestone.Project, userId, ProjectTeamSecurity.Milestone);
        }

        public static bool CanGoToFeed(Message discussion, Guid userId)
        {
            if (discussion == null || !IsProjectsEnabled(userId))
            {
                return false;
            }
            if (discussion.CreateBy == userId)
            {
                return true;
            }
            if (!IsInTeam(discussion.Project, userId, false) && !IsFollow(discussion.Project, userId))
            {
                return false;
            }

            var isSubscriber = new MessageEngine(GetFactory(), null).GetSubscribers(discussion).Any(r => new Guid(r.ID).Equals(userId));
            return isSubscriber && GetTeamSecurityForParticipants(discussion.Project, userId, ProjectTeamSecurity.Messages);
        }

        public static bool CanGoToFeed(Task task, Guid userId)
        {
            if (task == null || !IsProjectsEnabled(userId))
            {
                return false;
            }
            if (task.CreateBy == userId)
            {
                return true;
            }
            if (!IsInTeam(task.Project, userId, false) && !IsFollow(task.Project, userId))
            {
                return false;
            }
            if (task.Responsibles.Contains(userId))
            {
                return true;
            }
            if (task.Milestone != 0 && !CanReadMilestones(task.Project, userId))
            {
                var milestone = GetFactory().GetMilestoneDao().GetById(task.Milestone);
                if (milestone.Responsible == userId)
                {
                    return true;
                }
            }
            return GetTeamSecurityForParticipants(task.Project, userId, ProjectTeamSecurity.Tasks);
        }

        #endregion

        #region Can Read

        public static bool CanReadMessages(Project project, Guid userId)
        {
            return IsProjectsEnabled(userId) && GetTeamSecurity(project, userId, ProjectTeamSecurity.Messages);
        }

        public static bool CanReadMessages(Project project)
        {
            return CanReadMessages(project, CurrentUserId);
        }

        public static bool CanReadFiles(Project project, Guid userId)
        {
            return IsProjectsEnabled(userId) && GetTeamSecurity(project, userId, ProjectTeamSecurity.Files);
        }

        public static bool CanReadFiles(Project project)
        {
            return CanReadFiles(project, CurrentUserId);
        }

        public static bool CanReadTasks(Project project, Guid userId)
        {
            return IsProjectsEnabled(userId) && GetTeamSecurity(project, userId, ProjectTeamSecurity.Tasks);
        }

        public static bool CanReadTasks(Project project)
        {
            return CanReadTasks(project, CurrentUserId);
        }

        public static bool CanLinkContact(Project project)
        {
            return IsProjectsEnabled(CurrentUserId) && CanEdit(project);
        }

        public static bool CanReadMilestones(Project project, Guid userId)
        {
            return IsProjectsEnabled(userId) && GetTeamSecurity(project, userId, ProjectTeamSecurity.Milestone);
        }

        public static bool CanReadMilestones(Project project)
        {
            return CanReadMilestones(project, CurrentUserId);
        }

        public static bool CanReadContacts(Project project, Guid userId)
        {
            return IsCrmEnabled(userId) && IsProjectsEnabled(userId) && GetTeamSecurity(project, userId, ProjectTeamSecurity.Contacts);
        }

        public static bool CanReadContacts(Project project)
        {
            return CanReadContacts(project, CurrentUserId);
        }

        public static bool CanRead(Project project, Guid userId)
        {
            if (!IsProjectsEnabled(userId)) return false;
            if (project == null) return false;
            if (project.Private && IsPrivateDisabled) return false;
            return !project.Private || IsInTeam(project, userId);
        }

        public static bool CanRead(Project project)
        {
            return CanRead(project, CurrentUserId);
        }

        public static bool CanRead(Task task, Guid userId)
        {
            if (task == null || !CanRead(task.Project, userId)) return false;

            if (task.Responsibles.Contains(userId)) return true;

            if (!CanReadTasks(task.Project, userId)) return false;
            if (task.Milestone != 0 && !CanReadMilestones(task.Project, userId))
            {
                var m = GetFactory().GetMilestoneDao().GetById(task.Milestone);
                if (!CanRead(m, userId)) return false;
            }

            return true;
        }

        public static bool CanRead(Task task)
        {
            return CanRead(task, CurrentUserId);
        }

        public static bool CanRead(Subtask subtask)
        {
            if (!IsProjectsEnabled(CurrentUserId)) return false;
            if (subtask == null) return false;
            return subtask.Responsible == CurrentUserId;
        }

        public static bool CanRead(Milestone milestone, Guid userId)
        {
            if (milestone == null || !CanRead(milestone.Project, userId)) return false;
            if (milestone.Responsible == userId) return true;

            return CanReadMilestones(milestone.Project, userId);
        }

        public static bool CanRead(Milestone milestone)
        {
            return CanRead(milestone, CurrentUserId);
        }

        public static bool CanRead(Message message, Guid userId)
        {
            if (message == null || !CanRead(message.Project, userId)) return false;

            return CanReadMessages(message.Project, userId);
        }

        public static bool CanRead(Message message)
        {
            return CanRead(message, CurrentUserId);
        }

        public static bool CanReadGantt(Project project)
        {
            if (!ganttJsExists.HasValue && HttpContext.Current != null)
            {
                var file = HttpContext.Current.Server.MapPath("~/products/projects/js/ganttchart_min.js ");
                ganttJsExists = File.Exists(file);
            }
            if (ganttJsExists.HasValue && ganttJsExists.Value == false)
            {
                return false;
            }
            return CanReadTasks(project) && CanReadMilestones(project);
        }

        #endregion

        #region Can Create

        public static bool Can()
        {
            return !CurrentUserIsVisitor && IsProjectsEnabled(CurrentUserId);
        }

        public static bool Can(object obj)
        {
            if (!Can()) return false;
            return obj != null;
        }

        public static bool CanCreateProject()
        {
            return Can() && CurrentUserAdministrator;
        }

        public static bool CanCreateMilestone(Project project)
        {
            return Can(project) && IsProjectManager(project);
        }

        public static bool CanCreateMessage(Project project)
        {
            if (!Can(project)) return false;
            if (IsProjectManager(project)) return true;
            return IsInTeam(project) && CanReadMessages(project);
        }

        public static bool CanCreateTask(Project project)
        {
            if (!Can(project)) return false;
            if (IsProjectManager(project)) return true;
            return IsInTeam(project) && CanReadTasks(project);
        }

        public static bool CanCreateSubtask(Task task)
        {
            if (!Can(task)) return false;
            if (IsProjectManager(task.Project)) return true;

            return IsInTeam(task.Project) &&
                   ((task.CreateBy == CurrentUserId) ||
                    !task.Responsibles.Any() ||
                    task.Responsibles.Contains(CurrentUserId));
        }

        public static bool CanCreateComment()
        {
            return IsProjectsEnabled(CurrentUserId) && SecurityContext.IsAuthenticated && !CurrentUserIsOutsider;
        }

        public static bool CanCreateComment(Message message)
        {
            return message.Status == MessageStatus.Open && CanCreateComment();
        }

        public static bool CanCreateTimeSpend(Project project)
        {
            if (project.Status == ProjectStatus.Closed) return false;
            return Can(project) && IsInTeam(project);
        }

        public static bool CanCreateTimeSpend(Task task)
        {
            if (!Can(task)) return false;
            if (task.Project.Status != ProjectStatus.Open) return false;
            if (IsInTeam(task.Project)) return true;

            return task.Responsibles.Contains(CurrentUserId) ||
                   task.SubTasks.Select(r => r.Responsible).Contains(CurrentUserId);
        }

        #endregion

        #region Can Edit

        public static bool CanEdit(Project project)
        {
            return Can(project) && IsProjectManager(project);
        }

        public static bool CanEdit(Milestone milestone)
        {
            if (!Can(milestone)) return false;
            if (milestone.Project.Status == ProjectStatus.Closed) return false;
            if (IsProjectManager(milestone.Project)) return true;
            if (!CanRead(milestone)) return false;

            return IsInTeam(milestone.Project) &&
                   (milestone.CreateBy == CurrentUserId ||
                    milestone.Responsible == CurrentUserId);
        }

        public static bool CanEdit(Message message)
        {
            if (!Can(message)) return false;
            if (IsProjectManager(message.Project)) return true;
            if (!CanRead(message)) return false;

            return IsInTeam(message.Project) && message.CreateBy == CurrentUserId;
        }

        public static bool CanEdit(Task task)
        {
            if (!Can(task)) return false;
            if (task.Project.Status == ProjectStatus.Closed) return false;
            if (IsProjectManager(task.Project)) return true;

            return IsInTeam(task.Project) &&
                   (task.CreateBy == CurrentUserId ||
                    !task.Responsibles.Any() ||
                    task.Responsibles.Contains(CurrentUserId) ||
                    task.SubTasks.Select(r => r.Responsible).Contains(CurrentUserId));
        }

        public static bool CanEdit(Task task, Subtask subtask)
        {
            if (!Can(subtask)) return false;
            if (CanEdit(task)) return true;

            return IsInTeam(task.Project) &&
                   (subtask.CreateBy == CurrentUserId ||
                    subtask.Responsible == CurrentUserId);
        }

        public static bool CanEditTeam(Project project)
        {
            return Can(project) && IsProjectManager(project);
        }

        public static bool CanEditComment(Project project, Comment comment)
        {
            if (!IsProjectsEnabled(CurrentUserId)) return false;
            if (project == null || comment == null) return false;
            return comment.CreateBy == CurrentUserId || IsProjectManager(project);
        }

        public static bool CanEditComment(Message message, Comment comment)
        {
            return message.Status == MessageStatus.Open && CanEditComment(message.Project, comment);
        }

        public static bool CanEdit(TimeSpend timeSpend)
        {
            if (!Can(timeSpend)) return false;
            if (IsProjectManager(timeSpend.Task.Project)) return true;
            if (timeSpend.PaymentStatus == PaymentStatus.Billed) return false;

            return timeSpend.Person == CurrentUserId || timeSpend.CreateBy == CurrentUserId;
        }

        public static bool CanEditPaymentStatus(TimeSpend timeSpend)
        {
            return Can(timeSpend) && IsProjectManager(timeSpend.Task.Project);
        }

        #endregion

        #region Can Delete

        public static bool CanDelete(Project project)
        {
            return CurrentUserAdministrator;
        }

        public static bool CanDelete(Task task)
        {
            if (!Can(task)) return false;
            if (IsProjectManager(task.Project)) return true;

            return IsInTeam(task.Project) && task.CreateBy == CurrentUserId;
        }

        public static bool CanDelete(Milestone milestone)
        {
            if (!Can(milestone)) return false;
            if (IsProjectManager(milestone.Project)) return true;

            return IsInTeam(milestone.Project) && milestone.CreateBy == CurrentUserId;
        }

        public static bool CanDelete(TimeSpend timeSpend)
        {
            if (!Can(timeSpend)) return false;
            if (IsProjectManager(timeSpend.Task.Project)) return true;
            if (timeSpend.PaymentStatus == PaymentStatus.Billed) return false;

            return IsInTeam(timeSpend.Task.Project) &&
                   (timeSpend.CreateBy == CurrentUserId || timeSpend.Person == CurrentUserId);
        }

        #endregion

        #region Demand

        public static void DemandCreateProject()
        {
            if (!CanCreateProject()) throw CreateSecurityException();
        }

        public static void DemandCreateMessage(Project project)
        {
            if (!CanCreateMessage(project)) throw CreateSecurityException();
        }

        public static void DemandCreateMilestone(Project project)
        {
            if (!CanCreateMilestone(project)) throw CreateSecurityException();
        }

        public static void DemandCreateTask(Project project)
        {
            if (!CanCreateTask(project)) throw CreateSecurityException();
        }

        public static void DemandCreateComment()
        {
            if (!CanCreateComment()) throw CreateSecurityException();
        }

        public static void DemandCreateComment(Message message)
        {
            if (!CanCreateComment(message)) throw CreateSecurityException();
        }


        public static void DemandRead(Milestone milestone)
        {
            if (!CanRead(milestone != null ? milestone.Project : null)) throw CreateSecurityException();
        }

        public static void DemandRead(Message message)
        {
            if (!CanRead(message)) throw CreateSecurityException();
        }

        public static void DemandRead(Task task)
        {
            if (!CanRead(task)) throw CreateSecurityException();
        }

        public static void DemandReadFiles(Project project)
        {
            if (!CanReadFiles(project)) throw CreateSecurityException();
        }

        public static void DemandReadTasks(Project project)
        {
            if (!CanReadTasks(project)) throw CreateSecurityException();
        }

        public static void DemandLinkContact(Project project)
        {
            if (!CanEdit(project)) throw CreateSecurityException();
        }


        public static void DemandEdit(Project project)
        {
            if (!CanEdit(project)) throw CreateSecurityException();
        }

        public static void DemandEdit(Message message)
        {
            if (!CanEdit(message)) throw CreateSecurityException();
        }

        public static void DemandEdit(Milestone milestone)
        {
            if (!CanEdit(milestone)) throw CreateSecurityException();
        }

        public static void DemandEdit(Task task)
        {
            if (!CanEdit(task)) throw CreateSecurityException();
        }

        public static void DemandEdit(Task task, Subtask subtask)
        {
            if (!CanEdit(task, subtask)) throw CreateSecurityException();
        }

        public static void DemandEdit(TimeSpend timeSpend)
        {
            if (!CanEdit(timeSpend)) throw CreateSecurityException();
        }

        public static void DemandEditTeam(Project project)
        {
            if (!CanEditTeam(project)) throw CreateSecurityException();
        }

        public static void DemandEditComment(Project project, Comment comment)
        {
            if (!CanEditComment(project, comment)) throw CreateSecurityException();
        }

        public static void DemandEditComment(Message message, Comment comment)
        {
            if (!CanEditComment(message, comment)) throw CreateSecurityException();
        }


        public static void DemandDeleteTimeSpend(TimeSpend timeSpend)
        {
            if (!CanDelete(timeSpend)) throw CreateSecurityException();
        }

        public static void DemandDelete(Task task)
        {
            if (!CanDelete(task)) throw CreateSecurityException();
        }

        public static void DemandDelete(Milestone milestone)
        {
            if (!CanDelete(milestone)) throw CreateSecurityException();
        }


        public static void DemandAuthentication()
        {
            if (!SecurityContext.CurrentAccount.IsAuthenticated)
            {
                throw CreateSecurityException();
            }
        }

        #endregion

        #region GetFactory

        private static Core.DataInterfaces.IDaoFactory GetFactory()
        {
            return new DaoFactory("projects", CoreContext.TenantManager.GetCurrentTenant().TenantId);
        }

        #endregion

        #region Is.. block

        public static bool IsAdministrator(Guid userId)
        {
            return CoreContext.UserManager.IsUserInGroup(userId, Constants.GroupAdmin.ID) ||
                   WebItemSecurity.IsProductAdministrator(EngineFactory.ProductId, userId);
        }

        private static bool IsProjectManager(Project project)
        {
            return IsProjectManager(project, CurrentUserId);
        }

        private static bool IsProjectManager(Project project, Guid userId)
        {
            return (IsAdministrator(userId) || (project != null && project.Responsible == userId)) &&
                   !CurrentUserIsVisitor;
        }

        public static bool IsVisitor(Guid userId)
        {
            return CoreContext.UserManager.GetUsers(userId).IsVisitor();
        }

        public static bool IsOutsider(Guid userId)
        {
            return CoreContext.UserManager.GetUsers(userId).IsOutsider();
        }

        public static bool IsInTeam(Project project)
        {
            return IsInTeam(project, CurrentUserId);
        }

        public static bool IsInTeam(Project project, Guid userId, bool includeAdmin = true)
        {
            var isAdmin = includeAdmin && IsAdministrator(userId);
            return isAdmin || (project != null && GetFactory().GetProjectDao().IsInTeam(project.ID, userId));
        }

        private static bool IsFollow(Project project, Guid userId)
        {
            var isAdmin = IsAdministrator(userId);
            var isPrivate = project != null && (!project.Private || isAdmin);

            return isPrivate && GetFactory().GetProjectDao().IsFollow(project.ID, userId);
        }

        #endregion

        #region TeamSecurity

        private static bool GetTeamSecurity(Project project, Guid userId, ProjectTeamSecurity security)
        {
            if (IsProjectManager(project, userId) || project == null || !project.Private) return true;
            var dao = GetFactory().GetProjectDao();
            var s = dao.GetTeamSecurity(project.ID, userId);
            return (s & security) != security && dao.IsInTeam(project.ID, userId);
        }

        private static bool GetTeamSecurityForParticipants(Project project, Guid userId, ProjectTeamSecurity security)
        {
            if (IsProjectManager(project, userId) || !project.Private) return true;
            var s = GetFactory().GetProjectDao().GetTeamSecurity(project.ID, userId);
            return (s & security) != security;
        }

        #endregion

        #region Exception

        public static Exception CreateSecurityException()
        {
            throw new System.Security.SecurityException("Access denied.");
        }

        public static Exception CreateGuestSecurityException()
        {
            throw new System.Security.SecurityException("A guest cannot be appointed as responsible.");
        }

        #endregion
    }
}