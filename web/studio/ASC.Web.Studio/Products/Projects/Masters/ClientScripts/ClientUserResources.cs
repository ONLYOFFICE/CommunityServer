/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
            var result = new List<KeyValuePair<string, object>>(5)
                         {
                             RegisterObject("EntryCountOnPage", Global.EntryCountOnPage),
                             RegisterObject("VisiblePageCount", Global.VisiblePageCount)
                         };

            var filter = new TaskFilter
                {
                    SortBy = "title",
                    SortOrder = true,
                    ProjectStatuses = new List<ProjectStatus> {ProjectStatus.Open}
                };

            var projects = Global.EngineFactory.ProjectEngine.GetByFilter(filter)
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

            var tags = Global.EngineFactory.TagEngine.GetTags().Select(r => new {value = r.Key, title = r.Value.HtmlEncode()});

            result.Add(RegisterObject("Projects", new {response = projects}));
            result.Add(RegisterObject("Tags", new {response = tags}));


            if (context.Request.UrlReferrer != null && string.IsNullOrEmpty(HttpUtility.ParseQueryString(context.Request.GetUrlRewriter().Query)["prjID"]) && string.IsNullOrEmpty(HttpUtility.ParseQueryString(context.Request.UrlReferrer.Query)["prjID"]))
            {
                filter = new TaskFilter
                    {
                        SortBy = "deadline",
                        SortOrder = false,
                        MilestoneStatuses = new List<MilestoneStatus> {MilestoneStatus.Open}
                    };

                var milestones = Global.EngineFactory.MilestoneEngine.GetByFilter(filter)
                                       .Select(m => new
                                           {
                                               id = m.ID,
                                               title = m.Title,
                                               deadline = SetDate(m.DeadLine, TimeZoneInfo.Local),
                                               projectOwner = new {id = m.Project.ID}
                                           });

                result.Add(RegisterObject("Milestones", new {response = milestones}));
            }

            return result;
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
            var currentUserId = SecurityContext.CurrentAccount.ID.ToString();
            var userLastModified = CoreContext.UserManager.GetMaxUsersLastModified().Ticks.ToString(CultureInfo.InvariantCulture);
            var projectMaxLastModified = Global.EngineFactory.ProjectEngine.GetMaxLastModified().ToString(CultureInfo.InvariantCulture);
            var milestoneMaxLastModified = Global.EngineFactory.MilestoneEngine.GetLastModified();
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
                    MilestoneStatuses = new List<MilestoneStatus> {MilestoneStatus.Open},
                    ProjectIds = new List<int> {Convert.ToInt32(currentProject)}
                };

            var milestones = Global.EngineFactory.MilestoneEngine.GetByFilter(filter)
                                   .Select(m => new
                                       {
                                           id = m.ID,
                                           title = m.Title,
                                           deadline = ClientUserResources.SetDate(m.DeadLine, TimeZoneInfo.Local),
                                           projectOwner = new {id = m.Project.ID}
                                       });

            var team = Global.EngineFactory.ProjectEngine.GetTeam(Convert.ToInt32(currentProject))
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
                                     isManager = r.IsManager,
                                     canReadFiles = r.CanReadFiles,
                                     canReadMilestones = r.CanReadMilestones,
                                     canReadMessages = r.CanReadMessages,
                                     canReadTasks = r.CanReadTasks,
                                     canReadContacts = r.CanReadContacts,
                                     isAdministrator = r.UserInfo.IsAdmin(),
                                     title = r.UserInfo.Title,
                                     profileUrl = r.UserInfo.GetUserProfilePageURL()
                                 }).OrderBy(r => r.displayName).ToList();

            return new List<KeyValuePair<string, object>>(2)
                   {
                       RegisterObject("Milestones", new {response = milestones}),
                       RegisterObject("Team", new {response = team})
                   };
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

            var milestoneMaxLastModified = Global.EngineFactory.MilestoneEngine.GetLastModified();

            return string.Format("{0}|{1}|{2}|{3}", currentUserId.ToString(), currentProject, teamMaxLastModified, milestoneMaxLastModified);
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
            return new List<KeyValuePair<string, object>>
                   {
                       RegisterObject("IsModuleAdmin", CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, EngineFactory.ProductId))
                   };
        }

        protected override string GetCacheHash()
        {
            return SecurityContext.CurrentAccount.ID.ToString() +
                   CoreContext.UserManager.GetMaxUsersLastModified().Ticks.ToString(CultureInfo.InvariantCulture) +
                   CoreContext.UserManager.GetMaxGroupsLastModified().Ticks.ToString(CultureInfo.InvariantCulture);
        }
    }
}