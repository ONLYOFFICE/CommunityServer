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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

using ASC.Core;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Users;
using ASC.Web.Projects.Classes;

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
            yield return RegisterObject("EntryCountOnPage", Global.EntryCountOnPage);
            yield return RegisterObject("VisiblePageCount", Global.VisiblePageCount);

            var filter = new TaskFilter
            {
                SortBy = "title",
                SortOrder = true,
                ProjectStatuses = new List<ProjectStatus> { ProjectStatus.Open }
            };

            var projects = Global.EngineFactory.GetProjectEngine().GetByFilter(filter)
                .Select(pr => new
                    {
                        id = pr.ID,
                        title = pr.Title,
                        responsible = pr.Responsible,
                        //created = (ApiDateTime) pr.CreateOn,
                        security = new
                            {
                                canCreateMilestone = ProjectSecurity.CanCreateMilestone(pr),
                                canCreateMessage = ProjectSecurity.CanCreateMessage(pr),
                                canCreateTask = ProjectSecurity.CanCreateTask(pr),
                                canEditTeam = ProjectSecurity.CanEditTeam(pr),
                                canReadFiles = ProjectSecurity.CanReadFiles(pr),
                                canReadMilestones = ProjectSecurity.CanReadMilestones(pr),
                                canReadMessages = ProjectSecurity.CanReadMessages(pr),
                                canReadTasks = ProjectSecurity.CanReadTasks(pr),
                                isInTeam = ProjectSecurity.IsInTeam(pr, SecurityContext.CurrentAccount.ID, false),
                                canLinkContact = ProjectSecurity.CanLinkContact(pr),
                            },
                        isPrivate = pr.Private,
                        status = pr.Status
                    });

            var tags = Global.EngineFactory.GetTagEngine().GetTags().Select(r => new { id = r.Key, title = r.Value.HtmlEncode() });

            yield return RegisterObject("Projects", new { response = projects });
            yield return RegisterObject("Tags", new { response = tags });


            if (context.Request.UrlReferrer != null && string.IsNullOrEmpty(HttpUtility.ParseQueryString(context.Request.GetUrlRewriter().Query)["prjID"]) && string.IsNullOrEmpty(HttpUtility.ParseQueryString(context.Request.UrlReferrer.Query)["prjID"]))
            {
                filter = new TaskFilter
                    {
                        SortBy = "deadline",
                        SortOrder = false,
                        MilestoneStatuses = new List<MilestoneStatus> { MilestoneStatus.Open }
                    };

                var milestones = Global.EngineFactory.GetMilestoneEngine().GetByFilter(filter)
                    .Select(m => new
                        {
                            id = m.ID,
                            title = m.Title,
                            deadline = SetDate(m.DeadLine, TimeZoneInfo.Local)
                        });

                yield return RegisterObject("Milestones", new { response = milestones });
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
            var currentUserId = SecurityContext.CurrentAccount.ID;
            var userLastModified = CoreContext.UserManager.GetMaxUsersLastModified().Ticks.ToString(CultureInfo.InvariantCulture);
            var projectMaxLastModified = Global.EngineFactory.GetProjectEngine().GetMaxLastModified().ToString(CultureInfo.InvariantCulture);
            var milestoneMaxLastModified = Global.EngineFactory.GetMilestoneEngine().GetLastModified();
            return string.Format("{0}|{1}|{2}|{3}", currentUserId, userLastModified, projectMaxLastModified, milestoneMaxLastModified);
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
            
            var filter = new TaskFilter
            {
                SortBy = "deadline",
                SortOrder = false,
                MilestoneStatuses = new List<MilestoneStatus> { MilestoneStatus.Open },
                ProjectIds = new List<int> { Convert.ToInt32(currentProject) }
            };

            var milestones = Global.EngineFactory.GetMilestoneEngine().GetByFilter(filter)
                                   .Select(m => new
                                       {
                                           id = m.ID,
                                           title = m.Title,
                                           deadline = ClientUserResources.SetDate(m.DeadLine, TimeZoneInfo.Local)
                                       });

            var team = Global.EngineFactory.GetProjectEngine().GetTeam(Convert.ToInt32(currentProject))
                .Select(r => new
                            {
                                id = r.UserInfo.ID,
                                displayName = DisplayUserSettings.GetFullUserName(r.UserInfo.ID),
                                userName = r.UserInfo.UserName,
                                avatarSmall = UserPhotoManager.GetSmallPhotoURL(r.UserInfo.ID),
                                status = r.UserInfo.Status,
                                groups = CoreContext.UserManager.GetUserGroups(r.UserInfo.ID).Select(x => new
                                    {
                                        id = x.ID,
                                        name = x.Name,
                                        manager = CoreContext.UserManager.GetUsers(CoreContext.UserManager.GetDepartmentManager(x.ID)).UserName
                                    }).ToList(),
                                isVisitor = r.UserInfo.IsVisitor(),
                                isAdmin = r.UserInfo.IsAdmin(),
                                isOwner = r.UserInfo.IsOwner(),
                                canReadFiles = r.CanReadFiles,
                                canReadMilestones = r.CanReadMilestones,
                                canReadMessages = r.CanReadMessages,
                                canReadTasks = r.CanReadTasks,
                                canReadContacts = r.CanReadContacts,
                                isAdministrator = r.UserInfo.IsAdmin(),
                                title = r.UserInfo.Title
                            }).OrderBy(r=> r.displayName).ToList();

            yield return RegisterObject("Milestones", new { response = milestones });
            yield return RegisterObject("Team", new { response = team });
        }

        protected override string GetCacheHash()
        {
            var currentUserId = SecurityContext.CurrentAccount.ID;

            var currentProject = "0";

            if (HttpContext.Current.Request.GetUrlRewriter() != null )
            {
                currentProject = HttpUtility.ParseQueryString(HttpContext.Current.Request.GetUrlRewriter().Query)["prjID"];

                if (string.IsNullOrEmpty(currentProject) && HttpContext.Current.Request.UrlReferrer != null)
                {
                    currentProject = HttpUtility.ParseQueryString(HttpContext.Current.Request.UrlReferrer.Query)["prjID"];
                }
            }

            var teamMaxLastModified = DateTime.UtcNow;
            teamMaxLastModified = teamMaxLastModified.AddSeconds(-teamMaxLastModified.Second);

            var milestoneMaxLastModified = Global.EngineFactory.GetMilestoneEngine().GetLastModified();

            return string.Format("{0}|{1}|{2}|{3}", currentUserId, currentProject, teamMaxLastModified, milestoneMaxLastModified);
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
            yield return RegisterObject("IsModuleAdmin", CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, EngineFactory.ProductId));
        }

        protected override string GetCacheHash()
        {
            return SecurityContext.CurrentAccount.ID +
                CoreContext.UserManager.GetMaxUsersLastModified().Ticks.ToString(CultureInfo.InvariantCulture) +
                CoreContext.GroupManager.GetMaxGroupsLastModified().Ticks.ToString(CultureInfo.InvariantCulture);
        }
    }
}
