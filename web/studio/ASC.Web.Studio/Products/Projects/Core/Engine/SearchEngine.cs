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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using ASC.Common.Logging;
using ASC.Common.Utils;
using ASC.Files.Core;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Files;
using ASC.Web.Files.Api;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Core;
using Autofac;
using IDaoFactory = ASC.Projects.Core.DataInterfaces.IDaoFactory;

namespace ASC.Projects.Engine
{
    [DebuggerDisplay("SearchItem: EntityType = {EntityType}, ID = {ID}, Title = {Title}")]
    public class SearchItem
    {
        public EntityType EntityType { get; private set; }
        public string ItemPath { get; private set; }
        public string ID { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public DateTime CreateOn { get; private set; }
        public SearchItem Container { get; set; }

        public SearchItem(EntityType entityType, string id, string title, DateTime createOn, SearchItem container = null, string desc = "", string itemPath = "")
        {
            Container = container;
            EntityType = entityType;
            ID = id;
            Title = title;
            Description = desc;
            CreateOn = createOn;
            
            if (!string.IsNullOrEmpty(itemPath))
            {
                ItemPath = ItemPathToAbsolute(itemPath);
            }
            else if(container != null)
            {
                ItemPath = container.ItemPath;
            }
        }

        public SearchItem(ProjectEntity entity)
            : this(entity.EntityType, entity.ID.ToString(CultureInfo.InvariantCulture), entity.Title, entity.CreateOn, new SearchItem(entity.Project), entity.Description, entity.ItemPath)
        {
        }

        public SearchItem(Project entity)
            : this(entity.EntityType, entity.ID.ToString(CultureInfo.InvariantCulture), entity.Title, entity.CreateOn, desc: entity.Description, itemPath: entity.ItemPath)
        {
        }

        public Dictionary<string, object> GetAdditional()
        {
            var result = new Dictionary<string, object>
                             {
                                 { "Type", EntityType },
                                 { "Hint", LocalizedEnumConverter.ConvertToString(EntityType) }
                             };

            if (Container != null)
            {
                result.Add("ContainerValue", Container.Title);
                result.Add("ContainerTitle", LocalizedEnumConverter.ConvertToString(Container.EntityType));
                result.Add("ContainerPath", Container.ItemPath);
            }

            return result;
        }

        private string ItemPathToAbsolute(string itemPath)
        {
            var projectID = ID;
            var container = Container;

            while (true)
            {
                if(container == null) break;

                projectID = container.ID;

                container = container.Container;
            }

            return string.Format(itemPath, VirtualPathUtility.ToAbsolute(PathProvider.BaseVirtualPath), projectID, ID);
        }
    }

    public class SearchEngine
    {
        public IDaoFactory DaoFactory { get; set; }
        public ProjectSecurity ProjectSecurity { get; set; }

        public EngineFactory EngineFactory { get; set; }
        public CommentEngine CommentEngine { get { return EngineFactory.CommentEngine; } }
        public TaskEngine TaskEngine { get { return EngineFactory.TaskEngine; } }

        private readonly List<SearchItem> searchItems;

        public SearchEngine()
        {
            searchItems = new List<SearchItem>();
        }

        public IEnumerable<SearchItem> Search(string searchText, int projectId = 0)
        {
            var queryResult = DaoFactory.SearchDao.Search(searchText, projectId);
            using (var scope = DIHelper.Resolve())
            {
                var projectSecurity = scope.Resolve<ProjectSecurity>();

                foreach (var r in queryResult)
                {
                    switch (r.EntityType)
                    {
                        case EntityType.Project:
                            var project = (Project) r;
                            if (projectSecurity.CanRead(project))
                            {
                                searchItems.Add(new SearchItem(project));
                            }
                            continue;
                        case EntityType.Milestone:
                            var milestone = (Milestone) r;
                            if (projectSecurity.CanRead(milestone))
                            {
                                searchItems.Add(new SearchItem(milestone));
                            }
                            continue;
                        case EntityType.Message:
                            var message = (Message) r;
                            if (projectSecurity.CanRead(message))
                            {
                                searchItems.Add(new SearchItem(message));
                            }
                            continue;
                        case EntityType.Task:
                            var task = (Task) r;
                            if (projectSecurity.CanRead(task))
                            {
                                searchItems.Add(new SearchItem(task));
                            }
                            continue;
                        case EntityType.Comment:
                            var comment = (Comment) r;
                            var entity = CommentEngine.GetEntityByTargetUniqId(comment);
                            if (entity == null) continue;

                            searchItems.Add(new SearchItem(comment.EntityType,
                                comment.ID.ToString(CultureInfo.InvariantCulture), HtmlUtil.GetText(comment.Content),
                                comment.CreateOn, new SearchItem(entity)));
                            continue;
                        case EntityType.SubTask:
                            var subtask = (Subtask) r;
                            var parentTask = TaskEngine.GetByID(subtask.Task);
                            if (parentTask == null) continue;

                            searchItems.Add(new SearchItem(subtask.EntityType,
                                subtask.ID.ToString(CultureInfo.InvariantCulture), subtask.Title, subtask.CreateOn,
                                new SearchItem(parentTask)));
                            continue;
                    }
                }
            }

            try
            {
                // search in files
                var fileEntries = new List<FileEntry>();
                using (var folderDao = FilesIntegration.GetFolderDao())
                using (var fileDao = FilesIntegration.GetFileDao())
                {
                    fileEntries.AddRange(folderDao.Search(searchText, true));
                    fileEntries.AddRange(fileDao.Search(searchText, true));

                    var projectIds = projectId != 0
                                         ? new List<int> {projectId}
                                         : fileEntries.GroupBy(f => f.RootFolderId)
                                               .Select(g => folderDao.GetFolder(g.Key))
                                               .Select(f => f != null ? folderDao.GetBunchObjectID(f.RootFolderId).Split('/').Last() : null)
                                               .Where(s => !string.IsNullOrEmpty(s))
                                               .Select(int.Parse);

                    var rootProject = projectIds.ToDictionary(id => FilesIntegration.RegisterBunch("projects", "project", id.ToString(CultureInfo.InvariantCulture)));
                    fileEntries.RemoveAll(f => !rootProject.ContainsKey(f.RootFolderId));

                    var security = FilesIntegration.GetFileSecurity();
                    fileEntries.RemoveAll(f => !security.CanRead(f));

                    foreach (var f in fileEntries)
                    {
                        var id = rootProject[f.RootFolderId];
                        var project = DaoFactory.ProjectDao.GetById(id);

                        if (ProjectSecurity.CanReadFiles(project))
                        {
                            var itemId = f.FileEntryType == FileEntryType.File
                                             ? FilesLinkUtility.GetFileWebPreviewUrl(f.Title, f.ID)
                                             : Web.Files.Classes.PathProvider.GetFolderUrl((Folder) f, project.ID);
                            searchItems.Add(new SearchItem(EntityType.File, itemId, f.Title, f.CreateOn, new SearchItem(project), itemPath: "{2}"));
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LogManager.GetLogger("ASC").Error(err);
            }
            return searchItems;
        }
    }
}