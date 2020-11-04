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
using System.Web;
using ASC.Api.Attributes;
using ASC.Api.Documents;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Api.Projects.Calendars;
using ASC.Api.Utils;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Core.Calendars;
using ASC.Web.Projects;
using ASC.Web.Projects.Core;
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

                        var index = project.ID % CalendarColors.BaseColors.Count;
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
                        sharingOptions.PublicItems.Add(new SharingOptions.PublicItem { Id = userId, IsGroup = false });
                        foreach (var participant in team.Where(r => r.ProjectID == p.ID))
                        {
                            sharingOptions.PublicItems.Add(new SharingOptions.PublicItem
                            {
                                Id = participant.ID,
                                IsGroup = false
                            });
                        }

                        var index = p.ID % CalendarColors.BaseColors.Count;
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
        public ProjectsCommonSettings UpdateSettings(bool? everebodyCanCreate,
            bool? hideEntitiesInPausedProjects,
            StartModuleType? startModule,
            object folderId)
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

            if (startModule.HasValue || folderId != null)
            {
                if (!ProjectSecurity.IsProjectsEnabled(CurrentUserId)) ProjectSecurity.CreateSecurityException();
                var settings = ProjectsCommonSettings.LoadForCurrentUser();
                if (startModule.HasValue)
                {
                    settings.StartModuleType = startModule.Value;
                }

                if (folderId != null)
                {
                    settings.FolderId = folderId;
                }

                settings.SaveForCurrentUser();
                return settings;
            }

            return null;
        }

        [Read(@"settings")]
        public ProjectsCommonSettings GetSettings()
        {
            var commonSettings = ProjectsCommonSettings.Load();
            var userSettings = ProjectsCommonSettings.LoadForCurrentUser();

            return new ProjectsCommonSettings
            {
                EverebodyCanCreate = commonSettings.EverebodyCanCreate,
                HideEntitiesInPausedProjects = commonSettings.HideEntitiesInPausedProjects,
                StartModuleType = userSettings.StartModuleType,
                FolderId = userSettings.FolderId,
            };
        }


        [Create(@"status")]
        public CustomTaskStatus CreateStatus(CustomTaskStatus status)
        {
            return EngineFactory.StatusEngine.Create(status);
        }

        [Update(@"status")]
        public CustomTaskStatus UpdateStatus(CustomTaskStatus newStatus)
        {
            if (newStatus.IsDefault && !EngineFactory.StatusEngine.Get().Any(r => r.IsDefault && r.StatusType == newStatus.StatusType))
            {
                return CreateStatus(newStatus);
            }

            var status = EngineFactory.StatusEngine.Get().FirstOrDefault(r => r.Id == newStatus.Id).NotFoundIfNull();

            status.Title = Update.IfNotEmptyAndNotEquals(status.Title, newStatus.Title);
            status.Description = Update.IfNotEmptyAndNotEquals(status.Description, newStatus.Description);
            status.Color = Update.IfNotEmptyAndNotEquals(status.Color, newStatus.Color);
            status.Image = Update.IfNotEmptyAndNotEquals(status.Image, newStatus.Image);
            status.ImageType = Update.IfNotEmptyAndNotEquals(status.ImageType, newStatus.ImageType);
            status.Order = Update.IfNotEmptyAndNotEquals(status.Order, newStatus.Order);
            status.StatusType = Update.IfNotEmptyAndNotEquals(status.StatusType, newStatus.StatusType);
            status.Available = Update.IfNotEmptyAndNotEquals(status.Available, newStatus.Available);

            EngineFactory.StatusEngine.Update(status);

            return status;
        }

        [Update(@"statuses")]
        public List<CustomTaskStatus> UpdateStatuses(List<CustomTaskStatus> statuses)
        {
            foreach (var status in statuses)
            {
                UpdateStatus(status);
            }

            return statuses;
        }

        [Read(@"status")]
        public List<CustomTaskStatus> GetStatuses()
        {
            return EngineFactory.StatusEngine.GetWithDefaults();
        }

        [Delete(@"status/{id}")]
        public CustomTaskStatus DeleteStatus(int id)
        {
            var status = EngineFactory.StatusEngine.Get().FirstOrDefault(r => r.Id == id).NotFoundIfNull();
            EngineFactory.StatusEngine.Delete(status.Id);
            return status;
        }
    }
}