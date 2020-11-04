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
using System.Globalization;
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Core;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Users;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Core;
using Autofac;

namespace ASC.Web.Projects.Masters.ClientScripts
{
    public class ClientUserResources : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Projects.Master"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var filter = new TaskFilter
            {
                SortBy = "title",
                SortOrder = true,
                ProjectStatuses = new List<ProjectStatus> { ProjectStatus.Open }
            };

            if (!ProjectsCommonSettings.Load().HideEntitiesInPausedProjects)
            {
                filter.ProjectStatuses.Add(ProjectStatus.Paused);
            }

            using (var scope = DIHelper.Resolve())
            {
                var engineFactory = scope.Resolve<EngineFactory>();
                var projectSecurity = scope.Resolve<ProjectSecurity>();
                var projects = engineFactory.ProjectEngine.GetByFilter(filter)
                    .Select(pr => new
                    {
                        id = pr.ID,
                        title = pr.Title,
                        responsible = pr.Responsible,
                        //created = (ApiDateTime) pr.CreateOn,
                        security = new
                        {
                            canCreateMilestone = projectSecurity.CanCreate<Milestone>(pr),
                            canCreateMessage = projectSecurity.CanCreate<Message>(pr),
                            canCreateTask = projectSecurity.CanCreate<Task>(pr),
                            canCreateTimeSpend = projectSecurity.CanCreate<TimeSpend>(pr),
                            canEditTeam = projectSecurity.CanEditTeam(pr),
                            canReadFiles = projectSecurity.CanReadFiles(pr),
                            canReadMilestones = projectSecurity.CanRead<Milestone>(pr),
                            canReadMessages = projectSecurity.CanRead<Message>(pr),
                            canReadTasks = projectSecurity.CanRead<Task>(pr),
                            isInTeam = projectSecurity.IsInTeam(pr, SecurityContext.CurrentAccount.ID, false),
                            canLinkContact = projectSecurity.CanLinkContact(pr)
                        },
                        isPrivate = pr.Private,
                        status = pr.Status,
                        taskCountTotal = pr.TaskCountTotal
                    }).ToList();

                var tags = engineFactory.TagEngine.GetTags()
                        .Select(r => new { value = r.Key, title = r.Value.HtmlEncode() })
                        .ToList();

                var result = new List<KeyValuePair<string, object>>(1)
                {
                    RegisterObject(
                        new
                        {
                            Global.EntryCountOnPage,
                            Global.VisiblePageCount,
                            Projects = new {response = projects},
                            Tags = new {response = tags},
                            ProjectsCount = engineFactory.ProjectEngine.GetByFilterCount(new TaskFilter())
                        })
                };

                filter = new TaskFilter
                {
                    SortBy = "deadline",
                    SortOrder = false,
                    MilestoneStatuses = new List<MilestoneStatus> { MilestoneStatus.Open }
                };

                var milestones = engineFactory.MilestoneEngine.GetByFilter(filter)
                    .Select(m => new
                    {
                        id = m.ID,
                        title = m.Title,
                        deadline = SetDate(m.DeadLine, TimeZoneInfo.Local),
                        projectOwner = new { id = m.Project.ID },
                        status = (int)m.Status
                    }).ToList();

                result.Add(RegisterObject(new { Milestones = new { response = milestones } }));

                return result;
            }
        }

        public static string SetDate(DateTime value, TimeZoneInfo timeZone)
        {
            var timeZoneOffset = TimeSpan.Zero;
            var utcTime = DateTime.MinValue;

            if (value.Kind == DateTimeKind.Local)
            {
                value = TimeZoneInfo.ConvertTimeToUtc(new DateTime(value.Ticks, DateTimeKind.Unspecified), timeZone);
            }

            if (value.Kind == DateTimeKind.Utc)
            {
                utcTime = value; //Set UTC time
                timeZoneOffset = timeZone.GetUtcOffset(value);
            }

            utcTime = utcTime.Add(timeZoneOffset);

            var dateString = utcTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffff", CultureInfo.InvariantCulture);
            var offsetString = timeZoneOffset.Ticks == 0 ? "Z" : string.Format("{0}{1,2:00}:{2,2:00}", timeZoneOffset.Ticks > 0 ? "+" : "", timeZoneOffset.Hours, timeZoneOffset.Minutes);
            return dateString + offsetString;
        }

        protected override string GetCacheHash()
        {
            using (var scope = DIHelper.Resolve())
            {
                var engineFactory = scope.Resolve<EngineFactory>();
                var currentUserId = SecurityContext.CurrentAccount.ID.ToString();
                var userLastModified = CoreContext.UserManager.GetMaxUsersLastModified().Ticks.ToString(CultureInfo.InvariantCulture);
                var projectMaxLastModified = engineFactory.ProjectEngine.GetMaxLastModified().ToString(CultureInfo.InvariantCulture);
                var milestoneMaxLastModified = engineFactory.MilestoneEngine.GetLastModified();
                var paused = ProjectsCommonSettings.LoadForCurrentUser().HideEntitiesInPausedProjects;
                return string.Format("{0}|{1}|{2}|{3}|{4}", currentUserId, userLastModified, projectMaxLastModified, milestoneMaxLastModified, paused);
            }
        }
    }

    public class ClientProjectResources : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Projects.Master"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var currentProject = "0";

            if (context.Request.GetUrlRewriter() != null)
            {
                currentProject = HttpUtility.ParseQueryString(context.Request.GetUrlRewriter().Query)["prjID"];

                if (string.IsNullOrEmpty(currentProject) && context.Request.UrlReferrer != null)
                {
                    currentProject = HttpUtility.ParseQueryString(context.Request.UrlReferrer.Query)["prjID"];
                }
            }
            using (var scope = DIHelper.Resolve())
            {
                var engineFactory = scope.Resolve<EngineFactory>();

                var team = engineFactory.ProjectEngine.GetTeam(Convert.ToInt32(currentProject))
                    .Select(r => new
                    {
                        id = r.UserInfo.ID,
                        displayName = DisplayUserSettings.GetFullUserName(r.UserInfo.ID),
                        email = r.UserInfo.Email,
                        userName = r.UserInfo.UserName,
                        avatarSmall = UserPhotoManager.GetSmallPhotoURL(r.UserInfo.ID),
                        avatar = UserPhotoManager.GetBigPhotoURL(r.UserInfo.ID),
                        status = r.UserInfo.Status,
                        groups = CoreContext.UserManager.GetUserGroups(r.UserInfo.ID).Select(x => new
                        {
                            id = x.ID,
                            name = x.Name,
                            manager =
                                CoreContext.UserManager.GetUsers(CoreContext.UserManager.GetDepartmentManager(x.ID))
                                    .UserName
                        }).ToList(),
                        isVisitor = r.UserInfo.IsVisitor(),
                        isAdmin = r.UserInfo.IsAdmin(),
                        isOwner = r.UserInfo.IsOwner(),
                        isManager = r.IsManager,
                        canReadFiles = r.CanReadFiles,
                        canReadMilestones = r.CanReadMilestones,
                        canReadMessages = r.CanReadMessages,
                        canReadTasks = r.CanReadTasks,
                        canReadContacts = r.CanReadContacts,
                        title = r.UserInfo.Title,
                        profileUrl = r.UserInfo.GetUserProfilePageURL()
                    }).OrderBy(r => r.displayName).ToList();

                return new List<KeyValuePair<string, object>>(1)
                {
                    RegisterObject(
                        new
                        {
                            Team = new {response = team},
                            projectFolder = engineFactory.FileEngine.GetRoot(Convert.ToInt32(currentProject))
                        })
                };
            }
        }

        protected override string GetCacheHash()
        {
            var currentUserId = SecurityContext.CurrentAccount.ID;

            var currentProject = "0";

            if (HttpContext.Current.Request.GetUrlRewriter() != null)
            {
                currentProject = HttpUtility.ParseQueryString(HttpContext.Current.Request.GetUrlRewriter().Query)["prjID"];

                if (string.IsNullOrEmpty(currentProject) && HttpContext.Current.Request.UrlReferrer != null)
                {
                    currentProject = HttpUtility.ParseQueryString(HttpContext.Current.Request.UrlReferrer.Query)["prjID"];
                }
            }

            var teamMaxLastModified = DateTime.UtcNow;
            teamMaxLastModified = teamMaxLastModified.AddSeconds(-teamMaxLastModified.Second);

            return string.Format("{0}|{1}|{2}", currentUserId.ToString(), currentProject, teamMaxLastModified);
        }
    }

    public class ClientCurrentUserResources : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Projects.Master"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            using (var scope = DIHelper.Resolve())
            {
                var projectSecurity = scope.Resolve<ProjectSecurity>();
                return new List<KeyValuePair<string, object>>(1)
                {
                    RegisterObject(
                        new
                        {
                            CanCreateProject = projectSecurity.CanCreate<Project>(null),
                            IsModuleAdmin = projectSecurity.CurrentUserAdministrator,
                            ProjectsCommonSettings.LoadForCurrentUser().StartModuleType,
                            WebItemManager.ProjectsProductID
                        })
                };
            }
        }

        protected override string GetCacheHash()
        {
            using (var scope = DIHelper.Resolve())
            {
                var projectSecurity = scope.Resolve<ProjectSecurity>();

                return SecurityContext.CurrentAccount.ID.ToString() +
                       CoreContext.UserManager.GetMaxUsersLastModified().Ticks.ToString(CultureInfo.InvariantCulture) +
                       CoreContext.UserManager.GetMaxGroupsLastModified().Ticks.ToString(CultureInfo.InvariantCulture) +
                       projectSecurity.CanCreate<Project>(null) +
                       ProjectsCommonSettings.LoadForCurrentUser().StartModuleType;
            }
        }
    }
}