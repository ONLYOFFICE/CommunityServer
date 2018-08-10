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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Api.Attributes;
using ASC.Api.Documents;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Api.Projects.Calendars;
using ASC.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Core.Calendars;
using ASC.Web.Projects;
using ASC.Web.Projects.Core;
using ASC.Web.Studio.Utility;
using Autofac;

namespace ASC.Api.Projects
{
    ///<summary>
    ///Projects access
    ///</summary>
    public partial class ProjectApi : ProjectApiBase, IApiEntryPoint
    {
        private readonly DocumentsApi documentsApi;

        ///<summary>
        ///Api name entry
        ///</summary>
        public string Name
        {
            get { return "project"; }
        }

        public TaskFilter CreateFilter(EntityType entityType)
        {
            var filter = new TaskFilter
                   {
                       SortOrder = !Context.SortDescending,
                       SearchText = Context.FilterValue,
                       Offset = Context.StartIndex,
                       Max = Context.Count
                   };

            if (!string.IsNullOrEmpty(Context.SortBy))
            {
                var type = entityType.ToString();
                var sortColumns = filter.SortColumns.ContainsKey(type) ? filter.SortColumns[type] : null;
                if (sortColumns != null && sortColumns.Any())
                    filter.SortBy = sortColumns.ContainsKey(Context.SortBy) ? Context.SortBy : sortColumns.First().Key;
            }

            Context.SetDataFiltered().SetDataPaginated().SetDataSorted();

            return filter;
        }

        ///<summary>
        ///Constructor
        ///</summary>
        ///<param name="context"></param>
        ///<param name="documentsApi">Docs api</param>
        public ProjectApi(ApiContext context, DocumentsApi documentsApi)
        {
            this.documentsApi = documentsApi;

            Context = context;
        }

        private void SetTotalCount(int count)
        {
            Context.SetTotalCount(count);
        }

        private long StartIndex
        {
            get { return Context.StartIndex; }
        }

        private long Count
        {
            get { return Context.Count; }
        }

        private static HttpRequest Request
        {
            get { return HttpContext.Current.Request; }
        }


        internal static List<BaseCalendar> GetUserCalendars(Guid userId)
        {
            using (var scope = DIHelper.Resolve())
            {
                var engineFactory = scope.Resolve<EngineFactory>();

                var cals = new List<BaseCalendar>();
                var engine = engineFactory.ProjectEngine;
                var projects = engine.GetByParticipant(userId);

                if (projects != null)
                {
                    var team = engine.GetTeam(projects.Select(r => r.ID).ToList());

                    foreach (var project in projects)
                    {
                        var p = project;

                        var sharingOptions = new SharingOptions();
                        foreach (var participant in team.Where(r => r.ProjectID == p.ID))
                        {
                            sharingOptions.PublicItems.Add(new SharingOptions.PublicItem
                            {
                                Id = participant.ID,
                                IsGroup = false
                            });
                        }

                        var index = project.ID%CalendarColors.BaseColors.Count;
                        cals.Add(new ProjectCalendar(
                            project,
                            CalendarColors.BaseColors[index].BackgroudColor,
                            CalendarColors.BaseColors[index].TextColor,
                            sharingOptions, false));
                    }
                }

                var folowingProjects = engine.GetFollowing(userId);
                if (folowingProjects != null)
                {
                    var team = engine.GetTeam(folowingProjects.Select(r => r.ID).ToList());

                    foreach (var project in folowingProjects)
                    {
                        var p = project;

                        if (projects != null && projects.Any(proj => proj.ID == p.ID)) continue;

                        var sharingOptions = new SharingOptions();
                        sharingOptions.PublicItems.Add(new SharingOptions.PublicItem {Id = userId, IsGroup = false});
                        foreach (var participant in team.Where(r => r.ProjectID == p.ID))
                        {
                            sharingOptions.PublicItems.Add(new SharingOptions.PublicItem
                            {
                                Id = participant.ID,
                                IsGroup = false
                            });
                        }

                        var index = p.ID%CalendarColors.BaseColors.Count;
                        cals.Add(new ProjectCalendar(
                            p,
                            CalendarColors.BaseColors[index].BackgroudColor,
                            CalendarColors.BaseColors[index].TextColor,
                            sharingOptions, true));
                    }
                }

                return cals;
            }
        }

        [Update(@"settings")]
        public ProjectsCommonSettings UpdateSettings(bool? everebodyCanCreate, bool? hideEntitiesInPausedProjects, StartModuleType? startModule)
        {
            if (everebodyCanCreate.HasValue || hideEntitiesInPausedProjects.HasValue)
            {
                if (!ProjectSecurity.CurrentUserAdministrator) ProjectSecurity.CreateSecurityException();

                var settings = ProjectsCommonSettings.Load();

                if (everebodyCanCreate.HasValue)
                {
                    settings.EverebodyCanCreate = everebodyCanCreate.Value;
                }

                if (hideEntitiesInPausedProjects.HasValue)
                {
                    settings.HideEntitiesInPausedProjects = hideEntitiesInPausedProjects.Value;
                }

                settings.Save();
                return settings;
            }

            if (startModule.HasValue)
            {
                if (!ProjectSecurity.IsProjectsEnabled(CurrentUserId)) ProjectSecurity.CreateSecurityException();
                var settings = ProjectsCommonSettings.LoadForCurrentUser();
                settings.StartModuleType = startModule.Value;
                settings.SaveForCurrentUser();
                return settings;
            }

            return null;
        }

        [Read(@"settings")]
        public ProjectsCommonSettings GetSettings()
        {
            return ProjectsCommonSettings.Load();
        }
    }
}